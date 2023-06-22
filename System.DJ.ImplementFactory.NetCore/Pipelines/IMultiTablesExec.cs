﻿using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMultiTablesExec: ISingleInstance
    {
        /// <summary>
        /// 是存在多表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool ExistMultiTables(string sql);

        void Count(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func);
        void Query(AutoCall autoCall, string sql, Type dataModelType, DataPage dataPage, List<DbParameter> parameters, ref int recordCount, ref string err, Action<object> action, Func<DbCommand, object> func);

        void Insert(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func);

        void Update(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func);

        void Delete(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func);
    }
}
