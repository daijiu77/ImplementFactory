using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MicroServiceMethodImpl : IMicroServiceMethod
    {
        private static Dictionary<string, Type> _keyDic = new Dictionary<string, Type>();
        private static object _MicroServiceMethodImplLock = new object();

        private static Type MSTypeDic(string key, Type type)
        {
            lock (_MicroServiceMethodImplLock)
            {
                Type msType = null;
                _keyDic.TryGetValue(key, out msType);
                if (null != type)
                {
                    _keyDic[key] = type;
                }
                return msType;
            }
        }

        public static string GetLegalText(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return txt;
            string s = txt.Trim();
            if (string.IsNullOrEmpty(s)) return s;
            Regex rg = new Regex(@"[^a-z0-9_]", RegexOptions.IgnoreCase);
            int n = 0;
            const int max = 100;
            while (rg.IsMatch(txt) && (max > n))
            {
                s = rg.Replace(s, "_");
                s = s.Replace("__", "_");
                if (s.Substring(0, 1).Equals("_")) s = s.Substring(1);
                n++;
            }
            return s;
        }

        Type IMicroServiceMethod.GetMS(IInstanceCodeCompiler instanceCodeCompiler, AutoCall autoCall, MicroServiceRoute microServiceRoute, Type interfaceType)
        {
            string interfaceName = interfaceType.TypeToString(true);
            string controllerName = interfaceType.Name;
            if (!string.IsNullOrEmpty(microServiceRoute.ControllerName)) controllerName = microServiceRoute.ControllerName;
            controllerName = GetLegalText(controllerName);
            string routeName = GetLegalText(microServiceRoute.RouteName);
            string msKey = interfaceName + "-" + controllerName + "-" + routeName;
            Type msType = MSTypeDic(msKey, null);
            if (null != msType) return msType;

            string usingList = "";
            string methodList = "";
            string methodCode = "";
            string propertyList = "";
            string contractKey = "";
            string sign = ", ";
            string paraList = "";
            string data = "";
            string s = "";
            string err = "";
            string returnType = "";
            string actionName = "";
            string code = "{#usingList}";
            string namespaceStr = TempImplCode.msProjectName + "." + TempImplCode.dirName + "." + TempImplCode.libName;

            const string taskRunStartTag = "[_Task.Run-Start_]";
            const string taskRunEndTag = "[_Task.Run-End_]";

            EList<CKeyValue> elist = new EList<CKeyValue>();
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            elist.Add(new CKeyValue() { Key = "System" });
            elist.Add(new CKeyValue() { Key = "System.Collections.Generic" });
            elist.Add(new CKeyValue() { Key = "Newtonsoft.Json" });
            elist.Add(new CKeyValue() { Key = typeof(ExtMethod).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(MSDataVisitor).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(RouteAttr).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(MicroServiceRoute).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(MethodTypes).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(ISingleInstance).Namespace });

            string clssName = interfaceType.Name + "_" + controllerName + "_" + routeName;
            string clssPath = namespaceStr + "." + clssName;
            MethodInformation mInfo = new MethodInformation();
            mInfo.append(ref code, LeftSpaceLevel.one, "namespace {0}", namespaceStr);
            mInfo.append(ref code, "{");
            mInfo.append(ref code, LeftSpaceLevel.two, "public class {0}: ImplementAdapter, {1}, IExtMSDataVisitor, ISingleInstance", clssName, DJTools.GetClassName(interfaceType, true));
            mInfo.append(ref code, LeftSpaceLevel.two, "{");
            mInfo.append(ref code, "");

            mInfo.append(ref code, "{#structorMethod}");
            mInfo.append(ref code, "");
            mInfo.append(ref code, "{#methodList}");
            mInfo.append(ref code, "{#propertyList}");
            mInfo.append(ref code, LeftSpaceLevel.two, "}");
            mInfo.append(ref code, "}");

            string structorMethod = "";
            mInfo.append(ref structorMethod, LeftSpaceLevel.three, "public {0}()", clssName);
            mInfo.append(ref structorMethod, LeftSpaceLevel.three, "{");
            //mInfo.append(ref structorMethod, LeftSpaceLevel.four, "");
            mInfo.append(ref structorMethod, LeftSpaceLevel.three, "}");
            code = code.Replace("{#structorMethod}", structorMethod);

            mInfo.append(ref propertyList, "");
            mInfo.append(ref propertyList, LeftSpaceLevel.three, "WillExecute IExtMSDataVisitor.willExecute { get; set; }");
            mInfo.append(ref propertyList, "");
            mInfo.append(ref propertyList, LeftSpaceLevel.three, "IMSAllot IExtMSDataVisitor.mSAllot { get; set; }");
            mInfo.append(ref propertyList, "");
            mInfo.append(ref propertyList, LeftSpaceLevel.three, "object ISingleInstance.Instance { get; set; }");
            mInfo.append(ref propertyList, "");

            string propCode = "";
            string propMethod = "";
            Attribute attr = null;
            Dictionary<string, string> propMethodDic = new Dictionary<string, string>();
            PropertyInfo[] props = interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in props)
            {
                propMethod = "get_" + pi.Name;
                if (!propMethodDic.ContainsKey(propMethod)) propMethodDic[propMethod] = propMethod;

                propMethod = "set_" + pi.Name;
                if (!propMethodDic.ContainsKey(propMethod)) propMethodDic[propMethod] = propMethod;

                propCode = "";
                returnType = pi.PropertyType.TypeToString(true);
                s = "private {0} _{1} = {2};";
                attr = pi.GetCustomAttribute(typeof(RouteNameAttribute), true);
                if (null != attr)
                {
                    s = s.ExtFormat(returnType, pi.Name, "\"" + microServiceRoute.RouteName + "\"");
                }
                else
                {
                    s = s.ExtFormat(returnType, pi.Name, "default(" + returnType + ")");
                }
                mInfo.append(ref propCode, LeftSpaceLevel.three, s);

                s = "{0} {1}.{2}".ExtFormat(returnType, interfaceName, pi.Name);
                mInfo.append(ref propCode, LeftSpaceLevel.three, s);
                mInfo.append(ref propCode, LeftSpaceLevel.three, "{");
                if (pi.CanRead)
                {
                    mInfo.append(ref propCode, LeftSpaceLevel.three + 1, "get { return _{0}; }", pi.Name);
                }
                if (pi.CanWrite)
                {
                    mInfo.append(ref propCode, LeftSpaceLevel.three + 1, "set { _{0} = value; }", pi.Name);
                }
                mInfo.append(ref propCode, LeftSpaceLevel.three, "}");

                if (string.IsNullOrEmpty(propertyList))
                {
                    propertyList = propCode;
                }
                else
                {
                    mInfo.append(ref propertyList, "");
                    mInfo.append(ref propertyList, propCode);
                }
            }

            Regex rg = new Regex(@"\`[0-9]+\[");
            RequestMapping requestMapping = null;
            EMethodInfo eMethod = null;
            DynamicCodeTempImpl tempImp = new DynamicCodeTempImpl();
            MethodInfo[] ms = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo miItem in ms)
            {
                if (propMethodDic.ContainsKey(miItem.Name)) continue;

                paraList = "";
                data = "";
                err = "";
                actionName = "";
                contractKey = "";
                requestMapping = null;
                attr = miItem.GetCustomAttribute(typeof(RequestMapping));

                eMethod = new EMethodInfo(miItem);

                if (null != attr)
                {
                    requestMapping = (RequestMapping)attr;
                    actionName = requestMapping.Name;
                }
                if (string.IsNullOrEmpty(actionName)) actionName = miItem.Name;

                ParameterInfo[] pis = miItem.GetParameters();
                foreach (ParameterInfo p in pis)
                {
                    if (p.ParameterType.BaseType == typeof(System.MulticastDelegate) && rg.IsMatch(p.ParameterType.ToString()))
                    {
                        continue;
                    }
                    else if (p.IsOut)
                    {
                        err = "\"" + p.Name + "\"";
                        continue;
                    }

                    paraList += sign + p.ParameterType.TypeToString(true) + " " + p.Name;
                    data += sign + p.Name + " = " + p.Name;
                }

                if (!string.IsNullOrEmpty(paraList))
                {
                    paraList = paraList.Substring(sign.Length);
                }

                methodCode = "";
                returnType = eMethod.ReturnType == null ? "void" : eMethod.ReturnType.TypeToString(true);

                tempImp.GetTaskReturnTypeStr(eMethod, elist, ref returnType);

                s = returnType;
                s += " " + interfaceName + "." + miItem.Name + "(" + paraList + ")";
                mInfo.append(ref methodCode, LeftSpaceLevel.three, s);
                mInfo.append(ref methodCode, LeftSpaceLevel.three, "{");

                if (eMethod.IsTaskReturn)
                {
                    mInfo.append(ref methodCode, LeftSpaceLevel.four, taskRunStartTag);
                }

                if (!string.IsNullOrEmpty(data))
                {
                    data = data.Substring(sign.Length);
                    data = "new { " + data + " }";
                }
                else
                {
                    data = "null";
                }

                s = "";
                RouteAttr routeAttr = MicroServiceRoute.GetRouteAttributeByName(microServiceRoute.RouteName);
                if (null == routeAttr) routeAttr = new RouteAttr();
                contractKey = routeAttr.ContractKey;
                if (typeof(void) != eMethod.ReturnType)
                {
                    returnType = eMethod.ReturnType.TypeToString(true);
                    mInfo.append(ref s, LeftSpaceLevel.four, "MicroServiceMethodImpl msi = new MicroServiceMethodImpl();");
                    //object thisMe, string routeName, string url, string controllerName, string actionName, string contractKey, object data
                    //                        {0}             {1}             {2}                   {3}                {4}               {5}
                    mInfo.append(ref s, LeftSpaceLevel.four, "return msi.MSVisitor<{0}>(this, \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", {6});",
                        returnType, microServiceRoute.RouteName, routeAttr.Uri, controllerName, actionName, contractKey, data);
                }
                else
                {
                    mInfo.append(ref s, LeftSpaceLevel.four, "MicroServiceMethodImpl msi = new MicroServiceMethodImpl();");
                    //object thisMe, string routeName, string url, string controllerName, string actionName, string contractKey, object data
                    //                        {0}             {1}             {2}                   {3}                {4}               {5}
                    mInfo.append(ref s, LeftSpaceLevel.four, "msi.ExecMSVisitor(this, \"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", {5});",
                        microServiceRoute.RouteName, routeAttr.Uri, controllerName, actionName, contractKey, data);
                }

                if (eMethod.IsTaskReturn)
                {
                    mInfo.append(ref s, LeftSpaceLevel.four, taskRunEndTag);//Task-end
                }

                mInfo.append(ref methodCode, s);
                mInfo.append(ref methodCode, LeftSpaceLevel.three, "}");

                if (eMethod.IsTaskReturn)
                {
                    methodCode = tempImp.CreateTaskCode(mInfo, eMethod, methodCode, LeftSpaceLevel.four, taskRunStartTag, taskRunEndTag);
                }

                if (string.IsNullOrEmpty(methodList))
                {
                    methodList = methodCode;
                }
                else
                {
                    mInfo.append(ref methodList, "");
                    mInfo.append(ref methodList, methodCode);
                }
            }

            usingList = "";
            foreach (CKeyValue item in elist)
            {
                usingList += "using " + item.Key + ";\r\n";
            }

            code = code.Replace("{#usingList}", usingList);
            code = code.Replace("{#methodList}", methodList);
            code = code.Replace("{#propertyList}", propertyList);

            string binDirPath = Path.Combine(DJTools.RootPath, TempImplCode.dirName);
            binDirPath = Path.Combine(binDirPath, TempImplCode.libName);
            binDirPath.InitDirectory();
            instanceCodeCompiler.SavePathOfDll = Path.Combine(binDirPath, clssName + ".dll");

            err = "";
            Type type = null;
            string[] dllArr = new string[] { "System.dll", "System.Xml.dll" };
            Assembly assObj = instanceCodeCompiler.TranslateCode(dllArr, "v4.0", code, ref err);
            if (string.IsNullOrEmpty(err))
            {
                try
                {
                    type = assObj.GetType(clssPath);
                    MSTypeDic(msKey, type);
                }
                catch (Exception ex)
                {
                    err = "加载程序集时出错:\r\n" + ex.ToString();
                }
            }

            if (ImplementAdapter.sysConfig1.IsShowCode)
            {
                string txt = code;
                if (!string.IsNullOrEmpty(err))
                {
                    txt += "\r\n\r\n/**\r\n" + err + "\r\n**/";
                }
                TempImplCode.PrintCode(txt, clssName);
            }

            return type;
            //throw new NotImplementedException();
        }

        public T MSVisitor<T>(object me, string routeName, string url, string controllerName, string actionName, string contractKey, object data)
        {
            GetUrlInfo(routeName, ref url, ref contractKey);
            MSDataVisitor dataVisitor = new MSDataVisitor();
            string result = dataVisitor.GetResult(me, routeName, url, controllerName, actionName, contractKey, MethodTypes.Post, data);
            if (string.IsNullOrEmpty(result)) return default(T);
            Type type = typeof(T);
            if (type == typeof(Guid))
            {
                Guid guid = Guid.Empty;
                Guid.TryParse(result, out guid);
                object vObj = guid;
                return (T)vObj;
            }
            else if (type == typeof(DateTime))
            {
                DateTime dt = DateTime.Now;
                DateTime.TryParse(result, out dt);
                object vObj = dt;
                return (T)vObj;
            }
            else if (type.IsBaseType())
            {
                return DJTools.ConvertTo<T>(result);
            }
            else if (type.IsList())
            {
                return (T)result.JsonToList(type);
            }
            else if (type.IsArray)
            {
                return (T)result.JsonToList(type, true);
            }
            else if (type.Equals(typeof(object)))
            {
                return (T)DataTranslation.JsonToObject(result);
            }
            else if (type.IsClass && (typeof(string) != type))
            {
                return (T)result.JsonToEntity(type);
            }
            return default(T);
        }

        public void ExecMSVisitor(object me, string routeName, string url, string controllerName, string actionName, string contractKey, object data)
        {
            GetUrlInfo(routeName, ref url, ref contractKey);
            MSDataVisitor dataVisitor = new MSDataVisitor();
            dataVisitor.GetResult(me, routeName, url, controllerName, actionName, contractKey, MethodTypes.Post, data);
        }

        private static string httpStr = "";
        private static string areaName = "";
        private void GetUrlInfo(string serviceName, ref string url, ref string contractKey)
        {
            Regex rg = new Regex(@"^(?<HttpHeader>(http)|(https))\:\/\/(?<HttpBody>[^\/]+)(\/(?<AreaName>[^\/]+))?", RegexOptions.IgnoreCase);
            if (string.IsNullOrEmpty(httpStr))
            {
                if (!string.IsNullOrEmpty(url))
                {
                    if (rg.IsMatch(url))
                    {
                        Match m1 = rg.Match(url);
                        httpStr = m1.Groups["HttpHeader"].Value;
                        areaName = m1.Groups["AreaName"].Value;
                        if (!string.IsNullOrEmpty(areaName)) areaName = "/" + areaName;
                    }
                }
            }

            if (string.IsNullOrEmpty(serviceName)) return;
            if ((false == string.IsNullOrEmpty(url)) && (false == string.IsNullOrEmpty(contractKey))) return;

            string[] ucArr = SvrUrlContractKey(serviceName, null, null);
            if (null != ucArr)
            {
                url = ucArr[0];
                contractKey = ucArr[1];
                return;
            }

            if (null == MicroServiceRoute.ServiceManager) return;
            if (string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.Uri)
                || string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.ServiceManagerAddr)
                || string.IsNullOrEmpty(MicroServiceRoute.ServiceManager.ContractKey)) return;

            if (!rg.IsMatch(MicroServiceRoute.ServiceManager.Uri)) return;

            string msUrl = MicroServiceRoute.ServiceManager.Uri;
            if (rg.IsMatch(msUrl))
            {
                Match m = rg.Match(msUrl);
                string HttpHeader = m.Groups["HttpHeader"].Value;
                string HttpBody = m.Groups["HttpBody"].Value;
                msUrl = "{0}://{1}/".ExtFormat(HttpHeader, HttpBody);

                if (string.IsNullOrEmpty(httpStr)) httpStr = HttpHeader;
                if (string.IsNullOrEmpty(areaName))
                {
                    areaName = m.Groups["AreaName"].Value;
                    if (!string.IsNullOrEmpty(areaName)) areaName = "/" + areaName;
                }
            }
            msUrl = msUrl.Trim();
            if (!msUrl.Substring(msUrl.Length - 1).Equals("/"))
            {
                msUrl += "/";
            }
            msUrl += "{0}/{1}?serviceName={2}&port={3}".ExtFormat(MSConst.MSCommunication,
                MSConst.GetUrlInfoByServiceName,
                serviceName,
                MicroServiceRoute.Port);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(MSConst.contractKey, MicroServiceRoute.ServiceManager.ContractKey);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(MSConst.GetUrlInfoByServiceName_serviceName, serviceName);
            data.Add(MSConst.GetUrlInfoByServiceName_callerPort, MicroServiceRoute.Port);

            SvrAPIOption option = null;
            IHttpHelper httpHelper = new HttpHelper();
            httpHelper.SendData(msUrl, headers, data, true, (resultObj, err) =>
            {
                if (null == resultObj) return;
                string dataStr = ExtMethod.GetCollectionData(resultObj.ToString());
                option = ExtMethod.JsonToEntity<SvrAPIOption>(dataStr);
                if (null == option) return;
                if (string.IsNullOrEmpty(option.IP)) option = null;
            });

            if (null == option) return;
            if (false == string.IsNullOrEmpty(option.HttpType)) httpStr = option.HttpType;

            if (string.IsNullOrEmpty(httpStr))
            {
                url = "http://{0}:{1}{2}".ExtFormat(option.IP, option.Port, areaName);
            }
            else
            {
                url = "{0}://{1}:{2}{3}".ExtFormat(httpStr, option.IP, option.Port, areaName);
            }
            contractKey = option.ContractKey;
            SvrUrlContractKey(serviceName, url, contractKey);
        }

        private static Dictionary<string, string[]> ucDic = new Dictionary<string, string[]>();
        private static object SvrUrlContractKeyLock = new object();
        private string[] SvrUrlContractKey(string serviceName, string url, string contractKey)
        {
            lock (SvrUrlContractKeyLock)
            {
                string[] ucArr = null;
                if (string.IsNullOrEmpty(serviceName)) return ucArr;
                string key = serviceName.ToLower();
                if ((false == string.IsNullOrEmpty(url)) && (false == string.IsNullOrEmpty(contractKey)))
                {
                    if (!ucDic.ContainsKey(key))
                    {
                        ucDic.Add(key, new string[] { url, contractKey });
                    }
                }
                ucDic.TryGetValue(key, out ucArr);
                return ucArr;
            }
        }
    }
}
