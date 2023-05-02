using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MService
    {
        /// <summary>
        /// Start the service registration mechanism, which should be executed at project startup.
        /// </summary>
        public static void Start()
        {
            register();
            //serviceManage();
        }

        private static void register()
        {
            Task.Run(() =>
            {
                IHttpHelper httpHelper = new HttpHelper();
                MethodTypes methodTypes = MethodTypes.Get;
                List<string> errUrls = new List<string>();
                string[] uris = null;
                char c = ' ';
                string s = "", s1 = "";
                string url = "";
                string registerAddr = "";
                Regex rg = new Regex(@"[^a-z0-9_\:\/\.]", RegexOptions.IgnoreCase);
                Regex rg1 = new Regex(@"^((http)|(https))\:\/\/", RegexOptions.IgnoreCase);
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
                    heads[MSServiceImpl.contractKey] = contractValue;
                    methodTypes = RegisterActionType;
                    foreach (string item in uris)
                    {
                        url = item.Trim();
                        if (string.IsNullOrEmpty(url)) continue;
                        if (!rg1.IsMatch(url)) continue;
                        if (url.Substring(url.Length - 1).Equals("/"))
                        {
                            url = url.Substring(0, url.Length - 1);
                        }
                        url += "/" + registerAddr;
                        httpHelper.SendData(url, heads, null, true, methodTypes, (result, msg) =>
                        {
                            if (!string.IsNullOrEmpty(msg)) errUrls.Add(url + "\t" + ((int)methodTypes).ToString() + "\t" + contractValue);
                        });
                    }
                });

                int n = 0;
                int num = 0;
                int size = errUrls.Count;
                const int sleepNum = 1000 * 3;
                string[] arr = null;
                while (n < size)
                {
                    url = errUrls[n];
                    arr = url.Split('\t');
                    url = arr[0];
                    num = 0;
                    int.TryParse(arr[1], out num);
                    heads.Clear();
                    heads[MSServiceImpl.contractKey] = arr[2];
                    methodTypes = (MethodTypes)num;
                    httpHelper.SendData(url, heads, null, true, methodTypes, (result, msg) =>
                    {
                        if (string.IsNullOrEmpty(msg))
                        {
                            errUrls.RemoveAt(n);
                            n = 0;
                            size = errUrls.Count;
                        }
                        else
                        {
                            n++;
                        }
                    });

                    if ((n >= size) && (0 < size))
                    {
                        n = 0;
                    }
                    Thread.Sleep(sleepNum);
                }
            });
        }

        private static void serviceManage()
        {
            Task.Run(() =>
            {
                if (null == s_serviceManager) return;
                if (string.IsNullOrEmpty(s_serviceManager.Uri)
                    || string.IsNullOrEmpty(s_serviceManager.ServiceManagerAddr)
                    || string.IsNullOrEmpty(s_serviceManager.ContractKey)) return;

                Regex rg = new Regex(@"(?<controllerName>[a-z0-9_]+)controller$", RegexOptions.IgnoreCase);
                Regex rg1 = new Regex(@"(\[controller\])|(\{controller\})", RegexOptions.IgnoreCase);
                string controllerName = "";
                string actionName = "";

                Type[] attrTypes = new Type[] { };

                Dictionary<string, PropertyInfo> piDic = new Dictionary<string, PropertyInfo>();
                PipleList pipleList = new PipleList();
                pipleList.GetType().ForeachProperty((pi, pt, fn) =>
                {
                    piDic.Add("ms" + pi.Name.ToLower(), pi);
                });
                string fn1 = "";
                string fv1 = "";
                string binPath = DJTools.RootPath;
                binPath = DJTools.isWeb ? (binPath + "\\bin") : binPath;
                List<Assembly> assemblies = DJTools.GetAssemblyCollection(binPath, new string[] { "/TempImpl/bin/" });
                foreach (Assembly assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (string.IsNullOrEmpty(type.Name)) continue;
                        if (!rg.IsMatch(type.Name)) continue;
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
                            Attribute atr = null;
                            foreach (Type t in attrTypes)
                            {
                                atr = mi.GetCustomAttribute(t, true);
                                if (null != atr) break;
                            }

                            PipleInfo info = null;
                            if (null != atr)
                            {
                                string typeName = atr.GetType().Name.ToLower();
                                if(piDic.ContainsKey(typeName))
                                {
                                    info = new PipleInfo();
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
                PipleInfo pipleInfo = null;
                pipleList.ForeachProperty((pi, pt, fn, fv) =>
                {
                    if (null == fv) return;
                    pipleInfo = (PipleInfo)fv;
                    paraStr = "";
                    foreach (var param in pipleInfo.Parameters)
                    {
                        paraStr += ", {\"Name\": \"{0}\", \"Type\": \"{1}\"}".ExtFormat(param.Key, param.Value.TypeToString(true));
                    }

                    if (!string.IsNullOrEmpty(paraStr)) paraStr = paraStr.Substring(1);
                    paraStr = paraStr.Trim();
                    fv1 = "{\"Name\": \"{0}\", \"Uri\": \"{1}\", \"MethodType\": \"{2}\", \"Parameters\": [{3}]}";
                    fv1 = fv1.ExtFormat(fn, pipleInfo.Uri, pipleInfo.MethodType, paraStr);
                    jsonData += ", " + fv1;
                });

                if (!string.IsNullOrEmpty(jsonData)) jsonData = jsonData.Substring(1);
                jsonData = jsonData.Trim();
                jsonData = "{\"ServiceName\": \"{0}\", \"Data\": [{1}], \"CrateTime\": \"{2}\"}".ExtFormat(s_ServiceName, jsonData, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string svrUrl = s_serviceManager.Uri;
                string s1 = svrUrl.Substring(svrUrl.Length - 1);
                if (s1.Equals("\\") || s1.Equals("/"))
                {
                    svrUrl = svrUrl.Substring(0, svrUrl.Length - 1);
                }

                string ServiceManagerAddr = s_serviceManager.ServiceManagerAddr;
                s1 = ServiceManagerAddr.Substring(0, 1);
                if (s1.Equals("\\") || s1.Equals("/"))
                {
                    ServiceManagerAddr = ServiceManagerAddr.Substring(1);
                }

                svrUrl += "/" + ServiceManagerAddr;

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add(MSServiceImpl.contractKey, s_serviceManager.ContractKey);

                IHttpHelper httpHelper = new HttpHelper();
                MethodTypes methodTypes1 = s_serviceManager.ServiceManagerActionType;
                bool success = false;
                while (true)
                {
                    httpHelper.SendData(svrUrl, headers, jsonData, false, methodTypes1, (vObj, err) =>
                    {
                        success = string.IsNullOrEmpty(err);
                    });
                    if (success) break;
                    Thread.Sleep(5000);
                }
            });
        }

        public class PipleList
        {
            /// <summary>
            /// MSAddServiceRouteItemAction
            /// </summary>
            public PipleInfo AddServiceRouteItemAction { get; set; }
            /// <summary>
            /// MSClientRegisterAction
            /// </summary>
            public PipleInfo ClientRegisterAction { get; set; }
            /// <summary>
            /// MSConfiguratorAction
            /// </summary>
            public PipleInfo ConfiguratorAction { get; set; }
            /// <summary>
            /// MSRemoveServiceRouteItemAction
            /// </summary>
            public PipleInfo RemoveServiceRouteItemAction { get; set; }
        }

        public class PipleInfo
        {
            private Dictionary<string, Type> _parameters = new Dictionary<string, Type>();
            public string Uri { get; set; }
            public string MethodType { get; set; }
            public Dictionary<string, Type> Parameters { get { return _parameters; } }
        }
    }
}
