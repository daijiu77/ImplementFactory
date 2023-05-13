using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MService
    {
        public static Regex httpRg = new Regex(@"^((http)|(https))\:\/\/[^\/]+");
        private static int maxNum = 50;
        private static int maxNumber = 50;
        private const int sleepNum = 1000 * 3;
        /// <summary>
        /// Start the service registration mechanism, which should be executed at project startup.
        /// </summary>
        public static void Start()
        {
            maxNumber = ImplementAdapter.dbInfo1.TryTimeServiceRegister;
            maxNum = maxNumber;
            if (0 >= maxNumber) maxNum = 1;
            register();
            serviceManage();
            if (null != ImplementAdapter.mSService)
            {
                ImplementAdapter.mSService.ChangeEnabled += MSService_ChangeEnabled;
                ImplementAdapter.mSService.RegisterIP += MSService_RegisterIP;
            }
        }

        private static void MSService_RegisterIP(string ipAddr)
        {
            if (0 >= maxNumber) return;
            register();
        }

        private static void MSService_ChangeEnabled(DateTime startTime, DateTime endTime, string contractKey)
        {
            if (0 >= maxNumber) return;
            serviceManage();
        }

        private static void register()
        {
            Task.Run(() =>
            {
                int sleep1 = sleepNum * 2;
                while (true)
                {
                    exec_register();
                    if (0 < maxNumber) break;
                    Thread.Sleep(sleep1);
                }
            });
        }

        private static void serviceManage()
        {
            Task.Run(() =>
            {
                int sleep1 = sleepNum * 2;
                while (true)
                {
                    exec_serviceManage();
                    if (0 < maxNumber) break;
                    Thread.Sleep(sleep1);
                }
            });
        }

        private static void exec_register()
        {
            IHttpHelper httpHelper = new HttpHelper();
            MethodTypes methodTypes = MethodTypes.Get;
            List<string> errUrls = new List<string>();
            string[] uris = null;
            char c = ' ';
            string s = "", s1 = "";
            string url = "";
            string httpUrl = "";
            string testUrl = "";
            string registerAddr = "";
            string printMsg = "The current service has been successfully registered to the address: {0}.";
            Regex rg = new Regex(@"[^a-z0-9_\:\/\.]", RegexOptions.IgnoreCase);
            Dictionary<string, string> heads = new Dictionary<string, string>();
            MicroServiceRoute.Foreach(delegate (string MSRouteName, string Uri, string RegisterAddr, string TestAddr, string contractValue, MethodTypes RegisterActionType)
            {
                s = Uri.Trim();
                if (string.IsNullOrEmpty(s)) return;

                registerAddr = RegisterAddr.Trim();
                if (string.IsNullOrEmpty(registerAddr)) return;
                if (registerAddr.Substring(0, 1).Equals("/"))
                {
                    registerAddr = registerAddr.Substring(1);
                }

                if (null == TestAddr) TestAddr = "";
                TestAddr = TestAddr.Trim();
                if (!string.IsNullOrEmpty(TestAddr))
                {
                    TestAddr = httpRg.Replace(TestAddr, "");
                    TestAddr = TestAddr.Trim();
                    if (!string.IsNullOrEmpty(TestAddr))
                    {
                        while (TestAddr.Substring(0, 1).Equals("/"))
                        {
                            TestAddr = TestAddr.Substring(1);
                            if (string.IsNullOrEmpty(TestAddr)) break;
                        }
                    }
                }

                if (rg.IsMatch(s))
                {
                    s1 = rg.Match(s).Groups[0].Value;
                    c = s1.ToArray()[0];
                    uris = s.Split(c);
                }
                else
                {
                    uris = new string[] { s };
                }

                heads.Clear();
                heads[MServiceConst.contractKey] = contractValue;
                methodTypes = RegisterActionType;
                foreach (string item in uris)
                {
                    url = item.Trim();
                    if (string.IsNullOrEmpty(url)) continue;
                    if (!httpRg.IsMatch(url)) continue;
                    if (url.Substring(url.Length - 1).Equals("/"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }
                    url += "/";

                    if (!string.IsNullOrEmpty(TestAddr))
                    {
                        testUrl = url + TestAddr;
                        bool testSuccessfully = false;
                        httpHelper.SendData(testUrl, heads, null, true, methodTypes, (resultObj, msg) =>
                        {
                            string results = "";
                            if (null != resultObj) results = resultObj.ToString();
                            if (string.IsNullOrEmpty(msg))
                            {
                                testSuccessfully = true;
                            }
                            Test(MSRouteName, testUrl, methodTypes, contractValue, results, msg);
                        });
                        if (testSuccessfully) continue;
                    }

                    httpUrl = url + registerAddr;
                    httpHelper.SendData(httpUrl, heads, null, true, methodTypes, (resultObj, msg) =>
                    {
                        string results = "";
                        if (null != resultObj) results = resultObj.ToString();
                        if (string.IsNullOrEmpty(msg))
                        {
                            AbsActionFilterAttribute.PrintIpToLogs(printMsg.ExtFormat(httpUrl));
                            Success(MSRouteName, httpUrl, methodTypes, contractValue, results);
                            return;
                        }
                        else
                        {
                            Fail(MSRouteName, httpUrl, methodTypes, contractValue, results, msg);
                        }
                        errUrls.Add(httpUrl + "\t" + ((int)methodTypes).ToString() + "\t" + contractValue + "\t" + MSRouteName);
                    });
                }
            });

            int n = 0;
            int num = 0;
            int size = errUrls.Count;
            int timeNum = 0;
            string[] arr = null;
            while ((n < size) && (timeNum < maxNum))
            {
                url = errUrls[n];
                arr = url.Split('\t');
                url = arr[0];
                num = 0;
                int.TryParse(arr[1], out num);
                heads.Clear();
                heads[MServiceConst.contractKey] = arr[2];
                methodTypes = (MethodTypes)num;
                httpHelper.SendData(url, heads, null, true, methodTypes, (resultObj, msg) =>
                {
                    string results = "";
                    if (null != resultObj) results = resultObj.ToString();
                    if (string.IsNullOrEmpty(msg))
                    {
                        AbsActionFilterAttribute.PrintIpToLogs(printMsg.ExtFormat(url));
                        errUrls.RemoveAt(n);
                        n = 0;
                        size = errUrls.Count;
                        Success(arr[3], url, methodTypes, arr[2], results);

                    }
                    else
                    {
                        Fail(arr[3], url, methodTypes, arr[2], results, msg);
                        n++;
                    }
                });

                if ((n >= size) && (0 < size))
                {
                    n = 0;
                }
                timeNum++;
                Thread.Sleep(sleepNum);
            }
        }

        private static void Success(string routeName, string url, MethodTypes methodTypes, string contractValue, string message)
        {
            if (null != ImplementAdapter.serviceRegisterMessage)
            {
                try
                {
                    ImplementAdapter.serviceRegisterMessage.RegisterSuccess(routeName, url, methodTypes, contractValue, message);
                }
                catch { }
            }
        }

        private static void Fail(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            if (null != ImplementAdapter.serviceRegisterMessage)
            {
                try
                {
                    ImplementAdapter.serviceRegisterMessage.RegisterFail(routeName, url, methodTypes, contractValue, message, err);
                }
                catch { }
            }
        }

        private static void Test(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            if (null != ImplementAdapter.serviceRegisterMessage)
            {
                try
                {
                    ImplementAdapter.serviceRegisterMessage.TestVisit(routeName, url, methodTypes, contractValue, message, err);
                }
                catch { }
            }
        }

        private static void exec_serviceManage()
        {
            if (null == MicroServiceRoute.ServiceManager) return;
            if (string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.Uri)
                || string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.ServiceManagerAddr)
                || string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.ContractKey)) return;

            if (!httpRg.IsMatch(MicroServiceRoute.ServiceManager.Uri)) return;

            Regex rg = new Regex(@"(?<controllerName>[a-z0-9_]+)controller$", RegexOptions.IgnoreCase);
            Regex rg1 = new Regex(@"(\[controller\])|(\{controller\})", RegexOptions.IgnoreCase);
            string controllerName = "";
            string actionName = "";

            Dictionary<string, PropertyInfo> piDic = new Dictionary<string, PropertyInfo>();
            PipleList pipleList = new PipleList();
            pipleList.GetType().ForeachProperty((pi, pt, fn) =>
            {
                piDic.Add("ms" + pi.Name.ToLower(), pi);
            });
            string fn1 = "";
            string fv1 = "";
            string binPath = DJTools.RootPath;
            Regex rgPKT = new Regex(@"PublicKeyToken\=null", RegexOptions.IgnoreCase);
            string assembleStr = "";
            binPath = DJTools.isWeb ? (binPath + "\\bin") : binPath;
            List<Assembly> assemblies = DJTools.GetAssemblyCollection(binPath, new string[] { "/{0}/{1}/".ExtFormat(TempImplCode.dirName, TempImplCode.libName) });
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (string.IsNullOrEmpty(type.Name)) continue;
                    if (!rg.IsMatch(type.Name)) continue;

                    assembleStr = type.AssemblyQualifiedName;
                    if (string.IsNullOrEmpty(assembleStr)) assembleStr = "";
                    if (!rgPKT.IsMatch(assembleStr)) continue;

                    fv1 = "";
                    controllerName = rg.Match(type.Name).Groups["controllerName"].Value;
                    IEnumerable<Attribute> atrs = type.GetCustomAttributes();
                    foreach (Attribute atr in atrs)
                    {
                        fn1 = atr.GetType().Name.ToLower();
                        if (fn1.Equals("route") || fn1.Equals("routeattribute"))
                        {
                            fv1 = atr.GetPropertyValue<string>("Template");
                            if (!string.IsNullOrEmpty(fv1))
                            {
                                fv1 = rg1.Replace(fv1, controllerName);
                                controllerName = fv1;
                            }
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(fv1)) controllerName = "";

                    MethodInfo[] mis = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    foreach (MethodInfo mi in mis)
                    {
                        assembleStr = mi.DeclaringType.AssemblyQualifiedName;
                        if (string.IsNullOrEmpty(assembleStr)) assembleStr = "";
                        if (!rgPKT.IsMatch(assembleStr)) continue;

                        Attribute atr = mi.GetCustomAttribute(typeof(AbsSysAttributer), true);

                        PipleItem info = null;
                        if (null != atr)
                        {
                            string typeName = atr.GetType().Name.ToLower();
                            if (piDic.ContainsKey(typeName))
                            {
                                info = new PipleItem();
                                piDic[typeName].SetValue(pipleList, info);
                                piDic.Remove(typeName);
                            }
                        }

                        if (null != info)
                        {
                            actionName = mi.Name;
                            string methodType = "get";
                            atrs = mi.GetCustomAttributes();
                            foreach (Attribute item in atrs)
                            {
                                fn1 = item.GetType().Name.ToLower();
                                if (fn1.Equals("route") || fn1.Equals("routeattribute"))
                                {
                                    fv1 = item.GetPropertyValue<string>("Template");
                                    if (!string.IsNullOrEmpty(fv1)) actionName = fv1;
                                }
                                else if (fn1.Equals("httppost") || fn1.Equals("httppostattribute"))
                                {
                                    methodType = "post";
                                }
                            }
                            info.MethodType = methodType;

                            string s = "";
                            if (!string.IsNullOrEmpty(controllerName))
                            {
                                s = controllerName.Substring(controllerName.Length - 1);
                                if (s.Equals("/") || s.Equals("\\"))
                                {
                                    controllerName = controllerName.Substring(0, controllerName.Length - 1);
                                }
                                controllerName += "/";
                            }

                            s = actionName.Substring(0, 1);
                            if (s.Equals("/") || s.Equals("\\"))
                            {
                                actionName = actionName.Substring(1);
                            }
                            info.Uri = controllerName + actionName;

                            ParameterInfo[] paras = mi.GetParameters();
                            foreach (ParameterInfo param in paras)
                            {
                                info.Parameters.Add(param.Name, param.ParameterType);
                            }
                        }
                        if (0 == piDic.Count) break;
                        Thread.Sleep(100);
                    }
                    if (0 == piDic.Count) break;
                    Thread.Sleep(100);
                }
                if (0 == piDic.Count) break;
            }

            string jsonData = "";
            string paraStr = "";
            PipleItem pipleItem = null;
            pipleList.ForeachProperty((pi, pt, fn, fv) =>
            {
                if (null == fv) return;
                pipleItem = (PipleItem)fv;
                paraStr = "";
                foreach (var param in pipleItem.Parameters)
                {
                    paraStr += ", {\"Name\": \"{0}\", \"Type\": \"{1}\"}".ExtFormat(param.Key, param.Value.TypeToString(true));
                }

                if (!string.IsNullOrEmpty(paraStr)) paraStr = paraStr.Substring(1);
                paraStr = paraStr.Trim();
                fv1 = "{\"Name\": \"{0}\", \"Uri\": \"{1}\", \"MethodType\": \"{2}\", \"Parameters\": [{3}]}";
                fv1 = fv1.ExtFormat(fn, pipleItem.Uri, pipleItem.MethodType, paraStr);
                jsonData += ", " + fv1;
            });

            if (string.IsNullOrEmpty(jsonData))
            {
                return;
            }
            jsonData = jsonData.Substring(1);
            jsonData = jsonData.Trim();

            string port = null == MicroServiceRoute.Port ? "" : MicroServiceRoute.Port;
            string svrContractKey = MSServiceImpl.GetContractValue();
            jsonData = "{\"ServiceName\": \"{0}\", \"Port\": \"{1}\", \"{2}\": \"{3}\", \"Data\": [{4}], \"CrateTime\": \"{5}\"}"
                .ExtFormat(MicroServiceRoute.ServiceName, port, MServiceConst.svrMngcontractKey, svrContractKey, jsonData, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            string svrUrl = MicroServiceRoute.ServiceManager.Uri;
            string s1 = svrUrl.Substring(svrUrl.Length - 1);
            if (s1.Equals("\\") || s1.Equals("/"))
            {
                svrUrl = svrUrl.Substring(0, svrUrl.Length - 1);
            }

            string ServiceManagerAddr = MicroServiceRoute.ServiceManager.ServiceManagerAddr;
            s1 = ServiceManagerAddr.Substring(0, 1);
            if (s1.Equals("\\") || s1.Equals("/"))
            {
                ServiceManagerAddr = ServiceManagerAddr.Substring(1);
            }

            svrUrl += "/" + ServiceManagerAddr;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(MServiceConst.contractKey, MicroServiceRoute.ServiceManager.ContractKey);

            IHttpHelper httpHelper = new HttpHelper();
            MethodTypes methodTypes1 = MicroServiceRoute.ServiceManager.ServiceManagerActionType;
            string printMsg = "It has been successfully sent data to the ServiceManage: {0}";
            bool success = false;
            int timeNum = 0;
            while (timeNum < maxNum)
            {
                httpHelper.SendData(svrUrl, headers, jsonData, false, methodTypes1, (vObj, err) =>
                {
                    success = string.IsNullOrEmpty(err);
                    if (success)
                    {
                        AbsActionFilterAttribute.PrintIpToLogs(printMsg.ExtFormat(svrUrl));
                    }
                });
                if (success) break;
                timeNum++;
                Thread.Sleep(sleepNum);
            }
        }

        public class PipleList
        {
            /// <summary>
            /// MSAddServiceRouteItemAction
            /// </summary>
            public PipleItem AddServiceRouteItemAction { get; set; }
            /// <summary>
            /// MSClientRegisterAction
            /// </summary>
            public PipleItem ClientRegisterAction { get; set; }
            /// <summary>
            /// MSConfiguratorAction
            /// </summary>
            public PipleItem ConfiguratorAction { get; set; }
            /// <summary>
            /// MSRemoveServiceRouteItemAction
            /// </summary>
            public PipleItem RemoveServiceRouteItemAction { get; set; }
        }

        public class PipleItem
        {
            private Dictionary<string, Type> _parameters = new Dictionary<string, Type>();
            public string Uri { get; set; }
            public string MethodType { get; set; }
            public Dictionary<string, Type> Parameters { get { return _parameters; } }
        }
    }
}
