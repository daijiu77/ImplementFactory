using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.Commons
{
    public enum db_dialect
    {
        none,
        sqlserver,
        oracle,
        mysql,
        access,
        sqlite,
        odbc,
        oledb,
        firebird, //firebird
        postgresql, //postgresql
        db2, //db2
        informix,
        sqlserverce //sqlserverce
    }

    class DbAdapter : IDisposable
    {
        private DbConnection conn = null;
        private static DbAdapter dbAdapter = null;
        public static db_dialect dbDialect = db_dialect.sqlserverce;

        private DbAdapter() { }

        public static DbAdapter Instance
        {
            get
            {
                dbAdapter = new DbAdapter();
                return dbAdapter;
            }
        }

        public static bool SetConfig(string dialectName)
        {
            bool mbool = false;
            if (string.IsNullOrEmpty(dialectName)) return mbool;

            object dbType = null;
            Enum.TryParse(typeof(db_dialect), dialectName, true, out dbType);
            if (null != dbType)
            {
                dbDialect = (db_dialect)dbType;
                mbool = true;
            }
            return mbool;
        }

        public static bool IsPrintSQLToTrace { get; set; }
        public static bool IsPrintSqlToLog { get; set; }

        bool DbConnct(ref string err)
        {
            bool mbool = true;
            if (null != conn)
            {
                if (ConnectionState.Open == conn.State) return mbool;
                conn.Close();
                conn.Dispose();
            }

            mbool = false;
            if (null == dataServerProvider)
            {
                AutoCall.Instance.e("未提供 IDataServerProvider 接口实例", ErrorLevels.severe);
                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_CreatedFail(new Exception("未提供 IDataServerProvider 接口实例"));
                }
                return mbool;
            }

            conn = dataServerProvider.CreateDbConnection(dbConnectionString);
            if (null == conn) return mbool;

            if (null != dbConnectionState)
            {
                conn.StateChange += Conn_StateChange;
                conn.Disposed += Conn_Disposed;
            }

            if (ConnectionState.Open == conn.State)
            {
                mbool = true;
                return mbool;
            }

            try
            {
                conn.Open();
                mbool = true;
                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_Created(conn);
                }
            }
            catch (Exception ex)
            {
                err = ex.ToString();
                DJTools.append(ref err, "");
                DJTools.append(ref err, "ConnectionString: {0}", dbConnectionString);
                mbool = false;
                if (false == IgnoreError)
                {
                    AutoCall.Instance.e(err, ErrorLevels.severe);
                }
                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_CreatedFail(new Exception(err));
                }
                IgnoreError = false;
                //throw;
            }
            return mbool;
        }

        private void Conn_StateChange(object sender, StateChangeEventArgs e)
        {
            if (null == dbConnectionState) return;
            dbConnectionState.DbConnection_StateChange(conn, e);
        }

        private void Conn_Disposed(object sender, EventArgs e)
        {
            if (null == dbConnectionState) return;
            dbConnectionState.DbConnection_Disposed(conn, e);
        }

        private static object _DbAdapter = new object();
        public static void printSql(AutoCall autoCall, string sql)
        {
            lock (_DbAdapter)
            {
                if (IsPrintSQLToTrace)
                {
                    Trace.WriteLine(sql);
                    Trace.WriteLine("++++++++++++++++++++ SQL Expression +++++++++++++++++++++++++++");
                }

                if (IsPrintSqlToLog)
                {
                    if (null == autoCall) autoCall = new AutoCall();
                    autoCall.e(sql, ErrorLevels.debug);
                }
            }
        }

        public IDbConnectionState dbConnectionState { get; set; }

        public bool disposableAndClose { get; set; }

        public string dbConnectionString { get; set; }

        public IDataServerProvider dataServerProvider { get; set; }

        object _execObj = new object();
        public void ExecSql(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            lock (_execObj)
            {
                printSql(autoCall, sql);
                if (DbConnct(ref err))
                {
                    DbCommand cmd = dataServerProvider.CreateDbCommand(sql, conn);
                    if (null != parameters)
                    {
                        foreach (var item in parameters)
                        {
                            cmd.Parameters.Add(item);
                        }
                    }

                    object vObj = null;
                    try
                    {
                        vObj = func(cmd);
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        conn.Dispose();
                        conn = null;

                        err = ex.ToString();
                        err += "\r\n\r\n" + sql;
                        if (null != autoCall && false == IgnoreError)
                        {
                            autoCall.ExecuteException(this.GetType(), this, "Exec", null, new Exception(err));
                        }
                        //throw;
                    }
                    finally
                    {
                        cmd.Dispose();
                        if (null != conn)
                        {
                            if (disposableAndClose) conn.Close();
                            conn.Dispose();
                        }

                        action(vObj);
                        IgnoreError = false;
                    }
                }
            }
        }

        public bool IgnoreError { get; set; }
        void IDisposable.Dispose()
        {
            if (null == conn) return;
            conn.Close();
            conn.Dispose();
        }

        ~DbAdapter()
        {
            ((IDisposable)this).Dispose();
        }
    }
}
