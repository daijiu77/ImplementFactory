using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.DJ.ImplementFactory.NetCore.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Commons
{
    public class MultiTablesExec : IMultiTablesExec
    {
        private static Dictionary<string, object> tbDic = new Dictionary<string, object>();

        public MultiTablesExec() { }

        public MultiTablesExec(DbInfo dbInfo)
        {
            if (0 < tbDic.Count) return;
        }

        private List<string> GetTablesWithSqlServer(string dbName)
        {
            List<string> list = new List<string>();
            string sql = "select TABLE_NAME from {0}.dbo.sysobjects where type='U'";
            sql = string.Format(sql, dbName);
            return list;
        }

        private List<string> GetTablesWithMySql(string dbName)
        {
            List<string> list = new List<string>();
            string sql = "select TABLE_NAME from information_schema.tables table_schema='{0}';";
            sql = string.Format(sql, dbName);
            return list;
        }

        private List<string> GetTablesWithOracle(string dbName)
        {
            List<string> list = new List<string>();
            string sql = "SELECT TABLE_NAME FROM all_tables WHERE OWNER='{0}' ORDER BY TABLE_NAME";
            sql = string.Format(sql, dbName);
            /*
             获取所有的表字段，类型，长度和注释

select distinct a.TABLE_NAME,a.COLumn_name,a.data_type,a.data_length,c.comments from all_tab_columns a
inner join all_tables b on a.TABLE_NAME=b.TABLE_NAME and a.OWNER=b.OWNER

inner join all_col_comments c on a.TABLE_NAME=c.TABLE_NAME and a.COLumn_name=c.COLumn_name
where b.OWNER=‘数据库名称‘ order by a.TABLE_NAME;
             */
            return list;
        }

        object ISingleInstance.Instance { get; set; }

        IDbHelper IMultiTablesExec.dbHelper { get; set; }

        int IMultiTablesExec.Delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }

        bool IMultiTablesExec.ExistMultiTables(string sql)
        {
            throw new NotImplementedException();
        }

        int IMultiTablesExec.Insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }

        DataTable IMultiTablesExec.Query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }

        int IMultiTablesExec.Update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }
    }
}
