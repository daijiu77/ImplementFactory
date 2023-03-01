using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.Exts;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlScheme : DbSqlBody, IDbSqlScheme
    {
        private OverrideModel overrideModel = new OverrideModel();
        private AutoCall autoCall = new AutoCall();
        private string err = "";
        DbSqlBody IDbSqlScheme.dbSqlBody => this;

        string IDbSqlScheme.error => err;

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

        DataTable IDbSqlScheme.ToDataTable()
        {
            string sql = GetSql();
            DataTable dt = null;
            IDbHelper dbHelper = DbHelper;
            dbHelper.query(autoCall, sql, false, data =>
            {
                dt = data;
            }, ref err);
            ImplementAdapter.Destroy(dbHelper);
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
                if (typeof(ICollection).IsAssignableFrom(pi.PropertyType))
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
                    else if (typeof(IList).IsAssignableFrom(pi.PropertyType))
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
                try
                {
                    pi.SetValue(ele, _vObj);
                }
                catch (Exception ex)
                {
                    try
                    {
                        ele.SetPropertyValue(pi.Name, _vObj);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
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

        private IList<object> GetList(DataTable dt, Type modelType)
        {
            IList<object> list = new List<object>();
            if (null == dt) return list;
            if (0 == dt.Rows.Count) return list;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DataColumn item in dt.Columns)
            {
                dic.Add(item.ColumnName.ToLower(), item.ColumnName);
            }

            List<SqlFromUnit> sfList = GetSqlFromUnits();
            object ele = null;
            bool mbool = false;
            foreach (DataRow dr in dt.Rows)
            {
                mbool = FuncResult(dr, sfList, dic);
                if (!mbool) continue;
                if (isUseConstraintLoad)
                {
                    ele = overrideModel.CreateDataModel(modelType);
                }
                else
                {
                    ele = Activator.CreateInstance(modelType);
                }
                if (!string.IsNullOrEmpty(overrideModel.error)) err = overrideModel.error;
                if (null == ele) ele = Activator.CreateInstance(modelType);
                ((AbsDataModel)ele).parentModel = this.parentModel;
                DataRowToObj(dr, ele, dic);
                list.Add(ele);
            }

            return list;
        }

        IList<T> IDbSqlScheme.ToList<T>()
        {
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<T> list = new List<T>();
            IList<object> results= GetList(dt, typeof(T));
            if (null != results)
            {
                foreach (var item in results)
                {
                    list.Add((T)item);
                }
            }
            return list;
        }

        T IDbSqlScheme.DefaultFirst<T>()
        {
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<object> list = GetList(dt, typeof(T));
            if (0 < list.Count)
            {
                return (T)list[0];
            }
            return default(T);
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

        int IDbSqlScheme.Delete()
        {
            int num = 0;
            List<SqlDataItem> list = GetDelete();
            IDbHelper dbHelper = DbHelper;
            foreach (SqlDataItem item in list)
            {
                dbHelper.delete(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            ImplementAdapter.Destroy(dbHelper);
            return num;
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

        IList<object> IDbSqlScheme.ToList(Type modelType)
        {
            List<SqlFromUnit> sfList = GetSqlFromUnits();
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<object> results = GetList(dt, modelType);
            return results;
        }

        object IDbSqlScheme.DefaultFirst(Type modelType)
        {
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<object> list = GetList(dt, modelType);
            if (0 < list.Count)
            {
                return list[0];
            }
            return null;
        }
    }
}
