﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.NetCore.Commons;
using System.DJ.ImplementFactory.NetCore.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    class BasicExecForSQL : IDisposable
    {
        private DbAdapter dbAdapter = DbAdapter.Instance;
        private IMultiTablesExec multiTablesExec = new MultiTablesExec();

        private BasicExecForSQL() { }

        public static BasicExecForSQL Instance
        {
            get
            {
                return new BasicExecForSQL();
            }
        }

        public IDbConnectionState dbConnectionState
        {
            get { return dbAdapter.dbConnectionState; }
            set { dbAdapter.dbConnectionState = value; }
        }

        public bool disposableAndClose
        {
            get { return dbAdapter.disposableAndClose; }
            set { dbAdapter.disposableAndClose = value; }
        }

        public string dbConnectionString
        {
            get { return dbAdapter.dbConnectionString; }
            set { dbAdapter.dbConnectionString = value; }
        }

        public IDataServerProvider dataServerProvider
        {
            get { return dbAdapter.dataServerProvider; }
            set { dbAdapter.dataServerProvider = value; }
        }

        public void ExecSql(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            dbAdapter.ExecSql(autoCall, sql, parameters, ref err, action, func);
        }

        public void Exec(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();
            string methodName = methodBase.Name;
            bool isQuery = -1 != methodName.IndexOf("Pipelines.IDbHelper.query");
            Regex rg = new Regex(@"^((select\s)|(insert\s)|(update\s)|(delete\s))", RegexOptions.IgnoreCase);
            if (rg.IsMatch(sql))
            {
                string sign = rg.Match(sql).Groups[0].Value.Trim().ToLower();
                if (isQuery) sign = "select";
                switch (sign)
                {
                    case "select":
                        multiTablesExec.Query(autoCall, sql, parameters, ref err, action, func);
                        break;
                    case "insert":
                        multiTablesExec.Insert(autoCall, sql, parameters, ref err, action, func);
                        break;
                    case "update":
                        multiTablesExec.Update(autoCall, sql, parameters, ref err, action, func);
                        break;
                    case "delete":
                        multiTablesExec.Delete(autoCall, sql, parameters, ref err, action, func);
                        break;
                }
            }
            else
            {
                ExecSql(autoCall, sql, parameters, ref err, action, func);
            }
        }

        void IDisposable.Dispose()
        {
            if (null != dbAdapter) ((IDisposable)dbAdapter).Dispose();
            if (null != multiTablesExec) ((IDisposable)multiTablesExec).Dispose();
        }

        ~BasicExecForSQL()
        {
            ((IDisposable)this).Dispose();
        }
    }
}
