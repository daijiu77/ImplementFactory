using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.DJ.ImplementFactory.Commons
{
    public class DynamicEntity
    {
        IDbHelper dbHelper = new DbHelper();

        public T Exec<T>(MethodInformation method, DataOptType dataOptType, Action<T> action, string sql)
        {
            T result = default(T);
            AutoCall autoCall = (AutoCall)method.AutoCall;
            dynamicWhere(method, ref sql);

            IList<DataEntity<DataElement>> dataElements = null;
            DataEntity<DataElement> dataElements1 = null;
            DbList<DbParameter> dbParameters = null;

            if (0 < method.paraList.Count)
            {
                foreach (Para item in method.paraList)
                {
                    if (null != item.ParaValue)
                    {
                        if (null != item.ParaValue as IList<DataEntity<DataElement>>)
                        {
                            dataElements = item.ParaValue as IList<DataEntity<DataElement>>;
                            break;
                        }
                        else if (null != item.ParaValue as DataEntity<DataElement>)
                        {
                            dataElements = new List<DataEntity<DataElement>>();
                            dataElements1 = item.ParaValue as DataEntity<DataElement>;
                            dataElements.Add(dataElements1);
                            break;
                        }
                    }
                }                
            }

            if (null == dataElements) dataElements = new List<DataEntity<DataElement>>();

            string err = "";
            bool EnabledBuffer = method.methodComponent.EnabledBuffer;
            EnabledBuffer = method.methodComponent.IsAsync ? true : EnabledBuffer;
            
            if (DataOptType.select == dataOptType
                || DataOptType.count == dataOptType
                || DataOptType.procedure == dataOptType)
            {
                if (0 < dataElements.Count) dataElements1 = dataElements[0];
                dbParameters = GetDbParameters(method, dataElements1, ref sql);
                GetSqlByProvider(method, dbParameters, ref sql);                
                dbHelper.query(autoCall, sql, dbParameters, EnabledBuffer, (dt) =>
                {
                    DynamicCodeAutoCall dynamicCodeAutoCall = new DynamicCodeAutoCall();
                    string execClassPath = "";
                    string execMethodName = "";
                    bool paraExsit = false;
                    Type returnType = dynamicCodeAutoCall.GetParaTypeOfResultExecMethod(method, ref execClassPath, ref execMethodName, ref paraExsit);
                    if (paraExsit)
                    {
                        Type resultExecMethodType = execClassPath.GetClassTypeByPath();

                        if (null == resultExecMethodType)
                        {
                            err = "The ClassPath '" + execClassPath + "' is not exist.";
                            autoCall.e(err, ErrorLevels.dangerous);
                            throw new Exception(err);
                        }

                        object clsObj = null;
                        MethodInfo methodInfo = resultExecMethodType.GetMethod(execMethodName);
                        if (null != methodInfo)
                        {
                            if (methodInfo.ReturnType != typeof(T))
                            {
                                err = "";
                                string msg = "The return value type of the method '{0}' of the class '{1}' and the method '{2}' of the interface '{3}' are not the same.";
                                method.append(ref err, 0, msg, execMethodName, execClassPath, method.methodInfo.Name, method.methodInfo.DeclaringType.FullName);
                                autoCall.e(err, ErrorLevels.dangerous);
                                throw new Exception(err);
                            }

                            object vData = dataTableTo(method, dt, returnType);
                            try
                            {
                                clsObj = Activator.CreateInstance(resultExecMethodType);
                                result = (T)methodInfo.Invoke(clsObj, new object[] { vData });
                            }
                            catch (Exception ex)
                            {
                                autoCall.e(ex.ToString(), ErrorLevels.dangerous);
                                //throw;
                            }
                        }
                    }
                    else
                    {
                        result = dataTableTo<T>(method, dt);
                    }

                    if (null != action) action(result);
                }, ref err);
            }
            else
            {
                int n = 0;
                Func<int, T> funcResult = n1 =>
                {
                    object v = n1;
                    if (typeof(bool) == typeof(T))
                    {
                        v = 0 < n1;
                    }
                    return (T)v;
                };

                foreach (DataEntity<DataElement> item in dataElements)
                {
                    dbParameters = GetDbParameters(method, item, ref sql);
                    switch (dataOptType)
                    {
                        case DataOptType.insert:
                            dbHelper.insert(autoCall, sql, dbParameters, EnabledBuffer, (num) =>
                            {
                                n += num;
                                result = funcResult(n);
                                if (null != action) action(result);
                            }, ref err);
                            break;
                        case DataOptType.update:
                            dbHelper.update(autoCall, sql, dbParameters, EnabledBuffer, (num) =>
                            {
                                n += num;
                                result = funcResult(n);
                                if (null != action) action(result);
                            }, ref err);
                            break;
                        case DataOptType.delete:
                            dbHelper.delete(autoCall, sql, dbParameters, EnabledBuffer, (num) =>
                            {
                                n += num;
                                result = funcResult(n);
                                if (null != action) action(result);
                            }, ref err);
                            break;
                    }
                }
                
                result = funcResult(n);
            }
            return result;
        }

        void dynamicWhere(MethodInformation method, ref string sql)
        {
            if (string.IsNullOrEmpty(sql)) return;
            if (null == method.paraList) return;
            if (0 == method.paraList.Count) return;

            Para para = method.paraList[0];
            DataEntity<DataElement> dataElements = para.ParaValue as DataEntity<DataElement>;
            if (null == dataElements) return;

            Regex rg = new Regex(@"\swhere\s+\{" + para.ParaName + @"\}", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(sql)) return;

            string whereStr = getDynamicWhere(method, dataElements);
            whereStr = string.IsNullOrEmpty(whereStr) ? "1=1" : whereStr;
            whereStr = " where " + whereStr;
            sql = sql.Replace(rg.Match(sql).Groups[0].Value, whereStr);
        }

        string getDynamicWhere(MethodInformation method, DataEntity<DataElement> dataElements)
        {
            string sWhere = "";
            string sWhere1 = "";
            string sVal = "";
            bool mbool = false;
            foreach (var item in dataElements)
            {
                if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                    && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                if (!string.IsNullOrEmpty(item.fieldNameOfSourceTable))
                {
                    mbool = true;
                    if (null != item.IsEnabledContion)
                    {
                        if (!item.IsEnabledContion(item)) mbool = false;
                    }

                    if (mbool)
                    {
                        if (!string.IsNullOrEmpty(sWhere))
                        {
                            sWhere += " " + Enum.GetName(typeof(AndOr), item.logicUnion) + " ";
                        }

                        sVal = item.ToString();
                        if (item.compareSign.ToLower().Contains("like"))
                        {
                            if (db_dialect.oracle == DataAdapter.dbDialect)
                            {
                                sWhere += "0<instr(" + item.fieldNameOfSourceTable + ",'" + sVal + "')";
                            }
                            else
                            {
                                sVal = "'%" + sVal + "%'";
                                sWhere += item.fieldNameOfSourceTable + " " + item.compareSign + " " + sVal;
                            }
                        }
                        else
                        {
                            if (item.valueIsChar) sVal = "'" + sVal + "'";
                            sWhere += item.fieldNameOfSourceTable + " " + item.compareSign + " " + sVal;
                        }
                    }
                }

                if (0 < item.groupCondition.Count)
                {
                    sWhere1 = getDynamicWhere(method, item.groupCondition);
                    if (!string.IsNullOrEmpty(sWhere1))
                    {
                        if (!string.IsNullOrEmpty(sWhere))
                        {
                            sWhere += " " + Enum.GetName(typeof(AndOr), item.groupLogicUnion) + " (" + sWhere1 + ")";
                        }
                        else
                        {
                            sWhere = "(" + sWhere1 + ")";
                        }
                    }
                }
            }
            return sWhere;
        }

        void GetSqlByProvider(MethodInformation method, DbList<DbParameter> dbParameters, ref string sql)
        {
            string dataProviderNamespace = method.dataProviderNamespace;
            string dataProviderClassName = method.dataProviderClassName;
            if (string.IsNullOrEmpty(dataProviderNamespace) || string.IsNullOrEmpty(dataProviderClassName)) return;

            AutoCall autoCall = (AutoCall)method.AutoCall;
            ISqlExpressionProvider sqlExpressionProvider = autoCall.GetDataProvider(dataProviderNamespace, dataProviderClassName, autoCall);
            if (null == sqlExpressionProvider) return;
            autoCall.GetSqlByDataProvider(sqlExpressionProvider, method.paraList, dbParameters, autoCall, method.dataOptType, ref sql);
        }

        public static DbList<DbParameter> GetDbParameters(MethodInformation method, DataEntity<DataElement> dataElements, ref string sql)
        {
            DbList<DbParameter> dbParameters = new DbList<DbParameter>();
            if (string.IsNullOrEmpty(sql)) return dbParameters;
            if (null == dataElements) return dbParameters;

            string msg = "";
            Regex rg = DynamicCodeChange.rgParaField;

            if (rg.IsMatch(sql) && 0 < method.paraList.Count)
            {
                if (null == method.paraList[0].ParaValue)
                {
                    msg = "The parameter value cann't be null.";
                    ((AutoCall)method.AutoCall).e(msg);
                    throw new Exception(msg);
                }

                string LeftSign = "";
                string DbTag = DJTools.GetParaTagByDbDialect(DataAdapter.dbDialect);
                string _dbTag = "";
                string unit = "";
                string unit1 = "";
                string FieldName = "";
                string EndSign = "";
                object v = null;
                string sql1 = sql;
                Match m = null;
                int num = 0;

                //DataEntity<DataElement> dataElements = (DataEntity<DataElement>)method.paraList[0].ParaValue;
                MatchCollection mc = rg.Matches(sql);                
                while (rg.IsMatch(sql1) && 1000 > num)
                {

                    if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                      && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                    m = rg.Match(sql1);
                    LeftSign = m.Groups["LeftSign"].Value;
                    if (!DynamicCodeChange.isEnabledField(LeftSign)) continue;
                    _dbTag = m.Groups["DbTag"].Value;
                    FieldName = m.Groups["FieldName"].Value;
                    EndSign = m.Groups["EndSign"].Value;

                    if (dataElements.ContainsKey(FieldName))
                    {
                        v = dataElements[FieldName].value;
                        dbParameters.Add(FieldName, v);
                        sql1 = sql1.Replace(m.Groups[0].Value, "");
                        sql1 = EndSign + sql1;

                        if (!DbTag.Equals(_dbTag))
                        {
                            unit = m.Groups[0].Value;
                            unit1 = LeftSign + DbTag + FieldName + EndSign;
                            sql = sql.Replace(unit, unit1);
                        }                        
                    }                    
                    num++;
                }
            }

            return dbParameters;
        }

        T dataTableTo<T>(MethodInformation method, DataTable dt)
        {
            Type type = typeof(T);
            return (T)dataTableTo(method, dt, type);
        }

        object dataTableTo(MethodInformation method, DataTable dt, Type paraType)
        {
            object result = null;
            if (null == dt) return result;
            if (0 == dt.Rows.Count) return result;

            object v = null;
            if (DJTools.IsBaseType(paraType))
            {
                v = dt.Rows[0][0];
                v = DJTools.ConvertTo(v, paraType);
            }
            else if (typeof(DataEntity<DataElement>) == paraType)
            {
                DataEntity<DataElement> dataElements = new DataEntity<DataElement>();
                DataRow dr = dt.Rows[0];
                DataColumnCollection columns = dt.Columns;
                foreach (DataColumn item in columns)
                {
                    if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                        && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                    v = dr[item.ColumnName];
                    dataElements.Add(item.ColumnName, v);
                }
                v = dataElements;
            }
            else if (typeof(List<DataEntity<DataElement>>) == paraType || typeof(IList<DataEntity<DataElement>>) == paraType)
            {
                List<DataEntity<DataElement>> dataElements = new List<DataEntity<DataElement>>();
                DataEntity<DataElement> dataElements1 = null;
                DataColumnCollection columns = dt.Columns;
                foreach (DataRow dr in dt.Rows)
                {
                    if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                        && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                    dataElements1 = new DataEntity<DataElement>();
                    foreach (DataColumn dc in columns)
                    {
                        if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                            && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                        v = dr[dc.ColumnName];
                        dataElements1.Add(dc.ColumnName, v);
                    }
                    dataElements.Add(dataElements1);
                }

                v = dataElements;
            }
            else if (typeof(DataTable) == paraType)
            {
                v = dt;
            }
            else if (typeof(DataSet) == paraType)
            {
                v = new DataSet();
                ((DataSet)v).Tables.Add(dt);
            }
            else if (typeof(Collections.IEnumerable) == paraType.GetInterface("IEnumerable") && typeof(string) != paraType)
            {
                if (typeof(IList) == paraType.GetInterface("IList") || typeof(Array) == paraType.BaseType)
                {
                    if (paraType.BaseType == typeof(Array))
                    {
                        v = createArrayByType(paraType, dt.Rows.Count);
                        arrayAdd(method, v, dt);
                    }
                    else
                    {
                        v = createListByType(paraType);
                        listAdd(method, v, dt);
                    }
                }
                else
                {
                    //Dictionary
                }
            }
            else
            {
                v = Activator.CreateInstance(paraType);
                DataRowToEntity(method, dt.Rows[0], v);
            }
            if (null != v) result = v;
            return result;
        }

        object createArrayByType(Type type, int length)
        {
            object arr = null;
            if (false == type.IsArray) return arr;

            try
            {
                arr = type.InvokeMember("Set", BindingFlags.CreateInstance, null, arr, new object[] { length });
            }
            catch { }

            return arr;
        }

        object createListByType(Type type)
        {
            object list = null;
            if (null == type.GetInterface("IList")) return list;

            Type[] types = type.GetGenericArguments();
            Type listType = typeof(List<string>);
            string asseName = listType.Assembly.GetName().Name;
            string dicTypeName = listType.FullName;
            string s = @"\[[^\[\]]+\]";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            if (rg.IsMatch(dicTypeName))
            {
                int n = 0;
                int len = types.Length;
                string txt = "";
                Type ele = null;
                MatchCollection mc = rg.Matches(dicTypeName);
                foreach (Match item in mc)
                {
                    if (n == len) break;
                    ele = types[n];
                    s = ele.FullName;
                    s += ", " + ele.Assembly.GetName().Name;
                    s += ", Version=" + ele.Assembly.GetName().Version.ToString();
                    s += ", Culture=neutral";
                    s += ", PublicKeyToken=null";
                    s = "[" + s + "]";
                    txt = item.Groups[0].Value;
                    dicTypeName = dicTypeName.Replace(txt, s);
                    n++;
                }
            }

            object v = Activator.CreateInstance(asseName, dicTypeName) as ObjectHandle;
            if (null == v) return list;
            list = ((ObjectHandle)v).Unwrap();
            return list;
        }

        void listAdd(MethodInformation method, object list, DataTable dt)
        {
            if (null == list) return;
            if (null == dt) return;
            if (0 == dt.Rows.Count) return;

            Type listType = list.GetType();
            if (null == listType.GetInterface("IList")) return;

            MethodInfo methodInfo = listType.GetMethod("Add");
            if (null == methodInfo) return;

            Type[] types = listType.GetGenericArguments();
            object ele = null;

            foreach (DataRow dr in dt.Rows)
            {
                if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                    && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                ele = Activator.CreateInstance(types[0]);
                DataRowToEntity(method, dr, ele);
                try
                {
                    methodInfo.Invoke(list, new object[] { ele });
                }
                catch { }
            }
        }

        void arrayAdd(MethodInformation method, object arrObj, DataTable dt)
        {
            if (null == arrObj) return;
            if (null == dt) return;
            if (0 == dt.Rows.Count) return;

            Type type = arrObj.GetType();
            if (false == type.IsArray) return;

            Array array = (Array)arrObj;
            Type eleType = type.GetElementType();

            bool isBaseType = DJTools.IsBaseType(eleType);
            object ele = null;
            int n = 0;
            int len = array.Length;
            foreach (DataRow dr in dt.Rows)
            {
                if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                    && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                ele = Activator.CreateInstance(eleType);
                DataRowToEntity(method, dr, ele);
                array.SetValue(ele, n);
                n++;
            }
        }

        void DataRowToEntity(MethodInformation method, DataRow dr, object entity)
        {
            if (null == dr) return;
            if (null == entity) return;

            object v = null;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (DataColumn dc in dr.Table.Columns)
            {
                v = dr[dc.ColumnName];
                dic.Add(dc.ColumnName, v);
            }

            PropertyInfo[] piArr = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo item in piArr)
            {
                if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                    && 0 < method.methodComponent.Interval) Thread.Sleep(method.methodComponent.Interval);
                v = null;
                dic.TryGetValue(item.Name, out v);
                if (null != v)
                {
                    if (DJTools.IsBaseType(item.PropertyType))
                    {
                        v = DJTools.ConvertTo(v, item.PropertyType);
                        try
                        {
                            item.SetValue(entity, v, null);
                        }
                        catch (Exception ex)
                        {

                            //throw;
                        }
                    }
                }
            }
        }
    }
}
