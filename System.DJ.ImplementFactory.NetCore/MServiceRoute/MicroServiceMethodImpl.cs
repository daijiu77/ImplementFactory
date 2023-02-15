using Newtonsoft.Json.Linq;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class MicroServiceMethodImpl : IMicroServiceMethod
    {
        Type IMicroServiceMethod.GetMS(IInstanceCodeCompiler instanceCodeCompiler, AutoCall autoCall, MicroServiceRoute microServiceRoute, Type interfaceType)
        {
            string usingList = "";
            string methodList = "";
            string methodCode = "";
            string sign = ", ";
            string paraList = "";
            string data = "";
            string s = "";
            string err = "";
            string interfaceName = interfaceType.TypeToString(true);
            string returnType = "";
            string code = "{#usingList}";
            string namespaceStr = "System.DJ.MicroService." + TempImpl.dirName + "." + TempImpl.libName;
            string controllerName = interfaceType.Name;
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
            elist.Add(new CKeyValue() { Key = typeof(JToken).Namespace });

            string clssName = interfaceType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "_");
            string clssPath = namespaceStr + "." + clssName;
            MethodInformation mi = new MethodInformation();
            mi.append(ref code, LeftSpaceLevel.one, "namespace {0}", namespaceStr);
            mi.append(ref code, "{");
            mi.append(ref code, LeftSpaceLevel.two, "public class {0}: ImplementAdapter, {1}", clssName, DJTools.GetClassName(interfaceType, true));
            mi.append(ref code, LeftSpaceLevel.two, "{");
            mi.append(ref code, "");

            mi.append(ref code, "{#structorMethod}");
            mi.append(ref code, "");
            mi.append(ref code, "{#methodList}");
            mi.append(ref code, LeftSpaceLevel.two, "}");
            mi.append(ref code, "}");

            string structorMethod = "";
            mi.append(ref structorMethod, LeftSpaceLevel.three, "public {0}()", clssName);
            mi.append(ref structorMethod, LeftSpaceLevel.three, "{");
            //mi.append(ref structorMethod, LeftSpaceLevel.four, "");
            mi.append(ref structorMethod, LeftSpaceLevel.three, "}");
            code = code.Replace("{#structorMethod}", structorMethod);

            Regex rg = new Regex(@"\`[0-9]+\[");
            Attribute attr = null;
            RequestMapping requestMapping = null;
            string actionName = "";
            MethodInfo[] ms = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo miItem in ms)
            {
                paraList = "";
                data = "";
                err = "";
                actionName = "";
                requestMapping = null;
                attr = miItem.GetCustomAttribute(typeof(RequestMapping));
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
                s = typeof(void) == miItem.ReturnType ? "void" : miItem.ReturnType.TypeToString(true);
                s += " " + interfaceName + "." + miItem.Name + "(" + paraList + ")";
                mi.append(ref methodCode, LeftSpaceLevel.three, s);
                mi.append(ref methodCode, LeftSpaceLevel.three, "{");

                if (!string.IsNullOrEmpty(data))
                {
                    data = data.Substring(sign.Length);
                }
                data = "new { " + data + " }";

                s = "";
                mi.append(ref s, LeftSpaceLevel.four, "string responseResult = \"\";");

                if (typeof(void) != miItem.ReturnType)
                {
                    if (string.IsNullOrEmpty(actionName)) { actionName = miItem.Name; }
                    mi.append(ref s, LeftSpaceLevel.four, "MethodTypes methodTypes = MethodTypes.Post;");
                    if (null != requestMapping)
                    {
                        if (MethodTypes.Get == requestMapping.MethodType)
                        {
                            mi.append(ref s, LeftSpaceLevel.four, "methodTypes = MethodTypes.Get;");
                        }
                    }
                    mi.append(ref s, LeftSpaceLevel.four, "MSDataVisitor dataVisitor = new MSDataVisitor();");
                    mi.append(ref s, LeftSpaceLevel.four, "responseResult = dataVisitor.GetResult(\"{0}\", \"{1}\", \"{2}\", \"{3}\", methodTypes, {4});",
                        microServiceRoute.RouteName, microServiceRoute.Uri, controllerName, actionName, data);
                    mi.append(ref s, LeftSpaceLevel.four, "");
                    mi.append(ref s, LeftSpaceLevel.four, "if(null == responseResult) responseResult = \"\";");
                    mi.append(ref s, LeftSpaceLevel.one, "");
                    returnType = miItem.ReturnType.TypeToString(true);
                    if (typeof(Guid) == miItem.ReturnType)
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.Guid guid = Guid.Empty;");
                        mi.append(ref s, LeftSpaceLevel.four, "Guid.TryParse(responseResult, out guid);");
                        mi.append(ref s, LeftSpaceLevel.four, "return guid;");
                    }
                    else if (typeof(DateTime) == miItem.ReturnType)
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.DateTime dateTime = System.DateTime.Now;");
                        mi.append(ref s, LeftSpaceLevel.four, "System.DateTime.TryParse(responseResult, out dateTime);");
                        mi.append(ref s, LeftSpaceLevel.four, "return dateTime;");
                    }
                    else if (DJTools.IsBaseType(miItem.ReturnType))
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "if(typeof(string) == typeof({0})) return responseResult;", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "if(string.IsNullOrEmpty(responseResult)) return default({0});", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "Object _vObj = DJTools.ConvertTo(responseResult, typeof({0}));", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "return ({0})_vObj;", returnType);
                    }
                    else if (null != miItem.ReturnType.GetInterface("System.Collections.IEnumerable"))
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.Collections.IEnumerable list = responseResult.JsonToList<{0}>();", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "return ({0})list;", returnType);
                    }
                    else
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "{0} vObj = responseResult.JsonToEntity<{0}>();", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "return vObj;");
                    }
                }

                mi.append(ref methodCode, s);
                mi.append(ref methodCode, LeftSpaceLevel.three, "}");

                if (string.IsNullOrEmpty(methodList))
                {
                    methodList = methodCode;
                }
                else
                {
                    mi.append(ref methodList, "");
                    mi.append(ref methodList, methodCode);
                }
            }

            usingList = "";
            foreach (CKeyValue item in elist)
            {
                usingList += "using " + item.Key + ";\r\n";
            }

            code = code.Replace("{#usingList}", usingList);
            code = code.Replace("{#methodList}", methodList);

            string binDirPath = Path.Combine(DJTools.RootPath, TempImpl.dirName);
            binDirPath = Path.Combine(binDirPath, TempImpl.libName);
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
                clssPath = Path.Combine(DJTools.RootPath, TempImpl.dirName);
                clssPath = Path.Combine(clssPath, clssName + ".cs");
                File.WriteAllText(clssPath, txt);
            }

            return type;
            //throw new NotImplementedException();
        }
    }
}
