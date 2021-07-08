using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.Commons
{
    class BasicExecForSQL : IDisposable
    {
        DbConnection conn = null;

        private BasicExecForSQL() { }

        public static BasicExecForSQL Instance
        {
            get
            {
                return new BasicExecForSQL();
            }
        }

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
                AutoCall.Instance.e(err, ErrorLevels.severe);
                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_CreatedFail(new Exception(err));
                }
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

        public IDbConnectionState dbConnectionState { get; set; }

        public bool disposableAndClose { get; set; }

        public string dbConnectionString { get; set; }

        public IDataServerProvider dataServerProvider { get; set; }

        object _execObj = new object();
        public void Exec(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            lock (_execObj)
            {
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
                        if (null != autoCall)
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
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (null == conn) return;
            conn.Close();
            conn.Dispose();
        }

        ~BasicExecForSQL()
        {
            ((IDisposable)this).Dispose();
        }
    }
}
