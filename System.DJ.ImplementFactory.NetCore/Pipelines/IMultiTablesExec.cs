using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Pipelines
{
    public interface IMultiTablesExec: ISingleInstance
    {
        IDbHelper dbHelper { get; set; }
        /// <summary>
        /// 是存在多表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool ExistMultiTables(string sql);

        DataTable Query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);

        int Insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);

        int Update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);

        int Delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);
    }
}
