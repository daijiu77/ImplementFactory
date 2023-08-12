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

    public class DbAdapter : IDisposable
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

            try
            {
                conn = dataServerProvider.CreateDbConnection(dbConnectionString);
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
                else
                {
                    err = ex.Message;
                }

                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_CreatedFail(new Exception(err));
                }
                IgnoreError = false;
                //throw;
            }

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
                else
                {
                    err = ex.Message;
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
        public static void printSql(AutoCall autoCall, string sql, string printTag)
        {
            lock (_DbAdapter)
            {
                if (IsPrintSQLToTrace)
                {
                    Trace.WriteLine(sql);
                    Trace.WriteLine(printTag);
                }

                if (IsPrintSqlToLog)
                {
                    if (null == autoCall) autoCall = new AutoCall();
                    autoCall.e(sql, ErrorLevels.debug);
                }
            }
        }

        public static void printSql(AutoCall autoCall, string sql)
        {
            lock (_DbAdapter)
            {
                string printTag = "++++++++++++++++++++ SQL Expression +++++++++++++++++++++++++++";
                printSql(autoCall, sql, printTag);
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
                    DbCommand cmd = null;
                    try
                    {
                        cmd = dataServerProvider.CreateDbCommand(sql, conn);
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
                    if (null == cmd) return;

                    string paraJsonData = "";
                    if (null != parameters)
                    {
                        string paraVal = "";
                        foreach (var item in parameters)
                        {
                            cmd.Parameters.Add(item);
                            paraVal = GetParaJsonValue(item);
                            paraJsonData += ", \"{0}\": {1}".ExtFormat(item.ParameterName, paraVal);
                        }

                        if (!string.IsNullOrEmpty(paraJsonData))
                        {
                            paraJsonData = paraJsonData.Substring(1).Trim();
                            paraJsonData = "{" + paraJsonData + "}";
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
                        if (!string.IsNullOrEmpty(paraJsonData))
                        {
                            err += "\r\n\r\n" + paraJsonData;
                        }
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
                            //if (disposableAndClose) conn.Close();
                            conn.Close();
                            conn.Dispose();
                        }

                        action(vObj);
                        IgnoreError = false;
                    }
                }
            }
        }

        public void ExecSql<T>(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<T> action)
        {
            if (null == dataServerProvider) throw new Exception("DataServerProvider is not null.");
            AutoCall autoCall1 = autoCall;
            if (null == autoCall) autoCall1 = new AutoCall();
            string msg = "";
            ExecSql(autoCall1, sql, parameters, ref err, dtVal =>
            {
                try
                {
                    action((T)dtVal);
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                    //throw;
                }
            }, cmd =>
            {
                object vObj = default(T);
                if (typeof(T) == typeof(DataTable))
                {
                    try
                    {
                        DataTable dt = null;
                        DataSet ds = new DataSet();
                        DataAdapter adapter = dataServerProvider.CreateDataAdapter(cmd);
                        adapter.Fill(ds);
                        if (null != ds)
                        {
                            if (0 < ds.Tables.Count) dt = ds.Tables[0];
                        }
                        vObj = dt;
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                        //throw;
                    }
                }
                else
                {
                    int num = 0;
                    try
                    {
                        num = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                        //throw;
                    }

                    if (typeof(T) == typeof(bool))
                    {
                        vObj = 0 < num;
                    }
                    else
                    {
                        vObj = num;
                    }
                }
                return vObj;
            });
            if (!string.IsNullOrEmpty(msg)) err = msg;
        }

        public bool DbConnectionState(ref string err)
        {
            return DbConnct(ref err);
        }

        public bool IgnoreError { get; set; }
        void IDisposable.Dispose()
        {
            if (null == conn) return;
            conn.Close();
            conn.Dispose();
        }

        private string GetParaJsonValue(DbParameter item)
        {
            string paraVal = "";
            if (DbType.Binary == item.DbType)
            {
                if (null != item.Value)
                {
                    paraVal = "\"byte[{0}]\"".ExtFormat(((byte[])item.Value).Length);
                }
                else
                {
                    paraVal = "\"The value is null that its type is byte[]\"";
                }
            }
            else if (null == item.Value || DBNull.Value == item.Value)
            {
                paraVal = "null";
            }
            else
            {
                Type pt = item.Value.GetType();
                if ((typeof(Guid) == pt) || (typeof(Guid?) == pt) || (typeof(string) == pt))
                {
                    paraVal = "\"{0}\"".ExtFormat(item.Value.ToString().Replace("\"", @"\"""));
                }
                else if ((typeof(DateTime) == pt) || (typeof(DateTime?) == pt))
                {
                    paraVal = "\"{0}\"".ExtFormat(((DateTime)item.Value).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    paraVal = "{0}".ExtFormat(item.Value);
                }
            }
            return paraVal;
        }

        ~DbAdapter()
        {
            ((IDisposable)this).Dispose();
        }
    }
}
