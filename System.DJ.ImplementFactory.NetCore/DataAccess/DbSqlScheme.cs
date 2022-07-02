using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlScheme : DbSqlBody, IDbSqlScheme
    {
        private AutoCall autoCall = new AutoCall();
        private string err = "";
        DbSqlBody IDbSqlScheme.dbSqlBody => this;

        string IDbSqlScheme.error => err;

        public DbSqlScheme() { }

        int IDbSqlScheme.Count()
        {
            string sql = GetCountSql();
            int num = 0;
            ImplementAdapter.DbHelper.query(autoCall, sql, false, dt =>
            {
                num = Convert.ToInt32(dt.Rows[0][0]);
            }, ref err);
            return num;
        }

        DataTable IDbSqlScheme.ToDataTable()
        {
            string sql = GetSql();
            DataTable dt = null; ;
            DbHelper.query(autoCall, sql, false, data =>
            {
                dt = data;
            }, ref err);
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

        private object DataRowToObj(DataRow dr, object ele, Dictionary<string, string> dic)
        {
            object _vObj = null;
            if (null == ele) return _vObj;
            string _field = "";
            PropertyInfo[] piArr = ele.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in piArr)
            {
                _field = pi.Name.ToLower();
                if (!dic.ContainsKey(_field)) continue;
                _vObj = dr[dic[_field]];
                if (null == _vObj) continue;
                _vObj = _vObj.ConvertTo(pi.PropertyType);
                if (null == _vObj) continue;
                pi.SetValue(ele, _vObj);
            }
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

        private IList<T> GetList<T>(DataTable dt)
        {
            IList<T> list = new List<T>();
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
            OverrideModel overrideModel = new OverrideModel();
            foreach (DataRow dr in dt.Rows)
            {
                mbool = FuncResult(dr, sfList, dic);
                if (!mbool) continue;
                ele = overrideModel.CreateDataModel(typeof(T));
                if (null == ele) ele = Activator.CreateInstance(typeof(T));
                DataRowToObj(dr, ele, dic);
                list.Add((T)ele);
            }

            return list;
        }

        IList<T> IDbSqlScheme.ToList<T>()
        {
            List<SqlFromUnit> sfList = GetSqlFromUnits();
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<T> list = GetList<T>(dt);
            return list;
        }

        T IDbSqlScheme.DefaultFrist<T>()
        {
            DataTable dt = ((IDbSqlScheme)this).ToDataTable();
            IList<T> list = GetList<T>(dt);
            if (0 < list.Count)
            {
                return list[0];
            }
            return default(T);
        }

        int IDbSqlScheme.Update()
        {
            int num = 0;
            List<SqlDataItem> list = GetUpdate();
            foreach (SqlDataItem item in list)
            {
                DbHelper.update(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            return num;
        }

        int IDbSqlScheme.Insert()
        {
            int num = 0;
            List<SqlDataItem> list = GetInsert();
            foreach (SqlDataItem item in list)
            {
                DbHelper.insert(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            return num;
        }

        int IDbSqlScheme.Delete()
        {
            int num = 0;
            List<SqlDataItem> list = GetDelete();
            foreach (SqlDataItem item in list)
            {
                DbHelper.delete(autoCall, item.sql, (List<DbParameter>)item.parameters, false, n =>
                {
                    num += n;
                }, ref err);
            }
            return num;
        }

    }
}
