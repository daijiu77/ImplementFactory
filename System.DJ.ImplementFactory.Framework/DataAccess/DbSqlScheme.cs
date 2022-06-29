﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlScheme : DbBody, IDbSqlScheme
    {
        private AutoCall autoCall = new AutoCall();
        private string err = "";
        DbBody IDbSqlScheme.dbBody => this;

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
            object vObj = null;
            object ele = null;
            string field = "";
            bool mbool = false;
            foreach (DataRow dr in dt.Rows)
            {
                foreach (SqlFromUnit item in fromUnits)
                {
                    if (null == item.funcCondition) continue;
                    ele = Activator.CreateInstance(item.modelType);
                    ele.ForeachProperty((pi, t, fn, fv) =>
                    {
                        field = fn.ToLower();
                        if (!dic.ContainsKey(field)) return;
                        vObj = dr[dic[field]];
                        if (null == vObj) return;
                        vObj = vObj.ConvertTo(pi.PropertyType);
                        if (null == vObj) return;
                        pi.SetValue(ele, vObj);
                    });
                    mbool = item.funcCondition((AbsDataModel)ele);
                    if (!mbool) break;
                }
                if (!mbool) continue;
                ele = Activator.CreateInstance(typeof(T));
                ele.ForeachProperty((pi, t, fn, fv) =>
                {
                    field = fn.ToLower();
                    if (!dic.ContainsKey(field)) return;
                    vObj = dr[dic[field]];
                    if (null == vObj) return;
                    vObj = vObj.ConvertTo(pi.PropertyType);
                    if (null == vObj) return;
                    pi.SetValue(ele, vObj);
                });
                list.Add((T)ele);
            }

            return list;
        }

        IList<T> IDbSqlScheme.ToList<T>()
        {
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
