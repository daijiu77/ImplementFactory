using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public delegate string getDllAbsolutePathByRelativePath(string relativePath);
    public class AutoCall : Attribute
    {
        DynamicCodeAutoCall dynamicCodeAutoCall = new DynamicCodeAutoCall();

        public enum IgnoreLU { none = 0, is_true, is_false }

        public static List<Assembly> AssemblyCollection { get; set; }

        static string _sqlProviderRelativePath = "";

        static AutoCall _autoCall = null;

        public AutoCall() { }

        public AutoCall(string MatchRuleOrClassName, bool IgnoreCase)
        {
            this.MatchRuleOrClassName = MatchRuleOrClassName;
            this.IgnoreCase = IgnoreCase ? IgnoreLU.is_true : IgnoreLU.is_false;
        }

        public AutoCall(string MatchRuleOrClassName)
        {
            this.MatchRuleOrClassName = MatchRuleOrClassName;
        }

        public static AutoCall Instance
        {
            get
            {
                if (null == _autoCall) _autoCall = new AutoCall();
                return _autoCall;
            }
        }

        public string MatchRuleOrClassName { get; set; }

        public IgnoreLU IgnoreCase { get; set; } = IgnoreLU.none;

        /// <summary>
        /// AOP机制，加载接口实例前被调用
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <returns>Bool类型，返回true时允许继续加载实例，返回false时阻止加载当前接口实例</returns>
        public virtual bool LoadBeforeFilter(Type interfaceType)
        {
            return true;
        }

        /// <summary>
        /// AOP机制，加载接口实例后被调用
        /// </summary>
        /// <typeparam name="T">接口实例类型</typeparam>
        /// <param name="impl">接口实例</param>
        /// <returns>Bool类型，返回true时允许把已加载的接口实例赋给类成员接口变量，反之异然</returns>
        public virtual bool LoadAfterFilter<T>(T impl)
        {
            return true;
        }

        /// <summary>
        /// AOP机制，执行接口方法前被调用
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implement">接口实例</param>
        /// <param name="methodName">当前执行的接口方法</param>
        /// <param name="paras">接口参数</param>
        /// <returns>Bool类型，返回true时允许继续执行接口方法，反之异然</returns>
        public virtual bool ExecuteBeforeFilter(Type interfaceType, object implement, string methodName, PList<Para> paras)
        {
            return true;
        }

        /// <summary>
        /// AOP机制，执行接口方法后被调用
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implement">接口实例</param>
        /// <param name="methodName">当前执行的接口方法</param>
        /// <param name="paras">接口参数</param>
        /// <param name="result">接口方法返回值</param>
        /// <returns>Bool类型，返回true时允许把执行接口方法后的结果返回给调用者，反之异然</returns>
        public virtual bool ExecuteAfterFilter(Type interfaceType, object implement, string methodName, PList<Para> paras, object result)
        {
            return true;
        }

        object _excObj = new object();

        /// <summary>
        /// AOP机制，执行接口方法发生异常时调用，拦截异常信息
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implement">接口实例</param>
        /// <param name="methodName">当前执行的接口方法</param>
        /// <param name="paras">接口参数</param>
        /// <param name="ex">异常信息</param>
        /// <param name="errorLevels">异常信息等级: lesser[次要的], normal[普通的], dangerous[危险的], severe[严重的]</param>
        public virtual void ExecuteException(Type interfaceType, object implement, string methodName, PList<Para> paras, Exception ex, ErrorLevels errorLevels)
        {
            lock (_excObj)
            {
                LoadErrorLevel();

                if (errorLevels < errorLevels1[0] || errorLevels > errorLevels1[1]) return;

                //异常处理
                string rootPath = null == RootPath ? "" : RootPath;
                string dir = Path.Combine(rootPath, "Logs");
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch { }
                }

                if (!Directory.Exists(dir)) return;

                string txt = "";
                string date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                string msg = date;
                DJTools.append(ref msg, "The exception level: [{0}]{1}", ((int)errorLevels).ToString(), Enum.GetName(typeof(ErrorLevels), errorLevels));
                DJTools.append(ref msg, "Interface: {0}", (null == interfaceType ? "" : interfaceType.FullName));
                DJTools.append(ref msg, "Instance: {0}", (null == implement ? "" : implement.GetType().FullName));
                DJTools.append(ref msg, "Method name: {0}", (string.IsNullOrEmpty(methodName) ? "" : methodName));
                DJTools.append(ref msg, "Parameters:");
                if (null != paras)
                {
                    foreach (var item in paras)
                    {
                        DJTools.append(ref msg, 1, "{0} = {1}\t{2}", item.ParaName, (null == item.ParaValue ? "emply" : item.ParaValue.ToString()), item.ParaTypeName);
                    }
                }
                DJTools.append(ref msg, "");
                DJTools.append(ref msg, ex.ToString());

                string fName = "err_" + DateTime.Now.ToString("yyyyMMddHH") + ".txt";
                string file = Path.Combine(dir, fName);
                if (File.Exists(file))
                {
                    txt = File.ReadAllText(file);
                    txt = msg + "\r\n\r\n" + txt;
                }
                else
                {
                    txt = msg;
                }

                File.WriteAllText(file, txt);
            }
        }

        void LoadErrorLevel()
        {
            if (null != errorLevels1)
            {
                errorLevels1 = new List<ErrorLevels>();
                errorLevels1.Add(ErrorLevels.severe);
                errorLevels1.Add(ErrorLevels.debug);
                return;
            }
            string configFile = "ImplementFactory.xml";
            string rootPath = DJTools.RootPath;
            string f = Path.Combine(rootPath, configFile);
            if (File.Exists(f))
            {
                LoadErrorLevelByXml(f);
                return;
            }

            configFile = "ImplementFactory.config";
            string txt = File.ReadAllText(f);
            ErrorLevels min = ErrorLevels.severe;
            ErrorLevels max = ErrorLevels.debug;

            Action<ErrorLevels, ErrorLevels> action = (min1, max1) =>
            {
                errorLevels1 = new List<ErrorLevels>();
                errorLevels1.Add(min1);
                errorLevels1.Add(max1);
            };

            if (string.IsNullOrEmpty(txt))
            {
                ErrorLevelArea(ref min, ref max);
                action(min, max);
                return;
            }

            Regex rg = new Regex(@"\{[^\{\}]*((upperLimit)|(lowerLimit))[^\{\}]*\}", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(txt))
            {
                ErrorLevelArea(ref min, ref max);
                action(min, max);
                return;
            }

            string upperLimit = "";
            string lowerLimit = "";
            string s = rg.Match(txt).Groups[0].Value;
            rg = new Regex(@"upperLimit\s*\=\s*""(?<upperLimit>[^""]+)""", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s))
            {
                upperLimit = rg.Match(s).Groups["upperLimit"].Value.Trim();
            }

            rg = new Regex(@"lowerLimit\s*\=\s*""(?<lowerLimit>[^""]+)""", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s))
            {
                lowerLimit = rg.Match(s).Groups["lowerLimit"].Value.Trim();
            }

            if (string.IsNullOrEmpty(upperLimit) || string.IsNullOrEmpty(lowerLimit))
            {
                ErrorLevelArea(ref min, ref max);
            }

            rg = new Regex(@"^[0-9]$", RegexOptions.IgnoreCase);
            bool bool1 = false;
            ErrorLevels min2 = ErrorLevels.severe;
            ErrorLevels max2 = ErrorLevels.debug;
            object v = null;
            if (!string.IsNullOrEmpty(upperLimit))
            {
                if (rg.IsMatch(upperLimit))
                {
                    v= Enum.Parse(typeof(ErrorLevels), upperLimit);
                    min2 = min;
                    if (null != v) min2 = (ErrorLevels)v;
                }
                else
                {
                    bool1 = Enum.TryParse(upperLimit, out min2);
                    if (!bool1) min2 = min;
                }                
            }
            else
            {
                min2 = min;
            }

            if (!string.IsNullOrEmpty(lowerLimit))
            {
                if (rg.IsMatch(lowerLimit))
                {
                    v = Enum.Parse(typeof(ErrorLevels), lowerLimit);
                    max2 = max;
                    if (null != v) max2 = (ErrorLevels)v;
                }
                else
                {
                    bool1 = Enum.TryParse(lowerLimit, out max2);
                    if (!bool1) max2 = max;
                }                
            }
            else
            {
                max2 = max;
            }

            action(min2, max2);
        }

        void LoadErrorLevelByXml(string f)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(f);

            if (1 >= doc.ChildNodes.Count) return;
            XmlNode node = doc.ChildNodes[1];
            string nodeName = "";

            LogsRange logsRange1 = new LogsRange();
            string LogsRange = "LogsRange".ToLower();

            Action<XmlNode, object> action = (_node, _obj) =>
            {
                string node_name = "";
                string v = null;
                foreach (XmlNode item in _node.ChildNodes)
                {
                    node_name = item.Name.ToLower();
                    v = item.InnerText.Trim();
                    _obj.SetPropertyValue(node_name, v);
                }
            };

            foreach (XmlNode item in node.ChildNodes)
            {
                nodeName = item.Name.ToLower();
                if (nodeName.Equals(LogsRange))
                {
                    action(item, logsRange1);
                    break;
                }
            }

            errorLevels1 = null == errorLevels1 ? new List<ErrorLevels>() : errorLevels1;

            errorLevels1.Clear();
            ErrorLevels el1 = ErrorLevels.severe;
            bool bool1 = Enum.TryParse(logsRange1.upperLimit, out el1);
            if (!bool1)
            {
                el1 = ErrorLevels.severe;
            }
            errorLevels1.Add(el1);

            ErrorLevels el2 = ErrorLevels.debug;
            bool1 = Enum.TryParse(logsRange1.lowerLimit, out el2);
            if (!bool1)
            {
                el2 = ErrorLevels.debug;
            }
            errorLevels1.Add(el2);
        }

        class LogsRange
        {
            private string _upperLimit = "可以是数字或英文名称, 为数字时 upperLimit 应小于 lowerLimit";
            /// <summary>
            /// 上限值
            /// </summary>
            public string upperLimit { get; set; } = "severe";

            private string _lowerLimit = "可以是数字或英文名称, 为数字时 lowerLimit 应大于 upperLimit";
            /// <summary>
            /// 下限值
            /// </summary>
            public string lowerLimit { get; set; } = "debug";
        }

        void ErrorLevelArea(ref ErrorLevels min, ref ErrorLevels max)
        {
            IList<ErrorLevels> arr = (IList<ErrorLevels>)Enum.GetValues(typeof(ErrorLevels));
            min = arr[0];
            max = arr[arr.Count - 1];
        }

        public virtual void ExecuteException(Type interfaceType, object implement, string methodName, PList<Para> paras, Exception ex)
        {
            ExecuteException(interfaceType, implement, methodName, paras, ex, ErrorLevels.dangerous);
        }

        void ErrMsg(Action<object, string> action)
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(2);
            MethodBase methodBase = stackFrame.GetMethod();

            object impl = methodBase.DeclaringType;
            string methodName = methodBase.Name;
            action(impl, methodName);
        }

        public void exc(Exception ex, ErrorLevels errorLevels)
        {
            ErrMsg((impl, methodName) =>
            {
                ExecuteException(null, impl, methodName, null, ex, errorLevels);
            });
        }

        public void exc(Exception ex)
        {
            ErrMsg((impl, methodName) =>
            {
                ExecuteException(null, impl, methodName, null, ex, ErrorLevels.normal);
            });
        }

        public void e(string message, ErrorLevels errorLevels)
        {
            ErrMsg((impl, methodName) =>
            {
                ExecuteException(null, impl, methodName, null, new Exception(message), errorLevels);
            });
        }

        public void e(string message)
        {
            ErrMsg((impl, methodName) =>
            {
                ExecuteException(null, impl, methodName, null, new Exception(message), ErrorLevels.normal);
            });
        }

        /// <summary>
        /// 在接口代理方法内自定义执行实例接口方法及数据处理代码
        /// </summary>
        /// <param name="method">接口方法信息对象</param>
        /// <returns>string类型，返回自定义执行接口方法及数据处理代码字符串</returns>
        public virtual string ExecuteInterfaceMethodCodeString(MethodInformation method, ref string err)
        {
            MethodComponent mc = method.methodComponent;
            string code = "";
            err = "";
            if (null == method.ofInstanceType)
            {
                err = "throw new Exception(\"接口 {0} 不存在对应的实例\");";
                method.append(ref code, LeftSpaceLevel.one, err, method.ofInterfaceType.FullName);
                return code;
            }
            string resultVarName = string.IsNullOrEmpty(mc.ResultVariantName) ? "" : (mc.ResultVariantName + " = ");
            method.append(ref code, LeftSpaceLevel.one, "{0}{1}.{2}{3}({4});", resultVarName, mc.InstanceVariantName, mc.InterfaceMethodName, mc.GenericityParas, mc.MethodParas);
            return code;
        }

        /// <summary>
        /// 每次根据接口创建待装配的实例对象时调用
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implementType">接口实例对象类型</param>
        /// <param name="autoCall">AutoCall对象</param>
        /// <param name="tempImplementCount">接口临时实例数量</param>
        public virtual void CreateInstanceByInterface(Type interfaceType, Type implementType, AutoCall autoCall, int tempImplementCount) { }

        /// <summary>
        /// 执行select、insert、update、delete数据操作时被调用
        /// </summary>
        /// <param name="method">接口方法信息对象</param>
        /// <param name="dataOptType">数据操作类型select|insert|update|delete</param>
        /// <param name="sql">sql语句表达式</param>
        /// <param name="code">接口方法代码相关代码字符串</param>
        protected void ExecInterfaceMethodOfCodeStr_DataOpt(MethodInformation method, DataOptType dataOptType, string sql, ref string code)
        {
            StackTrace stackTrace = new StackTrace();
            Type type = stackTrace.GetFrame(1).GetMethod().DeclaringType;
            if (null == type.GetInterface("IDataOperateAttribute"))
            {
                e("调用该方法的类必须实现接口类 IDataOperateAttribute", ErrorLevels.severe);
                throw new Exception("调用该方法的类必须实现接口类 IDataOperateAttribute");
            }
            string sqlVarName = "sql";

            DynamicCodeChange dynamicCodeChange = new DynamicCodeChange();
            try
            {
                dynamicCodeChange.AnalyzeSql(method, dataOptType, sqlVarName, ref sql);
            }
            catch (Exception ex)
            {
                e(ex.ToString(), ErrorLevels.severe);
                throw ex;
            }
            
            code = dynamicCodeAutoCall.ExecReplaceForSqlByFieldName(sql, sqlVarName, method);

            string paraListVarName = "null";
            dynamicCodeAutoCall.DataProviderCode(sqlVarName, method, dataOptType, ref code, ref paraListVarName);
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(sql))
            {
                e("没有提供任何执行语句", ErrorLevels.severe);
                throw new Exception("没有提供任何执行语句");
            }

            string code1 = "";
            if (dataOptType == DataOptType.select 
                || dataOptType == DataOptType.count 
                || DataOptType.procedure == dataOptType)
            {
                code1 = dynamicCodeAutoCall.GetParametersBySqlParameter(sql, method, ref paraListVarName);
                appendCode(method, code1, ref code);

                dynamicCodeAutoCall.MadeExecuteSql(sqlVarName, paraListVarName, method, dataOptType, ref code);
            }
            else
            {
                code1 = dynamicCodeChange.GetParametersBySqlParameter(sql, sqlVarName, method, dataOptType, ref paraListVarName);
                appendCode(method, code1, ref code);
            }

        }

        void appendCode(MethodInformation method, string code1, ref string code)
        {
            if (!string.IsNullOrEmpty(code1))
            {
                code1 = "\r\n" + code1;
                method.append(ref code, LeftSpaceLevel.one, code1);
                method.append(ref code, LeftSpaceLevel.one, "");
            }
        }

        /// <summary>
        /// 当执行接口方法时,根据数据提供者获取sql语句表达式(动态创建sql语句)
        /// </summary>
        /// <param name="dataProvider">sql语句数据提供者</param>
        /// <param name="paraList">接口方法参数集合</param>
        /// <param name="dbParameters">sql语句参数集合</param>
        /// <param name="autoCall">AutoCall对象</param>
        /// <param name="dataOptType">数据操作类型select|insert|update|delete</param>
        /// <param name="sql">返回sql语句提供者动态生成的sql语句</param>
        public void GetSqlByDataProvider(ISqlExpressionProvider dataProvider, PList<Para> paraList, DbList<DbParameter> dbParameters, AutoCall autoCall, DataOptType dataOptType, ref string sql)
        {
            if (null != dataProvider)
            {
                int ncount = paraList.Count;
                object[] paras = new object[ncount];
                int n = 0;
                foreach (Para p in paraList)
                {
                    paras[n] = p.ParaValue;
                    n++;
                }

                MethodInfo m = typeof(ISqlExpressionProvider).GetMethod("provideSql");

                try
                {
                    object v = m.Invoke(dataProvider, new object[] { dbParameters, dataOptType, paraList, paras });
                    if (null != v) sql = v.ToString();
                }
                catch (Exception ex)
                {
                    autoCall.ExecuteException(typeof(ISqlExpressionProvider), null, "provideSql", paraList, ex);
                }


            }
        }

        /// <summary>
        /// 根据名称空间和类名获取sql语句提供者接口实例
        /// </summary>
        /// <param name="dataProviderNamespace">sql语句提供者接口实例所属名称空间</param>
        /// <param name="dataProviderClassName">sql语句提供者接口实例类名称</param>
        /// <param name="autoCall">AutoCall对象</param>
        /// <returns></returns>
        public ISqlExpressionProvider GetDataProvider(string dataProviderNamespace, string dataProviderClassName, AutoCall autoCall)
        {
            Assembly ass = null;
            string classPath = dataProviderNamespace + "." + dataProviderClassName;
            Type type = null;
            string interfaceName = typeof(ISqlExpressionProvider).FullName;

            if (!string.IsNullOrEmpty(_sqlProviderRelativePath) && null != GetDllAbsolutePathByRelativePath)
            {
                string dllFile = GetDllAbsolutePathByRelativePath(_sqlProviderRelativePath);
                if (!File.Exists(dllFile))
                {
                    Exception ex1 = new Exception(string.Format("路径{0}不存在", dllFile));
                    autoCall.ExecuteException(typeof(ISqlExpressionProvider), null, null, null, ex1);
                    return null;
                }
                ass = Assembly.LoadFrom(dllFile);
                type = ass.GetType(classPath);
                if (null == type.GetInterface(interfaceName)) type = null;
            }

            if (null == type && null != AssemblyCollection)
            {
                foreach (Assembly item in AssemblyCollection)
                {
                    type = item.GetType(classPath);
                    if (null != type)
                    {
                        if (null != type.GetInterface(interfaceName)) break;
                    }
                }
            }

            if (null == type)
            {
                Assembly curAss = Assembly.GetExecutingAssembly();
                StackTrace trace = new StackTrace();

                int n = 0;
                StackFrame stackFrame = trace.GetFrame(n);
                MethodBase methodBase = null;
                while (null != stackFrame && 10 > n)
                {
                    n++;
                    methodBase = stackFrame.GetMethod();
                    ass = methodBase.Module.Assembly;
                    type = ass.GetType(classPath);
                    if (null != type)
                    {
                        if (null != type.GetInterface(interfaceName)) break;
                    }
                    stackFrame = trace.GetFrame(n);
                }
            }

            if (null == type)
            {
                string f = ass.FullName;
                Exception ex2 = new Exception(string.Format("在程序集:{0} 下\r\n类路径 {1} 不存在", f, classPath));
                autoCall.ExecuteException(typeof(ISqlExpressionProvider), null, null, null, ex2);
                return null;
            }

            ISqlExpressionProvider dataProvider = null;
            try
            {
                object obj = Activator.CreateInstance(type);
                if (null == obj.GetType().GetInterface("ISqlExpressionProvider")) throw new Exception(string.Format("类 {0} 未实现接口类 ISqlExpressionProvider", classPath));
                dataProvider = obj as ISqlExpressionProvider;
            }
            catch (Exception ex)
            {
                autoCall.ExecuteException(typeof(ISqlExpressionProvider), null, null, null, ex);
            }

            return dataProvider;
        }

        /// <summary>
        /// 根据数据实体(接口方法参数中包含)和带参数的sql语句中包含的参数集合来创建DbParameter集合
        /// </summary>
        /// <param name="entity">数据实体(接口方法参数)</param>
        /// <param name="dbParas">DbParameter集合</param>
        /// <param name="paraNameList">带参数的sql包含的参数集合</param>
        public void GetDbParaListByEntity(object entity, DbList<DbParameter> dbParas, EList<CKeyValue> sqlParaNameList)
        {
            dbParas.Clear();
            if (null == entity) return;
            CKeyValue kv = null;
            object vObj = null;
            string paraTypeStr = "";
            PropertyInfo[] piArr = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo item in piArr)
            {
                kv = sqlParaNameList[item.Name.ToLower()];
                if (null == kv) continue;
                if (string.IsNullOrEmpty(paraTypeStr))
                {
                    paraTypeStr = kv.other.ToString();
                }

                //throw new Exception("未引入与[" + paraTypeStr + "]类型相关的程序集");
                vObj = item.GetValue(entity, null);

                GetDbParaByBaseType(item.PropertyType, paraTypeStr, vObj, kv.Value.ToString(), dbParas);
            }
        }

        /// <summary>
        /// 用数据实体中属于基本类型(int,string,bool等)的属性及其值来创建DbParameter集合
        /// </summary>
        /// <param name="dataType">数据类型(数据实体属性类型)</param>
        /// <param name="dbTag">数据源参数类型(如:sqlserver参数类型为@, mysql参数类型为?, oracle为:)</param>
        /// <param name="data">数据源属性值</param>
        /// <param name="fieldName">sql语句中字段名称(同时也是数据实体属性名称)</param>
        /// <param name="dbParas">DbParameter集合</param>
        public void GetDbParaByBaseType(Type dataType, string dbTag, object data, string fieldName, DbList<DbParameter> dbParas)
        {
            if (!DJTools.IsBaseType(dataType)) return;

            object val = DJTools.ConvertTo(data, dataType);

            val = null == val ? DBNull.Value : val;
            dbParas.Add(fieldName, val);
        }

        public void GetDbParaByDataRow(DataRow row, DbList<DbParameter> dbParas, EList<CKeyValue> sqlParaNameList, EList<CKeyValue> tableColumns)
        {
            dbParas.Clear();
            if (null == row) return;
            CKeyValue kv = null;
            Type type = null;
            object vObj = null;
            string paraTypeStr = "";
            foreach (var item in sqlParaNameList)
            {
                kv = tableColumns[item.Key];
                if (null == kv) continue;

                if (string.IsNullOrEmpty(paraTypeStr))
                {
                    paraTypeStr = item.other.ToString();
                }

                vObj = row[kv.Value.ToString()];
                type = kv.other as Type;
                type = null == type ? typeof(string) : type;
                GetDbParaByBaseType(type, paraTypeStr, vObj, item.Value.ToString(), dbParas);
            }
        }

        public static List<ErrorLevels> errorLevels1 { get; set; }

        /// <summary>
        /// 设置数据提供者程序集相对路径(dll文件相对路径),默认为当前程序集
        /// </summary>
        /// <param name="dllRelativePath"></param>
        public static void SetDataProviderAssemble(string dllRelativePath)
        {
            _sqlProviderRelativePath = dllRelativePath;
        }

        public static AutoCall GetDataOptAutoCall(object[] attributes)
        {
            AutoCall autoCall = null;
            foreach (var item in attributes)
            {
                if(null != (item as AutoCall))
                {
                    autoCall = (AutoCall)item;
                    break;
                }
            }
            return autoCall;
        }

        public static string RootPath { get; set; }

        public static getDllAbsolutePathByRelativePath GetDllAbsolutePathByRelativePath { get; set; }
    }
}
