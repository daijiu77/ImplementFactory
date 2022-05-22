using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    class ExecuteSql : IExecuteSql
    {
        List<TempData> tempDatas1 = new List<TempData>();
        List<TempData> tempDatas2 = new List<TempData>();

        string _Key = "";
        string _connectString = "";
        IDbHelper _dbHelper = null;

        int tdNum = 0;
        int dataCount = 0;
        int pulse = 0;
        bool isExecData = false;
        bool isRun = false;

        BasicExecForSQL basicExecForSQL = BasicExecForSQL.Instance;

        private ExecuteSql()
        {
            new Task(() =>
            {
                isRun = true;
                int n = 0;
                int oldNum = 0;
                while (isRun)
                {
                    n++;
                    if (oldNum != pulse)
                    {
                        n = 0;
                        oldNum = pulse;
                    }

                    if (5 == n)
                    {
                        n = 0;
                        oldNum = 0;
                        pulse = 0;
                        isExecData = false;
                    }
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public static ExecuteSql Instance
        {
            get
            {
                return new ExecuteSql();
            }
        }

        string IExecuteSql.Key
        {
            get { return _Key; }
            set { _Key = value; }
        }

        string IExecuteSql.connectString
        {
            get { return _connectString; }
            set
            {
                _connectString = value;
                basicExecForSQL.dbConnectionString = value;
            }
        }

        IDbConnectionState _dbConnectionState = null;
        IDbConnectionState IExecuteSql.dbConnectionState
        {
            get { return _dbConnectionState; }
            set
            {
                _dbConnectionState = value;
                if (null != _dbConnectionState)
                {
                    Type type = _dbHelper.dbConnectionState.GetType();
                    basicExecForSQL.dbConnectionState = (IDbConnectionState)Activator.CreateInstance(type);
                }
            }
        }

        bool _disposableAndClose = false;
        bool IExecuteSql.disposableAndClose
        {
            get { return _disposableAndClose; }
            set
            {
                _disposableAndClose = value;
                basicExecForSQL.disposableAndClose = _disposableAndClose;
            }
        }

        IDataServerProvider _dataServerProvider = null;
        IDataServerProvider IExecuteSql.dataServerProvider
        {
            get { return _dataServerProvider; }
            set
            {
                _dataServerProvider = value;
                basicExecForSQL.dataServerProvider = _dataServerProvider;
            }
        }

        void setPulse()
        {
            pulse++;
            if (99999 == pulse) pulse = 0;
        }

        void IExecuteSql.Add(object tempData)
        {
            if (null == tempData) return;
            TempData tempData1 = tempData as TempData;
            if (null == tempData1) return;

            if (0 == tdNum)
            {
                tempDatas1.Add(tempData1);
            }
            else
            {
                tempDatas2.Add(tempData1);
            }

            //if (0 == tempDatas1.Count && 0 == tempDatas2.Count) return;
            if (isExecData) return;
            isExecData = true;
            dataCount = 0;
            new Task(() =>
            {
                TempData td = null;
                string err = "";
                while (isExecData)
                {
                    setPulse();
                    tdNum = 0 < tempDatas1.Count ? 1 : tdNum;
                    while (0 < tempDatas1.Count)
                    {
                        setPulse();
                        td = tempDatas1[0];
                        if (null == td) continue;
                        basicExecForSQL.Exec(td.autoCall, td.sql, td.parameters, ref err, td.resultOfOpt, td.dataOpt);
                        tempDatas1.RemoveAt(0);
                    }

                    tdNum = 0 < tempDatas2.Count ? 0 : tdNum;
                    while (0 < tempDatas2.Count)
                    {
                        setPulse();
                        td = tempDatas2[0];
                        if (null == td) continue;
                        basicExecForSQL.Exec(td.autoCall, td.sql, td.parameters, ref err, td.resultOfOpt, td.dataOpt);
                        tempDatas2.RemoveAt(0);
                    }

                    if (0 < tempDatas1.Count || 0 < tempDatas2.Count)
                    {
                        dataCount = 0;
                    }
                    else
                    {
                        dataCount++;
                        Thread.Sleep(10);
                    }

                    if (1000 == dataCount) isExecData = false;
                }
                ((IDisposable)basicExecForSQL).Dispose();
                //Trace.WriteLine("numData: " + numData + " ***********************************");
            }).Start();
        }

        void IDisposable.Dispose()
        {
            isExecData = false;
            isRun = false;
            ((IDisposable)basicExecForSQL).Dispose();
        }

    }
}
