using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.MicroService
{
    public class MicroServiceMethodImpl : IMicroServiceMethod
    {
        Type IMicroServiceMethod.GetMS(IInstanceCodeCompiler codeCompiler, AutoCall autoCall, MicroServiceRoute microServiceRoute, Type interfaceType)
        {            
            string usingList = "";
            string methodList = "";
            string methodCode = "";
            string tokenCode = "";
            string sign = ", ";
            string paraList = "";
            string data = "";
            string s = "";
            string err = "";
            string interfaceName = interfaceType.TypeToString(true);
            string returnType = "";
            string code = "{#usingList}";
            string namespaceStr = "System.DJ.MicroService." + TempImpl.dirName + "." + TempImpl.libName;
            EList<CKeyValue> elist = new EList<CKeyValue>();
            elist.Add(new CKeyValue() { Key = "System.DJ.MicroService" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            elist.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            elist.Add(new CKeyValue() { Key = "System" });
            elist.Add(new CKeyValue() { Key = "Newtonsoft.Json" });

            string clssName = interfaceType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "_");
            string clssPath = namespaceStr + "." + clssName;
            MethodInformation mi = new MethodInformation();
            mi.append(ref code, LeftSpaceLevel.one, "namespace {0}", namespaceStr);
            mi.append(ref code, "{");
            mi.append(ref code, LeftSpaceLevel.two, "public class {0}: {1}", clssName, DJTools.GetClassName(interfaceType, true));
            mi.append(ref code, LeftSpaceLevel.two, "{");
            mi.append(ref code, LeftSpaceLevel.three, "private string _routeName = \"{0}\";", microServiceRoute.RouteName);
            mi.append(ref code, LeftSpaceLevel.three, "private string _uri = \"\";");
            mi.append(ref code, "");
            mi.append(ref code, "{#structorMethod}");
            mi.append(ref code, "");
            mi.append(ref code, "{#methodList}");
            mi.append(ref code, LeftSpaceLevel.two, "}");
            mi.append(ref code, "}");

            string structorMethod = "";
            mi.append(ref structorMethod, LeftSpaceLevel.three, "public {0}()", clssName);
            mi.append(ref structorMethod, LeftSpaceLevel.three, "{");
            mi.append(ref structorMethod, LeftSpaceLevel.four, "MicroServiceRoute msr = new MicroServiceRoute(_routeName);");
            mi.append(ref structorMethod, LeftSpaceLevel.four, "_uri = msr.Uri;");
            mi.append(ref structorMethod, LeftSpaceLevel.three, "}");
            code = code.Replace("{#structorMethod}", structorMethod);

            Regex rg = new Regex(@"\`[0-9]+\[");

            //{tokenCode: null, data: null}
            MethodInfo[] ms = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo miItem in ms)
            {
                paraList = "";
                data = "";
                err = "";
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
                mi.append(ref s, LeftSpaceLevel.four, "object datas = new { tokenCode = \"" + tokenCode + "\", data = " + data + "  };");
                mi.append(ref s, LeftSpaceLevel.four, "string jsonData = JsonConvert.SerializeObject(datas);");
                mi.append(ref s, LeftSpaceLevel.four, "string responseResult = \"\";");
                mi.append(ref s, LeftSpaceLevel.one, "");
                mi.append(ref s, LeftSpaceLevel.four, "HttpHelper httpHelper = new HttpHelper();");
                mi.append(ref s, LeftSpaceLevel.four, "httpHelper.SendData(_uri + \"/" + miItem.Name + "\", jsonData, (responseData, err) =>");
                mi.append(ref s, LeftSpaceLevel.four, "{");
                mi.append(ref s, LeftSpaceLevel.five, "responseResult = responseData;");
                if (!string.IsNullOrEmpty(err))
                {
                    mi.append(ref s, LeftSpaceLevel.five, "{0} = err;", err);
                }
                mi.append(ref s, LeftSpaceLevel.four, "});");

                if (typeof(void) != miItem.ReturnType)
                {
                    returnType = miItem.ReturnType.TypeToString(true);
                    if (typeof(Guid) == miItem.ReturnType)
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.Guid guid = Guid.Empty;");
                        mi.append(ref s, LeftSpaceLevel.four, "Guid.TryParse(responseResult, out guid);");
                        mi.append(ref s, LeftSpaceLevel.four, "return guid;");
                    }
                    else if(typeof(DateTime) == miItem.ReturnType)
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.DateTime dateTime = System.DateTime.Now;");
                        mi.append(ref s, LeftSpaceLevel.four, "System.DateTime.TryParse(responseResult, out dateTime);");
                        mi.append(ref s, LeftSpaceLevel.four, "return dateTime;");
                    }
                    else if(null != miItem.ReturnType.GetInterface("System.Collections.IEnumerable"))
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "System.Collections.IEnumerable list = responseResult.JsonToList<{0}>();", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "return ({0})list;", returnType);
                    }
                    else if (DJTools.IsBaseType(miItem.ReturnType))
                    {
                        mi.append(ref s, LeftSpaceLevel.four, "if(typeof(string) == typeof({0})) return responseResult;", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "if(string.IsNullOrEmpty(responseResult)) return default({0});", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "Object _vObj = DJTools.ConvertTo(responseResult, {0});", returnType);
                        mi.append(ref s, LeftSpaceLevel.four, "return ({0})_vObj;");
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
            codeCompiler.SavePathOfDll = Path.Combine(binDirPath, clssName + ".dll");

            err = "";
            Type type = null;
            string[] dllArr = new string[] { "System.dll", "System.Xml.dll" };
            Assembly assObj = codeCompiler.TranslateCode(dllArr, "v4.0", code, ref err);
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
        }

    }
}
