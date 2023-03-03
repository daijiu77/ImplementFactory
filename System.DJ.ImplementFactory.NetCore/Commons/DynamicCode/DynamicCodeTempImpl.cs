using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DCache;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    class DynamicCodeTempImpl
    {
        public static string InterfaceInstanceType { get { return "Get_InterfaceInstanceType__"; } }

        static Random random = new Random();
        string dirName = "";
        string PublicAutoCall = "{PublicAutoCall$}";

        public DynamicCodeTempImpl(string dirName)
        {
            this.dirName = dirName;
        }

        string GetInterfaceNames(List<Type> typeList, EList<CKeyValue> uskv, Type interfaceType, Type implementType)
        {
            string interfaceName = "";
            Type[] types = null;
            if (null == implementType)
            {
                typeList.Add(interfaceType);
                types = interfaceType.GetInterfaces();
                foreach (var item in types)
                {
                    typeList.Add(item);
                }
            }
            else
            {
                types = implementType.GetInterfaces();
                Type[] tps = null;
                Type type1 = null;
                Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
                foreach (var item in types)
                {
                    tps = item.GetInterfaces();
                    foreach (var item1 in tps)
                    {
                        type1 = null;
                        dic.TryGetValue(item, out type1);
                        if (null == type1) dic.Add(item1, item1);
                    }
                }

                interfaceName = "";
                int n1 = 0;
                foreach (var item in types)
                {
                    n1 = 0;
                    type1 = null;
                    dic.TryGetValue(item, out type1);
                    if (null != type1) n1 = 1;

                    if (0 == n1)
                    {
                        uskv.Add(new CKeyValue() { Key = item.Namespace });
                        interfaceName += ", " + DJTools.GetClassName(item, false);
                    }
                    typeList.Add(item);
                }

                if (!string.IsNullOrEmpty(interfaceName)) interfaceName = interfaceName.Substring(2);
            }
            return interfaceName;
        }

        string GetAutoCallNameBy(AutoCall dbAutoCall)
        {
            string autoName = "";
            if (null != dbAutoCall as AutoSelect)
            {
                autoName = "AutoSelect";
            }
            else if (null != dbAutoCall as AutoInsert)
            {
                autoName = "AutoInsert";
            }
            else if (null != dbAutoCall as AutoUpdate)
            {
                autoName = "AutoUpdate";
            }
            else if (null != dbAutoCall as AutoDelete)
            {
                autoName = "AutoDelete";
            }
            else if (null != dbAutoCall as AutoCount)
            {
                autoName = "AutoCount";
            }
            else if (null != dbAutoCall as AutoProcedure)
            {
                autoName = "AutoProcedure";
            }
            return autoName;
        }

        AutoCall getAutoCall(MethodInfo m, ref bool isDataOpt, ref AbsDataInterface absData, ref string methodAttr, ref bool isAsync, ref int msInterval, ref bool EnabledBuffer)
        {
            AutoCall autoCall = null;
            object[] arr = m.GetCustomAttributes(typeof(AutoCall), true);
            if (null == arr) return autoCall;

            if (0 < arr.Length)
            {
                string s = "";
                IsDataInterface = true;
                autoCall = AutoCall.GetDataOptAutoCall(arr);
                absData = autoCall as AbsDataInterface;
                if (null == absData)
                {
                    s = "The attribute '" + autoCall.GetType().Name + "' cannot be used on the method '" + m.Name + "'.";
                    autoCall.e(s);
                    throw new Exception(s);
                }

                if (false == isDataOpt)
                {
                    if ((null != autoCall as AutoSelect) ||
                        (null != autoCall as AutoInsert) ||
                        (null != autoCall as AutoUpdate) ||
                        (null != autoCall as AutoDelete) ||
                        (null != autoCall as AutoCount) ||
                        (null != autoCall as AutoProcedure))
                    {
                        isDataOpt = true;
                        methodAttr = "[" + GetAutoCallNameBy(autoCall) + "(";
                        if (null != autoCall as AutoSelect)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "selectExpression: \"" + absData.sql + "\"";
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\", " +
                                "dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }
                            if (null != absData.ResultExecMethod) methodAttr += ", ResultExecMethod: " + absData.ResultExecMethod.toArrayString();
                        }
                        else if (null != autoCall as AutoInsert)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "insertExpression: \"" + absData.sql + "\"," +
                                " fields: " + absData.fields.toArrayString() + "," +
                                " fieldsType: FieldsType." + Enum.GetName(typeof(FieldsType), absData.fieldsType);
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\"," +
                                " dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }

                            if (absData.EnabledBuffer)
                            {
                                methodAttr += ", EnabledBuffer: true";
                            }
                            else
                            {
                                methodAttr += ", sqlExecType: DataOptType." + Enum.GetName(typeof(DataOptType), absData.sqlExecType);
                            }
                        }
                        else if (null != autoCall as AutoUpdate)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "updateExpression: \"" + absData.sql + "\"," +
                                " fields: " + absData.fields.toArrayString() + "," +
                                " fieldsType: FieldsType." + Enum.GetName(typeof(FieldsType), absData.fieldsType);
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\"," +
                                " dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }

                            if (absData.EnabledBuffer)
                            {
                                methodAttr += ", EnabledBuffer: true";
                            }
                            else
                            {
                                methodAttr += ", sqlExecType: DataOptType." + Enum.GetName(typeof(DataOptType), absData.sqlExecType);
                            }
                        }
                        else if (null != autoCall as AutoDelete)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "deleteExpression: \"" + absData.sql + "\"";
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\"," +
                                " dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }

                            if (absData.EnabledBuffer)
                            {
                                methodAttr += ", EnabledBuffer: true";
                            }
                            else
                            {
                                methodAttr += ", sqlExecType: DataOptType." + Enum.GetName(typeof(DataOptType), absData.sqlExecType);
                            }
                        }
                        else if (null != autoCall as AutoCount)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "countExpression: \"" + absData.sql + "\"";
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\", " +
                                "dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }
                            if (null != absData.ResultExecMethod) methodAttr += ", ResultExecMethod: " + absData.ResultExecMethod.toArrayString();
                        }
                        else if (null != autoCall as AutoProcedure)
                        {
                            if (!string.IsNullOrEmpty(absData.sql))
                            {
                                methodAttr += "sqlExpression: \"" + absData.sql + "\"," +
                                " fields: " + absData.fields.toArrayString() + "," +
                                " fieldsType: FieldsType." + Enum.GetName(typeof(FieldsType), absData.fieldsType);
                            }
                            else
                            {
                                methodAttr += "dataProviderNamespace: \"" + absData.dataProviderNamespace + "\", " +
                                "dataProviderClassName: \"" + absData.dataProviderClassName + "\"";
                            }
                        }

                        isAsync = absData.IsAsync;
                        if (absData.IsAsync)
                        {
                            methodAttr += ", isAsync: true";
                        }

                        if (0 < absData.MsInterval)
                        {
                            msInterval = absData.MsInterval;
                            methodAttr += ", msInterval: " + absData.MsInterval;
                        }

                        methodAttr += ")]";
                    }
                }

                FieldInfo field = ((AbsDataInterface)autoCall).GetType().GetField("EnabledBuffer", BindingFlags.Public | BindingFlags.Instance);
                if (null != field)
                {
                    object v = field.GetValue(autoCall);
                    v = null == v ? true : v;
                    bool.TryParse(v.ToString().ToLower(), out EnabledBuffer);
                }
            }

            return autoCall;
        }

        string getParaListStr(MethodInfo m, MethodInformation mInfo, EList<CKeyValue> uskv, PList<Para> paraList, string paraListVarName, ref Type actionType,
            ref string actionParaName, ref string methodName, ref string paraStr, ref string lists, ref bool isDynamicEntity, ref string defaultV, ref string outParas,
            ref string autoCallPara, ref string autoCallParaName)
        {
            string plist = "";
            string s = "";
            ParameterInfo[] paras = m.GetParameters();
            Regex rg = new Regex(@"\`[0-9]+\[");
            Regex rg1 = new Regex(@"(?<TypeFull>^[a-z][^\`]+)\`[0-9]", RegexOptions.IgnoreCase);

            foreach (ParameterInfo p in paras)
            {
                if (true == p.ParameterType.IsGenericParameter)
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });

                    //泛型情况
                    methodName = m.Name;
                    s = p.ParameterType.TypeToString();
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1},IsGenericParameter=true});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;
                }
                else if (typeof(DataEntity<DataElement>) == p.ParameterType)
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });
                    isDynamicEntity = true;
                    s = "DataEntity<DataElement>";
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;
                    break;
                }
                else if (typeof(List<DataEntity<DataElement>>) == p.ParameterType || typeof(IList<DataEntity<DataElement>>) == p.ParameterType)
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });
                    isDynamicEntity = true;
                    s = "List<DataEntity<DataElement>>";
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;
                    break;
                }
                else if (p.ParameterType.BaseType == typeof(System.MulticastDelegate) && rg.IsMatch(p.ParameterType.ToString()))
                {
                    //Action<>, Func<> 情况
                    s = p.ParameterType.TypeToString(true);
                    paraStr += "," + s + " " + p.Name;
                    actionParaName = p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;

                    Type[] types = p.ParameterType.GetGenericArguments();
                    foreach (Type item in types)
                    {
                        actionType = item;
                        uskv.Add(new CKeyValue() { Key = item.Namespace });
                    }
                }
                else if (-1 != p.ParameterType.FullName.IndexOf("&"))
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });

                    //out, ref 情况
                    s = p.ParameterType.TypeToString(true);
                    s = s.Replace("+", ".");
                    if (p.IsOut)
                    {
                        paraStr += ",out " + s + " " + p.Name;
                        plist += ",out " + p.Name;
                        defaultV = DJTools.getDefaultByType(p.ParameterType);
                        mInfo.append(ref outParas, LeftSpaceLevel.four, "{0} = {1};", p.Name, defaultV);
                    }
                    else
                    {
                        paraStr += ",ref " + s + " " + p.Name;
                        plist += ",ref " + p.Name;
                    }
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                }
                else if (p.ParameterType.GetInterface("IEnumerable") == typeof(IEnumerable) && typeof(string) != p.ParameterType)
                {
                    s = p.ParameterType.TypeToString(true);

                    s = s.Replace("+", ".");
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;
                }
                else
                {
                    //普通参数类型情况
                    s = p.ParameterType.FullName;
                    s = s.Replace("+", ".");
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
                    plist += "," + p.Name;

                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });

                    if (typeof(AutoCall) == p.ParameterType || p.ParameterType.IsSubclassOf(typeof(AutoCall)))
                    {
                        autoCallParaName = p.Name;
                        mInfo.append(ref autoCallPara, LeftSpaceLevel.four, "if(null == {0})", p.Name);
                        mInfo.append(ref autoCallPara, LeftSpaceLevel.four, "{");
                        mInfo.append(ref autoCallPara, LeftSpaceLevel.five, "{0} = this.{1} as {2};", p.Name, PublicAutoCall, p.ParameterType.FullName);
                        mInfo.append(ref autoCallPara, LeftSpaceLevel.four, "}");
                    }
                }
                paraList.Add(new Para(Guid.NewGuid()) { ParaName = p.Name, ParaType = p.ParameterType, ParaTypeName = s });
            }

            return plist;
        }

        class DynamicEntityMInfoPara
        {
            public AutoCall autoCall { get; set; }
            public MethodInformation mInfo { get; set; }
            public EList<CKeyValue> uskv { get; set; }
            public Type implementType { get; set; }
            public Type objType { get; set; }
            public Type actionType { get; set; }
            public bool isAsync { get; set; }
            public bool EnabledBuffer { get; set; }
            public bool isNotInheritInterface { get; set; }
            public int msInterval { get; set; }
            public string autocall_name { get; set; }
            public string paraListVarName { get; set; }
            public string genericity { get; set; }
            public string interfaceName { get; set; }
            public string impl_name { get; set; }
            public string methodName { get; set; }
            public string plist { get; set; }
            public string actionParaName { get; set; }
            public string autoCallPara { get; set; }
            public string autoCallParaName { get; set; }
        }

        void dynamicEntityMInfo(MethodInfo m, DynamicEntityMInfoPara dynamicEntityMInfoPara, ref string code, ref string return_type)
        {
            if (!string.IsNullOrEmpty(dynamicEntityMInfoPara.autoCallPara))
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.one, "");
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.one, dynamicEntityMInfoPara.autoCallPara);
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.one, "");
            }

            dynamicEntityMInfoPara.uskv.Add(new CKeyValue() { Key = typeof(MethodInformation).Namespace });
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "MethodInformation method_info = new MethodInformation();");
            if (null != dynamicEntityMInfoPara.implementType)
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ofInstanceType = typeof({0});", dynamicEntityMInfoPara.implementType.FullName);
            }

            if (string.IsNullOrEmpty(dynamicEntityMInfoPara.autoCallParaName)) dynamicEntityMInfoPara.autoCallParaName = "null";
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ofInterfaceType = typeof({0});", dynamicEntityMInfoPara.objType.FullName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.paraList = paraList;");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.StartSpace = \"{0}\";", dynamicEntityMInfoPara.mInfo.getSpace(4));
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.AutoCallVarName = \"{0}\";", dynamicEntityMInfoPara.autocall_name);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ParaListVarName = \"{0}\";", dynamicEntityMInfoPara.paraListVarName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.AutoCall = {0};", dynamicEntityMInfoPara.autocall_name);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.autoCallParaValue = {0};", dynamicEntityMInfoPara.autoCallParaName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "StackTrace trace = new StackTrace();");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "StackFrame stackFrame = trace.GetFrame(0);");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodInfo = (MethodInfo)stackFrame.GetMethod();");
            dynamicEntityMInfoPara.uskv.Add(new CKeyValue() { Key = typeof(StackTrace).Namespace });
            dynamicEntityMInfoPara.uskv.Add(new CKeyValue() { Key = typeof(MethodBase).Namespace });
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.InstanceVariantName = \"{0}\";", dynamicEntityMInfoPara.impl_name);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.InterfaceMethodName = \"{0}\";", dynamicEntityMInfoPara.methodName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.MethodParas = \"{0}\";", dynamicEntityMInfoPara.plist);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ActionParaName = \"{0}\";", dynamicEntityMInfoPara.actionParaName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ActionType = {0};", null == dynamicEntityMInfoPara.actionType ? "null" : dynamicEntityMInfoPara.actionType.TypeToString());
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.IsAsync = {0};", dynamicEntityMInfoPara.isAsync.ToString().ToLower());
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.Interval = {0};", dynamicEntityMInfoPara.msInterval.ToString());
            if (null != dynamicEntityMInfoPara.actionType && null != dynamicEntityMInfoPara.autoCall)
            {
                return_type = dynamicEntityMInfoPara.actionType.TypeToString();
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultVariantName = \"result\";");
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultTypeName = \"{0}\";", return_type);
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultType = {0};", return_type);
            }
            else
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultVariantName = \"{0}\";", m.ReturnType != typeof(void) ? "result" : "");
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultTypeName = \"{0}\";", return_type);
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.ResultType = method_info.methodInfo.ReturnType;");
            }
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.GenericityParas = \"{0}\";", dynamicEntityMInfoPara.genericity);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.EnabledBuffer = {0};", dynamicEntityMInfoPara.EnabledBuffer.ToString().ToLower());

            if (dynamicEntityMInfoPara.isNotInheritInterface)
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.methodComponent.InstanceVariantName = \"base\";");
            }

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "object[] attrArr = method_info.methodInfo.GetCustomAttributes(typeof(AutoCall), true);");
            dynamicEntityMInfoPara.uskv.Add(new CKeyValue() { Key = typeof(AutoCall).Namespace });

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "if(0 < attrArr.Length)");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "{");

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "AutoCall dataAutoCall = AutoCall.GetDataOptAutoCall(attrArr);");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "if(null != dataAutoCall)");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "{");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "");

            string paraVarName = "";
            dynamicEntityMInfoPara.autoCall.ReplaceGenericSign(dynamicEntityMInfoPara.mInfo, LeftSpaceLevel.six, "",
                ((AbsDataInterface)dynamicEntityMInfoPara.autoCall).sql, ref paraVarName, ref code);

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "try");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "{");
            if (null != dynamicEntityMInfoPara.actionType)
            {
                return_type = dynamicEntityMInfoPara.actionType.FullName;
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.seven, "((AbsDataInterface)dataAutoCall).ExecuteInterfaceMethod<{0}>(method_info, {1});", return_type, dynamicEntityMInfoPara.actionParaName);
            }
            else if (m.ReturnType != typeof(void))
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.seven, "result = ((AbsDataInterface)dataAutoCall).ExecuteInterfaceMethod<{0}>(method_info, null);", return_type);
            }
            else
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.seven, "((AbsDataInterface)dataAutoCall).ExecuteInterfaceMethod<object>(method_info, null);");
            }
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "}");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "catch(Exception ex)");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "{");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.seven, "{0}.ExecuteException(typeof({1}),{2},\"{3}\",{4},ex);",
                dynamicEntityMInfoPara.autocall_name, dynamicEntityMInfoPara.interfaceName, dynamicEntityMInfoPara.impl_name, dynamicEntityMInfoPara.methodName, dynamicEntityMInfoPara.paraListVarName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "}");

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "}");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "else");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "{");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.six, "{0}.ExecuteException(typeof({1}),{2},\"{3}\",{4},new Exception(\"You must use 'AutoSelect/AutoInsert/AutoUpdate/AutoDelete' on method.\"));",
                dynamicEntityMInfoPara.autocall_name, dynamicEntityMInfoPara.interfaceName, dynamicEntityMInfoPara.impl_name, dynamicEntityMInfoPara.methodName, dynamicEntityMInfoPara.paraListVarName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "}");

            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "}");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "else");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "{");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.five, "{0}.ExecuteException(typeof({1}),{2},\"{3}\",{4},new Exception(\"You must use 'AutoSelect/AutoInsert/AutoUpdate/AutoDelete' on method.\"));",
                dynamicEntityMInfoPara.autocall_name, dynamicEntityMInfoPara.interfaceName, dynamicEntityMInfoPara.impl_name, dynamicEntityMInfoPara.methodName, dynamicEntityMInfoPara.paraListVarName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "}");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "");
        }

        class NormalEntityPara
        {
            public AutoCall autoCall { get; set; }
            public AutoCall autoCall_Impl { get; set; }
            public Type implementType { get; set; }
            public Type objType { get; set; }
            public Type actionType { get; set; }
            public PList<Para> paraList { get; set; }
            public bool EnabledBuffer { get; set; }
            public bool isNotInheritInterface { get; set; }
            public string autocall_name { get; set; }
            public string paraListVarName { get; set; }
            public string impl_name { get; set; }
            public string interfaceName { get; set; }
            public string methodName { get; set; }
            public string plist { get; set; }
            public bool isAsync { get; set; }
            public int msInterval { get; set; }
            public string actionParaName { get; set; }
            public string return_type { get; set; }
            public string genericity { get; set; }
        }

        void normalEntity(MethodInfo m, NormalEntityPara normalEntityPara, ref string code, ref string err)
        {
            MethodInformation method_info = new MethodInformation();
            method_info.ofInstanceType = normalEntityPara.implementType;
            method_info.ofInterfaceType = normalEntityPara.objType;
            method_info.paraList = normalEntityPara.paraList;
            method_info.StartSpace = method_info.getSpace(4);
            method_info.AutoCallVarName = normalEntityPara.autocall_name;
            method_info.ParaListVarName = normalEntityPara.paraListVarName;
            method_info.methodInfo = m;
            method_info.methodComponent.InstanceVariantName = normalEntityPara.impl_name;
            method_info.methodComponent.InterfaceMethodName = normalEntityPara.methodName;
            method_info.methodComponent.MethodParas = normalEntityPara.plist;
            method_info.methodComponent.IsAsync = normalEntityPara.isAsync;
            method_info.methodComponent.Interval = normalEntityPara.msInterval;
            method_info.methodComponent.ActionParaName = normalEntityPara.actionParaName;
            method_info.methodComponent.ActionType = normalEntityPara.actionType;
            if (null != normalEntityPara.actionType && null != normalEntityPara.autoCall)
            {
                method_info.methodComponent.ResultVariantName = "result";
                method_info.methodComponent.ResultTypeName = normalEntityPara.actionType.TypeToString();
                method_info.methodComponent.ResultType = normalEntityPara.actionType;
            }
            else
            {
                method_info.methodComponent.ResultVariantName = m.ReturnType != typeof(void) ? "result" : "";
                method_info.methodComponent.ResultTypeName = normalEntityPara.return_type;
                method_info.methodComponent.ResultType = m.ReturnType;
            }
            method_info.methodComponent.GenericityParas = normalEntityPara.genericity;
            method_info.methodComponent.EnabledBuffer = normalEntityPara.EnabledBuffer;

            if (normalEntityPara.isNotInheritInterface)
            {
                method_info.methodComponent.InstanceVariantName = "base";
            }

            string defaultV = "";
            err = "";
            if (null != normalEntityPara.autoCall)
            {
                defaultV = normalEntityPara.autoCall.ExecuteInterfaceMethodCodeString(method_info, ref err);
            }
            else
            {
                defaultV = normalEntityPara.autoCall_Impl.ExecuteInterfaceMethodCodeString(method_info, ref err);
            }

            method_info.StartSpace = "";
            method_info.append(ref code, LeftSpaceLevel.four, "try");
            method_info.append(ref code, LeftSpaceLevel.four, "{");
            method_info.append(ref code, LeftSpaceLevel.five, "#region ****** 执行 AutoCall.ExecuteInterfaceMethodCodeString 产生的代码 *******");
            method_info.append(ref code, defaultV);
            method_info.append(ref code, LeftSpaceLevel.five, "#endregion");
            method_info.append(ref code, LeftSpaceLevel.four, "}");
            method_info.append(ref code, LeftSpaceLevel.four, "catch(Exception ex)");
            method_info.append(ref code, LeftSpaceLevel.four, "{");
            method_info.append(ref code, LeftSpaceLevel.five, "{0}.ExecuteException(typeof({1}),{2},\"{3}\",{4},ex);",
                normalEntityPara.autocall_name, normalEntityPara.interfaceName, normalEntityPara.impl_name, normalEntityPara.methodName, normalEntityPara.paraListVarName);
            method_info.append(ref code, LeftSpaceLevel.four, "}");
        }

        /// <summary>
        /// 判断是否是 async Task 方法或 Task 方法，及返回方法值类型
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="isAsyncReturn">Task 方法中是否含有 async 标识</param>
        /// <param name="isTaskReturn">是否是 Task 方法</param>
        /// <param name="return_type">方法返回值类型</param>
        public void JudgeTaskMethod(MethodInfo mi, ref bool isAsyncReturn, ref bool isTaskReturn, ref Type return_type)
        {
            isAsyncReturn = false;
            isTaskReturn = false;
            return_type = null;
            if (0 < mi.CustomAttributes.Count())
            {
                /**
                 * 判断方法是否是 async Task 方法:
                 * public async Task UpdateInfo(Guid Id, CustomerInfo) { }
                 * **/
                #region 判断方法是否是 async Task 方法
                IEnumerable<CustomAttributeData> attrs = mi.CustomAttributes;
                Type attrType = null;
                List<Type> listTypes = new List<Type>();
                listTypes.Add(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute));
                listTypes.Add(typeof(System.Diagnostics.DebuggerStepThroughAttribute));
                int n = 0;
                foreach (CustomAttributeData item in attrs)
                {
                    attrType = item.AttributeType;
                    if (listTypes.Contains(attrType)) n++;
                }

                if (2 == n)
                {
                    isAsyncReturn = true;
                    isTaskReturn = true;
                }
                #endregion
            }

            Type rtnType = mi.ReturnType;
            isTaskReturn = -1 != rtnType.Name.ToLower().IndexOf("task");
            if (isTaskReturn)
            {
                /**
                 * 如果是 Task 方法, 判断 Task 是否带有参数:
                 * public Task<bool> UpdateInfo(Guid Id, CustomerInfo) { }
                 * return_type = bool类型
                 * **/
                Type[] tys = rtnType.GetGenericArguments();
                if (0 < tys.Length) return_type = tys[0];
            }
            else
            {
                return_type = mi.ReturnType;
            }

            if (typeof(void) == return_type) return_type = null;
        }

        public string GetCodeByImpl(Type interfaceType, Type implementType, AutoCall autoCall_Impl, ref string classPath)
        {
            MethodInformation mInfo = new MethodInformation();

            Type[] genericTypes = interfaceType.GetGenericArguments();
            string interfaceNamespace = interfaceType.Namespace;
            string interfaceName = DJTools.GetClassName(interfaceType, true);

            string implNamespace = null, implName = null, implName1 = null, GenericArr = null;

            if (null != implementType)
            {
                implNamespace = implementType.Namespace;
                implName = DJTools.GetClassName(implementType);
                implName1 = implementType.Name;
            }
            else
            {
                implNamespace = interfaceNamespace;
                implName = "dynamicImpl";
                implName1 = implName;
            }

            Type autoCallType = autoCall_Impl.GetType();
            string autocallImplNamespace = autoCallType.Namespace;
            string autocallImplName = DJTools.GetClassName(autoCallType);
            string autocallImplName1 = autoCallType.Name;

            string dt = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + random.Next(10, 99);

            string code = "";
            string References = "#_References_#";
            bool isNotInheritInterface = false, _isDataOpt = false;
            List<Type> typeList = new List<Type>();

            EList<CKeyValue> uskv = new EList<CKeyValue>();
            uskv.Add(new CKeyValue() { Key = "System" });
            uskv.Add(new CKeyValue() { Key = "System.Diagnostics" });
            uskv.Add(new CKeyValue() { Key = "System.Reflection" });
            uskv.Add(new CKeyValue() { Key = "System.Collections.Generic" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Pipelines" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Pipelines.Pojo" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.DynamicCode" });
            uskv.Add(new CKeyValue() { Key = interfaceNamespace });
            uskv.Add(new CKeyValue() { Key = autocallImplNamespace });
            //uskv.Add(new CKeyValue() { Key = "" });

            uskv.Add(new CKeyValue() { Key = typeof(DataCachePool).Namespace });
            uskv.Add(new CKeyValue() { Key = typeof(DJTools).Namespace });
            uskv.Add(new CKeyValue() { Key = typeof(CKeyValue).Namespace });
            uskv.Add(new CKeyValue() { Key = typeof(EList<CKeyValue>).Namespace });

            //append(ref code, "");
            if (null != implementType)
            {
                uskv.Add(new CKeyValue() { Key = implNamespace });
            }

            mInfo.append(ref code, LeftSpaceLevel.one, References);

            mInfo.append(ref code, LeftSpaceLevel.one, null);

            mInfo.append(ref code, LeftSpaceLevel.one, "namespace {0}.{1}", implNamespace, dirName);
            mInfo.append(ref code, LeftSpaceLevel.one, "{"); //namespce start

            classPath = implNamespace + "." + dirName + "." + implName + "_" + dt;
            MethodInfo[] methods1 = null;
            PropertyInfo[] piArr1 = null;
            EventInfo[] eiArr1 = null;

            if (interfaceType.Equals(typeof(IEmplyInterface)))
            {
                isNotInheritInterface = true;
                typeList.Add(implementType);
                interfaceName = DJTools.GetClassName(implementType, true);
            }
            else
            {
                string interfaceN = GetInterfaceNames(typeList, uskv, interfaceType, implementType);
                if (false == string.IsNullOrEmpty(interfaceN)) interfaceName = interfaceN;
            }

            string implInterfacesTag = "[_implInterfacesTag_]";
            string implInterfaces = interfaceName;

            mInfo.append(ref code, LeftSpaceLevel.two, "public class {0}_{1} : {2}", implName, dt, implInterfacesTag);
            mInfo.append(ref code, LeftSpaceLevel.two, "{"); //class start

            string privateVarName = "[_PrivateVarientName_]";
            string impl_name = "";
            string isDisables_Str = "";
            string impl_name1 = implName1 + "_01";
            string autocall_name = autocallImplName1 + "_01";
            if (null != implementType && false == isNotInheritInterface)
            {
                mInfo.append(ref code, LeftSpaceLevel.three, "private {0} {1} = ({0})ImplementAdapter.Register(new {0}());", implName, impl_name1);
                mInfo.append(ref code, LeftSpaceLevel.one, "");
                mInfo.append(ref code, LeftSpaceLevel.three, "public {0} {1} { get { return {2}; } }", DJTools.GetClassName(implementType, true), InterfaceInstanceType, impl_name1);
                mInfo.append(ref code, LeftSpaceLevel.one, "");
                DJTools.append(ref code, privateVarName);
            }
            else
            {
                impl_name1 = "null";
            }
            mInfo.append(ref code, LeftSpaceLevel.three, "private AutoCall {0} = new {1}();", autocall_name, autocallImplName);

            ParameterInfo[] paras = null;
            PList<Para> paraList = new PList<Para>();
            string paraStr = "", lists = "", plist = "", returnType = "", outParas = "";
            string defaultV = "", methodName = "", return_type = "", genericity = "";
            string paraListVarName = "paraList";
            string methodAttr = "", actionParaName = "", err = "", autoCallPara = "", autoCallParaName = "";
            AutoCall autoCall = null;
            bool EnabledBuffer = true;
            bool isDynamicEntity = false;
            bool isAsync = false;
            AbsDataInterface absData = null;
            Type actionType = null;
            DataCache dataCache = null;
            DataCachePool dataCacheImpl = new DataCachePool();
            int msInterval = 0;

            IsDataInterface = false;


            Func<string, MethodInfo, string> funcResultStr = (result_Var, _m) =>
            {
                string sr = "";
                if (_m.ReturnType != typeof(void) && null != absData)
                {
                    //如果接口方法返回类型不为 void
                    if (DataOptType.select != ((IDataOperateAttribute)absData).dataOptType && DataOptType.procedure != ((IDataOperateAttribute)absData).dataOptType)
                    {
                        Type actionType1 = null == actionType ? _m.ReturnType : actionType;
                        if (typeof(bool) == _m.ReturnType && typeof(int) == actionType1)
                        {
                            sr = string.Format("0 < {0}", result_Var);
                        }
                        else if (typeof(int) == _m.ReturnType && typeof(bool) == actionType1)
                        {
                            sr = string.Format("{0} ? 1 : 0", result_Var);
                        }
                        else
                        {
                            sr = result_Var;
                        }
                    }
                    else
                    {
                        sr = result_Var;
                    }
                }
                else
                {
                    sr = result_Var;
                }
                return sr;
            };

            Action<Type, MethodInfo[]> actionFunction = (objType, methods) =>
            {
                foreach (MethodInfo m in methods)
                {
                    if (null == m) continue;

                    if (isNotInheritInterface)
                    {
                        if (false == m.IsVirtual) continue;
                        if (false == m.DeclaringType.Equals(implementType)) continue;
                    }

                    autoCall = null;
                    dataCache = null;

                    EnabledBuffer = false;
                    isDynamicEntity = false;
                    bool isDataOpt = false;
                    isAsync = false;
                    methodAttr = "";
                    msInterval = 0;

                    autoCall = getAutoCall(m, ref isDataOpt, ref absData, ref methodAttr, ref isAsync, ref msInterval, ref EnabledBuffer);
                    if (false == _isDataOpt) _isDataOpt = isDataOpt;

                    methodName = m.Name;
                    paras = m.GetParameters();
                    paraStr = "";
                    lists = "";
                    plist = "";
                    outParas = "";

                    actionType = null;
                    actionParaName = "null";

                    paraList.Clear();

                    return_type = m.ReturnType.TypeToString(true);

                    paraStr = "";
                    lists = "";
                    autoCallPara = "";
                    autoCallParaName = "";
                    plist = getParaListStr(m, mInfo, uskv, paraList, paraListVarName, ref actionType, ref actionParaName, ref methodName, ref paraStr, ref lists,
                        ref isDynamicEntity, ref defaultV, ref outParas, ref autoCallPara, ref autoCallParaName);

                    if (!string.IsNullOrEmpty(paraStr))
                    {
                        paraStr = paraStr.Substring(1);
                    }

                    if (!string.IsNullOrEmpty(plist))
                    {
                        plist = plist.Substring(1);
                    }

                    returnType = m.ReturnType == typeof(void) ? "void" : return_type;
                    genericity = "";
                    if (m.IsGenericMethod)
                    {
                        Type[] gts = m.GetGenericArguments();
                        foreach (Type item in gts)
                        {
                            if (null != item.FullName)
                            {
                                genericity += "," + item.FullName;
                            }
                            else
                            {
                                genericity += "," + item.Name;
                            }
                        }

                        if (!string.IsNullOrEmpty(genericity))
                        {
                            genericity = genericity.Substring(1);
                            genericity = "<" + genericity + ">";
                        }
                    }

                    mInfo.append(ref code, "");
                    if (isNotInheritInterface)
                    {
                        mInfo.append(ref code, LeftSpaceLevel.three, "public override {0} {1}{2}({3})", returnType, methodName, genericity, paraStr);
                    }
                    else
                    {
                        if (isDataOpt) mInfo.append(ref code, LeftSpaceLevel.three, "[Method_Attribute]");
                        mInfo.append(ref code, LeftSpaceLevel.three, "{0} {1}.{2}{3}({4})", returnType, interfaceName, methodName, genericity, paraStr);
                    }
                    mInfo.append(ref code, LeftSpaceLevel.three, "{");//method start

                    if (!string.IsNullOrEmpty(GenericArr))
                        mInfo.append(ref code, LeftSpaceLevel.four, GenericArr);

                    if (!string.IsNullOrEmpty(outParas))
                    {
                        //初始化 out 参数
                        mInfo.append(ref code, outParas);
                        mInfo.append(ref code, "");
                    }

                    EList<CKeyValue> pvList = null;
                    if ((m.ReturnType != typeof(void) || null != actionType))
                    {
                        #region 数据缓存代码实现 DataCache
                        dataCache = DataCache.GetDataCache(m);
                        if (null != dataCache)
                        {
                            mInfo.append(ref methodAttr, LeftSpaceLevel.three, "[DataCache({0}, {1})]", dataCache.CacheTime.ToString(),
                                    dataCache.PersistenceCache.ToString().ToLower());

                            mInfo.append(ref code, LeftSpaceLevel.four, "DataCachePool _dataCachePool = (DataCachePool)Activator.CreateInstance(ImplementAdapter.dataCache);");
                            mInfo.append(ref code, LeftSpaceLevel.four, "string cacheKey = \"null\";");
                            pvList = dataCacheImpl.GetParaNameList(m);
                            if (null != pvList)
                            {
                                //mInfo.append(ref code, LeftSpaceLevel.four, "");
                                mInfo.append(ref code, LeftSpaceLevel.four, "EList<CKeyValue> paraInfoList = new EList<CKeyValue>();");
                                foreach (var item in pvList)
                                {
                                    mInfo.append(ref code, LeftSpaceLevel.four, "paraInfoList.Add(new CKeyValue(){Key = \"{0}\", " +
                                        "Value = {0}, ValueType = typeof({1})});", item.Key, item.ValueType.TypeToString(true));
                                }
                                mInfo.append(ref code, LeftSpaceLevel.four, "");
                                mInfo.append(ref code, LeftSpaceLevel.four, "cacheKey = _dataCachePool.GetParaKey(paraInfoList);");
                            }
                            mInfo.append(ref code, LeftSpaceLevel.four, "object dataCacheVal = ((IDataCache)_dataCachePool).Get(cacheKey);");
                            mInfo.append(ref code, LeftSpaceLevel.four, "");
                        }
                        #endregion

                        string eleT = "";
                        //声明方法返回值变量
                        if (null != actionType && null != autoCall)
                        {
                            uskv.Add(new CKeyValue() { Key = actionType.Namespace });
                            string action_type = actionType.TypeToString(true);

                            if (null != dataCache)
                            {
                                //数据缓存代码实现 DataCache  
                                mInfo.append(ref code, LeftSpaceLevel.four, "if (null != dataCacheVal)");
                                mInfo.append(ref code, LeftSpaceLevel.four, "{");
                                if (typeof(IList).IsAssignableFrom(actionType))
                                {
                                    eleT = actionType.GetGenericArguments()[0].TypeToString(true);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "List<{0}> rtnList = new List<{0}>();", eleT);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "System.Collections.IList _ilist2 = (System.Collections.IList)dataCacheVal;");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "foreach(var listItem in _ilist2)");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "{");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 3, "rtnList.Add(({0})listItem);", eleT);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "}");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "dataCacheVal = rtnList;");
                                }

                                mInfo.append(ref code, LeftSpaceLevel.four + 1, "{0}(({1})dataCacheVal);", actionParaName, action_type);
                                if (m.ReturnType != typeof(void))
                                {
                                    mInfo.append(ref code, LeftSpaceLevel.four + 1, "return ({0})dataCacheVal;", action_type);
                                }
                                else
                                {
                                    mInfo.append(ref code, LeftSpaceLevel.four + 1, "return;");
                                }
                                mInfo.append(ref code, LeftSpaceLevel.four, "}");
                                mInfo.append(ref code, LeftSpaceLevel.four, "");
                            }

                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result = {1};", action_type, DJTools.getDefaultByType(actionType));
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result1 = result;", action_type);
                            mInfo.append(ref code, "");
                        }
                        else if (m.ReturnType != typeof(void))
                        {
                            if (null != dataCache)
                            {
                                //数据缓存代码实现 DataCache                                
                                mInfo.append(ref code, LeftSpaceLevel.four, "if (null != dataCacheVal)");
                                mInfo.append(ref code, LeftSpaceLevel.four, "{");
                                if (typeof(IList).IsAssignableFrom(m.ReturnType))
                                {
                                    eleT = m.ReturnType.GetGenericArguments()[0].TypeToString(true);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "List<{0}> rtnList = new List<{0}>();", eleT);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "System.Collections.IList _ilist2 = (System.Collections.IList)dataCacheVal;");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "foreach(var listItem in _ilist2)");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "{");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 3, "rtnList.Add(({0})listItem);", eleT);
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "}");
                                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "dataCacheVal = rtnList;");
                                }

                                mInfo.append(ref code, LeftSpaceLevel.four + 1, "return ({0})dataCacheVal;", return_type);
                                mInfo.append(ref code, LeftSpaceLevel.four, "}");
                                mInfo.append(ref code, LeftSpaceLevel.four, "");
                            }
                            uskv.Add(new CKeyValue() { Key = m.ReturnType.Namespace });
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result = {1};", return_type, DJTools.getDefaultByType(m.ReturnType));
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result1 = result;", return_type);
                            mInfo.append(ref code, "");
                        }
                    }

                    if (!string.IsNullOrEmpty(autoCallPara))
                    {
                        mInfo.append(ref code, LeftSpaceLevel.one, autoCallPara);
                    }

                    #region 定义  PList<Para> 参数集合
                    mInfo.append(ref code, LeftSpaceLevel.four, "PList<Para> {0} = new PList<Para>();", paraListVarName);
                    if (!string.IsNullOrEmpty(lists))
                    {
                        mInfo.append(ref code, LeftSpaceLevel.one, lists);
                    }
                    mInfo.append(ref code, "");
                    #endregion

                    #region ExecuteBeforeFilter -- 执行接口方法之前
                    mInfo.append(ref code, LeftSpaceLevel.four, "StackTrace trace1 = new StackTrace();");
                    mInfo.append(ref code, LeftSpaceLevel.four, "StackFrame stackFrame1 = trace1.GetFrame(1);");
                    mInfo.append(ref code, LeftSpaceLevel.four, "MethodBase methodBase1 = stackFrame1.GetMethod();");
                    mInfo.append(ref code, LeftSpaceLevel.four, "if(false == {0}.ExecuteBeforeFilter(typeof({1}),{2},\"{3}\",{4}) ", autocall_name, interfaceName, impl_name, methodName, paraListVarName);
                    mInfo.append(ref code, LeftSpaceLevel.four + 1, "|| false == {0}.ExecuteBeforeFilter(typeof({1}),methodBase1,{2},\"{3}\",{4}))", autocall_name, interfaceName, impl_name, methodName, paraListVarName);
                    mInfo.append(ref code, LeftSpaceLevel.four, "{"); //ExecuteBeforeFilter start
                    defaultV = m.ReturnType != typeof(void) ? (" " + funcResultStr("result1", m)) : "";
                    mInfo.append(ref code, LeftSpaceLevel.five, "return{0};", defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four, "}"); //ExecuteBeforeFilter end
                    mInfo.append(ref code, "");
                    #endregion

                    #region 参数重新赋值 -- 考虑到如果在执行接口方法之前 PList<Para> 集合里的 ParaValue 有变更, 那么需要把变更的值重新赋给现有参数
                    foreach (var item in paraList)
                    {
                        mInfo.append(ref code, LeftSpaceLevel.four, "{0} = null == {2}[\"{0}\"] ? {0} : ({1})DJTools.ConvertTo({2}[\"{0}\"].ParaValue, {2}[\"{0}\"].ParaType);", item.ParaName, item.ParaTypeName, paraListVarName);
                    }
                    if (0 < paraList.Count) mInfo.append(ref code, "");
                    #endregion

                    #region 执行接口方法   
                    if (isDynamicEntity)
                    {
                        mInfo.methodInfo = m;
                        DynamicEntityMInfoPara dynamicEntityMInfoPara = new DynamicEntityMInfoPara()
                        {
                            autoCall = autoCall,
                            mInfo = mInfo,
                            uskv = uskv,
                            implementType = implementType,
                            objType = objType,
                            actionType = actionType,
                            isAsync = isAsync,
                            EnabledBuffer = EnabledBuffer,
                            isNotInheritInterface = isNotInheritInterface,
                            msInterval = msInterval,
                            autocall_name = autocall_name,
                            paraListVarName = paraListVarName,
                            genericity = genericity,
                            interfaceName = interfaceName,
                            impl_name = impl_name,
                            methodName = methodName,
                            plist = plist,
                            actionParaName = actionParaName,
                            autoCallPara = autoCallPara,
                            autoCallParaName = autoCallParaName
                        };
                        dynamicEntityMInfo(m, dynamicEntityMInfoPara, ref code, ref return_type);
                    }
                    else
                    {
                        err = "";
                        NormalEntityPara normalEntityPara = new NormalEntityPara
                        {
                            autoCall = autoCall,
                            autoCall_Impl = autoCall_Impl,
                            implementType = implementType,
                            objType = objType,
                            actionType = actionType,
                            paraList = paraList,
                            EnabledBuffer = EnabledBuffer,
                            isNotInheritInterface = isNotInheritInterface,
                            autocall_name = autocall_name,
                            paraListVarName = paraListVarName,
                            impl_name = impl_name,
                            interfaceName = interfaceName,
                            methodName = methodName,
                            plist = plist,
                            isAsync = isAsync,
                            msInterval = msInterval,
                            actionParaName = actionParaName,
                            return_type = return_type,
                            genericity = genericity
                        };
                        normalEntity(m, normalEntityPara, ref code, ref err);

                        if (!string.IsNullOrEmpty(err))
                        {
                            isDisables_Str = err;
                        }
                    }
                    mInfo.append(ref code, "");
                    #endregion

                    #region ExecuteAfterFilter -- 执行接口方法之后
                    defaultV = m.ReturnType != typeof(void) ? "result" : "null";
                    mInfo.append(ref code, LeftSpaceLevel.four, "if(false == {0}.ExecuteAfterFilter(typeof({1}),{2},\"{3}\",{4},{5}) ", autocall_name, interfaceName, impl_name, methodName, paraListVarName, defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four + 2, "|| false == {0}.ExecuteAfterFilter(typeof({1}),methodBase1,{2},\"{3}\",{4},{5}))", autocall_name, interfaceName, impl_name, methodName, paraListVarName, defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four, "{"); //ExecuteBeforeFilter start
                    defaultV = m.ReturnType != typeof(void) ? " " + funcResultStr("result1", m) : "";
                    mInfo.append(ref code, LeftSpaceLevel.five, "return{0};", defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four, "}"); //ExecuteBeforeFilter end
                    mInfo.append(ref code, "");
                    #endregion

                    if (m.ReturnType != typeof(void))
                    {
                        string resultStr = funcResultStr("result", m);
                        if (null != dataCache)
                        {
                            //数据缓存代码实现 DataCache
                            mInfo.append(ref code, LeftSpaceLevel.four, "if (null != ({0}))", resultStr);
                            mInfo.append(ref code, LeftSpaceLevel.four, "{");
                            mInfo.append(ref code, LeftSpaceLevel.four + 1, "((IDataCache)_dataCachePool).Set(cacheKey, {0}, {1}, {2});",
                                resultStr, dataCache.CacheTime.ToString(), dataCache.PersistenceCache.ToString().ToLower());
                            mInfo.append(ref code, LeftSpaceLevel.four, "}");
                        }
                        //如果接口方法返回类型不为 void                        
                        mInfo.append(ref code, LeftSpaceLevel.four, "return {0};", resultStr);
                    }

                    mInfo.append(ref code, LeftSpaceLevel.three, "}");//method end
                    if (isDataOpt) code = code.Replace("[Method_Attribute]", methodAttr);
                }
            };

            Action<Type, PropertyInfo[]> actionProperty = (objType, piArr) =>
            {
                int piNum = 0;
                string objTypeName = objType.TypeToString(true);
                foreach (PropertyInfo item in piArr)
                {
                    returnType = item.PropertyType.TypeToString(true);
                    if (0 < piNum)
                    {
                        DJTools.append(ref code, 0, "");
                    }

                    if (isNotInheritInterface)
                    {
                        if (item.GetGetMethod().IsVirtual)
                        {
                            DJTools.append(ref code, 2, "public override {0} {1}", returnType, item.Name);
                        }
                        else
                        {
                            DJTools.append(ref code, 2, "public new {0} {1}", returnType, item.Name);
                        }
                        DJTools.append(ref code, 2, "{");
                        if (item.CanRead)
                        {
                            DJTools.append(ref code, 3, "get { return base.{0}; }", item.Name);
                        }

                        if (item.CanWrite)
                        {
                            DJTools.append(ref code, 3, "set { base.{0} = value; }", item.Name);
                        }
                        DJTools.append(ref code, 2, "}");
                    }
                    else if (impl_name.Equals("null"))
                    {
                        DJTools.append(ref code, 2, "{0} {1}.{2} { get; set; }", returnType, objTypeName, item.Name);
                    }
                    else
                    {
                        DJTools.append(ref code, 2, "{0} {1}.{2}", returnType, objTypeName, item.Name);
                        DJTools.append(ref code, 2, "{");
                        if (item.CanRead)
                        {
                            DJTools.append(ref code, 3, "get { return {0}.{1}; }", impl_name, item.Name);
                        }

                        if (item.CanWrite)
                        {
                            DJTools.append(ref code, 3, "set { {0}.{1} = value; }", impl_name, item.Name);
                        }
                        DJTools.append(ref code, 2, "}");
                    }
                    piNum++;
                }
            };

            Action<Type, EventInfo[]> actionEvent = (objType, eiArr) =>
            {
                string objTypeName = objType.TypeToString(true);
                foreach (EventInfo item in eiArr)
                {
                    DJTools.append(ref code, 1, "");
                    DJTools.append(ref code, 2, "event {0} {1}.{2}", item.EventHandlerType.TypeToString(true), objTypeName, item.Name);
                    DJTools.append(ref code, 2, "{");
                    DJTools.append(ref code, 3, "add");
                    DJTools.append(ref code, 3, "{");
                    DJTools.append(ref code, 4, "{0}.{1} += value;", impl_name, item.Name);
                    DJTools.append(ref code, 3, "}");
                    DJTools.append(ref code, 3, "remove");
                    DJTools.append(ref code, 3, "{");
                    DJTools.append(ref code, 4, "{0}.{1} -= value;", impl_name, item.Name);
                    DJTools.append(ref code, 3, "}");
                    DJTools.append(ref code, 2, "}");
                }
            };

            Action<MemberInfo, MethodInfo[], string, Action<int>> findPropertyMethod = (propertyInfo, methodInfoArr, attrOptName, propertyMethod) =>
            {
                string aName = attrOptName + "_" + propertyInfo.Name;
                int num = 0;
                bool isEvent = null != (propertyInfo as EventInfo);
                foreach (var itemM in methods1)
                {
                    if (null != itemM)
                    {
                        if (isEvent)
                        {
                            ParameterInfo[] paraArr = itemM.GetParameters();
                            if (0 == paraArr.Length) continue;
                            if (1 != paraArr.Length) continue;
                            ParameterInfo pi = paraArr[0];
                            if (itemM.Name.Equals(aName)
                                && itemM.ReturnType.Equals(typeof(void))
                                && pi.ParameterType.IsSubclassOf(typeof(Delegate)))
                            {
                                propertyMethod(num);
                                break;
                            }
                        }
                        else
                        {
                            if (itemM.Name.Equals(aName) && (itemM.ReturnType.Equals(typeof(void)) || itemM.ReturnType.Equals(((PropertyInfo)propertyInfo).PropertyType)))
                            {
                                propertyMethod(num);
                                break;
                            }
                        }
                    }
                    num++;
                }
            };

            if (false == impl_name1.Equals("null"))
            {
                uskv.Add(new CKeyValue() { Key = typeof(FieldInfo).Namespace });
                mInfo.append(ref code, "");
                mInfo.append(ref code, LeftSpaceLevel.three, "public {0}_{1}()", implName, dt);
                mInfo.append(ref code, LeftSpaceLevel.three, "{");
                mInfo.append(ref code, LeftSpaceLevel.four, "FieldInfo[] fiArr = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);");
                mInfo.append(ref code, LeftSpaceLevel.four, "foreach (FieldInfo fi in fiArr)");
                mInfo.append(ref code, LeftSpaceLevel.four, "{");
                mInfo.append(ref code, LeftSpaceLevel.five, "if (fi.Name.Equals(\"{0}\")) continue;", impl_name1);
                mInfo.append(ref code, LeftSpaceLevel.five, "if (!fi.FieldType.IsAssignableFrom(typeof({0}))) continue;", implName);
                mInfo.append(ref code, LeftSpaceLevel.five, "fi.SetValue(this, {0});", impl_name1);
                //mInfo.append(ref code, LeftSpaceLevel.five, "");
                mInfo.append(ref code, LeftSpaceLevel.four, "}");
                mInfo.append(ref code, LeftSpaceLevel.three, "}");
            }

            string privateVarNameCode = "";
            foreach (Type typeItem in typeList)
            {
                GenericArr = null;
                if (isNotInheritInterface)
                {
                    methods1 = typeItem.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    piArr1 = typeItem.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    eiArr1 = typeItem.GetEvents(BindingFlags.Public | BindingFlags.Instance);
                    impl_name = "this";
                }
                else
                {
                    methods1 = typeItem.GetMethods();
                    piArr1 = typeItem.GetProperties();
                    eiArr1 = typeItem.GetEvents();

                    interfaceType = typeItem;
                    interfaceName = DJTools.GetClassName(interfaceType, true);

                    if (interfaceType.IsGenericType)
                    {
                        Type[] types1 = interfaceType.GetGenericArguments();
                        string ts = "", tt = "";
                        int tn = 0;
                        foreach (Type item in types1)
                        {
                            //ts += ",\"" + item.Name + "\"";
                            tt = "T";
                            for (int i = 0; i < tn; i++)
                            {
                                tt += "T";
                            }
                            ts += ",\"" + tt + "\"";
                            tn++;
                        }
                        ts = ts.Substring(1);
                        GenericArr = "string[] GenericArr = new string[] {" + ts + "};";
                    }

                    impl_name = "null";
                    if (false == impl_name1.Equals("null"))
                    {
                        impl_name = interfaceType.Name + "_01";
                        mInfo.append(ref privateVarNameCode, LeftSpaceLevel.three, "private {0} {1} = null;", interfaceName, impl_name);
                    }
                }

                foreach (var itemPi in piArr1)
                {
                    findPropertyMethod(itemPi, methods1, "set", methodIndex =>
                    {
                        methods1[methodIndex] = null;
                    });

                    findPropertyMethod(itemPi, methods1, "get", methodIndex =>
                    {
                        methods1[methodIndex] = null;
                    });
                }

                foreach (var itemEi in eiArr1)
                {
                    findPropertyMethod(itemEi, methods1, "add", methodIndex =>
                    {
                        methods1[methodIndex] = null;
                    });

                    findPropertyMethod(itemEi, methods1, "remove", methodIndex =>
                    {
                        methods1[methodIndex] = null;
                    });
                }

                actionFunction(typeItem, methods1);
                if (!isNotInheritInterface)
                {
                    actionEvent(typeItem, eiArr1);
                }

                if (0 < piArr1.Length)
                {
                    DJTools.append(ref code, "");
                    actionProperty(typeItem, piArr1);
                }
            }

            mInfo.append(ref code, "");
            mInfo.append(ref code, LeftSpaceLevel.two, "}"); //class end
            mInfo.append(ref code, "}");//namespace end

            string referencesCode = "";
            foreach (CKeyValue item in uskv)
            {
                mInfo.append(ref referencesCode, LeftSpaceLevel.one, "using {0};", item.Key);
            }

            if (_isDataOpt && (-1 == implInterfaces.IndexOf("IUnSingleInstance"))) implInterfaces += ", IUnSingleInstance";
            if (_isDataOpt && (-1 == implInterfaces.IndexOf("IDataInterface"))) implInterfaces += ", IDataInterface";

            code = code.Replace(References, referencesCode);
            code = code.Replace(privateVarName, privateVarNameCode);
            code = code.Replace(implInterfacesTag, implInterfaces);
            code = code.Replace(PublicAutoCall, autocall_name);

            if (!string.IsNullOrEmpty(isDisables_Str))
            {
                code = "";
            }

            return code;
        }

        public bool IsDataInterface { get; set; }

        public SynchronizationContext SynicContext { get; set; }

    }
}
