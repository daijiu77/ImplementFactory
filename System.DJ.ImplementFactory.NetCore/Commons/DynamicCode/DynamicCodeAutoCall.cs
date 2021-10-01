using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    class DynamicCodeAutoCall
    {
        public void DataProviderCode(string sqlVarName, MethodInformation method, DataOptType dataOptType, ref string code, ref string paraListVarName)
        {
            string dataProviderNamespace = method.dataProviderNamespace;
            string dataProviderClassName = method.dataProviderClassName;
            if (string.IsNullOrEmpty(dataProviderNamespace) || string.IsNullOrEmpty(dataProviderClassName)) return;

            if (string.IsNullOrEmpty(paraListVarName) || paraListVarName.ToLower().Equals("null"))
            {
                paraListVarName = "dbParaList";
                method.append(ref code, LeftSpaceLevel.one, "DbList<System.Data.Common.DbParameter> {0} = new DbList<System.Data.Common.DbParameter>();", paraListVarName);
            }

            method.append(ref code, LeftSpaceLevel.one, "string {0} = \"\";", sqlVarName);
            method.append(ref code, LeftSpaceLevel.one, "ISqlExpressionProvider dataProvider = {0}.GetDataProvider(\"{1}\",\"{2}\",{0});", method.AutoCallVarName, dataProviderNamespace, dataProviderClassName);
            method.append(ref code, LeftSpaceLevel.one, "{0}.GetSqlByDataProvider(dataProvider,{1},{2},{3},(DataOptType){4}, ref {5});", method.AutoCallVarName, method.ParaListVarName, paraListVarName, method.AutoCallVarName, ((int)dataOptType).ToString(), sqlVarName);
        }


        #region Made 'replace string' by sql body contain '{FieldName}'
        /// <summary>
        /// 根据sql语句中包含的 {FieldName} 字段名称,从复杂对象(Dictionary\EntityObject)匹配key或propertyName生成替换{FieldName}的代码字符串
        /// </summary>
        /// <param name="sqlVarName">在sql2 = sql2.Replace("{id}",id) 中 sqlVarName = sql2</param>
        /// <param name="FieldName">sql语句中包含的{FieldName}字段名称</param>
        /// <param name="para">接口方法中的参数</param>
        /// <param name="method">接口方法</param>
        /// <param name="func"> Func<FuncPara>
        /// sqlVarName1: sql语句字符串的变量名, 
        /// FieldName1: sql语句中包含的字段名称 {FieldName}, 
        /// ParaName1: 接口方法参数名称, 
        /// PropertyName: 如果参数是Dictionary类型,PropertyName为key; 如果参数是类实体对象,PropertyName为该对象的属性名称, 
        /// PropertyType: 接口方法参数对象中属性类型
        /// </param>
        /// <returns>例: sql = sql.Replace("{FieldName}", ParaName.PropertyName)</returns>
        string ExecReplaceForSqlByFieldName_Mixed(string sqlVarName, string FieldName, Para para, MethodInformation method, Func<FuncPara, string> func)
        {
            string code = "";
            string fn = FieldName.ToLower();
            object val = null;

            //参数要考虑List,Dictionary,Array集合类型
            if (null != para.ParaType.GetInterface("IEnumerable"))
            {
                //仅对键值对(Key/value)进行处理
                if (null != para.ParaType.GetInterface("IDictionary"))
                {
                    string key = "";
                    bool mbool = false;
                    IDictionary dic = (IDictionary)para.ParaValue;
                    foreach (var item in dic)
                    {
                        key = item.GetType().GetProperty("Key").GetValue(item, null).ToString();
                        val = item.GetType().GetProperty("Value").GetValue(item, null);
                        if (!key.ToLower().Equals(fn)) continue;
                        val = null == val ? "" : val;
                        if (!mbool)
                        {
                            if (!DJTools.IsBaseType(val.GetType())) break;
                            mbool = true;
                        }
                        //sqlVarName, FieldName, para.ParaName, key, item.GetType()
                        code = func(new FuncPara()
                        {
                            sqlVarName1 = sqlVarName,
                            FieldName1 = FieldName,
                            ParaName1 = para.ParaName,
                            PropertyName = key,
                            PropertyType = item.GetType(),
                            ValueType = item.GetType().GetProperty("Value").PropertyType.FullName
                        });
                        //method.append(ref code, LeftSpaceLevel.one, "{0} = {0}.Replace(\"{{1}}\", {2}[\"{3}\"].ToString());", sqlVarName, FieldName, para.ParaName, key);
                        break;
                    }
                }
            }
            else
            {
                PropertyInfo[] piArr = para.ParaType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo pi in piArr)
                {
                    if (!pi.Name.ToLower().Equals(fn)) continue;
                    if (!DJTools.IsBaseType(pi.PropertyType)) continue;
                    code = func(new FuncPara()
                    {
                        sqlVarName1 = sqlVarName,
                        FieldName1 = FieldName,
                        ParaName1 = para.ParaName,
                        PropertyName = pi.Name,
                        PropertyType = typeof(PropertyInfo),
                        ValueType = pi.PropertyType.FullName
                    });
                    //method.append(ref code, LeftSpaceLevel.one, "{0} = {0}.Replace(\"{{1}}\", {2}.{3}.ToString());", sqlVarName, FieldName, para.ParaName, pi.Name);
                    break;
                }
            }
            return code;
        }

        void ReplaceTagForSql(MethodInformation method, LeftSpaceLevel leftSpaceLevel, string resultStrVarName, string oldStrVarName, string newStrVarName, string newStrTypeName, ref string code)
        {
            //method.append(ref code, leftSpaceLevel, "");             
            method.append(ref code, leftSpaceLevel, "_objVal = {0};", newStrVarName);
            method.append(ref code, leftSpaceLevel, "if(typeof({0}) == typeof(string)) _objVal = null == _objVal ? \"\" : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "else if(typeof({0}) == typeof(decimal)) _objVal = null == _objVal ? 0 : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "else if(typeof({0}) == typeof(bool)) _objVal = null == _objVal ? false : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "else if(typeof({0}) == typeof(DateTime)) _objVal = null == _objVal ? DateTime.MinValue : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "else if(typeof({0}) == typeof(Guid)) _objVal = null == _objVal ? Guid.Empty : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "else if(typeof({0}) == typeof(Int16) || typeof({0}) == typeof(int) || typeof({0}) == typeof(Int64) || typeof({0}) == typeof(float) || typeof({0}) == typeof(double)) _objVal = null == _objVal ? 0 : _objVal;", newStrTypeName);
            method.append(ref code, leftSpaceLevel, "{0} = {0}.Replace(\"{{1}}\", _objVal.ToString());", resultStrVarName, oldStrVarName);
        }

        /// <summary>
        /// 根据复杂对象获取替换语句
        /// </summary>
        /// <param name="funcPara"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        string GetReplaceByMixed(FuncPara funcPara, MethodInformation method)
        {
            string code2 = "";
            string sqlVarName1 = funcPara.sqlVarName1;
            string FieldName1 = funcPara.FieldName1;
            string ParaName1 = funcPara.ParaName1;
            string PropertyName = funcPara.PropertyName;
            Type PropertyType = funcPara.PropertyType;

            method.append(ref code2, LeftSpaceLevel.one, "");
            if (PropertyType == typeof(PropertyInfo))
            {
                //method.append(ref code2, LeftSpaceLevel.one, "{0} = {0}.Replace(\"{{1}}\", {2}.{3}.ToString());", sqlVarName1, FieldName1, ParaName1, PropertyName);
                method.append(ref code2, LeftSpaceLevel.one, "var {1}_01 = {0}.{1};", ParaName1, PropertyName);
            }
            else
            {
                //Dictionary                
                //method.append(ref code2, LeftSpaceLevel.one, "{0} = {0}.Replace(\"{{1}}\", {2}[\"{3}\"].ToString());", sqlVarName1, FieldName1, ParaName1, PropertyName);
                method.append(ref code2, LeftSpaceLevel.one, "var {1}_01 = {0}[\"{1}\"];", ParaName1, PropertyName);
            }
            ReplaceTagForSql(method, LeftSpaceLevel.one, sqlVarName1, FieldName1, PropertyName + "_01", funcPara.ValueType, ref code2);
            return code2;
        }

        /// <summary>
        /// 替换包含 {FieldName} 的sql语句, 例: update UserInfo set UserName='{UserName}' where id='{id}'
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlVarName">在sql2 = sql2.Replace("{id}",id) 中 sqlVarName = sql2</param>
        /// <param name="method"></param>
        /// <returns></returns>
        public string ExecReplaceForSqlByFieldName(string sql, string sqlVarName, MethodInformation method)
        {
            if (string.IsNullOrEmpty(sql)) return "";
            string code = "";
            string sql1 = sql;
            string s = @"\{(?<FieldName>[^\{\}]*[a-z][^\{\}]*)\}";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);

            Type paraType = null;
            bool isDynamicEntity = false;
            bool isMulti = false;
            if (null != method.paraList)
            {
                if (0 < method.paraList.Count) paraType = method.paraList[0].ParaType;
            }

            if (typeof(DataEntity<DataElement>) == paraType)
            {
                code = sql;
                isDynamicEntity = true;
            }
            else if(typeof(List<DataEntity<DataElement>>) == paraType || typeof(IList<DataEntity<DataElement>>) == paraType)
            {
                code = sql;
                isDynamicEntity = true;
                isMulti = true;
            }
            else
            {
                method.append(ref code, LeftSpaceLevel.one, "string {0} = \"{1}\";", sqlVarName, sql);
            }

            if (rg.IsMatch(sql) && 0 < method.paraList.Count)
            {
                string FieldName = "";
                string fv = "";
                object v = null;
                string code1 = "";
                Para para = null;
                PList<Para> paras = method.paraList;
                DataEntity<DataElement> dataElements = null;
                DataElement dataElement = null;
                if (isDynamicEntity)
                {
                    para = paras[0];
                    if(null != para)
                    {
                        if (isMulti)
                        {
                            IList<DataEntity<DataElement>> list = (IList<DataEntity<DataElement>>)para.ParaValue;
                            if (0 < list.Count) dataElements = list[0];
                        }
                        else
                        {
                            dataElements = (DataEntity<DataElement>)para.ParaValue;
                        }
                    }                                      
                }
                else
                {
                    method.append(ref code, LeftSpaceLevel.one, "object _objVal = null;");
                }

                MatchCollection mc = rg.Matches(sql1);
                foreach (Match m in mc)
                {
                    FieldName = m.Groups["FieldName"].Value;
                    if (isDynamicEntity)
                    {
                        v = "";
                        if (null != dataElements)
                        {
                            if (FieldName.Equals(para.ParaName)) continue;
                            dataElement = dataElements[FieldName];
                            if (null == dataElement) continue;
                            v = dataElement.value;
                        }
                        
                        fv = null == v ? "" : v.ToString();
                        code = code.Replace("{" + FieldName + "}", fv);
                        continue;
                    }

                    para = paras[FieldName];
                    if (null != para)
                    {
                        if (DJTools.IsBaseType(para.ParaType))
                        {
                            //method.append(ref code, LeftSpaceLevel.one, "{0} = {0}.Replace(\"{{1}}\", {2});", sqlVarName, FieldName, para.ParaName);
                            method.append(ref code, LeftSpaceLevel.one, "");
                            ReplaceTagForSql(method, LeftSpaceLevel.one, sqlVarName, FieldName, para.ParaName, para.ParaType.FullName, ref code);
                        }
                        else
                        {
                            code1 = ExecReplaceForSqlByFieldName_Mixed(sqlVarName, FieldName, para, method, (funcPara) =>
                            {
                                string code2 = GetReplaceByMixed(funcPara, method);
                                return code2;
                            });

                            if (!string.IsNullOrEmpty(code1))
                            {
                                code += "\r\n" + code1;
                            }
                        }
                    }
                    else
                    {
                        foreach (Para item in paras)
                        {
                            code1 = ExecReplaceForSqlByFieldName_Mixed(sqlVarName, FieldName, item, method, (funcPara) =>
                            {
                                string code2 = GetReplaceByMixed(funcPara, method);
                                return code2;
                            });

                            if (!string.IsNullOrEmpty(code1))
                            {
                                code += "\r\n" + code1;
                            }
                        }
                    }
                }
            }
            method.append(ref code, LeftSpaceLevel.one, "{0} = DynamicCodeExec.Calculate({0});", sqlVarName);

            ReplaceGenericSign(method, sqlVarName, LeftSpaceLevel.one, ref code);
            return code;
        }
        #endregion

        #region Made a parameter collection by parameter tag in sql body.

        /// <summary>
        /// 从复杂对象获取元素添加到参数集合DbList<DbParamert>
        /// </summary>
        /// <param name="paraClassName">不同数据源参数类名称</param>
        /// <param name="funcPara"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        string AddElementToParamerCollectionFromMixed(string paraClassName, FuncPara funcPara, MethodInformation method)
        {
            string code2 = "";
            string sqlVarName1 = funcPara.sqlVarName1;
            string FieldName1 = funcPara.FieldName1;
            string ParaName1 = funcPara.ParaName1;
            string PropertyName = funcPara.PropertyName;
            Type PropertyType = funcPara.PropertyType;
            if (PropertyType == typeof(PropertyInfo))
            {
                method.append(ref code2, LeftSpaceLevel.one, "{0}.Add(\"{2}\",{3}.{4});", sqlVarName1, paraClassName, FieldName1, ParaName1, PropertyName);
            }
            else
            {
                //Dictionary
                method.append(ref code2, LeftSpaceLevel.one, "{0}.Add(\"{2}\",{3}[\"{4}\"]);", sqlVarName1, paraClassName, FieldName1, ParaName1, PropertyName);
            }
            return code2;
        }

        /// <summary>
        /// 根据sql参数获取参数列表, 如果sql语句里包含参数(@Parameter, :Parameter, ?Parameter)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="method"></param>
        /// <param name="paraListVarName">List<DbParameter> 的变量名称</param>
        /// <returns></returns>
        public string GetParametersBySqlParameter(string sql, MethodInformation method, ref string paraListVarName)
        {
            string code = "";
            string sql1 = sql;
            //sql server @ParameterName
            //oracle :ParameterName
            //mysql ?ParameterName
            Regex rg = DynamicCodeChange.rgParaField;
            Match mc = null;
            if (rg.IsMatch(sql) && 0 < method.paraList.Count)
            {
                if (string.IsNullOrEmpty(paraListVarName) || paraListVarName.ToLower().Equals("null"))
                {
                    paraListVarName = "dbParaList";
                    method.append(ref code, LeftSpaceLevel.one, "DbList<System.Data.Common.DbParameter> {0} = new DbList<System.Data.Common.DbParameter>();", paraListVarName);
                }

                //string parameterName = "";
                //object parameterValue = null;
                //var a = new Data.OracleClient.OracleParameter(parameterName, parameterValue);
                //var b = new MySql.Data.MySqlClient.MySqlParameter(parameterName, parameterValue);
                //var c = new System.Data.SqlClient.SqlParameter(parameterName, parameterValue);     
                string LeftSign = "";
                string DbTag = "";
                string FieldName = "";
                string EndSign = "";
                string code1 = "";
                string paraClassName = "";
                int n = 0;
                Para para = null;
                PList<Para> paras = method.paraList;
                while (rg.IsMatch(sql1) && 1000 > n)
                {
                    mc = rg.Match(sql1);
                    LeftSign = mc.Groups["LeftSign"].Value;
                    DbTag = mc.Groups["DbTag"].Value;
                    FieldName = mc.Groups["FieldName"].Value;
                    EndSign = mc.Groups["EndSign"].Value;
                    sql1 = sql1.Replace(mc.Groups[0].Value, "" + EndSign);
                    if (!DynamicCodeChange.isEnabledField(LeftSign))
                    {
                        continue;
                    }
                    //paraClassName = DJTools.GetParamertClassNameByDbTag(DbTag);
                    para = paras[FieldName];
                    if (null != para)
                    {
                        if (DJTools.IsBaseType(para.ParaType))
                        {
                            method.append(ref code, LeftSpaceLevel.one, "{0}.Add(\"{2}\",{3});", paraListVarName, paraClassName, FieldName, para.ParaName);
                        }
                        else
                        {
                            code1 = ExecReplaceForSqlByFieldName_Mixed(paraListVarName, FieldName, para, method, (funcPara) =>
                            {
                                string code2 = AddElementToParamerCollectionFromMixed(paraClassName, funcPara, method);
                                return code2;
                            });

                            if (!string.IsNullOrEmpty(code1))
                            {
                                code += "\r\n" + code1;
                            }
                        }
                    }
                    else
                    {
                        foreach (Para item in paras)
                        {
                            code1 = ExecReplaceForSqlByFieldName_Mixed(paraListVarName, FieldName, item, method, (funcPara) =>
                            {
                                string code2 = AddElementToParamerCollectionFromMixed(paraClassName, funcPara, method);
                                return code2;
                            });

                            if (!string.IsNullOrEmpty(code1))
                            {
                                code += "\r\n" + code1;
                            }
                        }
                    }
                    n++;
                }

            }
            return code;
        }
        #endregion

        /// <summary>
        /// 替换 sql 表达式中的泛型标识, 例: select * from {T} order by id asc
        /// </summary>
        /// <param name="method">泛型方法</param>
        /// <param name="sqlVarName"></param>
        /// <param name="leftSpaceLevel"></param>
        /// <param name="code"></param>
        private void ReplaceGenericSign(MethodInformation method, string sqlVarName, LeftSpaceLevel leftSpaceLevel, ref string code)
        {
            if (string.IsNullOrEmpty(method.methodComponent.GenericityParas)) return;
            string paras = method.methodComponent.GenericityParas;
            paras = paras.Replace("<", "");
            paras = paras.Replace(">", "");
            string[] arr = paras.Split(',');
            method.append(ref code, leftSpaceLevel, "System.Attribute att = null;");
            foreach (string item in arr)
            {
                method.append(ref code, leftSpaceLevel, "att = typeof({0}).GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute));", item);
                method.append(ref code, leftSpaceLevel, "if(null == att)");
                method.append(ref code, leftSpaceLevel, "{");
                method.append(ref code, leftSpaceLevel + 1, "{0} = {0}.Replace(\"{{1}}\", typeof({1}).GetType().Name);", sqlVarName, item);
                method.append(ref code, leftSpaceLevel, "}");
                method.append(ref code, leftSpaceLevel, "else");
                method.append(ref code, leftSpaceLevel, "{");
                method.append(ref code, leftSpaceLevel + 1, "{0} = {0}.Replace(\"{{1}}\", ((System.ComponentModel.DataAnnotations.Schema.TableAttribute)att).Name);", sqlVarName, item);
                method.append(ref code, leftSpaceLevel, "}");
                method.append(ref code, leftSpaceLevel, "");
            }
        }

        public Type GetClassTypeByClassPath(string classPath)
        {
            string[] arr = classPath.Split('.');
            string asseName = "";
            string an = "";
            string rootPath = ImplementAdapter.rootPath;
            string dllFile = "";
            Assembly ass = null;
            Type type1 = null;
            foreach (var item in arr)
            {
                asseName += "." + item;
                an = asseName.Substring(1);
                if (DJTools.isWeb)
                {
                    dllFile = Path.Combine(rootPath, "bin\\" + an + ".dll");
                }
                else
                {
                    dllFile = Path.Combine(rootPath, an + ".dll");
                }
                if (!File.Exists(dllFile)) continue;
                ass = Assembly.Load(an);
                if (null != ass)
                {
                    type1 = ass.GetType(classPath);
                    if (null != type1) break;
                }
            }

            if (null == type1)
            {
                Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in asses)
                {
                    type1 = item.GetType(classPath);
                    if (null != type1) break;
                }
            }
            return type1;
        }

        public Type GetParaTypeOfResultExecMethod(MethodInformation method, ref string execClassPath, ref string execMethodName, ref bool paraExsit)
        {
            execClassPath = "";
            execMethodName = "";
            if (null == method.ResultExecMethod) method.ResultExecMethod = new string[] { };

            if (2 == method.ResultExecMethod.Length)
            {
                execClassPath = method.ResultExecMethod[0];
                execMethodName = method.ResultExecMethod[1];
            }
            else if (3 == method.ResultExecMethod.Length)
            {
                execClassPath = method.ResultExecMethod[0] + "." + method.ResultExecMethod[1];
                execMethodName = method.ResultExecMethod[2];
            }

            Type returnType = null;
            paraExsit = false;
            if (!string.IsNullOrEmpty(execClassPath))
            {
                Type type = GetClassTypeByClassPath(execClassPath);
                MethodInfo methodInfo = null;
                if (null != type)
                {
                    methodInfo = type.GetMethod(execMethodName);
                }

                if (null != methodInfo)
                {
                    ParameterInfo[] paras = methodInfo.GetParameters();
                    if (0 < paras.Length)
                    {
                        paraExsit = true;
                        returnType = paras[0].ParameterType;
                    }
                }
            }
            return returnType;
        }

        /// <summary>
        /// 执行 select 时,生成执行sql语句的字符串
        /// </summary>
        /// <param name="sqlVarName">sql语句字符串的变量名称</param>
        /// <param name="paraListVarName">List<DbParameter>参数集合的变量名称</param>
        /// <param name="method"></param>
        /// <param name="code"></param>
        public void MadeExecuteSql(string sqlVarName, string paraListVarName, MethodInformation method, DataOptType dataOptType, ref string code)
        {
            method.append(ref code, LeftSpaceLevel.one, "");
            //1. 执行sql语句, 要考虑是否存在参数 List<DbParameter>, 也就是 paraListVarName 是否为空
            method.append(ref code, LeftSpaceLevel.one, "IDbHelper dbHelper = ImplementAdapter.DbHelper;");

            //2. 要得到接口方法的返回类型
            MethodComponent mc = method.methodComponent;

            string methodNameOfIDbHelper = "query";
            string enabledBuffer = method.methodComponent.EnabledBuffer.ToString().ToLower();
            string autCall = method.AutoCallVarName;

            enabledBuffer = method.methodComponent.IsAsync ? "true" : enabledBuffer;

            DynamicCodeChange dynamicCodeChange = new DynamicCodeChange();
            string dbTag = DJTools.GetParaTagByDbDialect(DataAdapter.dbDialect);

            method.append(ref code, LeftSpaceLevel.one, "string err = \"\";");
            method.append(ref code, LeftSpaceLevel.one, "");
            method.append(ref code, LeftSpaceLevel.one, "if(null != dbHelper)");
            method.append(ref code, LeftSpaceLevel.one, "{");
            method.append(ref code, LeftSpaceLevel.two, "{0} = {0}.Replace(\"{1}\", \"{2}\");", sqlVarName, dynamicCodeChange.procParaSign, dbTag);
            method.append(ref code, LeftSpaceLevel.two, "dbHelper.{0}({1}, {2}, {3}, {4}, dtTable =>", methodNameOfIDbHelper, autCall, sqlVarName, paraListVarName, enabledBuffer);
            method.append(ref code, LeftSpaceLevel.two, "{");
            #region ***** action
            method.append(ref code, LeftSpaceLevel.two + 1, "object vObj = dtTable;");
            if (DataOptType.select == dataOptType || DataOptType.procedure == dataOptType)
            {
                string execClassPath = "";
                string execMethodName = "";
                string rvName = mc.ResultVariantName;
                string defaultV = "";
                string typeName = "";
                bool paraExsit = false;
                Type returnType = GetParaTypeOfResultExecMethod(method, ref execClassPath, ref execMethodName, ref paraExsit);
                if (null != returnType)
                {
                    rvName = "result_execM";
                    defaultV = DJTools.getDefaultByType(returnType);
                    typeName = returnType.TypeToString();
                    method.append(ref code, LeftSpaceLevel.two + 1, "{0} {1} = {2};", typeName, rvName, defaultV);
                }

                returnType = null == returnType ? method.methodInfo.ReturnType : returnType;
                returnType = null != method.methodComponent.ActionType ? method.methodComponent.ActionType : returnType;

                method.append(ref code, LeftSpaceLevel.two + 1, "object resultObj = null;");
                method.append(ref code, LeftSpaceLevel.two + 1, "");

                if (typeof(DataTable) == returnType)
                {
                    method.append(ref code, LeftSpaceLevel.two + 1, "resultObj = null == vObj ? new System.Data.DataTable() : vObj;");
                }
                else if (typeof(DataSet) == returnType)
                {
                    method.append(ref code, LeftSpaceLevel.two + 1, "vObj = null == vObj ? new System.Data.DataTable() : vObj;");
                    method.append(ref code, LeftSpaceLevel.two + 1, "resultObj = ((System.Data.DataTable)vObj).DataSet;");
                }
                else if (DJTools.IsBaseType(returnType))
                {
                    method.append(ref code, LeftSpaceLevel.two + 1, "vObj = null == vObj ? new System.Data.DataTable() : vObj;");
                    method.append(ref code, LeftSpaceLevel.two + 1, "System.Data.DataTable resultTable = (System.Data.DataTable)vObj;");
                    method.append(ref code, LeftSpaceLevel.two + 1, "if(0 < resultTable.Rows.Count)");
                    method.append(ref code, LeftSpaceLevel.two + 1, "{");
                    if (typeof(string) == returnType)
                    {
                        method.append(ref code, LeftSpaceLevel.two + 2, "{0} = System.DBNull.Value == resultTable.Rows[0][0] ? \"\" : resultTable.Rows[0][0].ToString();", rvName);
                    }
                    else
                    {
                        method.append(ref code, LeftSpaceLevel.two + 2, "string results = System.DBNull.Value == resultTable.Rows[0][0] ? \"\" : resultTable.Rows[0][0].ToString();");
                        method.append(ref code, LeftSpaceLevel.two + 2, "{0}.TryParse(results, out {1});", returnType.Name, rvName);
                    }
                    method.append(ref code, LeftSpaceLevel.two + 2, "resultObj = {0};", rvName);
                    method.append(ref code, LeftSpaceLevel.two + 1, "}");
                    //method.append(ref code, LeftSpaceLevel.one, "");
                }
                else
                {
                    string sp = method.StartSpace;
                    method.StartSpace += method.getSpace(2);
                    DataTableToEntity(method, returnType, rvName, "resultObj", ref code);
                    method.StartSpace = sp;
                }

                if (paraExsit)
                {
                    string paraTypeName = DJTools.TypeToString(returnType);                    
                    method.append(ref code, LeftSpaceLevel.two + 1, "");
                    method.append(ref code, LeftSpaceLevel.two + 1, "{0} resultExecM = new {0}();", execClassPath);
                    method.append(ref code, LeftSpaceLevel.two + 1, "if(null == resultObj)");
                    method.append(ref code, LeftSpaceLevel.two + 1, "{");
                    method.append(ref code, LeftSpaceLevel.two + 2, "{0} = resultExecM.{1}({2});", mc.ResultVariantName, execMethodName, defaultV);
                    method.append(ref code, LeftSpaceLevel.two + 1, "}");
                    method.append(ref code, LeftSpaceLevel.two + 1, "else");
                    method.append(ref code, LeftSpaceLevel.two + 1, "{");
                    if (typeof(string) == returnType)
                    {
                        method.append(ref code, LeftSpaceLevel.two + 2, "{0} = resultExecM.{1}(resultObj.ToString());", mc.ResultVariantName, execMethodName);
                    }
                    else
                    {
                        method.append(ref code, LeftSpaceLevel.two + 2, "{0} = resultExecM.{1}(({2})resultObj);", mc.ResultVariantName, execMethodName, paraTypeName);
                    }
                    method.append(ref code, LeftSpaceLevel.two + 1, "}");
                }
                else
                {
                    method.append(ref code, LeftSpaceLevel.two + 1, "");
                    method.append(ref code, LeftSpaceLevel.two + 1, "{0} = ({1})resultObj;", rvName, mc.ResultTypeName);
                }
            }
            else if (DataOptType.count == dataOptType)
            {
                method.append(ref code, LeftSpaceLevel.two + 1, "vObj = null == vObj ? new System.Data.DataTable() : vObj;");
                method.append(ref code, LeftSpaceLevel.two + 1, "System.Data.DataTable resultDataTable = (System.Data.DataTable)vObj;");
                method.append(ref code, LeftSpaceLevel.two + 1, "if(0 < resultDataTable.Rows.Count)", mc.ResultVariantName);
                method.append(ref code, LeftSpaceLevel.two + 1, "{");
                method.append(ref code, LeftSpaceLevel.two + 2, "int nCount = Convert.ToInt32(resultDataTable.Rows[0][0].ToString());");
                if (typeof(bool) == method.methodInfo.ReturnType || typeof(Boolean) == method.methodInfo.ReturnType)
                {
                    method.append(ref code, LeftSpaceLevel.two + 2, "{0} = 0 < nCount;", mc.ResultVariantName);
                }
                else
                {
                    method.append(ref code, LeftSpaceLevel.two + 2, "{0} = nCount;", mc.ResultVariantName);
                }
                method.append(ref code, LeftSpaceLevel.two + 1, "}");
                method.append(ref code, LeftSpaceLevel.two + 1, "else");
                method.append(ref code, LeftSpaceLevel.two + 1, "{");
                if (typeof(bool) == method.methodInfo.ReturnType || typeof(Boolean) == method.methodInfo.ReturnType)
                {
                    method.append(ref code, LeftSpaceLevel.two + 2, "{0} = false;", mc.ResultVariantName);
                }
                else
                {
                    method.append(ref code, LeftSpaceLevel.two + 2, "{0} = 0;", mc.ResultVariantName);
                }
                method.append(ref code, LeftSpaceLevel.two + 1, "}");
            }

            if (null != method.methodComponent.ActionType)
            {
                method.append(ref code, LeftSpaceLevel.two + 1, "");
                method.append(ref code, LeftSpaceLevel.two + 1, "{0}({1});", method.methodComponent.ActionParaName, mc.ResultVariantName);
            }

            #endregion
            method.append(ref code, LeftSpaceLevel.two, "}, ref err);");
            method.append(ref code, LeftSpaceLevel.one, "}");

            //method.append(ref code, LeftSpaceLevel.one, "");
            method.append(ref code, LeftSpaceLevel.one, "");
            method.append(ref code, LeftSpaceLevel.one, "if(!string.IsNullOrEmpty(err))");
            method.append(ref code, LeftSpaceLevel.one, "{");
            method.append(ref code, LeftSpaceLevel.two, "throw new Exception(err);");
            method.append(ref code, LeftSpaceLevel.one, "}");
            method.append(ref code, LeftSpaceLevel.one, "");

        }

        void DataTableToEntity(MethodInformation method, Type returnType, string resultVarName, string resultVarName1, ref string code)
        {
            //分别对集合类型、单体类型的复杂对象进行赋值
            if (false == string.IsNullOrEmpty(resultVarName))
            {
                MethodComponent mc = method.methodComponent;
                bool isBaseArray = false;

                Func<string, LeftSpaceLevel, string, string> declearVar = (code1, leftSpaceLevel, typeName1) =>
                {
                    method.append(ref code1, leftSpaceLevel, "System.Reflection.PropertyInfo[] piArr = typeof({0}).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);", typeName1);
                    method.append(ref code1, leftSpaceLevel, "string fieldName = \"\";");
                    method.append(ref code1, leftSpaceLevel, "string fieldMapping = \"\";");
                    method.append(ref code1, leftSpaceLevel, "string attrName = \"\";");
                    method.append(ref code1, leftSpaceLevel, "object v = null;");
                    return code1;
                };

                Func<string, string, LeftSpaceLevel, string> dynamicEntityFunc = (code1, collectName, leftSpaceLevel) =>
                 {
                     method.append(ref code1, leftSpaceLevel, "foreach(System.Data.DataColumn dc in dt.Columns)");
                     method.append(ref code1, leftSpaceLevel, "{");
                     if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                        && 0 < method.methodComponent.Interval)
                     {
                         method.append(ref code1, leftSpaceLevel + 1, "System.Threading.Thread.Sleep({0});", method.methodComponent.Interval.ToString());
                     }
                     method.append(ref code1, leftSpaceLevel + 1, "{0}.Add(dc.ColumnName, dr[dc.ColumnName]);", collectName);
                     method.append(ref code1, leftSpaceLevel, "}");
                     return code1;
                 };

                method.append(ref code, LeftSpaceLevel.one, "");
                if (isBaseArray)
                {
                    //接口方法返回类型为: 基本类型的数组 (例: System.Int32[], System.String[])
                    //目前暂无这样的可能性, 执行sql要么返回单个数值, 要么返回一个 table
                }
                else if (typeof(DataEntity<DataElement>) == returnType)
                {
                    method.append(ref code, LeftSpaceLevel.one, "if(null != vObj)");
                    method.append(ref code, LeftSpaceLevel.one, "{");

                    method.append(ref code, LeftSpaceLevel.two, "if (vObj.GetType() == typeof(System.Data.DataTable))");
                    method.append(ref code, LeftSpaceLevel.two, "{");
                    method.append(ref code, LeftSpaceLevel.three, "DataEntity<DataElement> dataElements = new DataEntity<DataElement>();");
                    method.append(ref code, LeftSpaceLevel.three, "System.Data.DataTable dt = vObj as System.Data.DataTable;");
                    method.append(ref code, LeftSpaceLevel.three, "foreach (System.Data.DataRow dr in dt.Rows)");
                    method.append(ref code, LeftSpaceLevel.three, "{");
                    code = dynamicEntityFunc(code, "dataElements", LeftSpaceLevel.four);
                    method.append(ref code, LeftSpaceLevel.four, "break;");
                    method.append(ref code, LeftSpaceLevel.three, "}");
                    //method.append(ref code, LeftSpaceLevel.three, "");
                    method.append(ref code, LeftSpaceLevel.three, "{0} = dataElements;", resultVarName1);
                    method.append(ref code, LeftSpaceLevel.two, "}");

                    method.append(ref code, LeftSpaceLevel.one, "}");
                }
                else if (typeof(List<DataEntity<DataElement>>) == returnType || typeof(IList<DataEntity<DataElement>>) == returnType)
                {
                    method.append(ref code, LeftSpaceLevel.one, "if(null != vObj)");
                    method.append(ref code, LeftSpaceLevel.one, "{");

                    method.append(ref code, LeftSpaceLevel.two, "if (vObj.GetType() == typeof(System.Data.DataTable))");
                    method.append(ref code, LeftSpaceLevel.two, "{");
                    method.append(ref code, LeftSpaceLevel.three, "List<DataEntity<DataElement>> dataElements = new List<DataEntity<DataElement>>();");
                    method.append(ref code, LeftSpaceLevel.three, "DataEntity<DataElement> dataElements1 = null;");
                    method.append(ref code, LeftSpaceLevel.three, "System.Data.DataTable dt = vObj as System.Data.DataTable;");
                    method.append(ref code, LeftSpaceLevel.three, "foreach (System.Data.DataRow dr in dt.Rows)");
                    method.append(ref code, LeftSpaceLevel.three, "{");
                    if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                        && 0 < method.methodComponent.Interval)
                    {
                        method.append(ref code, LeftSpaceLevel.four, "System.Threading.Thread.Sleep({0});", method.methodComponent.Interval.ToString());
                    }
                    method.append(ref code, LeftSpaceLevel.four, "dataElements1 = new DataEntity<DataElement>();");
                    code = dynamicEntityFunc(code, "dataElements1", LeftSpaceLevel.four);
                    method.append(ref code, LeftSpaceLevel.four, "dataElements.Add(dataElements1);");
                    method.append(ref code, LeftSpaceLevel.three, "}");
                    //method.append(ref code, LeftSpaceLevel.three, "");
                    method.append(ref code, LeftSpaceLevel.three, "{0} = dataElements;", resultVarName1);
                    method.append(ref code, LeftSpaceLevel.two, "}");

                    method.append(ref code, LeftSpaceLevel.one, "}");
                }
                else
                {
                    //接口方法返回值类型为复杂对象类型
                    method.append(ref code, LeftSpaceLevel.one, "if(null != vObj)");
                    method.append(ref code, LeftSpaceLevel.one, "{");

                    string typeName = "";
                    if (typeof(IEnumerable) == returnType.GetInterface("IEnumerable")
                        && typeof(string) != returnType)
                    {
                        if (typeof(IList) == returnType.GetInterface("IList")
                            || typeof(Array) == returnType.BaseType)
                        {
                            //接口方法返回值为 List<T> 集合 或 复杂对象数组  
                            Type[] types = returnType.GetGenericArguments();                            
                            if (0 < types.Length)
                            {
                                typeName = types[0].TypeToString();
                            }
                            else
                            {
                                typeName = returnType.TypeToString();
                            }

                            method.append(ref code, LeftSpaceLevel.two, "if (vObj.GetType() == typeof(System.Data.DataTable))");
                            method.append(ref code, LeftSpaceLevel.two, "{");

                            method.append(ref code, LeftSpaceLevel.three, "System.Data.DataTable dt = vObj as System.Data.DataTable;");

                            //method.append(ref code, LeftSpaceLevel.three, "");                           
                            if (returnType.BaseType == typeof(Array))
                            {
                                method.append(ref code, LeftSpaceLevel.three, "int rowIndex = 0;");
                                method.append(ref code, LeftSpaceLevel.three, "int rowNum = dt.Rows.Count;");
                                method.append(ref code, LeftSpaceLevel.three, "{0}[] dtArr = new {0}[rowNum];", typeName);
                            }
                            else
                            {
                                method.append(ref code, LeftSpaceLevel.three, "List<{0}> dtList = new List<{0}>();", typeName);
                            }

                            method.append(ref code, LeftSpaceLevel.three, "{0} entity = null;", typeName);
                            code = declearVar(code, LeftSpaceLevel.three, typeName);

                            GetColumnsForDataTable(method, LeftSpaceLevel.three, ref code);

                            method.append(ref code, LeftSpaceLevel.three, "");
                            method.append(ref code, LeftSpaceLevel.three, "foreach (System.Data.DataRow dr in dt.Rows)");
                            method.append(ref code, LeftSpaceLevel.three, "{");
                            if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                                && 0 < method.methodComponent.Interval)
                            {
                                method.append(ref code, LeftSpaceLevel.four, "System.Threading.Thread.Sleep({0});", method.methodComponent.Interval.ToString());
                            }
                            method.append(ref code, LeftSpaceLevel.four, "entity = ({0})Activator.CreateInstance(typeof({0}));", typeName);

                            DataRowToEntity(method, LeftSpaceLevel.four, ref code);

                            if (returnType.BaseType == typeof(Array))
                            {
                                method.append(ref code, LeftSpaceLevel.four, "dtArr[rowIndex] = entity;");
                                method.append(ref code, LeftSpaceLevel.four, "rowIndex++;");
                            }
                            else
                            {
                                method.append(ref code, LeftSpaceLevel.four, "dtList.Add(entity);");
                            }

                            method.append(ref code, LeftSpaceLevel.three, "}"); //foreach (System.Data.DataRow dr in dt.Rows)

                            method.append(ref code, LeftSpaceLevel.three, "");

                            if (returnType.BaseType == typeof(Array))
                            {
                                method.append(ref code, LeftSpaceLevel.three, "{0} = dtArr;", resultVarName1);
                            }
                            else
                            {
                                method.append(ref code, LeftSpaceLevel.three, "{0} = dtList;", resultVarName1);
                            }

                            method.append(ref code, LeftSpaceLevel.two, "}"); //if (vObj.GetType() == typeof(System.Data.DataTable))

                            //method.append(ref code, LeftSpaceLevel.two, "");

                        }
                        else
                        {
                            //Dictionary 不做处理
                        }
                    }
                    else
                    {
                        typeName = returnType.TypeToString();
                        //接口方法返回值为 单体复杂对象
                        method.append(ref code, LeftSpaceLevel.two, "if (vObj.GetType() == typeof(System.Data.DataTable))");
                        method.append(ref code, LeftSpaceLevel.two, "{");
                        method.append(ref code, LeftSpaceLevel.three, "System.Data.DataTable dt = vObj as System.Data.DataTable;");

                        method.append(ref code, LeftSpaceLevel.three, "if(0 < dt.Rows.Count)");
                        method.append(ref code, LeftSpaceLevel.three, "{");

                        GetColumnsForDataTable(method, LeftSpaceLevel.four, ref code);

                        method.append(ref code, LeftSpaceLevel.four, "System.Data.DataRow dr = dt.Rows[0];");
                        method.append(ref code, LeftSpaceLevel.four, "{0} entity = ({0})Activator.CreateInstance(typeof({0}));", typeName);
                        code = declearVar(code, LeftSpaceLevel.four, typeName);

                        DataRowToEntity(method, LeftSpaceLevel.four, ref code);

                        method.append(ref code, LeftSpaceLevel.four, "{0} = entity;", resultVarName1);

                        method.append(ref code, LeftSpaceLevel.three, "}"); //if(0 < dt.Rows.Count)

                        method.append(ref code, LeftSpaceLevel.two, "}");
                        method.append(ref code, LeftSpaceLevel.two, "else");
                        method.append(ref code, LeftSpaceLevel.two, "{");
                        method.append(ref code, LeftSpaceLevel.three, "{0} = ({1})vObj;", resultVarName1, typeName);
                        method.append(ref code, LeftSpaceLevel.two, "}"); //if (vObj.GetType() == typeof(System.Data.DataTable))
                    }
                    //method.append(ref code, LeftSpaceLevel.two, "");

                    method.append(ref code, LeftSpaceLevel.one, "}"); //if(null != vObj)
                }
            }
        }

        void GetColumnsForDataTable(MethodInformation method, LeftSpaceLevel leftSpaceLevel, ref string code)
        {
            method.append(ref code, leftSpaceLevel, "");
            method.append(ref code, leftSpaceLevel, "EList<CKeyValue> fields = new EList<CKeyValue>();");
            method.append(ref code, leftSpaceLevel, "CKeyValue ckv = null;");
            method.append(ref code, leftSpaceLevel, "");
            method.append(ref code, leftSpaceLevel, "foreach (System.Data.DataColumn dc in dt.Columns)");
            method.append(ref code, leftSpaceLevel, "{");
            method.append(ref code, leftSpaceLevel + 1, "fields.Add(new CKeyValue(){Key = dc.ColumnName.ToLower(), Value = dc.ColumnName});");
            //method.append(ref code, leftSpaceLevel + 1, "");
            method.append(ref code, leftSpaceLevel, "}"); //foreach (System.Data.DataColumn dc in dt.Columns)
        }

        void DataRowToEntity(MethodInformation method, LeftSpaceLevel leftSpaceLevel, ref string code)
        {
            method.append(ref code, leftSpaceLevel, "foreach (System.Reflection.PropertyInfo pi in piArr)");
            method.append(ref code, leftSpaceLevel, "{");
            if ((method.methodComponent.IsAsync || method.methodComponent.EnabledBuffer)
                && 0 < method.methodComponent.Interval)
            {
                method.append(ref code, leftSpaceLevel + 1, "System.Threading.Thread.Sleep({0});", method.methodComponent.Interval.ToString());
            }
            method.append(ref code, leftSpaceLevel + 1, "attrName = pi.Name;");
            method.append(ref code, leftSpaceLevel + 1, "fieldMapping = System.DJ.ImplementFactory.Commons.Attrs.FieldMapping.GetFieldMapping(pi);");
            method.append(ref code, leftSpaceLevel + 1, "fieldName = \"\";");
            method.append(ref code, leftSpaceLevel + 1, "ckv = null;");
            method.append(ref code, leftSpaceLevel + 1, "if(!string.IsNullOrEmpty(fieldMapping))");
            method.append(ref code, leftSpaceLevel + 1, "{");
            method.append(ref code, leftSpaceLevel + 2, "ckv = fields[fieldMapping.ToLower()];");
            method.append(ref code, leftSpaceLevel + 1, "}");

            method.append(ref code, leftSpaceLevel + 1, "");
            method.append(ref code, leftSpaceLevel + 1, "if(null == ckv)");
            method.append(ref code, leftSpaceLevel + 1, "{");
            method.append(ref code, leftSpaceLevel + 2, "ckv = fields[attrName.ToLower()];");
            method.append(ref code, leftSpaceLevel + 1, "}");

            method.append(ref code, leftSpaceLevel + 1, "");
            method.append(ref code, leftSpaceLevel + 1, "if(null != ckv)");
            method.append(ref code, leftSpaceLevel + 1, "{");
            method.append(ref code, leftSpaceLevel + 2, "fieldName = ckv.Value.ToString();");
            method.append(ref code, leftSpaceLevel + 1, "}");

            method.append(ref code, leftSpaceLevel + 1, "");
            method.append(ref code, leftSpaceLevel + 1, "if (!string.IsNullOrEmpty(fieldName))");
            method.append(ref code, leftSpaceLevel + 1, "{");
            method.append(ref code, leftSpaceLevel + 2, "v = dr[fieldName];");
            method.append(ref code, leftSpaceLevel + 2, "v = DJTools.ConvertTo(v, pi.PropertyType);");
            method.append(ref code, leftSpaceLevel + 2, "if(Convert.IsDBNull(v)) continue;");
            method.append(ref code, leftSpaceLevel + 2, "try");
            method.append(ref code, leftSpaceLevel + 2, "{");
            method.append(ref code, leftSpaceLevel + 3, "entity.GetType().GetProperty(attrName).SetValue(entity, v, null);");
            method.append(ref code, leftSpaceLevel + 2, "}");
            method.append(ref code, leftSpaceLevel + 2, "catch (Exception ex)");
            method.append(ref code, leftSpaceLevel + 2, "{");
            string interfaceName = DJTools.GetClassName(method.ofInterfaceType, true);
            method.append(ref code, leftSpaceLevel + 3, "string errMsg = \"Type '\"+entity.GetType().FullName+\"' find not property name：\"+attrName+\"\\r\\n\"+ex.ToString();");
            method.append(ref code, leftSpaceLevel + 3, "{0}.ExecuteException(typeof({1}),{2},\"{3}\",{4},new Exception(errMsg));",
                method.AutoCallVarName, interfaceName, "null", method.methodComponent.InterfaceMethodName, method.ParaListVarName);
            method.append(ref code, leftSpaceLevel + 2, "}");
            //method.append(ref code, leftSpaceLevel + 2, "");
            method.append(ref code, leftSpaceLevel + 1, "}"); //if (!string.IsNullOrEmpty(fieldName))
            method.append(ref code, leftSpaceLevel, "}"); //foreach (System.Reflection.PropertyInfo pi in piArr)
        }


        class FuncPara
        {
            /// <summary>
            /// sql语句字符串的变量名
            /// </summary>
            public string sqlVarName1 { get; set; }
            /// <summary>
            /// sql语句中包含的字段名称 {FieldName}
            /// </summary>
            public string FieldName1 { get; set; }
            /// <summary>
            /// 接口方法参数名称
            /// </summary>
            public string ParaName1 { get; set; }
            /// <summary>
            /// 如果参数是Dictionary类型,PropertyName为key; 如果参数是类实体对象,PropertyName为该对象的属性名称
            /// </summary>
            public string PropertyName { get; set; }
            /// <summary>
            /// 接口方法参数对象中属性类型
            /// </summary>
            public Type PropertyType { get; set; }

            /// <summary>
            /// 值类型
            /// </summary>
            public string ValueType { get; set; }
        }

    }
}
