using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DataOperate;
using System.DJ.ImplementFactory.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class DbHelper : IDbHelper, IDisposable
    {
        static Dictionary<string, IExecuteSql> execSqlDic = new Dictionary<string, IExecuteSql>();

        static string connStr = "";
        static IDataServerProvider _dataServerProvider = null;
        static AbsBatch insertBatch = null;
        static AbsBatch updateBatch = null;
        static AbsBatch deleteBatch = null;

        static BasicExecForSQL basicExecForSQL = BasicExecForSQL.Instance;

        SynchronizationContext m_SyncContext = SynchronizationContext.Current;

        IDbConnectionState _dbConnectionState = null;
        IDbConnectionState IDbHelper.dbConnectionState
        {
            get { return _dbConnectionState; }
            set
            {
                _dbConnectionState = value;
                basicExecForSQL.dbConnectionState = value;
            }
        }

        string IDbHelper.connectString
        {
            get { return connStr; }
            set
            {
                connStr = value;
                basicExecForSQL.dbConnectionString = value;
            }
        }

        IDataServerProvider IDbHelper.dataServerProvider
        {
            get { return _dataServerProvider; }
            set
            {
                _dataServerProvider = value;
                Type type = value.GetType();
                basicExecForSQL.dataServerProvider = (IDataServerProvider)Activator.CreateInstance(type);
            }
        }

        int IDbHelper.optByBatchMaxNumber { get; set; }
        int IDbHelper.optByBatchWaitSecond { get; set; }
        int IDbHelper.sqlMaxLengthForBatch { get; set; }

        bool _disposableAndClose = false;
        bool IDbHelper.disposableAndClose
        {
            get { return _disposableAndClose; }
            set
            {
                _disposableAndClose = value;
                basicExecForSQL.disposableAndClose = value;
            }
        }

        bool IDbHelper.isNormalBatchInsert { get; set; } = true;

        string IDbHelper.splitTablesRule { get; set; }

        long IDbHelper.splitTablesRecordQuantity { get; set; }

        static object _bufferData = new object();
        static void bufferDatas(IDbHelper dbHelper, AutoCall autoCall, DataOptType dataOptType, string sql, List<DbParameter> parameters, Action<object> action, Func<DbCommand, object> func)
        {
            lock (_bufferData)
            {
                if (DataOptType.insert == dataOptType)
                {
                    if (null == insertBatch)
                    {
                        insertBatch = new InsertBatch();
                        ((InsertBatch)insertBatch).isNormalBatchInsert = dbHelper.isNormalBatchInsert;
                        setAsbBatchProperty(insertBatch, dbHelper);
                    }

                    insertBatch.analysis(dbHelper, autoCall, sql, parameters, (sql_1, tableName_1, para_1) =>
                    {
                        addDataToCollection(dbHelper, autoCall, action, func, sql_1, tableName_1, para_1);
                    });
                }
                else if (DataOptType.update == dataOptType)
                {
                    if (null == updateBatch)
                    {
                        updateBatch = new UpdateBatch();
                        setAsbBatchProperty(updateBatch, dbHelper);
                    }

                    updateBatch.analysis(dbHelper, autoCall, sql, parameters, (sql_1, tableName_1, para_1) =>
                    {
                        addDataToCollection(dbHelper, autoCall, action, func, sql_1, tableName_1, para_1);
                    });
                }
                else if (DataOptType.delete == dataOptType)
                {
                    if (null == deleteBatch)
                    {
                        deleteBatch = new DeleteBatch();
                        setAsbBatchProperty(deleteBatch, dbHelper);
                    }

                    deleteBatch.analysis(dbHelper, autoCall, sql, parameters, (sql_1, tableName_1, para_1) =>
                    {
                        addDataToCollection(dbHelper, autoCall, action, func, sql_1, tableName_1, para_1);
                    });
                }
            }
        }

        static void setAsbBatchProperty(AbsBatch absBatch, IDbHelper dbHelper)
        {
            absBatch.optByBatchMaxNumber = dbHelper.optByBatchMaxNumber;
            absBatch.optByBatchWaitSecond = dbHelper.optByBatchWaitSecond;
            absBatch.sqlMaxLengthForBatch = dbHelper.sqlMaxLengthForBatch;
        }

        static object _addDataToCollection = new object();
        static void addDataToCollection(IDbHelper dbHelper, AutoCall autoCall, Action<object> action, Func<DbCommand, object> func, string sql_1, string tableName_1, List<DbParameter> para_1)
        {
            lock (_addDataToCollection)
            {
                IExecuteSql executeSql = null;
                execSqlDic.TryGetValue(tableName_1, out executeSql);

                if (null == executeSql)
                {
                    executeSql = ExecuteSql.Instance;
                    executeSql.Key = tableName_1;
                    executeSql.connectString = dbHelper.connectString;
                    Type type = dbHelper.dataServerProvider.GetType();
                    executeSql.dataServerProvider = (IDataServerProvider)Activator.CreateInstance(type);
                    executeSql.disposableAndClose = dbHelper.disposableAndClose;
                    executeSql.dbConnectionState = dbHelper.dbConnectionState;
                    execSqlDic.Add(tableName_1, executeSql);
                }

                TempData td = TempData.Instance;
                td.dbHelper = dbHelper;
                td.autoCall = autoCall;
                td.sql = sql_1;
                td.parameters = para_1;
                td.resultOfOpt = action;
                td.dataOpt = func;

                executeSql.Add(td);
            }
        }

        void DataOpt(bool EnabledBuffer, AutoCall autoCall_1, string sql, List<DbParameter> parameters, Action<object> resultAction, ref string err)
        {
            bool isExec = true;
            int num = 0;
            string msg = "";
            if (EnabledBuffer)
            {
                bufferDatas(this, autoCall_1, DataOptType.delete, sql, parameters, result =>
                {
                    if (null == result) result = 0;
                    int.TryParse(result.ToString(), out num);
                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        if (null != m_SyncContext)
                        {
                            m_SyncContext.Post(PostInt, new object[] { resultAction, num });
                        }
                        else
                        {
                            resultAction(num);
                        }
                    }
                }, cmd =>
                {
                    try
                    {
                        num = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        num = 0;
                        msg = ex.ToString();
                    }

                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        if (null != m_SyncContext)
                        {
                            m_SyncContext.Post(PostInt, new object[] { resultAction, num });
                        }
                        else
                        {
                            resultAction(num);
                        }
                    }

                    if (!string.IsNullOrEmpty(msg)) throw new Exception(msg);
                    return num;
                });
            }
            else
            {
                num = 0;
                basicExecForSQL.Exec(autoCall_1, sql, parameters, ref err, result =>
                {
                    if (null == result) result = 0;
                    int.TryParse(result.ToString(), out num);
                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        resultAction(num);
                    }
                }, cmd =>
                {
                    try
                    {
                        num = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                    }

                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        resultAction(num);
                    }
                    if (!string.IsNullOrEmpty(msg)) throw new Exception(msg);
                    return num;
                });
            }
            err = msg;
        }

        int IDbHelper.delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            int num = 1;
            AutoCall autoCall_1 = autoCall as AutoCall;
            DataOpt(EnabledBuffer, autoCall_1, sql, parameters, data=> {
                num = Convert.ToInt32(data);
                resultAction(num);
            }, ref err);
            return num;
        }

        int IDbHelper.insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            int num = 1;
            AutoCall autoCall_1 = autoCall as AutoCall;
            DataOpt(EnabledBuffer, autoCall_1, sql, parameters, data => {
                num = Convert.ToInt32(data);
                resultAction(num);
            }, ref err);
            return num;
        }

        DataTable IDbHelper.query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err)
        {
            DataTable dt = new DataTable();
            string msg = "";
            AutoCall autoCall_1 = autoCall as AutoCall;
            
            Action action = () =>
            {
                bool isExec = true;
                basicExecForSQL.Exec(autoCall_1, sql, parameters, ref msg, result =>
                {
                    dt = result as DataTable;
                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        if (EnabledBuffer)
                        {
                            if (null != m_SyncContext)
                            {
                                m_SyncContext.Post(PostDataTable, new object[] { resultAction, dt });
                            }
                            else
                            {
                                resultAction(dt);
                            }
                        }
                        else
                        {
                            resultAction(dt);
                        }
                    }
                }, cmd =>
                {
                    IDataServerProvider dataServerProvider = ((IDbHelper)this).dataServerProvider;
                    Data.Common.DataAdapter da = dataServerProvider.CreateDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds);
                        if (0 < ds.Tables.Count) dt = ds.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        msg = ex.ToString();
                        //throw;
                    }
                    if (null != resultAction && isExec)
                    {
                        isExec = false;
                        if (EnabledBuffer)
                        {
                            if (null != m_SyncContext)
                            {
                                m_SyncContext.Post(PostDataTable, new object[] { resultAction, dt });
                            }
                            else
                            {
                                resultAction(dt);
                            }
                        }
                        else
                        {
                            resultAction(dt);
                        }
                    }
                    if (!string.IsNullOrEmpty(msg)) throw new Exception(msg);
                    return dt;
                });
            };

            if (EnabledBuffer)
            {
                Task task = new Task(() =>
                  {
                      Thread.Sleep(50);
                      action();
                  });
                task.Start();
            }
            else
            {
                action();
            }
            err = msg;
            return dt;
        }

        DataTable IDbHelper.query(object autoCall, string sql, bool EnabledBuffer, Action<DataTable> resultAction, ref string err)
        {
            return ((IDbHelper)this).query(autoCall, sql, null, EnabledBuffer, resultAction, ref err);
        }

        int IDbHelper.update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            int num = 1;
            AutoCall autoCall_1 = autoCall as AutoCall;
            DataOpt(EnabledBuffer, autoCall_1, sql, parameters, data => {
                num = Convert.ToInt32(data);
                resultAction(num);
            }, ref err);
            return num;
        }

        void PostInt(object para)
        {
            object[] arr = para as object[];
            Action<int> resultAction = arr[0] as Action<int>;
            int num = (int)arr[1];
            resultAction(num);
        }

        void PostDataTable(object para)
        {
            object[] arr = para as object[];
            Action<DataTable> resultAction = arr[0] as Action<DataTable>;
            DataTable dt = (DataTable)arr[1];
            resultAction(dt);
        }

        public void Dispose()
        {
            ((IDisposable)basicExecForSQL).Dispose();

            if (null != insertBatch) ((IDisposable)insertBatch).Dispose();
            if (null != updateBatch) ((IDisposable)updateBatch).Dispose();
            if (null != deleteBatch) ((IDisposable)deleteBatch).Dispose();
        }

    }
}
