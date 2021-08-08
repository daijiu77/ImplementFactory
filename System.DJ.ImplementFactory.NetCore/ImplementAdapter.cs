using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.DotNetCore.CodeCompiler;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using static System.DJ.ImplementFactory.Commons.Attrs.AutoCall;

namespace System.DJ.ImplementFactory
{
    /// <summary>
    /// Author: 代久 - Allan
    /// QQ: 564343162
    /// Email: 564343162@qq.com
    /// CreateDate: 2020-03-05
    /// </summary>
    public abstract class ImplementAdapter
    {
        object currentObj = null;

        private static readonly string configFile = "ImplementFactory.config";
        private static readonly string configFile_Xml = "ImplementFactory.xml";
        private static string dbConnectionFreeStrategy = "";
        private static EList<CKeyValue> matchRules = null;
        private static readonly DbInfo dbInfo = new DbInfo();
        private static List<ErrorLevels> errorLevels1 = new List<ErrorLevels>();
        private static List<Assembly> assemblies = null;
        private static List<Assembly> assembliesOfTemp = new List<Assembly>();
        private static Type UserType = null;
        private static IMicroServiceMethod microServiceMethod = null;

        private static Dictionary<string, InstanceObj> interfaceImplements = new Dictionary<string, InstanceObj>();

        public static readonly SysConfig sysConfig1 = new SysConfig();

        static ImplementAdapter()
        {
            int n = 2;
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(n);
            while (null == stackFrame && 0 <= n)
            {
                n--;
                stackFrame = trace.GetFrame(n);
            }

            if (null != stackFrame)
            {
                UserType = stackFrame.GetMethod().DeclaringType;
            }
            else
            {
                UserType = typeof(ImplementAdapter);
            }

            loadSysInstance();
            init();
        }

        /// <summary>
        /// 多继承的情况, 可以考虑使用此方法注册当前类
        /// </summary>
        /// <param name="currentObj"></param>
        public static void Register(object currentObj)
        {
            new ImplAdapter(currentObj);
        }

        static void init()
        {
            if (string.IsNullOrEmpty(rootPath)) return;

            matchRules = MatchRules();
            AutoCall.errorLevels1 = errorLevels1;

            getTempBin();
            clearTempImplBin();

            TempImpl tempImpl = new TempImpl();
            tempImpl.SetRootPath(rootPath);
            tempImpl.IsShowCodeOfDataResourceDLL = dbInfo.IsShowCode;

            DataAdapter.SetConfig(dbInfo.ConnectionString, dbInfo.DatabaseType);

            string binPath = DJTools.isWeb ? (rootPath + "\\bin") : rootPath;
            assemblies = DJTools.GetAssemblyCollection(binPath, new string[] { "/TempImpl/bin/" });
            AutoCall.AssemblyCollection = assemblies;
            AutoCall.SetDataProviderAssemble(dbInfo.SqlProviderRelativePathOfDll);
            AutoCall.RootPath = rootPath;
            AutoCall.GetDllAbsolutePathByRelativePath = GetDllAbsolutePathByRelativePath;

            DbList<Data.Common.DbParameter>.dataServerProvider = dataServerProvider;

            if (null != DbHelper)
            {
                DbHelper.connectString = dbInfo.ConnectionString;
                DbHelper.optByBatchMaxNumber = dbInfo.optByBatchMaxNumber;
                DbHelper.optByBatchWaitSecond = dbInfo.optByBatchWaitSecond;
                DbHelper.sqlMaxLengthForBatch = dbInfo.sqlMaxLengthForBatch;
                DbHelper.disposableAndClose = dbInfo.close;
                if (!string.IsNullOrEmpty(dbConnectionFreeStrategy))
                {
                    DbHelper.disposableAndClose = DbConnectionFreeStrategy.disposeAndClose == dbInfo.dbConnectionFreeStrategy;
                }
                DbHelper.isNormalBatchInsert = InsertBatchStrategy.normalBatch == dbInfo.insertBatchStrategy;
            }
        }

        static T loadInterfaceInstance<T>(string likeName, Type[] excludeTypes, ref Assembly asse)
        {
            asse = null;
            object _obj = default(T);
            string binPath = DJTools.isWeb ? (rootPath + "\\bin") : rootPath;
            string[] files = Directory.GetFiles(binPath, "*.dll");
            string file = "";
            string interfaceName = typeof(T).FullName;
            if (!string.IsNullOrEmpty(likeName))
            {
                Regex rg = new Regex(@".*" + likeName + @".*\.dll$", RegexOptions.IgnoreCase);
                foreach (var item in files)
                {
                    if (rg.IsMatch(item))
                    {
                        file = item;
                        break;
                    }
                }
            }

            bool isExist = false;
            Action<Type[]> action = types1 =>
            {
                foreach (var type in types1)
                {
                    if (type.IsAbstract) continue;
                    if (type.IsInterface) continue;
                    if (null == type.GetInterface(interfaceName)) continue;
                    if (null != excludeTypes)
                    {
                        isExist = false;
                        foreach (var excludeType in excludeTypes)
                        {
                            if (null == excludeType) continue;
                            if (type.Equals(excludeType))
                            {
                                isExist = true;
                                break;
                            }
                        }
                        if (isExist) continue;
                    }

                    try
                    {
                        _obj = (T)Activator.CreateInstance(type);
                    }
                    catch { }
                    if (null != _obj) break;
                }
            };

            Type[] types = null;
            if (!string.IsNullOrEmpty(file))
            {
                try
                {
                    asse = Assembly.LoadFrom(file);
                    types = asse.GetTypes();
                    action(types);
                }
                catch { }

                if (null == _obj) return default(T);
            }

            if (null == _obj)
            {
                foreach (var item in files)
                {
                    try
                    {
                        asse = Assembly.LoadFrom(item);
                        types = asse.GetTypes();
                        action(types);
                    }
                    catch { }
                    if (null != _obj) break;
                }
            }

            if (null == _obj) return default(T);

            return (T)_obj;
        }

        static void loadSysInstance()
        {
            if (string.IsNullOrEmpty(rootPath)) return;

            Func<string, Type, Type> getInstanceTypeByInterfaceType = (filePath, interfaceType) =>
            {
                if (!File.Exists(filePath)) return null;
                Type type2 = null;
                Assembly assembly1 = Assembly.LoadFrom(filePath);
                Type[] ts = null;
                try
                {
                    ts = assembly1.GetTypes();
                }
                catch { }
                if (null == ts) return type2;
                string fName = interfaceType.FullName;
                foreach (Type item in ts)
                {
                    if (null != item.GetInterface(fName))
                    {
                        type2 = item;
                        break;
                    }
                }
                return type2;
            };

            Assembly asse = null;
            IDataServerProvider dsp = null;
            Type dspType = null;

            string f = Assembly.GetExecutingAssembly().GetName().Name + ".dll";
            if (DJTools.isWeb) f = "bin\\" + f;
            f = Path.Combine(rootPath, f);
            Type type1 = getInstanceTypeByInterfaceType(f, typeof(IInstanceCodeCompiler));

            codeCompiler = loadInterfaceInstance<IInstanceCodeCompiler>("CodeCompiler", new Type[] { type1 }, ref asse);
            if (null == codeCompiler) codeCompiler = new CodeCompiler();
            codeCompiler.SetRootPath(rootPath);

            dspType = getInstanceTypeByInterfaceType(f, typeof(IDataServerProvider));
            try
            {
                dsp = (IDataServerProvider)Activator.CreateInstance(dspType);
            }
            catch (Exception)
            {

                //throw;
            }

            type1 = getInstanceTypeByInterfaceType(f, typeof(IDataServerProvider));
            Type[] dsTypes = new Type[] { dspType, type1 };

            Assembly asse1 = null;
            DbHelper = loadInterfaceInstance<IDbHelper>("DbHelper", new Type[] { typeof(DbHelper) }, ref asse1);
            if (null == DbHelper) DbHelper = new DbHelper();

            Assembly asse2 = null;
            dataServerProvider = loadInterfaceInstance<IDataServerProvider>("DataServerProvider", dsTypes, ref asse2);
            if (null == dataServerProvider) dataServerProvider = dsp;
            DbHelper.dataServerProvider = dataServerProvider;

            Assembly asse3 = null;
            dbConnectionState = loadInterfaceInstance<IDbConnectionState>("ConnectionState", null, ref asse3);
            DbHelper.dbConnectionState = dbConnectionState;

            microServiceMethod = loadInterfaceInstance<IMicroServiceMethod>("", null, ref asse3);
        }

        static void getTempBin()
        {
            if (0 < assembliesOfTemp.Count) return;

            string dir = Path.Combine(rootPath, TempImpl.dirName);
            dir = Path.Combine(dir, TempImpl.libName);

            if (Directory.Exists(dir))
            {
                Assembly asse = null;
                string[] fs = Directory.GetFiles(dir, "*.dll");
                foreach (var item in fs)
                {
                    try
                    {
                        asse = Assembly.LoadFrom(item);
                        assembliesOfTemp.Add(asse);
                    }
                    catch { }
                }
            }
        }

        public static string GetRootPath()
        {
            return ImplementAdapter.rootPath;
        }

        static string _rootPath = "";
        public static string rootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_rootPath))
                {
                    _rootPath = DJTools.RootPath;
                }
                return _rootPath;
            }
        }

        public static IDbHelper DbHelper { get; set; }

        public static IDataServerProvider dataServerProvider { get; set; }

        public static IInstanceCodeCompiler codeCompiler { get; set; }

        public static IDbConnectionState dbConnectionState { get; set; }

        private static string _connStr = "";
        public static string dbConnectionString
        {
            get { return _connStr; }
            set
            {
                _connStr = value;
                if (null != DbHelper)
                {
                    DbHelper.connectString = _connStr;
                }
            }
        }

        public static string GetDllAbsolutePathByRelativePath(string dllRelativePath)
        {
            string dllFile = "";
            dllRelativePath = dllRelativePath.Replace("/", "\\");
            if (dllRelativePath.Substring(0, 1) == "\\")
            {
                dllRelativePath = dllRelativePath.Substring(1);
            }

            if (-1 != dllRelativePath.ToLower().LastIndexOf(".dll"))
            {
                if (dllRelativePath.ToLower().LastIndexOf(".dll") != dllRelativePath.Length - 4)
                {
                    dllRelativePath += ".dll";
                }
            }
            else
            {
                dllRelativePath += ".dll";
            }

            dllFile = Path.Combine(rootPath, dllRelativePath);
            return dllFile;
        }

        public static void Destroy()
        {
            if (null == DbHelper) return;
            if (null == (DbHelper as IDisposable)) return;
            ((IDisposable)DbHelper).Dispose();
        }

        static SynchronizationContext _SynicContext = null;
        public static SynchronizationContext SynicContext
        {
            get
            {
                if (null == _SynicContext) _SynicContext = SynchronizationContext.Current;
                return _SynicContext;
            }
            set
            {
                _SynicContext = value;
            }
        }

        public ImplementAdapter()
        {
            currentObj = this;
            try
            {
                Adapter();
            }
            catch (Exception ex)
            {
                new AutoCall().e(ex.ToString());
                //throw;
            }
        }

        private ImplementAdapter(object currentObj)
        {
            this.currentObj = currentObj;
            try
            {
                Adapter();
            }
            catch (Exception ex)
            {
                new AutoCall().e(ex.ToString());
                //throw;
            }
        }

        class ImplAdapter : ImplementAdapter
        {
            public ImplAdapter(object currentObj) : base(currentObj) { }
        }

        static object _adapterOfImplement = new object();
        void Adapter()
        {
            if (string.IsNullOrEmpty(rootPath)) return;

            object[] arr = null;

            string resetKeyName = "";
            Type interfaceType = null;
            Type implType = null;
            CKeyValue kv = null;
            MatchRule mr = null;
            object impl = null;
            Type implNew = null;
            object impl_1 = null;

            bool isShowCode = false;
            bool isSingleCall = false;
            bool isSingleInstance = false;
            bool isUnSingleInstance = false;
            InstanceObj instanceObj = null;

            string unSingleInstanceStr = typeof(IUnSingleInstance).FullName;

            TempImpl temp = new TempImpl();
            temp.codeCompiler = codeCompiler;
            temp.IsShowCodeOfAll = sysConfig1.IsShowCode;

            bool enableCompiler = false;
            if (null != codeCompiler && null != dataServerProvider && null != DbHelper)
            {
                enableCompiler = true;
            }

            Action<object, Action> action = (obj, action1) =>
            {
                if (null != obj)
                {
                    if (null != (obj as IUnSingleInstance))
                    {
                        isUnSingleInstance = true;
                        isSingleCall = false;
                        isSingleInstance = false;
                    }

                    bool mboolSg = false;
                    if (null != (obj as Type))
                    {
                        string cl = DJTools.GetClassName(typeof(ISingleInstance), true);
                        mboolSg = null != ((Type)obj).GetInterface(cl);
                    }

                    if ((null != (obj as ISingleInstance) || isSingleCall || mboolSg) && false == isUnSingleInstance)
                    {
                        isSingleCall = true;
                        isSingleInstance = true;
                        action1?.Invoke();
                    }
                }
            };

            Func<Type, Type, bool> func_IsCompile = (interfac_type, impl_type) =>
            {
                Type t1 = null;
                if (null != impl_type && null != interfac_type)
                {
                    if (interfac_type.IsInterface)
                    {
                        try
                        {
                            PropertyInfo pi = impl_type.GetProperty(TempImpl.InterfaceInstanceType);
                            if (null != pi)
                            {
                                t1 = pi.PropertyType;
                            }
                        }
                        catch (Exception ex)
                        {
                            //throw;
                        }

                        if (null == t1)
                        {
                            if (null != impl_type.GetInterface(typeof(IDataInterface).FullName))
                            {
                                t1 = typeof(IDataInterface);
                            }
                        }
                    }
                    else
                    {
                        t1 = typeof(object);
                    }
                }

                return (sysConfig1.Recomplie || null == impl_type || null == t1);
            };

            Regex rg = null;
            string implName = "";
            bool isIgnoreCase = false;

            AutoCall autoCall = null;
            FieldInfo[] fArr = null; // currentObj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            List<Type> objList = new List<Type>();
            Type _objType = currentObj.GetType();
            int n = 0;
            while (20 > n && typeof(object) != _objType && typeof(ImplementAdapter) != _objType)
            {
                objList.Add(_objType);
                _objType = _objType.BaseType;
                n++;
            }

            if (0 == objList.Count && null != currentObj)
            {
                objList.Add(currentObj.GetType());
            }

            foreach (Type typeItem in objList)
            {
                fArr = typeItem.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (FieldInfo p in fArr)
                {
                    arr = p.GetCustomAttributes(typeof(AutoCall), true);
                    if (0 == arr.Length) continue;

                    autoCall = null;
                    foreach (var item in arr)
                    {
                        if (null != (item as AutoCall))
                        {
                            autoCall = (AutoCall)item;
                            break;
                        }
                    }

                    interfaceType = p.FieldType;

                    if (!autoCall.LoadBeforeFilter(interfaceType)) continue;

                    impl = null;
                    impl_1 = null;
                    implType = null;
                    implNew = null;
                    resetKeyName = null;
                    isSingleInstance = false;
                    isUnSingleInstance = false;
                    instanceObj = null;

                    arr = p.GetCustomAttributes(typeof(SingleCall), true);
                    isSingleCall = 0 < arr.Length;

                    resetKeyName = DJTools.GetClassName(interfaceType, true);
                    interfaceImplements.TryGetValue(resetKeyName, out instanceObj);
                    if (null != instanceObj) impl = instanceObj.newInstance;
                    if (null != instanceObj)
                    {
                        implName = "";
                        rg = null;
                        isIgnoreCase = false;
                        AutoCallMatch(autoCall, ref implName, ref rg, ref isIgnoreCase);
                        if (null != rg)
                        {
                            if (!MatchImpl(rg, instanceObj.oldInstanceType, implName, isIgnoreCase)) impl = null;
                        }
                    }
                    impl_1 = impl;

                    action(impl, () =>
                    {
                        impl = null;
                    });

                    if (null == impl)
                    {
                        kv = GetKvByInterfaceType(interfaceType);
                        mr = null == kv ? null : ((MatchRule)kv.Value);
                        isShowCode = false;
                        if (interfaceType.IsInterface)
                        {
                            Attribute msAtt = interfaceType.GetCustomAttribute(typeof(MicroServiceRoute), true);
                            if (null != msAtt)
                            {
                                if (null != microServiceMethod)
                                {
                                    MicroServiceRoute microServiceRoute = (MicroServiceRoute)msAtt;
                                    implType = microServiceMethod.GetMS(codeCompiler, autoCall, microServiceRoute, interfaceType);
                                }
                                else
                                {
                                    autoCall.e("未引入微服务组件", ErrorLevels.severe);
                                    continue;
                                }
                            }

                            if (null == implType)
                            {
                                implType = GetImplementTypeOfTemp(interfaceType, autoCall);
                                if (null == implType)
                                {
                                    if (null != mr)
                                    {
                                        isShowCode = mr.IsShowCode;
                                        implType = LoadImplementTypeByMatchRule(mr, interfaceType, autoCall);
                                    }
                                    else
                                    {
                                        implType = LoadImplementTypeByInterface(interfaceType, autoCall);
                                    }

                                    if (null == implType)
                                    {
                                        implType = LoadImplementTypeByAssemblies(interfaceType, autoCall);
                                    }

                                    if (enableCompiler && null == (autoCall as ExistCall))
                                    {
                                        if (func_IsCompile(interfaceType, implType))
                                        {
                                            implNew = temp.NewImplement(interfaceType, implType, autoCall, isShowCode, false);
                                        }
                                    }
                                }
                            }
                            
                            if (null == impl)
                            {
                                try
                                {
                                    if (null != implNew)
                                    {
                                        impl = Activator.CreateInstance(implNew);
                                        assembliesOfTemp.Add(implNew.Assembly);
                                    }
                                    else if (null != implType)
                                    {
                                        impl = Activator.CreateInstance(implType);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string err = "[" + implType.FullName + "] 实例可能缺少一个无参构造函数(或该类访问权限不够)\r\n" + ex.ToString();
                                    autoCall.ExecuteException(interfaceType, null, null, null, new Exception(err));
                                    //throw;
                                }
                            }
                        }
                        else
                        {
                            implType = GetImplementTypeOfTemp(interfaceType, autoCall);
                            if (null == implType)
                            {
                                implType = LoadImplementTypeByAssemblies(interfaceType);

                                if (enableCompiler)
                                {
                                    if (func_IsCompile(interfaceType, implType))
                                    {
                                        implType = interfaceType;
                                        interfaceType = typeof(IEmplyInterface);
                                        implNew = temp.NewImplement(interfaceType, implType, autoCall, isShowCode, false);
                                    }
                                }
                            }

                            if (null == impl)
                            {
                                try
                                {
                                    if (null != implNew)
                                    {
                                        impl = Activator.CreateInstance(implNew);
                                        assembliesOfTemp.Add(implNew.Assembly);
                                    }
                                    else if (null != implType)
                                    {
                                        impl = Activator.CreateInstance(implType);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string err = "[" + implType.FullName + "] 实例可能缺少一个无参构造函数(或该类访问权限不够)\r\n" + ex.ToString();
                                    autoCall.ExecuteException(interfaceType, null, null, null, new Exception(err));
                                    //throw;
                                }
                            }
                        }

                        if (false == isUnSingleInstance)
                        {
                            action(impl, null);
                        }

                        if (null != impl && null == impl_1 && false == isSingleCall)
                        {
                            lock (_adapterOfImplement)
                            {
                                interfaceImplements.TryGetValue(resetKeyName, out instanceObj);
                                if (null == instanceObj)
                                {
                                    interfaceImplements.Add(resetKeyName, new InstanceObj()
                                    {
                                        newInstance = impl,
                                        oldInstanceType = implType
                                    });
                                }
                                else
                                {
                                    if (null != (impl as IDisposable)) ((IDisposable)impl).Dispose();
                                    impl = instanceObj.newInstance;
                                }
                            }
                        }
                    }

                    if (null == impl) continue;

                    isSingleInstance = false;
                    if (null != (impl as ISingleInstance))
                    {
                        isSingleInstance = true;
                    }

                    if (isSingleInstance)
                    {
                        if (null == ((ISingleInstance)impl).Instance)
                        {
                            ((ISingleInstance)impl).Instance = impl;
                        }
                    }

                    if (!autoCall.LoadAfterFilter(impl)) continue;

                    p.SetValue(currentObj, impl);
                }
            }

        }

        CKeyValue GetKvByInterfaceType(Type interfaceType)
        {
            CKeyValue kv = null;
            string interfaceNS = interfaceType.Namespace;
            string interfaceName = interfaceType.Name;
            string interfaceFile = interfaceNS + "." + interfaceName;
            kv = matchRules[interfaceFile];
            if (null == kv)
            {
                kv = matchRules[interfaceName];
            }

            if (null != kv) return kv;

            int n = 0;
            string s = "";
            string s1 = "";
            while (-1 != interfaceFile.IndexOf(".") && 20 > n)
            {
                s += "." + interfaceFile.Substring(0, interfaceFile.IndexOf("."));
                s1 = s.Substring(1);
                kv = matchRules[s1];
                if (null != kv) break;
                interfaceFile = interfaceFile.Substring(interfaceFile.IndexOf(".") + 1);
                n++;
            }

            return kv;
        }

        void AutoCallMatch(AutoCall autoCall, ref string implName, ref Regex regex, ref bool isIgnoreCase1)
        {
            string fn1 = autoCall.MatchRuleOrClassName;

            fn1 = string.IsNullOrEmpty(fn1) ? implName : fn1;
            fn1 = null == fn1 ? "" : fn1;
            fn1 = fn1.Trim();

            bool isIgnoreCase = isIgnoreCase1;
            if (IgnoreLU.none != autoCall.IgnoreCase)
            {
                isIgnoreCase = IgnoreLU.is_true == autoCall.IgnoreCase ? true : isIgnoreCase;
                isIgnoreCase = IgnoreLU.is_false == autoCall.IgnoreCase ? false : isIgnoreCase;
            }

            Regex rg = new Regex(@"[^0-9a-z_\s]", RegexOptions.IgnoreCase);
            Regex rg1 = null;
            if (rg.IsMatch(fn1))
            {
                if (isIgnoreCase)
                {
                    rg1 = new Regex(fn1, RegexOptions.IgnoreCase);
                }
                else
                {
                    rg1 = new Regex(fn1);
                }
            }
            else
            {
                fn1 = isIgnoreCase ? fn1.ToLower() : fn1;
            }

            if (string.IsNullOrEmpty(fn1)) return;
            implName = fn1;
            regex = rg1;
            isIgnoreCase1 = isIgnoreCase;
        }

        bool MatchImpl(Regex rg1, Type implType, string implName, bool isIgnoreCase)
        {
            bool mbool = true;
            if (!string.IsNullOrEmpty(implName))
            {
                string fn2 = implType.Name;
                fn2 = isIgnoreCase ? fn2.ToLower() : fn2;
                if (null != rg1)
                {
                    if (!rg1.IsMatch(fn2)) mbool = false;
                }
                else if (!implName.Equals(fn2))
                {
                    mbool = false;
                }
            }
            return mbool;
        }

        Type GetImplementTypeOfTemp(Type interfaceType, AutoCall autoCall)
        {
            string fn1 = "";
            Regex rg1 = null;
            bool isIgnoreCase = false;
            AutoCallMatch(autoCall, ref fn1, ref rg1, ref isIgnoreCase);

            Type[] types = null;
            Type impl_type = null;
            foreach (var item in assembliesOfTemp)
            {
                try
                {
                    types = item.GetTypes();
                    impl_type = GetImplTypeByTypes(types, interfaceType, autoCall, rg1, fn1, isIgnoreCase);
                }
                catch { }

                if (null != impl_type) break;
            }
            return impl_type;
        }

        Type GetImplTypeByTypes(Type[] types, Type interfaceType, AutoCall autoCall, Regex rg1, string implName, bool isIgnoreCase)
        {
            Type impl_type = null;
            Type type1 = null;
            foreach (Type t in types)
            {
                if (false == t.IsImplementInterface(interfaceType)) continue;
                type1 = null;
                try
                {
                    PropertyInfo pi = t.GetProperty(TempImpl.InterfaceInstanceType);
                    if (null != pi)
                    {
                        type1 = pi.PropertyType;
                    }
                }
                catch (Exception)
                {
                    //throw;
                }

                if (null != type1)
                {
                    if (!MatchImpl(rg1, type1, implName, isIgnoreCase)) continue;
                }
                else
                {
                    if (!MatchImpl(rg1, t, implName, isIgnoreCase)) continue;
                }
                impl_type = t;

                break;
            }
            return impl_type;
        }

        Type LoadImplementTypeByInterface(Type interfaceType, AutoCall autoCall)
        {
            Assembly objAss = null;
            Type[] types = null;

            string fn1 = "";
            Regex rg1 = null;
            bool isIgnoreCase = false;
            AutoCallMatch(autoCall, ref fn1, ref rg1, ref isIgnoreCase);

            objAss = interfaceType.Assembly;
            types = objAss.GetTypes();
            Type impl_type = GetImplTypeByTypes(types, interfaceType, autoCall, rg1, fn1, isIgnoreCase);

            return impl_type;
        }

        Type LoadImplementTypeByMatchRule(MatchRule mr, Type interfaceType, AutoCall autoCall)
        {
            Type impl_type = null;
            Assembly objAss = null;
            string err = "";
            string DllRelativePathOfImpl = mr.DllRelativePathOfImpl;
            if (!string.IsNullOrEmpty(DllRelativePathOfImpl))
            {
                string dllFile = GetDllAbsolutePathByRelativePath(DllRelativePathOfImpl);
                if (!File.Exists(dllFile)) return null;

                try
                {
                    objAss = Assembly.LoadFrom(dllFile);
                }
                catch (Exception ex)
                {
                    err = "加载程序集[" + dllFile + "]出错\r\n" + ex.ToString();
                    autoCall.ExecuteException(interfaceType, null, null, null, new Exception(err));
                }
            }
            else
            {
                objAss = interfaceType.Assembly; //Assembly.GetExecutingAssembly(); //当前应用程序集
            }

            if (null == objAss)
            {
                err = "加载程序集失败";
                autoCall.ExecuteException(interfaceType, null, null, null, new Exception(err));
                return impl_type;
            }

            Regex rg1 = null;
            string mie = mr.MatchImplExpression;
            bool isIgnoreCase = mr.IgnoreCase;

            AutoCallMatch(autoCall, ref mie, ref rg1, ref isIgnoreCase);

            if ((!string.IsNullOrEmpty(mr.ImplementNameSpace)) && null == rg1)
            {
                string s = mr.ImplementNameSpace + "." + mie;
                Type type = null;
                try
                {
                    type = objAss.GetType(s);
                }
                catch { }
                if (null == type) return impl_type;
                if (type.IsImplementInterface(interfaceType))
                {
                    impl_type = type;
                }

                return impl_type;
            }

            string fn1 = isIgnoreCase ? mie.ToLower() : mie;

            Type[] types = null;
            try
            {
                types = objAss.GetTypes();
            }
            catch { }

            impl_type = GetImplTypeByTypes(types, interfaceType, autoCall, rg1, fn1, isIgnoreCase);

            return impl_type;
        }

        Type LoadImplementTypeByAssemblies(Type interfaceType, AutoCall autoCall)
        {
            if (null == assemblies) return null;
            if (0 == assemblies.Count) return null;

            Type impl_type = null;
            string fn1 = "";
            Regex rg1 = null;
            bool isIgnoreCase = false;
            AutoCallMatch(autoCall, ref fn1, ref rg1, ref isIgnoreCase);

            InstanceObj impl1 = null;
            Type type1 = null;
            Type[] types = null;

            foreach (KeyValuePair<string, InstanceObj> item in interfaceImplements)
            {
                impl1 = item.Value;
                if (null == impl1) continue;

                type1 = impl1.newInstance.GetType();
                if (false == type1.IsImplementInterface(interfaceType)) continue;

                if (!MatchImpl(rg1, type1, fn1, isIgnoreCase)) continue;

                impl_type = impl1.newInstance.GetType();
                break;
            }

            if (null != impl_type) return impl_type;

            foreach (Assembly item in assemblies)
            {
                try
                {
                    types = item.GetTypes();
                    impl_type = GetImplTypeByTypes(types, interfaceType, autoCall, rg1, fn1, isIgnoreCase);
                }
                catch { }

                if (null != impl_type) break;
            }

            return impl_type;
        }

        Type LoadImplementTypeByAssemblies(Type classType)
        {
            if (null == assemblies) return null;
            if (0 == assemblies.Count) return null;

            Type type = null;
            Type[] ts = null;
            foreach (Assembly ass in assemblies)
            {
                ts = ass.GetTypes();
                foreach (Type t in ts)
                {
                    if (t.BaseType == classType)
                    {
                        type = t;
                        break;
                    }
                }
                if (null != type) break;
            }
            return type;
        }

        /// <summary>
        /// 配置文件配置规则(多项配置换行)：
        /// {DllRelativePathOfImpl="BLL.dll",ImplementNameSpace="BLL.SaleOrder",MatchImplExpression="^data.+",InterFaceName="IGetSaleOrderInfo",IgnoreCase=true}
        /// {DllRelativePathOfImpl="BLL.dll",ImplementNameSpace="BLL.MemberManage.impl",MatchImplExpression="^produce.+",InterFaceName="BLL.MemberManage.IProduceMaintain",IgnoreCase=true}
        /// </summary>
        /// <returns></returns>
        private static EList<CKeyValue> MatchRules()
        {
            EList<CKeyValue> list = new EList<CKeyValue>();
            string file = Path.Combine(rootPath, configFile_Xml);
            string file1 = Path.Combine(rootPath, configFile);

            if (false == File.Exists(file) && false == File.Exists(file1))
            {
                createXmlConfig();
            }

            if (File.Exists(file))
            {
                list = MatchRulesOfXml();
                return list;
            }

            file = Path.Combine(rootPath, configFile);
            if (!File.Exists(file))
            {
                defaultConfig(file);
            }

            if (!File.Exists(file)) return list;

            LogsRange logsRange1 = new LogsRange();
            string[] arr = File.ReadAllLines(file);
            MatchRule mr = null;

            string FieldName = "";
            string FieldValue = "";
            int n = 0;
            Match m = null;
            PropertyInfo pi = null;
            object v = null;
            RuleType tag = RuleType.none;
            object entity = null;

            string _dbConnStrategy = "dbConnectionFreeStrategy".ToLower();

            string s = "";// @"(?<FieldName>[^\{\=\,\s]+)\s*\=\s*((""(?<FieldValue>[^""\}\,]+)"")|(?<FieldValue>[^""\}\=\,\s]+))";
            s = @"(?<FieldName>[^\{\=\,\s]+)\s*\=\s*((""(?<FieldValue>[^""]+)"")|(?<FieldValue>[^""\}\=\,\s]+))";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            foreach (string item in arr)
            {
                s = item;
                n = 0;
                tag = RuleType.none;
                while (rg.IsMatch(s) && 20 > n)
                {
                    m = rg.Match(s);
                    FieldName = m.Groups["FieldName"].Value;
                    FieldValue = m.Groups["FieldValue"].Value;
                    if (0 == n)
                    {
                        pi = GetPropertyInfoByName(typeof(DbInfo), FieldName);
                        tag = null != pi ? RuleType.DbInfo : tag;

                        if (null == pi)
                        {
                            pi = GetPropertyInfoByName(typeof(MatchRule), FieldName);
                            tag = null != pi ? RuleType.MatchRule : tag;
                            if (RuleType.MatchRule == tag)
                            {
                                mr = new MatchRule();
                            }
                        }

                        if (null == pi)
                        {
                            pi = GetPropertyInfoByName(typeof(LogsRange), FieldName);
                            tag = null != pi ? RuleType.LogsRange : tag;
                        }

                        if (null == pi)
                        {
                            pi = GetPropertyInfoByName(typeof(SysConfig), FieldName);
                            tag = null != pi ? RuleType.SysConfig : tag;
                        }
                    }

                    if (RuleType.DbInfo == tag)
                    {
                        if (FieldName.ToLower().Equals(_dbConnStrategy))
                        {
                            dbConnectionFreeStrategy = FieldName;
                        }
                        pi = GetPropertyInfoByName(typeof(DbInfo), FieldName);
                        entity = ImplementAdapter.dbInfo;
                    }
                    else if (RuleType.MatchRule == tag)
                    {
                        pi = GetPropertyInfoByName(typeof(MatchRule), FieldName);
                        entity = mr;
                    }
                    else if (RuleType.LogsRange == tag)
                    {
                        pi = GetPropertyInfoByName(typeof(LogsRange), FieldName);
                        entity = logsRange1;
                    }
                    else if (RuleType.SysConfig == tag)
                    {
                        pi = GetPropertyInfoByName(typeof(SysConfig), FieldName);
                        entity = sysConfig1;
                    }

                    if (null != pi)
                    {
                        v = null;
                        if (pi.PropertyType.IsEnum)
                        {
                            string _fv = "";
                            string[] _fs = Enum.GetNames(pi.PropertyType);
                            Array _arr = Enum.GetValues(pi.PropertyType);
                            int _len = _fs.Length;
                            Regex _rg = new Regex(@"^[0-9]$", RegexOptions.IgnoreCase);
                            if (_rg.IsMatch(FieldValue))
                            {
                                int _n = Convert.ToInt32(FieldValue);
                                int _n1 = 0;
                                for (int i = 0; i < _len; i++)
                                {
                                    _n1 = Convert.ToInt32(_arr.GetValue(i));
                                    if (_n1 == _n)
                                    {
                                        v = _arr.GetValue(i);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                _fv = FieldValue.ToLower();
                                for (int i = 0; i < _len; i++)
                                {
                                    if (_fs[i].ToLower().Equals(_fv))
                                    {
                                        v = _arr.GetValue(i);
                                        break;
                                    }
                                }
                            }

                            if (null == v)
                            {
                                v = DbConnectionFreeStrategy.onlyDispose;
                                dbConnectionFreeStrategy = "";
                            }
                        }
                        else
                        {
                            v = DJTools.ConvertTo(FieldValue, pi.PropertyType);
                        }

                        try
                        {
                            entity.GetType().GetProperty(pi.Name).SetValue(entity, v, null);
                        }
                        catch { }
                    }

                    s = s.Replace(m.Groups[0].Value, "");
                    n++;
                }

                if (RuleType.MatchRule == tag)
                {
                    if (string.IsNullOrEmpty(mr.MatchImplExpression)) continue;
                    if (string.IsNullOrEmpty(mr.InterFaceName)) continue;

                    list.Add(new CKeyValue() { Key = mr.InterFaceName, Value = mr });
                    mr = null;
                }
            }

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

            rg = new Regex(@"Recomplie\s*\=\s*true", RegexOptions.IgnoreCase);
            string txt = File.ReadAllText(file);
            txt = rg.Replace(txt, "Recomplie=false");
            File.WriteAllText(file, txt);

            if (DJTools.IsDebug(UserType))
            {
                sysConfig1.Recomplie = true;
            }

            if (sysConfig1.Recomplie)
            {
                clearTempImplBin();
            }

            return list;
        }

        private static EList<CKeyValue> MatchRulesOfXml()
        {
            EList<CKeyValue> list = new EList<CKeyValue>();

            string f = Path.Combine(rootPath, configFile_Xml);
            XmlDocument doc = new XmlDocument();
            doc.Load(f);

            if (1 >= doc.ChildNodes.Count) return list;

            LogsRange logsRange1 = new LogsRange();
            XmlNode node = doc.ChildNodes[1];
            string nodeName = "";
            object entity = null;

            string LogsRange = "LogsRange".ToLower();
            string SysConfig = "SysConfig".ToLower();
            string MatchRules = "MatchRules".ToLower();
            string Recomplie = "Recomplie".ToLower();

            Action<XmlNode, object> action = (_node, _obj) =>
             {
                 string node_name = "";
                 string v = null;
                 foreach (XmlNode item in _node.ChildNodes)
                 {
                     node_name = item.Name.ToLower();
                     v = item.InnerText.Trim();
                     _obj.SetPropertyValue(node_name, v);

                     if (node_name.Equals(Recomplie))
                     {
                         item.InnerText = "false";
                     }
                 }
             };

            foreach (XmlNode item in node.ChildNodes)
            {
                nodeName = item.Name.ToLower();
                if (nodeName.Equals("database"))
                {
                    entity = ImplementAdapter.dbInfo;
                }
                else if (nodeName.Equals(LogsRange))
                {
                    entity = logsRange1;
                }
                else if (nodeName.Equals(SysConfig))
                {
                    entity = ImplementAdapter.sysConfig1;
                }

                if (nodeName.Equals(MatchRules))
                {
                    foreach (XmlNode item1 in item.ChildNodes)
                    {
                        entity = new MatchRule();
                        action(item1, entity);
                        if (!string.IsNullOrEmpty(((MatchRule)entity).InterFaceName))
                        {
                            list.Add(new CKeyValue()
                            {
                                Key = ((MatchRule)entity).InterFaceName,
                                Value = entity
                            });
                        }
                    }
                }
                else
                {
                    action(item, entity);
                }
            }

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

            try
            {
                doc.Save(f);
            }
            catch (Exception)
            {

                //throw;
            }

            if (DJTools.IsDebug(UserType))
            {
                sysConfig1.Recomplie = true;
            }

            if (sysConfig1.Recomplie)
            {
                clearTempImplBin();
            }

            return list;
        }

        private static void clearTempImplBin()
        {
            string f = Path.Combine(rootPath, TempImpl.dirName);
            f = Path.Combine(f, TempImpl.libName);
            if (Directory.Exists(f))
            {
                string[] fs = Directory.GetFiles(f);
                foreach (var item in fs)
                {
                    try
                    {
                        File.Delete(item);
                    }
                    catch (Exception ex)
                    {

                        //throw ex;
                    }
                }
            }
        }

        static PropertyInfo GetPropertyInfoByName(Type type, string propertyName)
        {
            PropertyInfo info = null;
            string pn = propertyName.ToLower();
            PropertyInfo[] arr = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo pi in arr)
            {
                if (pi.Name.ToLower().Equals(pn))
                {
                    info = pi;
                    break;
                }
            }
            return info;
        }

        private static void defaultConfig(string file)
        {
            string ruleStr = GetDefultValueByClass(new MatchRule());

            string dbStr = GetDefultValueByClass(new DbInfo());

            string logs = GetDefultValueByClass(new LogsRange());

            string sysConfig = GetDefultValueByClass(new SysConfig());

            #region 使用说明
            string s1 = "/**<Summary>";
            s1 += "\r\n*1.如果[*必选*]为空,那么将在接口类所在的程序集扫描接口实例; ";
            s1 += "\r\n*2.如果[*必选*]不为空,程序集文件路径(DllRelativePathOfImpl)为空,那么将在接口类所在的程序集扫描接口实例; ";
            s1 += "\r\n*</Summary>";
            s1 += "\r\n*<Param>DllRelativePathOfImpl: [可选] - 实例类所在dll文件的相对路径, 如果为空,表示实例类和exe文件属同一dll文件 </Param>";
            s1 += "\r\n*<Param>ImplementNameSpace - : [可选] - 指定实现interface类的实例所在的namespace </Param>";
            s1 += "\r\n*<Param>MatchImplExpression  : [*必选*] - 匹配实现接口(interface)类的实例名称,可以是一个完整的类名称, 但不包含namespace. 也可以是一个正则表达式 </Param>";
            s1 += "\r\n*<Param>InterFaceName ------ : [*必选*] - 接口名称, 可以是一个 namespace.interfaceClassName 完整的接口名称, 或部分名称空间, 也可是interfaceClassName </Param>";
            s1 += "\r\n*<Param>IgnoreCase --------- : [可选] - 匹配 MatchImplExpression 时是否忽略大小写, 默认false[区分大小写], true[忽略大小写] </Param>";
            s1 += "\r\n*<Param>IsShowCode --------- : [可选] - 是否显示对应临时dll文件的代码, 默认false[不显示], true[显示] </Param>";
            s1 += "\r\n*";
            s1 += "\r\n*<Summary>";
            s1 += "\r\n*数据库访问配置, 如已配置IDbHelper, 则IDbHelper的优先级高于该配置";
            s1 += "\r\n*如果数据库非 Micrsoft Sql Server 数据库，必需重新实现 IDataServerProvider 接口，且该实例为 public ,该实例会被组件会自动加载";
            s1 += "\r\n*</Summary>";
            s1 += "\r\n*<Param>ConnectionString ----------- : [*必须*]数据库接连字符串</Param>";
            s1 += "\r\n*<Param>DatabaseType: -------------- : [*必须*]数据库类型(sqlserver/oracle/mysql/access)</Param>";
            s1 += "\r\n*<Param>SqlProviderRelativePathOfDll : [可选]动态 sql 提供者所在的dll文件程序集的相对路径,注：该提供者必须继承 System.DJ.ImplementFactory.Pipelines.ISqlExpressionProvider 接口</Param>";
            s1 += "\r\n*<Param>optByBatchMaxNumber: ------- : [可选] - insert/update/delete 批量操作最大数量, 默认100条数据 </Param>";
            s1 += "\r\n*<Param>optByBatchWaitSecond: ------ : [可选] - insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒 </Param>";
            s1 += "\r\n*<Param>sqlMaxLengthForBatch: ------ : [可选] - insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000 </Param>";
            s1 += "\r\n*<Param>close: --------------------- : [可选] - 释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接) </Param>";
            s1 += "\r\n*<Param>dbConnectionFreeStrategy: -- : [可选] - 数据库连接释放策略，onlyDispose/disposeAndClose 或整数值: 0/1, close 属性的补充，此属性优先级高于 close 属性</Param>";
            s1 += "\r\n*<Param>InsertBatchStrategy: ------- : [可选] - 缓存时批量插入策略，normalBatch/singleList 或整数值: 0/1, 默认采用通用批量插入</Param>"; //InsertBatchStrategy
            s1 += "\r\n*<Param>IsShowCode: ---------------- : [可选] - 是否显示对应临时dll文件的代码, 默认false[不显示], true[显示]</Param>";
            s1 += "\r\n*";
            s1 += "\r\n*<Summary>";
            s1 += "\r\n*日志输出等级范围, 依次为: 0 severe(严重的), 1 dangerous(危险的), 2 normal(一般的), 3 lesser(次要的), 4 debug(调试)";
            s1 += "\r\n*上限值必须小于或等于下限值";
            s1 += "\r\n*</Summary>";
            s1 += "\r\n*<Param>upperLimit ----------- : 上限值</Param>";
            s1 += "\r\n*<Param>lowerLimit ----------- : 下限值</Param>";
            s1 += "\r\n*";
            s1 += "\r\n*<Summary>";
            s1 += "\r\n*首次加载时是否执行重新编译机制(耗时及有损性能)";
            s1 += "\r\n*</Summary>";
            s1 += "\r\n*<Param>Recompile --------- : 默认值 false, true 重新编译, false 如果已存在则不进行重新编译</Param>";
            s1 += "\r\n*<Param>IsShowCode -------- : 默认值 false, true 显示所有参与编译的代码</Param>";
            s1 += "\r\n**/";
            #endregion

            s1 += "\r\n\r\n" + sysConfig;
            s1 += "\r\n\r\n" + logs;
            s1 += "\r\n\r\n" + dbStr;
            s1 += "\r\n\r\n" + ruleStr;

            try
            {
                File.WriteAllText(file, s1);
            }
            catch (Exception)
            {

                //throw;
            }
        }

        private static string GetDefultValueByClass(object entity)
        {
            string valStr = "";
            string fv = "";
            object vObj = null;
            Type type = entity.GetType();
            PropertyInfo[] arr = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo p in arr)
            {
                vObj = p.GetValue(entity, null);
                if (p.PropertyType.IsEnum)
                {
                    fv = Enum.GetName(p.PropertyType, Convert.ToInt32(vObj));
                }
                else
                {
                    fv = null == vObj ? "" : vObj.ToString();
                    fv = fv.ToLower().Equals("true") ? "true" : fv;
                    fv = fv.ToLower().Equals("false") ? "false" : fv;
                }

                if (p.PropertyType == typeof(string) || p.PropertyType.IsEnum)
                {
                    valStr += "," + p.Name + "=\"" + fv + "\"";
                }
                else
                {
                    valStr += "," + p.Name + "=" + fv;
                }
            }

            if (!string.IsNullOrEmpty(valStr))
            {
                valStr = valStr.Substring(1);
                valStr = "{" + valStr + "}";
            }
            return valStr;
        }

        private static void createXmlConfig()
        {
            string des = "description";
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement XMLroot = doc.CreateElement("configurations");
            doc.AppendChild(XMLroot);

            Dictionary<string, FieldInfo> dic = new Dictionary<string, FieldInfo>();

            Action<object, XmlElement> createChildrenNode = (_obj, parentNode) =>
            {
                dic.Clear();
                FieldInfo[] fields = _obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo item in fields)
                {
                    dic.Add(item.Name, item);
                }

                XmlElement child = null;
                XmlAttribute att = null;
                FieldInfo fi = null;
                object v = null;
                string sv = "";
                _obj.ForeachProperty((propertyInfo, type, fieldName, fieldValue) =>
                {
                    v = null == fieldValue ? "" : fieldValue;
                    sv = v.ToString();
                    sv = sv.ToLower().Trim().Equals("true") ? "true" : sv;
                    sv = sv.ToLower().Trim().Equals("false") ? "false" : sv;
                    child = doc.CreateElement(fieldName);
                    child.InnerText = sv;
                    fi = null;
                    dic.TryGetValue("_" + fieldName, out fi);
                    if (null != fi)
                    {
                        v = fi.GetValue(_obj);
                        v = null == v ? "" : v;
                        att = doc.CreateAttribute(des);
                        att.Value = v.ToString();
                        child.Attributes.Append(att);
                    }
                    parentNode.AppendChild(child);
                });
            };

            XmlElement ele = doc.CreateElement("database");
            DbInfo dbInfo = new DbInfo();
            createChildrenNode(dbInfo, ele);
            XMLroot.AppendChild(ele);

            LogsRange logsRange = new LogsRange();
            ele = doc.CreateElement("LogsRange");
            XmlAttribute attribute = doc.CreateAttribute(des);
            attribute.Value = "日志策略,依次为: 0 severe(严重的), 1 dangerous(危险的), 2 normal(一般的), 3 lesser(次要的), 4 debug(调试)";
            ele.Attributes.Append(attribute);
            createChildrenNode(logsRange, ele);
            XMLroot.AppendChild(ele);

            SysConfig sysConfig = new SysConfig();
            ele = doc.CreateElement("SysConfig");
            createChildrenNode(sysConfig, ele);
            XMLroot.AppendChild(ele);

            MatchRule rules = new MatchRule();
            ele = doc.CreateElement("MatchRules");
            attribute = doc.CreateAttribute(des);
            attribute.Value = "可以不需要配置匹配规则，由组件自动适配, 但配置后可提高组件加载速度";
            ele.Attributes.Append(attribute);
            XMLroot.AppendChild(ele);

            XmlElement ele1 = doc.CreateElement("item");
            ele.AppendChild(ele1);
            createChildrenNode(rules, ele1);

            string f = Path.Combine(rootPath, configFile_Xml);
            try
            {
                doc.Save(f);
            }
            catch (Exception ex)
            {

                //throw;
            }
        }

        enum RuleType
        {
            none,
            DbInfo,
            MatchRule,
            LogsRange,
            SysConfig
        }

        enum DbConnectionFreeStrategy
        {
            onlyDispose,
            disposeAndClose
        }

        enum InsertBatchStrategy
        {
            /// <summary>
            /// 正常的批量 insert into tablename(fieldList) values(valueList1),(valueList2),(valueList3)
            /// </summary>
            normalBatch,
            /// <summary>
            /// 独立的列表 insert into tablename(fieldList) values(valueList1);insert into tablename(fieldList) values(valueList2)
            /// </summary>
            singleList
        }

        class DbInfo
        {
            private string _ConnectionString = "数据库连接字符串";
            public string ConnectionString { get; set; } = "Data Source=(local);Initial Catalog=DatabaseName;User Id=sa;Password=sa;";

            private string _DatabaseType = "数据库类型:sqlserver,oracle,mysql,access";
            public string DatabaseType { get; set; } = "sqlserver";

            private string _SqlProviderRelativePathOfDll = "动态 sql 提供者所在的dll文件程序集的相对路径,注：该提供者必须继承 System.DJ.ImplementFactory.Pipelines.ISqlExpressionProvider 接口";
            public string SqlProviderRelativePathOfDll { get; set; }

            private string _optByBatchMaxNumber = "insert/update/delete 批量操作最大数量, 默认100条数据";
            /// <summary>
            /// insert/update/delete 批量操作最大数量, 默认100条数据
            /// </summary>
            public int optByBatchMaxNumber { get; set; } = 100;

            private string _optByBatchWaitSecond = "insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒";
            /// <summary>
            /// insert/update/delete 执行最后的批量操作等待时间(秒), 默认3秒
            /// </summary>
            public int optByBatchWaitSecond { get; set; } = 3;

            private string _sqlMaxLengthForBatch = "insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000";
            /// <summary>
            /// insert/update/delete 批量操作 sql 表达式最大长度, 默认 50000
            /// </summary>
            public int sqlMaxLengthForBatch { get; set; } = 50000;

            private string _close = "释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接)";
            /// <summary>
            /// 释放资源并关闭连接, true/false, 默认 false(释放资源但不关闭连接)
            /// </summary>
            public bool close { get; set; } = false;

            private string _dbConnectionFreeStrategy = "数据库连接释放策略，onlyDispose/disposeAndClose 或整数值: 0/1, close 属性的补充，此属性优先级高于 close 属性";
            /// <summary>
            /// 数据库连接释放策略
            /// </summary>
            public DbConnectionFreeStrategy dbConnectionFreeStrategy { get; set; } = DbConnectionFreeStrategy.onlyDispose;

            private string _insertBatchStrategy = "缓存时批量插入策略，normalBatch/singleList 或整数值: 0/1, 默认采用通用批量插入";
            /// <summary>
            /// 批量插入策略
            /// </summary>
            public InsertBatchStrategy insertBatchStrategy { get; set; } = InsertBatchStrategy.normalBatch;

            private string _IsShowCode = "是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]";
            /// <summary>
            /// 是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]
            /// </summary>
            public bool IsShowCode { get; set; }
        }

        class MatchRule
        {
            private string _DllRelativePathOfImpl = "[可选] - 实例类所在dll文件的相对路径,如果为空,表示实例类和exe文件属同一dll文件";
            /// <summary>
            /// [可选] - 实例类所在dll文件的相对路径,
            /// 如果为空,表示实例类和exe文件属同一dll文件
            /// </summary>
            public string DllRelativePathOfImpl { get; set; }

            private string _ImplementNameSpace = "[可选] - 指定实现interface类的实例所在的namespace";
            /// <summary>
            /// [可选] - 指定实现interface类的实例所在的namespace
            /// </summary>
            public string ImplementNameSpace { get; set; }

            private string _MatchImplExpression = "[*必选*] - 匹配实现 interface 类的实例名称,可以是一个完整的类名称, 但不包含namespace。也可以是一个正则表达式";
            /// <summary>
            /// [*必选*] - 匹配实现 interface 类的实例名称,可以是一个完整的类名称, 但不包含namespace.
            /// 也可以是一个正则表达式
            /// </summary>
            public string MatchImplExpression { get; set; }

            private string _InterFaceName = "[*必选*] - 接口名称, 可以是一个 namespace.interfaceClassName 完整的接口名称, 也可是interfaceClassName";
            /// <summary>
            /// [*必选*] - 接口名称, 可以是一个 namespace.interfaceClassName 完整的接口名称, 
            /// 也可是interfaceClassName
            /// </summary>
            public string InterFaceName { get; set; }

            private string _IgnoreCase = "[可选] - 匹配 MatchImplExpression 时是否忽略大小写, 默认true[忽略大小写], false[区分大小写]";

            /// <summary>
            /// [可选] - 匹配 MatchImplExpression 时是否忽略大小写, 默认true[忽略大小写], false[区分大小写]
            /// </summary>
            public bool IgnoreCase { get; set; } = true;

            private string _IsShowCode = "是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]";
            /// <summary>
            /// 是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]
            /// </summary>
            public bool IsShowCode { get; set; }
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

        public class SysConfig
        {
            private string _Recomplie = "是否启用重新编译机制, false(不启用), true(启用), 默认为 false";
            /// <summary>
            /// 是否启用重新编译机制
            /// </summary>
            public bool Recomplie { get; set; } = false;

            private string _IsShowCode = "显示所有参与编译的代码, false(不显示), true(显示)";
            /// <summary>
            /// 显示所有参与编译的代码
            /// </summary>
            public bool IsShowCode { get; set; } = false;
        }

        class InstanceObj
        {
            public object newInstance { get; set; }
            public Type oldInstanceType { get; set; }
        }

    }
}
