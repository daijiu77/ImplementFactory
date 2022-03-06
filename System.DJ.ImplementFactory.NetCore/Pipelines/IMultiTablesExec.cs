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
        /// <summary>
        /// 是存在多表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool ExistMultiTables(string sql);

        void Query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err);

        void Insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);

        void Update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);

        void Delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err);
    }
}
