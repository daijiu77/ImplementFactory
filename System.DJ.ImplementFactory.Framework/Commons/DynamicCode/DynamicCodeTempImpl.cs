using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
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
        public static string InterfaceInstanceType => "Get_InterfaceInstanceType__";

        static Random random = new Random();
        string dirName = "";

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
            ref string actionParaName, ref string methodName, ref string paraStr, ref string lists, ref bool isDynamicEntity, ref string defaultV, ref string outParas)
        {
            string plist = "";
            string s = "";
            ParameterInfo[] paras = m.GetParameters();
            Regex rg = new Regex(@"\`[0-9]+\[");
            Regex rg1 = new Regex(@"(?<TypeFull>^[a-z][^\`]+)\`[0-9]", RegexOptions.IgnoreCase);

            foreach (ParameterInfo p in paras)
            {
                if (null == p.ParameterType.FullName)
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });

                    //泛型情况
                    methodName = m.Name + "<" + p.ParameterType.Name + ">";
                    s = p.ParameterType.TypeToString();
                    paraStr += "," + s + " " + p.Name;
                    mInfo.append(ref lists, LeftSpaceLevel.four, "{2}.Add(new Para(Guid.NewGuid()){ParaType=typeof({0}),ParaTypeName=\"{0}\",ParaName=\"{1}\",ParaValue={1}});", s, p.Name, paraListVarName);
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
                else if (-1 != p.ParameterType.FullName.IndexOf("&"))
                {
                    uskv.Add(new CKeyValue() { Key = p.ParameterType.Namespace });

                    //out, ref 情况
                    s = p.ParameterType.FullName.Replace("&", "");
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
                else if (p.ParameterType.BaseType == typeof(System.MulticastDelegate) && rg.IsMatch(p.ParameterType.ToString()))
                {
                    //Action<>, Func<> 情况
                    s = p.ParameterType.TypeToString();
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
                else if (p.ParameterType.GetInterface("IEnumerable") == typeof(IEnumerable) && typeof(string) != p.ParameterType)
                {
                    s = p.ParameterType.FullName;
                    if (rg1.IsMatch(s))
                    {
                        s = rg1.Match(s).Groups["TypeFull"].Value;
                    }

                    Type[] types = p.ParameterType.GetGenericArguments();
                    //集合类型: 数组 List Dictionary   
                    if (p.ParameterType.GetInterface("IDictionary") == typeof(IDictionary))
                    {
                        uskv.Add(new CKeyValue() { Key = types[0].Namespace });
                        uskv.Add(new CKeyValue() { Key = types[1].Namespace });

                        //集合类型: Dictionary            
                        string s1 = "";
                        foreach (Type item in types)
                        {
                            s1 += "," + item.FullName;
                        }

                        if (!string.IsNullOrEmpty(s1))
                        {
                            s1 = s1.Substring(1);
                        }
                        s += "<" + s1 + ">";
                    }
                    else if (p.ParameterType.IsArray)
                    {
                        uskv.Add(new CKeyValue() { Key = p.ParameterType.GetElementType().Namespace });
                        //集合类型: 数组       
                        //p.ParameterType.FullName 已经是 '参数类型[]' ,所以不必再组合
                    }
                    else if (p.ParameterType.GetInterface("IList") == typeof(IList))
                    {
                        uskv.Add(new CKeyValue() { Key = types[0].Namespace });
                        s += "<" + types[0].FullName + ">";
                        //集合类型: List                       
                    }

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
        }

        void dynamicEntityMInfo(MethodInfo m, DynamicEntityMInfoPara dynamicEntityMInfoPara, ref string code, ref string return_type)
        {
            dynamicEntityMInfoPara.uskv.Add(new CKeyValue() { Key = typeof(MethodInformation).Namespace });
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "MethodInformation method_info = new MethodInformation();");
            if (null != dynamicEntityMInfoPara.implementType)
            {
                dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ofInstanceType = typeof({0});", dynamicEntityMInfoPara.implementType.FullName);
            }
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ofInterfaceType = typeof({0});", dynamicEntityMInfoPara.objType.FullName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.paraList = paraList;");
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.StartSpace = \"{0}\";", dynamicEntityMInfoPara.mInfo.getSpace(4));
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.AutoCallVarName = \"{0}\";", dynamicEntityMInfoPara.autocall_name);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.ParaListVarName = \"{0}\";", dynamicEntityMInfoPara.paraListVarName);
            dynamicEntityMInfoPara.mInfo.append(ref code, LeftSpaceLevel.four, "method_info.AutoCall = {0};", dynamicEntityMInfoPara.autocall_name);
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

        public string GetCodeByImpl(Type interfaceType, Type implementType, AutoCall autoCall_Impl, ref string classPath)
        {
            MethodInformation mInfo = new MethodInformation();

            Type[] genericTypes = interfaceType.GetGenericArguments();
            string interfaceNamespace = interfaceType.Namespace;
            string interfaceName = DJTools.GetClassName(interfaceType, true);

            string implNamespace = null;
            string implName = null;
            string implName1 = null;

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
            bool isNotInheritInterface = false;
            bool isDataOpt = false;
            List<Type> typeList = new List<Type>();

            EList<CKeyValue> uskv = new EList<CKeyValue>();
            uskv.Add(new CKeyValue() { Key = "System" });
            uskv.Add(new CKeyValue() { Key = "System.Collections.Generic" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Pipelines" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Pipelines.Pojo" });
            uskv.Add(new CKeyValue() { Key = interfaceNamespace });
            uskv.Add(new CKeyValue() { Key = autocallImplNamespace });
            //uskv.Add(new CKeyValue() { Key = "" });

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
                mInfo.append(ref code, LeftSpaceLevel.three, "private static {0} {1} = new {0}();", implName, impl_name1);
                mInfo.append(ref code, LeftSpaceLevel.one, "");
                mInfo.append(ref code, LeftSpaceLevel.three, "public {0} {1} { get; set; }", DJTools.GetClassName(implementType, true), InterfaceInstanceType);
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
            string paraStr = "";
            string lists = "";
            string plist = "";
            string returnType = "";
            string outParas = "";
            string defaultV = "";
            string methodName = "";
            string return_type = "";
            string genericity = "";
            string paraListVarName = "paraList";
            string methodAttr = "";
            string actionParaName = "";
            string err = "";
            AutoCall autoCall = null;
            bool EnabledBuffer = true;
            bool isDynamicEntity = false;
            bool isAsync = false;
            AbsDataInterface absData = null;
            Type actionType = null;
            int msInterval = 0;

            IsDataInterface = false;

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
                    //notDataMethod = NotDataMethod.getDJNormal(m);

                    EnabledBuffer = false;
                    isDynamicEntity = false;
                    isDataOpt = false;
                    isAsync = false;
                    methodAttr = "";
                    msInterval = 0;

                    autoCall = getAutoCall(m, ref isDataOpt, ref absData, ref methodAttr, ref isAsync, ref msInterval, ref EnabledBuffer);

                    methodName = m.Name;
                    paras = m.GetParameters();
                    paraStr = "";
                    lists = "";
                    plist = "";
                    outParas = "";

                    actionType = null;
                    actionParaName = "null";

                    paraList.Clear();

                    return_type = m.ReturnType.TypeToString();

                    paraStr = "";
                    lists = "";
                    plist = getParaListStr(m, mInfo, uskv, paraList, paraListVarName, ref actionType, ref actionParaName, ref methodName, ref paraStr, ref lists,
                        ref isDynamicEntity, ref defaultV, ref outParas);

                    if (!string.IsNullOrEmpty(paraStr))
                    {
                        paraStr = paraStr.Substring(1);
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
                        mInfo.append(ref code, LeftSpaceLevel.three, "[Method_Attribute]");
                        mInfo.append(ref code, LeftSpaceLevel.three, "{0} {1}.{2}{3}({4})", returnType, interfaceName, methodName, genericity, paraStr);
                    }
                    mInfo.append(ref code, LeftSpaceLevel.three, "{");//method start

                    if (!string.IsNullOrEmpty(outParas))
                    {
                        //初始化 out 参数
                        mInfo.append(ref code, outParas);
                        mInfo.append(ref code, "");
                    }

                    if ((m.ReturnType != typeof(void) || null != actionType))
                    {
                        //声明方法返回值变量
                        if (null != actionType && null != autoCall)
                        {
                            uskv.Add(new CKeyValue() { Key = actionType.Namespace });
                            return_type = actionType.TypeToString();
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result = {1};", return_type, DJTools.getDefaultByType(actionType));
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result1 = result;", return_type);
                            mInfo.append(ref code, "");
                        }
                        else if (m.ReturnType != typeof(void))
                        {
                            uskv.Add(new CKeyValue() { Key = m.ReturnType.Namespace });
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result = {1};", return_type, DJTools.getDefaultByType(m.ReturnType));
                            mInfo.append(ref code, LeftSpaceLevel.four, "{0} result1 = result;", return_type);
                            mInfo.append(ref code, "");
                        }
                    }

                    #region 定义  PList<Para> 参数集合
                    mInfo.append(ref code, LeftSpaceLevel.four, "PList<Para> {0} = new PList<Para>();", paraListVarName);
                    if (!string.IsNullOrEmpty(lists))
                    {
                        mInfo.append(ref code, LeftSpaceLevel.one, lists);
                    }
                    mInfo.append(ref code, "");
                    #endregion

                    Func<string, string> funcResultStr = (result_Var) =>
                    {
                        string sr = "";
                        if (m.ReturnType != typeof(void) && null != absData)
                        {
                            //如果接口方法返回类型不为 void
                            if (DataOptType.select != ((IDataOperateAttribute)absData).dataOptType && DataOptType.procedure != ((IDataOperateAttribute)absData).dataOptType)
                            {
                                Type actionType1 = null == actionType ? m.ReturnType : actionType;
                                if (typeof(bool) == m.ReturnType && typeof(int) == actionType1)
                                {
                                    sr = string.Format("0 < {0}", result_Var);
                                }
                                else if (typeof(int) == m.ReturnType && typeof(bool) == actionType1)
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

                    #region ExecuteBeforeFilter -- 执行接口方法之前
                    mInfo.append(ref code, LeftSpaceLevel.four, "if(!{0}.ExecuteBeforeFilter(typeof({1}),{2},\"{3}\",{4}))", autocall_name, interfaceName, impl_name, methodName, paraListVarName);
                    mInfo.append(ref code, LeftSpaceLevel.four, "{"); //ExecuteBeforeFilter start
                    defaultV = m.ReturnType != typeof(void) ? (" " + funcResultStr("result1")) : "";
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
                            actionParaName = actionParaName
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
                    mInfo.append(ref code, LeftSpaceLevel.four, "if(!{0}.ExecuteAfterFilter(typeof({1}),{2},\"{3}\",{4},{5}))", autocall_name, interfaceName, impl_name, methodName, paraListVarName, defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four, "{"); //ExecuteBeforeFilter start
                    defaultV = m.ReturnType != typeof(void) ? " " + funcResultStr("result1") : "";
                    mInfo.append(ref code, LeftSpaceLevel.five, "return{0};", defaultV);
                    mInfo.append(ref code, LeftSpaceLevel.four, "}"); //ExecuteBeforeFilter end
                    mInfo.append(ref code, "");
                    #endregion

                    if (m.ReturnType != typeof(void))
                    {
                        //如果接口方法返回类型不为 void                        
                        mInfo.append(ref code, LeftSpaceLevel.four, "return {0};", funcResultStr("result"));
                    }

                    mInfo.append(ref code, LeftSpaceLevel.three, "}");//method end
                    code = code.Replace("[Method_Attribute]", methodAttr);
                }
            };

            Action<Type, PropertyInfo[]> actionProperty = (objType, piArr) =>
            {
                int piNum = 0;
                foreach (PropertyInfo item in piArr)
                {
                    if (isNotInheritInterface)
                    {
                        if (item.CanRead)
                        {
                            if (false == item.GetGetMethod().IsVirtual) continue;
                        }

                        if (item.CanWrite)
                        {
                            if (false == item.GetSetMethod().IsVirtual) continue;
                        }
                    }

                    returnType = item.PropertyType.TypeToString(true);
                    string objTypeName = objType.TypeToString(true);
                    if (0 < piNum)
                    {
                        DJTools.append(ref code, 0, "");
                    }

                    if (isNotInheritInterface)
                    {
                        DJTools.append(ref code, 2, "public override {0} {1}", returnType, item.Name);
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

            Action<PropertyInfo, MethodInfo[], string, Action<int>> findPropertyMethod = (propertyInfo, methodInfoArr, attrOptName, propertyMethod) =>
            {
                string aName = attrOptName + "_" + propertyInfo.Name;
                int num = 0;
                foreach (var itemM in methods1)
                {
                    if (null != itemM)
                    {
                        if (itemM.Name.Equals(aName) && (itemM.ReturnType.Equals(typeof(void)) || itemM.ReturnType.Equals(propertyInfo.PropertyType)))
                        {
                            propertyMethod(num);
                            break;
                        }
                    }
                    num++;
                }
            };

            string privateVarNameCode = "";
            foreach (Type typeItem in typeList)
            {
                if (isNotInheritInterface)
                {
                    methods1 = typeItem.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    piArr1 = typeItem.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    impl_name = "this";
                }
                else
                {
                    methods1 = typeItem.GetMethods();
                    piArr1 = typeItem.GetProperties();
                    interfaceType = typeItem;
                    interfaceName = DJTools.GetClassName(interfaceType, true);

                    impl_name = "null";
                    if (false == impl_name1.Equals("null"))
                    {
                        impl_name = interfaceType.Name + "_01";
                        mInfo.append(ref privateVarNameCode, LeftSpaceLevel.three, "private {0} {1} = {2};", interfaceName, impl_name, impl_name1);
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

                actionFunction(typeItem, methods1);
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

            if (isDataOpt && (-1 == implInterfaces.IndexOf("IUnSingleInstance"))) implInterfaces += ", IUnSingleInstance";
            if (isDataOpt && (-1 == implInterfaces.IndexOf("IDataInterface"))) implInterfaces += ", IDataInterface";

            code = code.Replace(References, referencesCode);
            code = code.Replace(privateVarName, privateVarNameCode);
            code = code.Replace(implInterfacesTag, implInterfaces);

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
