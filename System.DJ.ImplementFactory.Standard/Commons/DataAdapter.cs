using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
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

    public class DataAdapter: IDisposable
    {
        static DbConnection dbConnection = null;
        static string constr = "";
        public static db_dialect dbDialect = db_dialect.sqlserverce;

        public static void SetConfig(string connectionString, string dialectName)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(dialectName)) return;
            
            DataAdapter.constr = connectionString;
            string[] dbNames = typeof(db_dialect).GetEnumNames();
            string dn = dialectName.ToLower();
            Array arr = typeof(db_dialect).GetEnumValues();
            int n = 0;
            foreach (var item in dbNames)
            {
                if (item.ToLower().Equals(dn))
                {
                    dbDialect = (db_dialect)arr.GetValue(n);
                    break;
                }
                n++;
            }
        }

        class ProviderFactory
        {
            private static Dictionary<db_dialect, string> providerInvariantNames = new Dictionary<db_dialect, string>();
            private static Dictionary<db_dialect, DbProviderFactory> providerFactoies = new Dictionary<db_dialect, DbProviderFactory>(20);

            static ProviderFactory()
            {
                if (providerInvariantNames.Count == 0)
                {
                    //加载已知的数据库访问类的程序集   
                    providerInvariantNames.Add(db_dialect.sqlserver, "System.Data.SqlClient");
                    providerInvariantNames.Add(db_dialect.oledb, "System.Data.OleDb");
                    providerInvariantNames.Add(db_dialect.access, "System.Data.OleDb");
                    providerInvariantNames.Add(db_dialect.odbc, "System.Data.ODBC");
                    providerInvariantNames.Add(db_dialect.oracle, "System.Data.OracleClient"); // ,Oracle.Data.Client
                    providerInvariantNames.Add(db_dialect.mysql, "MySql.Data.MySqlClient");
                    providerInvariantNames.Add(db_dialect.sqlite, "System.Data.SQLite");
                    providerInvariantNames.Add(db_dialect.firebird, "FirebirdSql.Data.Firebird");
                    providerInvariantNames.Add(db_dialect.postgresql, "Npgsql");
                    providerInvariantNames.Add(db_dialect.db2, "IBM.Data.DB2.iSeries");
                    providerInvariantNames.Add(db_dialect.informix, "IBM.Data.Informix");
                    providerInvariantNames.Add(db_dialect.sqlserverce, "System.Data.SqlServerCe");
                }
            }

            /// <summary>   
            /// 获取指定数据库类型对应的程序集名称   
            /// </summary>   
            /// <param name="providerType">数据库类型枚举</param>   
            /// <returns></returns>   
            public static string GetProviderInvariantName(db_dialect providerType)
            {
                return providerInvariantNames[providerType];
            }

            /// <summary>   
            /// 获取指定类型的数据库对应的DbProviderFactory   
            /// </summary>   
            /// <param name="providerType">数据库类型枚举</param>   
            /// <returns></returns>   
            public static DbProviderFactory GetDbProviderFactory(db_dialect providerType)
            {
                //如果还没有加载，则加载该DbProviderFactory   
                if (!providerFactoies.ContainsKey(providerType))
                {
                    providerFactoies.Add(providerType, ImportDbProviderFactory(providerType));
                }
                return providerFactoies[providerType];
            }

            /// <summary>   
            /// 加载指定数据库类型的DbProviderFactory   
            /// </summary>   
            /// <param name="providerType">数据库类型枚举</param>   
            /// <returns></returns>   
            private static DbProviderFactory ImportDbProviderFactory(db_dialect providerType)
            {
                string providerName = providerInvariantNames[providerType];
                DbProviderFactory factory = null;
                try
                {
                    //从全局程序集中查找   
                    factory = DbProviderFactories.GetFactory(providerName);
                }
                catch (ArgumentException e)
                {
                    factory = null;
                }
                return factory;
            }
        }

        bool CreateConnection()
        {
            bool isSuccess = true;
            if (null != dbConnection)
            {
                if (dbConnection.State == Data.ConnectionState.Open) return isSuccess;
                dbConnection.Close();
                dbConnection.Dispose();
            }

            if (string.IsNullOrEmpty(constr))
            {
                err = "数据库连接字符串为空";
                isSuccess = false;
                return isSuccess;
            }

            if (db_dialect.none == dbDialect)
            {
                err = "数据库类型配置不正确";
                isSuccess = false;
                return isSuccess;
            }

            try
            {
                DbProviderFactory dbpf = ProviderFactory.GetDbProviderFactory(dbDialect);
                dbConnection = dbpf.CreateConnection();
                dbConnection.StateChange += DbConnection_StateChange;
                dbConnection.Disposed += DbConnection_Disposed;
                dbConnection.ConnectionString = constr;
                dbConnection.Open();
                if (null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_Created(dbConnection);
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                string[] arr = typeof(db_dialect).GetEnumNames();
                err = constr + "\r\n";
                err += arr[(int)dbDialect] + "\r\n";
                err += ex.ToString();
                if(null != dbConnectionState)
                {
                    dbConnectionState.DbConnection_CreatedFail(new Exception(err));
                }
                //throw;
            }
            return isSuccess;
        }

        private void DbConnection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (null == dbConnectionState) return;
            dbConnectionState.DbConnection_StateChange((DbConnection)sender, e);
            //throw new NotImplementedException();
        }

        private void DbConnection_Disposed(object sender, EventArgs e)
        {
            if (null == dbConnectionState) return;
            dbConnectionState.DbConnection_Disposed((DbConnection)sender, e);
            //throw new NotImplementedException();
        }

        void AddCommandPara(DbCommand cmd, List<DbParameter> dbParameters)
        {
            if (null == cmd) return;

            if (null != dbParameters)
            {
                DbParameter DbPara = null;
                foreach (var item in dbParameters)
                {
                    DbPara = cmd.CreateParameter();
                    DbPara.ParameterName = item.ParameterName;
                    DbPara.Value = item.Value;
                    cmd.Parameters.Add(DbPara);
                }
            }
        }

        public DataTable dataTableQuery(string sql, List<DbParameter> dbParameters)
        {
            DataTable dt = new DataTable();
            if (CreateConnection())
            {
                DbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                AddCommandPara(cmd, dbParameters);

                try
                {
                    DbDataAdapter da = ProviderFactory.GetDbProviderFactory(dbDialect).CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    da.Dispose();
                    da = null;
                }
                catch (Exception ex)
                {
                    err = ex.ToString();
                    //throw;
                }
                finally
                {
                    if (null != cmd)
                    {
                        cmd.Dispose();
                    }
                }
            }
            return dt;
        }

        public DataTable dataTableQuery(string sql)
        {
            return dataTableQuery(sql, null);
        }

        public int query(string sql, List<DbParameter> dbParameters)
        {
            int num = 0;
            if (CreateConnection())
            {
                DbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                AddCommandPara(cmd, dbParameters);

                try
                {
                    num = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    err = ex.ToString();
                    //throw;
                }
                finally
                {
                    if (null != cmd)
                    {
                        cmd.Dispose();
                    }
                }
            }
            return num;
        }

        public int query(string sql)
        {
            return query(sql, null);
        }

        void IDisposable.Dispose()
        {
            if (null == dbConnection) return;
            dbConnection.Close();
            dbConnection.Dispose();
        }

        public IDbConnectionState dbConnectionState { get; set; }

        public string err { get; set; }
    }
}
