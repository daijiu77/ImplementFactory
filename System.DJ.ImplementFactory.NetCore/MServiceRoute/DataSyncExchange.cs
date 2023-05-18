using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    internal class DataSyncExchange
    {
        private static Dictionary<IMSDataSyncBase, MethodList> outputDic = new Dictionary<IMSDataSyncBase, MethodList>();
        private static Dictionary<IMSDataSyncBase, MethodList> inputDic = new Dictionary<IMSDataSyncBase, MethodList>();

        private static List<DataSyncMessage> dataSyncMessages = new List<DataSyncMessage>();

        private static Task task = null;
        private static IHttpHelper httpHelper = null;
        private static object _dataSyncExchangeLock = new object();

        private const string _Insert = "Insert";
        private const string _Update = "Update";
        private const string _Delete = "Delete";

        private static List<DataSyncItem> DataSyncItems = new List<DataSyncItem>();
        private static object _ReceiveDataSyncLock = new object();
        private static bool IsExecDataSync = false;
        private static DataSyncItem syncItem = null;
        private static int dataSyncPulse = 0;
        private static int syncConfigIndex = 0;

        static DataSyncExchange()
        {
            httpHelper = new HttpHelper();
            task = Task.Run(() =>
            {
                List<Assembly> assemblies = DJTools.GetAssemblyCollection(DJTools.RootPath);
                Type[] types = null;
                Type dataSyncOutputType = typeof(IMSDataSyncOutput);
                Type dataSyncInputType = typeof(IMSDataSyncInput);
                object dataSyncOutput = null;
                foreach (Assembly assembly in assemblies)
                {
                    types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsInterface || type.IsEnum || type.IsAbstract) continue;
                        if (dataSyncOutputType.IsAssignableFrom(type))
                        {
                            try
                            {
                                dataSyncOutput = Activator.CreateInstance(type);
                                GetEMethodInfo((IMSDataSyncOutput)dataSyncOutput, type);
                            }
                            catch (Exception)
                            {

                                //throw;
                            }
                        }
                        else if (dataSyncInputType.IsAssignableFrom(type))
                        {
                            try
                            {
                                dataSyncOutput = Activator.CreateInstance(type);
                                GetEMethodInfo((IMSDataSyncInput)dataSyncOutput, type);
                            }
                            catch (Exception)
                            {

                                //throw;
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            });
        }

        private static void GetEMethodInfo<T>(T instance, Type type)
        {
            Type interfaceType = null;
            bool isInput = false;
            if (null != (instance as IMSDataSyncInput))
            {
                isInput = true;
                interfaceType = typeof(IMSDataSyncInput);
                if (inputDic.ContainsKey((IMSDataSyncBase)instance)) return;
            }
            else
            {
                interfaceType = typeof(IMSDataSyncOutput);
                if (outputDic.ContainsKey((IMSDataSyncBase)instance)) return;
            }

            MethodInfo[] mis = interfaceType.GetMethods();
            MethodList methodList = new MethodList();
            foreach (MethodInfo mi in mis)
            {
                methodList.Add(new MyMethod(mi, type));
            }

            if (isInput)
            {
                inputDic.Add((IMSDataSyncBase)instance, methodList);
            }
            else
            {
                outputDic.Add((IMSDataSyncBase)instance, methodList);
            }
        }

        public static void Start()
        {
            Task.Run(() =>
            {
                task.Wait();
                const int sleepNum = 1000 * 3;
                RouteAttr routeAttr = null;
                DataSyncMessage syncMessage = null;
                DataSyncConfigList<DataSyncConfig> syncConfig = null;
                bool success = false;
                while (true)
                {
                    if (0 < dataSyncMessages.Count)
                    {
                        syncMessage = dataSyncMessages[0];
                        syncConfig = MicroServiceRoute.GetDataSyncMessageByName(syncMessage.DataSyncsName);
                        success = false;
                        routeAttr = null;
                        if (null != syncConfig)
                        {
                            routeAttr = syncConfig[syncConfigIndex];
                            if (null != routeAttr)
                            {
                                success = true;
                                Send(routeAttr.Name, syncMessage, err =>
                                {
                                    if (!string.IsNullOrEmpty(err))
                                    {
                                        success = false;
                                        syncConfigIndex++;
                                    }
                                });
                            }
                        }

                        if ((null == routeAttr) || success)
                        {
                            dataSyncMessages.Remove(syncMessage);
                        }
                    }
                    Thread.Sleep(sleepNum);
                }
            });

            Task.Run(() =>
            {
                task.Wait();
                const int sleepNum = 1000 * 3;
                while (true)
                {
                    GetDataByPipleLine();
                    Thread.Sleep(sleepNum);
                }
            });

            exec_dataSync();
        }

        private static void GetDataByPipleLine()
        {
            if (0 == outputDic.Count) return;
            MethodList methodList = null;
            DataSyncMessage syncMessage = null;
            object instance = null;
            foreach (var item in outputDic)
            {
                instance = item.Key;
                methodList = item.Value;
                methodList.Foreach(myMethod =>
                {
                    Thread.Sleep(100);
                    if (null == myMethod.microServiceRoute) return;
                    if (SyncCylces.Always != myMethod.dataSyncCylceAttribute.Cylce)
                    {
                        if (((int)myMethod.dataSyncCylceAttribute.Cylce) <= myMethod.ExecTime) return;
                        myMethod.ExecTime++;
                    }
                    syncMessage = ExecuteMethod(instance, myMethod);
                    if (null == syncMessage) return;
                    AddExchnage(syncMessage);
                });
                Thread.Sleep(100);
            }
        }

        private static DataSyncMessage ExecuteMethod(object instance, MyMethod myMethod)
        {
            DataSyncMessage syncMessage = null;
            object data = null;
            try
            {
                data = myMethod.GetMethodInfo().Invoke(instance, null);
            }
            catch (Exception)
            {

                //throw;
            }

            if (null == data) return syncMessage;

            DataSyncItem syncItem = new DataSyncItem()
            {
                Data = data
            };

            string dataSyncsName = MicroServiceRoute.GetDataSyncsNameByRouteName(myMethod.microServiceRoute.RouteName);
            syncItem.SetDataSyncsName(dataSyncsName);

            syncMessage = new DataSyncMessage()
            {
                DataSyncOption = syncItem,
            };
            syncMessage.SetDataSyncsName(dataSyncsName);
            syncMessage.SetResourceKey(MicroServiceRoute.Key);

            switch (myMethod.Name)
            {
                case _Insert:
                    syncItem.DataType = DataTypes.Add;
                    break;
                case _Update:
                    syncItem.DataType = DataTypes.Change;
                    break;
                case _Delete:
                    syncItem.DataType = DataTypes.Delete;
                    break;
            }

            return syncMessage;
        }

        public static void AddExchnage(DataSyncMessage syncMessage)
        {
            lock (_dataSyncExchangeLock)
            {
                dataSyncMessages.Add(syncMessage);
            }
        }

        private static void Send(string routeName, DataSyncMessage message, Action<string> errAction)
        {
            lock (_dataSyncExchangeLock)
            {
                RouteAttr routeAttr = MicroServiceRoute.GetRouteAttributeByName(routeName);

                string url = routeAttr.Uri;
                if (string.IsNullOrEmpty(url)) return;
                url = url.Trim();

                if (!MService.httpRg.IsMatch(url)) return;

                if (url.Substring(url.Length - 1).Equals("/")) url = url.Substring(0, url.Length - 1);

                url += "/";
                Regex rg = new Regex(@"^(?<HttpHeader>(http)|(https))\:\/\/(?<HttpBody>[^\/]+)\/.+\/$", RegexOptions.IgnoreCase);
                if (rg.IsMatch(url))
                {
                    Match m = rg.Match(url);
                    string HttpHeader = m.Groups["HttpHeader"].Value;
                    string HttpBody = m.Groups["HttpBody"].Value;
                    url = "{0}://{1}/".ExtFormat(HttpHeader, HttpBody);
                }
                url += "DataSync/Receiver";

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add(MServiceConst.contractKey, routeAttr.ContractKey);

                httpHelper.SendData(url, headers, message, true, (resultObj, err) =>
                {
                    errAction(err);
                });
            }
        }

        #region Data receive
        private static void exec_dataSync()
        {
            const int sleepNum = 1000 * 3;
            Task.Run(() =>
            {
                IsExecDataSync = false;
                while (true)
                {
                    ExecDataSyncItem();
                    Thread.Sleep(sleepNum);
                }
            });

            Task.Run(() =>
            {
                int Muniter = sleepNum * 20;
                int maxNum = (Muniter * 2) / sleepNum;
                while (true)
                {
                    if (IsExecDataSync) dataSyncPulse++;
                    if (maxNum <= dataSyncPulse)
                    {
                        if (null != syncItem) RemoveDataSyncItem(syncItem);
                        syncItem = null;
                        dataSyncPulse = 0;
                        IsExecDataSync = false;
                    }
                    Thread.Sleep(sleepNum);
                }
            });
        }

        private static void RemoveDataSyncItem(DataSyncItem syncItem)
        {
            lock (_ReceiveDataSyncLock)
            {
                DataSyncItems.Remove(syncItem);
            }
        }

        private static void ExecDataSyncItem()
        {
            lock (_ReceiveDataSyncLock)
            {
                if (0 == DataSyncItems.Count) return;
                if (IsExecDataSync) return;
                IsExecDataSync = true;
                dataSyncPulse = 0;
                syncItem = DataSyncItems[0];
                Task.Run(() =>
                {
                    DataSync_Add(syncItem);
                    DataSync_Change(syncItem);
                    DataSync_Delete(syncItem);

                    DataSyncItems.Remove(syncItem);
                    syncItem = null;
                    IsExecDataSync = false;
                });
            }
        }

        private static void DataSync_Exec(DataSyncItem item, string methodName, Action<IMSDataSyncInput> action)
        {
            MyMethod mlist = null;
            string dataSyncsNameLower = item.DataSyncsName.ToLower();
            foreach (var inputItem in inputDic)
            {
                mlist = inputItem.Value[methodName];
                if (null == mlist) continue;
                if (null != mlist.dataSyncReceiverAttribute)
                {
                    if (!string.IsNullOrEmpty(mlist.dataSyncReceiverAttribute.DataSyncsName))
                    {
                        if (!mlist.dataSyncReceiverAttribute.DataSyncsName.ToLower().Equals(dataSyncsNameLower)) continue;
                    }
                }

                try
                {
                    action((IMSDataSyncInput)inputItem.Key);
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }

        private static void DataSync_Add(DataSyncItem item)
        {
            if (DataTypes.Add != (item.DataType & DataTypes.Add)) return;
            DataSync_Exec(item, _Insert, input =>
            {
                input.Insert(item.DataSyncsName, item);
            });
        }

        private static void DataSync_Change(DataSyncItem item)
        {
            if (DataTypes.Change != (item.DataType & DataTypes.Change)) return;
            DataSync_Exec(item, _Update, input =>
            {
                input.Update(item.DataSyncsName, item);
            });
        }

        private static void DataSync_Delete(DataSyncItem item)
        {
            if (DataTypes.Delete != (item.DataType & DataTypes.Delete)) return;
            DataSync_Exec(item, _Delete, input =>
            {
                input.Delete(item.DataSyncsName, item);
            });
        }

        public static void DataSyncToLocal(DataSyncItem item)
        {
            lock (_ReceiveDataSyncLock)
            {
                DataSyncItems.Add(item);
            }
        }
        #endregion

        class MyMethod : EMethodInfo
        {
            private int _execTime = -1;
            /// <summary>
            /// 执行次数
            /// </summary>
            public int ExecTime
            {
                get { return _execTime; }
                set { _execTime = value; }
            }
            public MicroServiceRoute microServiceRoute { get; private set; }
            public MSDataSyncCylceAttribute dataSyncCylceAttribute { get; private set; }
            public MSDataSyncReceiverAttribute dataSyncReceiverAttribute { get; private set; }
            public MyMethod(MethodInfo mi, Type instanceType) : base(mi)
            {
                SetImplementType(instanceType);
                object[] attrs = null;
                bool isInput = typeof(IMSDataSyncInput).IsAssignableFrom(instanceType);
                if (isInput)
                {
                    attrs = WholeAttributes(typeof(MSDataSyncReceiverAttribute), true);
                    if (null != attrs)
                    {
                        if (0 < attrs.Length) dataSyncReceiverAttribute = (MSDataSyncReceiverAttribute)attrs[0];
                    }
                    if (null == dataSyncReceiverAttribute) dataSyncReceiverAttribute = new MSDataSyncReceiverAttribute();
                }
                else
                {
                    attrs = WholeAttributes(typeof(MicroServiceRoute), true);
                    if (null != attrs)
                    {
                        if (0 < attrs.Length) microServiceRoute = (MicroServiceRoute)attrs[0];
                    }

                    attrs = WholeAttributes(typeof(MSDataSyncCylceAttribute), true);
                    if (null != attrs)
                    {
                        if (0 < attrs.Length) dataSyncCylceAttribute = (MSDataSyncCylceAttribute)attrs[0];
                    }

                    if (null == dataSyncCylceAttribute) dataSyncCylceAttribute = new MSDataSyncCylceAttribute();
                }
            }
        }

        class MethodList
        {
            private Dictionary<string, MyMethod> dic = new Dictionary<string, MyMethod>();

            public MyMethod this[string methodName]
            {
                get
                {
                    MyMethod myMethod = null;
                    dic.TryGetValue(methodName, out myMethod);
                    return myMethod;
                }
            }

            public void Add(MyMethod myMethod)
            {
                string fn = myMethod.Name;
                if (dic.ContainsKey(fn)) return;
                dic[fn] = myMethod;
            }

            public int Count
            {
                get { return dic.Count; }
            }

            public void Foreach(Action<MyMethod> action)
            {
                foreach (var item in dic)
                {
                    action(item.Value);
                }
            }
        }
    }
}
