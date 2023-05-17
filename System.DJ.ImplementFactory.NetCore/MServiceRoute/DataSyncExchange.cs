using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    internal class DataSyncExchange
    {
        private static Dictionary<IMSDataSyncOutput, MethodList> outputDic = new Dictionary<IMSDataSyncOutput, MethodList>();
        private static List<DataSyncMessage> dataSyncMessages = new List<DataSyncMessage>();

        private static Task task = null;
        private static IHttpHelper httpHelper = null;
        private static object _dataSyncExchangeLock = new object();

        private const string _Insert = "Insert";
        private const string _Update = "Update";
        private const string _Delete = "Delete";

        static DataSyncExchange()
        {
            httpHelper = new HttpHelper();
            task = Task.Run(() =>
            {
                List<Assembly> assemblies = DJTools.GetAssemblyCollection(DJTools.RootPath);
                Type[] types = null;
                Type dataSyncOutputType = typeof(IMSDataSyncOutput);
                object dataSyncOutput = null;
                foreach (Assembly assembly in assemblies)
                {
                    types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        Thread.Sleep(100);
                        if (type.IsInterface || type.IsEnum || type.IsAbstract) continue;
                        if (!dataSyncOutputType.IsAssignableFrom(type)) continue;
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
                    Thread.Sleep(100);
                }
            });
        }

        private static void GetEMethodInfo(IMSDataSyncOutput instance, Type type)
        {
            if (outputDic.ContainsKey(instance)) return;
            MethodInfo[] mis = typeof(IMSDataSyncOutput).GetMethods();
            MethodList methodList = new MethodList();
            foreach (MethodInfo mi in mis)
            {
                MyMethod eMethod = (MyMethod)new MyMethod(mi, type);
                methodList.Add(eMethod);
            }
            outputDic.Add(instance, methodList);
        }

        public static void Start()
        {
            Task.Run(() =>
            {
                task.Wait();
                const int sleepNum = 1000 * 3;
                DataSyncMessage syncMessage = null;
                MyMethod myMethod = null;
                DataSyncConfig syncConfig = null;
                string routeName = "";
                while (true)
                {
                    if (0 < dataSyncMessages.Count)
                    {
                        syncMessage = dataSyncMessages[0];
                        syncConfig = MicroServiceRoute.GetDataSyncMessageByName(syncMessage.DataSyncsName);
                        if (null != syncConfig)
                        {
                            routeName = syncConfig.Name;
                            switch (syncMessage.DataSyncOption.DataType)
                            {
                                case DataTypes.Add:
                                    if (null != myMethod)
                                    {
                                        Send(routeName, syncMessage);
                                    }
                                    break;
                                case DataTypes.Change:
                                    if (null != myMethod)
                                    {
                                        Send(routeName, syncMessage);
                                    }
                                    break;
                                case DataTypes.Delete:
                                    if (null != myMethod)
                                    {
                                        Send(routeName, syncMessage);
                                    }
                                    break;
                            }
                        }
                        dataSyncMessages.Remove(syncMessage);
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
                    if (null == myMethod.microServiceRoute) return;
                    if (SyncCylces.Always != myMethod.dataSyncCylceAttribute.Cylce)
                    {
                        if (((int)myMethod.dataSyncCylceAttribute.Cylce) <= myMethod.ExecTime) return;
                        myMethod.ExecTime++;
                    }
                    syncMessage = ExecuteMethod(instance, myMethod);
                    if (null == syncMessage) return;
                    AddExchnage(syncMessage);
                    Thread.Sleep(100);
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

        private static void Send(string routeName, DataSyncMessage message)
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
                    //
                });
            }
        }

        class MyMethod : EMethodInfo
        {
            /// <summary>
            /// 执行次数
            /// </summary>
            public int ExecTime { get; set; }
            public MicroServiceRoute microServiceRoute { get; private set; }
            public DataSyncCylceAttribute dataSyncCylceAttribute { get; private set; }
            public MyMethod(MethodInfo mi, Type instanceType) : base(mi)
            {
                SetImplementType(instanceType);
                object[] attrs = WholeAttributes(typeof(MicroServiceRoute), true);
                if (null != attrs)
                {
                    if (0 < attrs.Length) microServiceRoute = (MicroServiceRoute)attrs[0];
                }

                attrs = WholeAttributes(typeof(DataSyncCylceAttribute), true);
                if (null != attrs)
                {
                    if (0 < attrs.Length) dataSyncCylceAttribute = (DataSyncCylceAttribute)attrs[0];
                }

                if (null == dataSyncCylceAttribute) dataSyncCylceAttribute = new DataSyncCylceAttribute();
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
