using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Commons.Exts;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlScheme : DbSqlBody, IDbSqlScheme
    {
        private OverrideModel overrideModel = new OverrideModel();
        private AutoCall autoCall = new AutoCall();
        private string err = "";
        private int _recordCount = 0;
        private int _pageCount = 0;

        DbSqlBody IDbSqlScheme.dbSqlBody => this;

        string IDbSqlScheme.error => err;

        AbsDataModel IDbSqlScheme.parentModel { get; set; }

        int IDbSqlScheme.RecordCount => _recordCount;

        int IDbSqlScheme.PageCount => _pageCount;

        public DbSqlScheme() { }

        int IDbSqlScheme.Count()
        {
            string sql = GetCountSql();
            int num = 0;
            IDbHelper dbHelper = DbHelper;
            dbHelper.query(autoCall, sql, false, dt =>
            {
                num = Convert.ToInt32(dt.Rows[0][0]);
            }, ref err);
            ImplementAdapter.Destroy(dbHelper);
            return num;
        }

        private DataPage getDataPage(string sql)
        {
            DataPage dataPage = null;
            if (0 < pageSize)
            {
                dataPage = new DataPage()
                {
                    PageSize = pageSize,
                    StartQuantity = (pageNumber - 1) * pageSize,
                    PageSizeSignOfSql = PageSizeSignOfSql,
                    StartQuantitySignOfSql = StartQuantitySignOfSql
                };

                string tableName = "";
                dataPage.InitSql(ref sql, ref tableName);
                if (!string.IsNullOrEmpty(tableName))
                {
                    Regex rg = new Regex(@"(?<tbFlag>[a-z0-9_]+)\.((rowNumber)|(ROWNUM))\s*[\<\>\=\!]", RegexOptions.IgnoreCase);
                    string tb = "";
                    if (rg.IsMatch(dataPage.PageSizeSignOfSql))
                    {
                        tb = rg.Match(dataPage.PageSizeSignOfSql).Groups["tbFlag"].Value;
                        if (!tableName.Equals(tb))
                        {
                            dataPage.PageSizeSignOfSql = dataPage.PageSizeSignOfSql.Replace(tb + ".", tableName + ".");
                        }
                    }

                    if (rg.IsMatch(dataPage.StartQuantitySignOfSql))
                    {
                        tb = rg.Match(dataPage.StartQuantitySignOfSql).Groups["tbFlag"].Value;
                        if (!tableName.Equals(tb))
                        {
                            dataPage.StartQuantitySignOfSql = dataPage.StartQuantitySignOfSql.Replace(tb + ".", tableName + ".");
                        }
                    }
                }

                if (0 < top)
                {
                    dataPage.PageSize = top;
                    dataPage.StartQuantity = 0;
                }

                foreach (var item in orderbyItems)
                {
                    dataPage.OrderBy.Add(new DataPage.PageOrderBy()
                    {
                        FieldName = item.FieldName,
                        IsDesc = OrderByRule.Desc == item.Rule
                    });
                }
            }
            return dataPage;
        }

        private void Page_Count(DataPage dataPage, int recordCount)
        {
            _recordCount = recordCount;
            if (0 < _recordCount)
            {
                _pageCount = _recordCount / dataPage.PageSize;
                if (0 < (_recordCount % dataPage.PageSize)) _pageCount++;
            }
        }

        DataTable IDbSqlScheme.ToDataTable()
        {
            string sql = GetSql();
            _recordCount = 0;
            _pageCount = 0;
            string err = "";
            DataTable dt = null;
            IDbHelper dbHelper = new DbAccessHelper();
            DataPage dataPage = getDataPage(sql);
            int recordCount1 = 0;
            dbHelper.query(autoCall, sql, null, dataPage, true, null, false, data =>
            {
                if (null != data) dt = (DataTable)data;
            }, ref recordCount1, ref err);
            Page_Count(dataPage, recordCount1);
            ((IDisposable)dbHelper).Dispose();
            if (null == dt) dt = new DataTable();
            return dt;
            //throw new NotImplementedException();
        }

        private List<SqlFromUnit> GetSqlFromUnits()
        {
            List<SqlFromUnit> sfList = new List<SqlFromUnit>();
            foreach (SqlFromUnit item in fromUnits)
            {
                if (null == item.funcCondition) continue;
                sfList.Add(item);
            }
            return sfList;
        }

        private void GetConstraintData(PropertyInfo pi, DataRow dr, object ele, Dictionary<string, string> dic)
        {
            if (isUseConstraintLoad) return;
            Attribute attr = pi.GetCustomAttribute(typeof(Commons.Attrs.Constraint));
            if (null == attr) return;
            Commons.Attrs.Constraint constraint = (Commons.Attrs.Constraint)attr;
            string fn = constraint.ForeignKey.ToLower();
            if (!dic.ContainsKey(fn)) return;
            fn = dic[fn];
            object vObj = dr[fn];
            if (DBNull.Value == vObj) return;
            if (null == vObj) return;

            Func<Type, IList<object>> func = (_type) =>
            {
                if (_type.IsBaseType()) return null;
                if (!typeof(AbsDataModel).IsAssignableFrom(_type)) return null;
                bool isConstraint = !((AbsDataModel)ele).IsLegalType(_type);
                DbVisitor _db = new DbVisitor();
                IDbSqlScheme _scheme = _db.CreateSqlFrom(isConstraint, SqlFromUnit.Me.From(_type));
                _scheme.dbSqlBody.Where(ConditionItem.Me.And(constraint.RefrenceKey, ConditionRelation.Equals, vObj.ToString()));
                _scheme.parentModel = (AbsDataModel)ele;
                IList<object> _list = _scheme.ToList(_type);
                return _list;
            };

            Type type = null;
            if (typeof(ICollection).IsAssignableFrom(pi.PropertyType))
            {
                if (pi.PropertyType.IsArray)
                {
                    string s = pi.PropertyType.TypeToString(true);
                    s = s.Replace("[]", "");
                    type = DJTools.GetClassTypeByPath(s);
                    if (null == type) return;
                    IList<object> results = func(type);
                    if (null == results) return;
                    object arr = ExtCollection.createArrayByType(type, results.Count);
                    int n = 0;
                    foreach (var item in results)
                    {
                        ((AbsDataModel)item).parentModel = (AbsDataModel)ele;
                        ExtCollection.arrayAdd(arr, item, n);
                        n++;
                    }
                    try
                    {
                        pi.SetValue(ele, arr);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ele.SetPropertyValue(pi.Name, arr);
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                        //throw;
                    }
                }
                else if (typeof(IList).IsAssignableFrom(pi.PropertyType))
                {
                    type = pi.PropertyType.GetGenericArguments()[0];
                    IList<object> results = func(type);
                    if (null == results) return;
                    object list = ExtCollection.createListByType(type);
                    foreach (var item in results)
                    {
                        ((AbsDataModel)item).parentModel = (AbsDataModel)ele;
                        ExtCollection.listAdd(list, item);
                    }
                    try
                    {
                        pi.SetValue(ele, list);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ele.SetPropertyValue(pi.Name, list);
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                        //throw;
                    }
                }
            }
            else if (pi.PropertyType.IsClass)
            {
                IList<object> results = func(pi.PropertyType);
                if (null == results) return;
                if (0 == results.Count) return;
                object md = results[0];
                ((AbsDataModel)md).parentModel = (AbsDataModel)ele;
                try
                {
                    pi.SetValue(ele, md);
                }
                catch (Exception ex)
                {
                    try
                    {
                        ele.SetPropertyValue(pi.Name, md);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                    //throw;
                }
            }
        }

        private object DataRowToObj(DataRow dr, object ele, Dictionary<string, string> dic)
        {
            object _vObj = null;
            if (null == ele) return _vObj;
            string _field = "";
            ele.GetType().ForeachProperty((pi, pt, fieldName) =>
            {
                _field = sqlAnalysis.GetLegalName(pi.Name);
                _field = _field.ToLower();
                GetConstraintData(pi, dr, ele, dic);
                if (!dic.ContainsKey(_field)) return;
                _vObj = dr[dic[_field]];
                if (System.DBNull.Value == _vObj) return;
                if (null == _vObj) return;
                if (typeof(byte[]) == pi.PropertyType)
                {
                    _vObj = (byte[])_vObj;
                }
                else if (typeof(ICollection).IsAssignableFrom(pi.PropertyType))
                {
                    Type type = null;
                    object list = null;
                    object v = null;
                    string[] arr = null;
                    string s = "";
                    if (pi.PropertyType.IsArray)
                    {
                        s = pi.PropertyType.TypeToString(true);
                        s = s.Replace("[]", "");
                        type = Type.GetType(s);
                        if (null == type) return;
                        if (!type.IsBaseType()) return;
                        int n = 0;
                        arr = _vObj.ToString().Split(',');
                        list = ExtCollection.createArrayByType(type, arr.Length);
                        foreach (var item in arr)
                        {
                            s = item.Replace(EnDH, ",");
                            v = DJTools.ConvertTo(s, type);
                            ExtCollection.arrayAdd(list, v, n);
                            n++;
                        }
                        _vObj = list;
                    }
                    else if (pi.PropertyType.IsList())
                    {
                        Type[] types = pi.PropertyType.GetGenericArguments();
                        if (1 != types.Length) return;
                        type = types[0];
                        if (!type.IsBaseType()) return;
                        list = ExtCollection.createListByType(type);
                        arr = _vObj.ToString().Split(',');
                        foreach (var item in arr)
                        {
                            s = item.Replace(EnDH, ",");
                            v = DJTools.ConvertTo(s, type);
                            ExtCollection.listAdd(list, v);
                        }
                        _vObj = list;
                    }
                    else
                    {
                        return;
                    }
                }
                if (pi.PropertyType.IsBaseType()) _vObj = _vObj.ConvertTo(pi.PropertyType);
                if (null == _vObj) return;
                if (null != (ele as IEntityCopy))
                {
                    ((IEntityCopy)ele).AssignmentNo = true;
                }
                try
                {
                    pi.SetValue(ele, _vObj);
                }
                catch (Exception ex)
                {
                    ele.SetMethodValue(fieldName, _vObj, _vObj);
                    //throw;
                }
            });
            return _vObj;
        }

        private bool FuncResult(DataRow dr, List<SqlFromUnit> sfList, Dictionary<string, string> dic)
        {
            bool mbool = true;
            if (0 == sfList.Count) return mbool;
            object ele = null;
            foreach (SqlFromUnit item in sfList)
            {
                ele = Activator.CreateInstance(item.modelType);
                DataRowToObj(dr, ele, dic);
                mbool = item.funcCondition((AbsDataModel)ele);
                if (!mbool) break;
            }
            return mbool;
        }

        private void ListData(Type modelType, Action<object> action)
        {
            string sql = GetSql();
            _recordCount = 0;
            _pageCount = 0;
            IDbHelper dbHelper = new DbAccessHelper();
            DataPage dataPage = getDataPage(sql);
            int recordCount1 = 0;
            object ele = null;
            bool mbool = false;
            CommonMethods commonMethods = new CommonMethods();
            MethodInfo srcMethod = commonMethods.GetSrcTypeMethod(typeof(DbSqlScheme), typeof(IDbSqlScheme)) as MethodInfo;
            Type srcType = srcMethod.DeclaringType;
            List<SqlFromUnit> sfList = GetSqlFromUnits();
            dbHelper.query(autoCall, sql, delegate (DataRow dr, Dictionary<string, string> dic)
            {
                mbool = FuncResult(dr, sfList, dic);
                if (!mbool) return null;
                if (isUseConstraintLoad)
                {
                    ele = overrideModel.CreateDataModel(srcMethod.DeclaringType, srcMethod, modelType, this);
                }
                else
                {
                    ele = Activator.CreateInstance(modelType);
                }
                if (!string.IsNullOrEmpty(overrideModel.error)) err = overrideModel.error;
                if (null == ele) ele = Activator.CreateInstance(modelType);
                ((AbsDataModel)ele).parentModel = this.parentModel;
                DataRowToObj(dr, ele, dic);
                action(ele);
                return ele;
            }, dataPage, true, null, false, data => { }, ref recordCount1, ref err);
            Page_Count(dataPage, recordCount1);
            ((IDisposable)dbHelper).Dispose();
        }

        IList<T> IDbSqlScheme.ToList<T>()
        {
            Type modelType = typeof(T);
            IList<T> dataList = new DOList<T>();
            ListData(modelType, ele =>
            {
                dataList.Add((T)ele);
            });
            return dataList;
        }

        IList<object> IDbSqlScheme.ToList(Type modelType)
        {
            IList<object> dataList = new DOList<object>();
            ListData(modelType, ele =>
            {
                dataList.Add(ele);
            });
            return dataList;
        }

        T IDbSqlScheme.DefaultFirst<T>()
        {
            Skip(1, 1);
            initOrderby();

            IList<T> list = ((IDbSqlScheme)this).ToList<T>();
            if (null != list)
            {
                if (0 < list.Count) return list[0];
            }
            return default(T);
        }

        object IDbSqlScheme.DefaultFirst(Type modelType)
        {
            Skip(1, 1);
            initOrderby();

            IList<object> list1 = ((IDbSqlScheme)this).ToList(modelType);
            if (null != list1)
            {
                if (0 < list1.Count) return list1[0];
            }
            return null;
        }

        int IDbSqlScheme.Update()
        {
            int num = 0;
            List<SqlDataItem> list = GetUpdate();
            IDbHelper dbHelper = DbHelper;
            foreach (SqlDataItem item in list)
            {
                dbHelper.update(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
            return num;
        }

        int IDbSqlScheme.Insert()
        {
            int num = 0;
            List<SqlDataItem> list = GetInsert();
            IDbHelper dbHelper = DbHelper;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            DataRow dr = null;
            string k = "";
            object v = null;
            foreach (SqlDataItem item in list)
            {
                dbHelper.query(autoCall, item.sql, (List<DbParameter>)item.parameters, false, (dt) =>
                {
                    if (null == dt) return;
                    num = dt.Rows.Count;
                    if (0 >= num) return;
                    if (null == item.model) return;
                    if (0 == dic.Count)
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            dic.Add(dc.ColumnName.ToLower(), dc.ColumnName);
                        }
                    }
                    dr = dt.Rows[0];
                    DataRowToObj(dr, item.model, dic);
                }, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
            return num;
        }

        class ChildModelInfo
        {
            private List<ChildModelInfo> childModelInfos = new List<ChildModelInfo>();
            public Type modelType { get; set; }
            public string foreignKeyName { get; set; }
            public string foreignKeyValue { get; set; }
            public string referenceKeyName { get; set; }

            public List<ChildModelInfo> Items { get { return childModelInfos; } }

            public void SetForeign_refrenceKeys(ChildModelInfo childModelInfo)
            {
                childModelInfos.Add(childModelInfo);
            }
        }

        public Dictionary<string, PropertyInfo> GetPrimaryKey(Type modelType)
        {
            Dictionary<string, PropertyInfo> keyDic = new Dictionary<string, PropertyInfo>();
            if (null == modelType) return keyDic;
            Commons.Attrs.FieldMapping mapping = null;
            string fName = "";
            modelType.ForeachProperty((pi, pt, fn) =>
            {
                mapping = pi.GetCustomAttribute<Commons.Attrs.FieldMapping>();
                if (null == mapping) return;
                if (!mapping.IsPrimaryKey) return;
                fName = fn.ToLower();
                if (keyDic.ContainsKey(fName)) return;
                keyDic.Add(fName, pi);
            });
            return keyDic;
        }

        private int deleteRelationData(IDbSqlScheme parentScheme, Type parentModelType)
        {
            int num = 0;
            Dictionary<string, PropertyInfo> keyDic = GetPrimaryKey(parentModelType);
            if (0 == keyDic.Count) return num;
            List<ChildModelInfo> childs = new List<ChildModelInfo>();
            Commons.Attrs.Constraint constraint = null;
            bool mbool = true;
            parentModelType.ForeachProperty((pi, pt, fn) =>
            {
                constraint = pi.GetCustomAttribute<Commons.Attrs.Constraint>(true);
                if (null == constraint) return;
                if (string.IsNullOrEmpty(constraint.RefrenceKey)
                    || string.IsNullOrEmpty(constraint.ForeignKey)) return;
                if (typeof(string) == pi.PropertyType) return;

                Type mType = null;
                if (typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)) //IEnumerable
                {
                    if (pi.PropertyType.IsList())
                    {
                        Type[] types = pi.PropertyType.GetGenericArguments();
                        mType = types[0];
                    }
                    else if (pi.PropertyType.IsArray)
                    {
                        string tpName = pi.PropertyType.TypeToString(true);
                        tpName = tpName.Replace("[]", "");
                        mType = DJTools.GetTypeByFullName(tpName);
                    }
                }
                else
                {
                    mType = pi.PropertyType;
                }

                if (null == mType) return;
                if (!keyDic.ContainsKey(constraint.ForeignKey.ToLower())) return;

                if (null != constraint.Foreign_refrenceKeys)
                {
                    mbool = true;
                    foreach (var item in constraint.Foreign_refrenceKeys)
                    {
                        if (mbool)
                        {
                            mbool = false;
                            if (!keyDic.ContainsKey(item.ToLower())) break;
                        }
                        else
                        {
                            mbool = true;
                        }
                    }
                    if (false == mbool) return;
                }

                childs.Add(new ChildModelInfo()
                {
                    modelType = mType,
                    foreignKeyName = constraint.RefrenceKey,
                    referenceKeyName = constraint.ForeignKey
                });

                if (null != constraint.Foreign_refrenceKeys)
                {
                    ChildModelInfo childModelInfo = null;
                    mbool = true;
                    foreach (var item in constraint.Foreign_refrenceKeys)
                    {
                        if (mbool)
                        {
                            childModelInfo = new ChildModelInfo();
                            childModelInfo.referenceKeyName = item;
                            mbool = false;
                        }
                        else
                        {
                            childModelInfo.foreignKeyName = item;
                            childs.Add(childModelInfo);
                            mbool = true;
                        }
                    }
                }
            });

            if (0 == childs.Count) return num;

            IList<object> list = parentScheme.ToList(parentModelType);
            if (null == list) return num;
            string vObj = null;
            List<ConditionItem> conditions = new List<ConditionItem>();
            foreach (var item_1 in list)
            {
                foreach (ChildModelInfo item_2 in childs)
                {
                    vObj = item_1.GetPropertyValue<string>(item_2.referenceKeyName);
                    if (string.IsNullOrEmpty(vObj)) continue;
                    conditions.Clear();
                    item_2.foreignKeyValue = vObj;

                    if (0 < item_2.Items.Count)
                    {
                        mbool = true;
                        string vObj2 = null;
                        foreach (ChildModelInfo cm in item_2.Items)
                        {
                            vObj2 = item_1.GetPropertyValue<string>(cm.referenceKeyName);
                            if (string.IsNullOrEmpty(vObj2))
                            {
                                mbool = false;
                                break;
                            }
                            conditions.Add(ConditionItem.Me.And(cm.foreignKeyName, ConditionRelation.Equals, vObj2));
                        }
                        if (false == mbool) continue;
                    }
                    conditions.Add(ConditionItem.Me.And(item_2.foreignKeyName, ConditionRelation.Equals, item_2.foreignKeyValue));
                    DbVisitor db = new DbVisitor();
                    IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.Me.From(item_2.modelType));
                    scheme.dbSqlBody.Where(conditions.ToArray());
                    num += scheme.Delete(true);
                    ((IDisposable)db).Dispose();
                }
            }
            return num;
        }

        private int delete_data(bool deleteRelation)
        {
            int num = 0;
            List<SqlDataItem> list = GetDelete();
            IDbHelper dbHelper = DbHelper;
            foreach (SqlDataItem item in list)
            {
                if (deleteRelation)
                {
                    num += deleteRelationData(this, item.model.GetType());
                }
                dbHelper.delete(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
            return num;
        }

        int IDbSqlScheme.Delete(bool deleteRelation)
        {
            IsDeleteRelation = deleteRelation;
            return delete_data(deleteRelation);
        }

        int IDbSqlScheme.Delete()
        {
            return delete_data(false);
        }

        int IDbSqlScheme.AppendUpdate(Dictionary<string, object> keyValue)
        {
            SetAppendUpdate(keyValue);
            return ((IDbSqlScheme)this).Update();
        }

        int IDbSqlScheme.AppendInsert(Dictionary<string, object> keyValue)
        {
            SetAppendInsert(keyValue);
            return ((IDbSqlScheme)this).Insert();
        }

    }
}
