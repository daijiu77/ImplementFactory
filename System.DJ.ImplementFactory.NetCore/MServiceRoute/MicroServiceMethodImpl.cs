using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.DJ.ImplementFactory.MServiceRoute.Attrs.MicroServiceRoute;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MicroServiceMethodImpl : IMicroServiceMethod
    {
        Type IMicroServiceMethod.GetMS(IInstanceCodeCompiler instanceCodeCompiler, AutoCall autoCall, MicroServiceRoute microServiceRoute, Type interfaceType)
        {
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
            string interfaceName = interfaceType.TypeToString(true);
            string returnType = "";
            string code = "{#usingList}";
            string namespaceStr = "System.DJ.MicroService." + TempImplCode.dirName + "." + TempImplCode.libName;
            string controllerName = interfaceType.Name;
            string actionName = "";

            const string taskRunStartTag = "[_Task.Run-Start_]";
            const string taskRunEndTag = "[_Task.Run-End_]";

            if (!string.IsNullOrEmpty(microServiceRoute.ControllerName)) controllerName = microServiceRoute.ControllerName;

            EList<CKeyValue> elist = new EList<CKeyValue>();
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            elist.Add(new CKeyValue() { Key = "System" });
            elist.Add(new CKeyValue() { Key = "System.Collections.Generic" });
            elist.Add(new CKeyValue() { Key = "Newtonsoft.Json" });
            elist.Add(new CKeyValue() { Key = typeof(ExtMethod).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(MSDataVisitor).Namespace });
            elist.Add(new CKeyValue() { Key = typeof(MethodTypes).Namespace });

            string clssName = interfaceType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "_");
            string clssPath = namespaceStr + "." + clssName;
            MethodInformation mInfo = new MethodInformation();
            mInfo.append(ref code, LeftSpaceLevel.one, "namespace {0}", namespaceStr);
            mInfo.append(ref code, "{");
            mInfo.append(ref code, LeftSpaceLevel.two, "public class {0}: ImplementAdapter, {1}", clssName, DJTools.GetClassName(interfaceType, true));
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
                }
                data = "new { " + data + " }";

                s = "";
                mInfo.append(ref s, LeftSpaceLevel.four, "string responseResult = \"\";");

                RouteAttr routeAttr = MicroServiceRoute.GetRouteAttributeByName(microServiceRoute.RouteName);
                if (null != routeAttr) contractKey = routeAttr.ContractKey;
                if ((typeof(void) != eMethod.ReturnType) && (false == string.IsNullOrEmpty(contractKey)))
                {
                    if (string.IsNullOrEmpty(actionName)) { actionName = miItem.Name; }
                    mInfo.append(ref s, LeftSpaceLevel.four, "MethodTypes methodTypes = MethodTypes.Post;");
                    if (null != requestMapping)
                    {
                        if (MethodTypes.Get == requestMapping.MethodType)
                        {
                            mInfo.append(ref s, LeftSpaceLevel.four, "methodTypes = MethodTypes.Get;");
                        }
                    }

                    mInfo.append(ref s, LeftSpaceLevel.four, "MSDataVisitor dataVisitor = new MSDataVisitor();");
                    mInfo.append(ref s, LeftSpaceLevel.four, "responseResult = dataVisitor.GetResult(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", methodTypes, {5});",
                        microServiceRoute.RouteName, microServiceRoute.Uri, controllerName, actionName, contractKey, data);
                    mInfo.append(ref s, LeftSpaceLevel.four, "");
                    mInfo.append(ref s, LeftSpaceLevel.four, "if(null == responseResult) responseResult = \"\";");
                    mInfo.append(ref s, LeftSpaceLevel.one, "");
                    returnType = eMethod.ReturnType.TypeToString(true);
                    if (typeof(Guid) == eMethod.ReturnType)
                    {
                        mInfo.append(ref s, LeftSpaceLevel.four, "System.Guid guid = Guid.Empty;");
                        mInfo.append(ref s, LeftSpaceLevel.four, "Guid.TryParse(responseResult, out guid);");
                        mInfo.append(ref s, LeftSpaceLevel.four, "return guid;");
                    }
                    else if (typeof(DateTime) == eMethod.ReturnType)
                    {
                        mInfo.append(ref s, LeftSpaceLevel.four, "System.DateTime dateTime = System.DateTime.Now;");
                        mInfo.append(ref s, LeftSpaceLevel.four, "System.DateTime.TryParse(responseResult, out dateTime);");
                        mInfo.append(ref s, LeftSpaceLevel.four, "return dateTime;");
                    }
                    else if (DJTools.IsBaseType(eMethod.ReturnType))
                    {
                        mInfo.append(ref s, LeftSpaceLevel.four, "if(typeof(string) == typeof({0})) return ({0})DJTools.ConvertTo(responseResult, typeof({0}));", returnType);
                        mInfo.append(ref s, LeftSpaceLevel.four, "if(string.IsNullOrEmpty(responseResult)) return default({0});", returnType);
                        mInfo.append(ref s, LeftSpaceLevel.four, "Object _vObj = DJTools.ConvertTo(responseResult, typeof({0}));", returnType);
                        mInfo.append(ref s, LeftSpaceLevel.four, "return ({0})_vObj;", returnType);
                    }
                    else if (null != eMethod.ReturnType.GetInterface("System.Collections.IEnumerable"))
                    {
                        string dtType = returnType;
                        if (typeof(IList).IsAssignableFrom(eMethod.ReturnType))
                        {
                            Type tp = eMethod.ReturnType.GetGenericArguments()[0];
                            dtType = tp.TypeToString(true);
                        }
                        else if (eMethod.ReturnType.IsArray)
                        {
                            dtType = eMethod.ReturnType.TypeToString(true);
                        }
                        mInfo.append(ref s, LeftSpaceLevel.four, "System.Collections.IEnumerable list = responseResult.JsonToList<{0}>();", dtType);
                        mInfo.append(ref s, LeftSpaceLevel.four, "return ({0})list;", returnType);
                    }
                    else if ((typeof(void) != eMethod.ReturnType)
                        && eMethod.ReturnType.IsClass
                        && (false == eMethod.ReturnType.IsInterface)
                        && (false == eMethod.ReturnType.IsAbstract)
                        && (false == eMethod.ReturnType.IsBaseType()))
                    {
                        mInfo.append(ref s, LeftSpaceLevel.four, "{0} vObj = responseResult.JsonToEntity<{0}>();", returnType);
                        mInfo.append(ref s, LeftSpaceLevel.four, "return vObj;");
                    }
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
                clssPath = Path.Combine(DJTools.RootPath, TempImplCode.dirName);
                clssPath = Path.Combine(clssPath, clssName + ".cs");
                File.WriteAllText(clssPath, txt);
            }

            return type;
            //throw new NotImplementedException();
        }
    }
}
