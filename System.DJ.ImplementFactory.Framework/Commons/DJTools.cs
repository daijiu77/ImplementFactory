using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    /// <summary>
    /// Author: 代久 - Allan
    /// QQ: 564343162
    /// Email: 564343162@qq.com
    /// CreateDate: 2020-03-05
    /// </summary>
    public static class DJTools
    {
        public static void append(ref string srcStr, int level, string newstr, char[] splitStr, params string[] arr)
        {
            string space = "";
            for (int i = 0; i < level; i++)
            {
                space += "    ";
            }

            if (null == arr) arr = new string[] { };
            if (0 < arr.Length)
            {
                int len = arr.Length;
                for (int i = 0; i < len; i++)
                {
                    newstr = newstr.Replace("{" + i + "}", arr[i]);
                }
            }

            if (string.IsNullOrEmpty(srcStr))
            {
                srcStr = space + newstr;
            }
            else
            {
                if (null == splitStr) splitStr = new char[] { };
                string ss = string.Join("", splitStr);
                srcStr += ss + space + newstr;
            }
        }

        public static void append(ref string srcStr, int level, string newstr, params string[] arr)
        {
            append(ref srcStr, level, newstr, "\r\n".ToCharArray(), arr);
        }

        public static void append(ref string srcStr, string newstr, params string[] arr)
        {
            append(ref srcStr, 0, newstr, "\r\n".ToCharArray(), arr);
        }

        public static object ConvertTo(object value, Type type, ref bool isSuccess)
        {
            isSuccess = true;
            if (null == value) return value;
            if (null == type) return value;
            if (!IsBaseType(value.GetType())) return value;
            if (!IsBaseType(type)) return value;

            object obj = null;
            object v = value;
            if (type == typeof(Guid?))
            {
                v = v == null ? Guid.Empty.ToString() : v;
                Guid guid = new Guid(v.ToString());
                obj = guid;
            }
            else if (type == typeof(int?)
                || type == typeof(short?)
                || type == typeof(long?)
                || type == typeof(float?)
                || type == typeof(double?)
                || type == typeof(decimal?))
            {
                v = v == null ? 0 : v;
                value = v;
            }
            else if (type == typeof(bool?))
            {
                v = v == null ? false : v;
                value = v;
            }
            else if (type == typeof(DateTime?))
            {
                v = v == null ? DateTime.MinValue : v;
                value = v;
            }

            if (type == typeof(Guid))
            {
                string sv = null == value ? "" : value.ToString();
                sv = string.IsNullOrEmpty(sv) ? Guid.Empty.ToString() : sv;
                obj = new Guid(sv);
            }
            else if (null == obj)
            {
                string s = type.ToString();
                string typeName = s.Substring(s.LastIndexOf(".") + 1);
                typeName = typeName.Replace("]", "");
                typeName = typeName.Replace("&", "");
                string methodName = "To" + typeName;
                try
                {
                    Type t = Type.GetType("System.Convert");
                    //执行Convert的静态方法
                    obj = t.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { value });
                }
                catch (Exception ex)
                {
                    _err = ex.ToString();
                    obj = value;
                    isSuccess = false;
                    //throw;
                }
            }

            return obj;
        }

        public static object ConvertTo(object value, Type type)
        {
            bool isSuccess = false;
            return ConvertTo(value, type, ref isSuccess);
        }

        public static T ConvertTo<T>(object value)
        {
            bool isSuccess = false;
            T t = default(T);
            Type type = typeof(T);
            object v = ConvertTo(value, type, ref isSuccess);
            if (null != v) t = (T)v;
            return t;
        }

        public static bool IsBaseType(Type type)
        {
            byte[] arr = type.Assembly.GetName().GetPublicKeyToken();
            if (0 == arr.Length) return false;
            bool mbool = ((typeof(ValueType) == type.BaseType) || (typeof(string) == type));
            if (!mbool)
            {
                mbool = typeof(Guid) == type || typeof(DateTime) == type;
            }
            return mbool;

            //string s = type.ToString();
            //string typeName = s.Substring(s.LastIndexOf(".") + 1);
            //typeName = typeName.Replace("]", "");
            //typeName = typeName.Replace("&", "");
            //string methodName = "To" + typeName;

            //Type t = Type.GetType("System.Convert");
            //MethodInfo[] miArr = t.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public);
            //foreach (MethodInfo m in miArr)
            //{
            //    if (methodName.Equals(m.Name))
            //    {
            //        mbool = true;
            //        break;
            //    }
            //}
            //return mbool;
        }

        public static void ForeachProperty(this object obj, Func<PropertyInfo, Type, string, object, bool> func)
        {
            if (null == obj) return;
            PropertyInfo[] piArr = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            object v = null;
            bool mbool = false;
            foreach (var item in piArr)
            {
                v = item.GetValue(obj, null);
                mbool = func(item, item.PropertyType, item.Name, v);
                if (false == mbool) break;
            }
        }

        public static void ForeachProperty(this object obj, Action<PropertyInfo, Type, string, object> action)
        {
            obj.ForeachProperty((pi, fieldType, fName, fValue) =>
            {
                action(pi, fieldType, fName, fValue);
                return true;
            });
        }

        public static PropertyInfo find(this object obj, string fieldName)
        {
            PropertyInfo propertyInfo = null;
            if (string.IsNullOrEmpty(fieldName)) return propertyInfo;
            string fn = fieldName.Trim().ToLower();
            obj.ForeachProperty((pi, fieldType, fName, fValue) =>
            {
                if (fName.ToLower().Equals(fn))
                {
                    propertyInfo = pi;
                    return false;
                }
                return true;
            });
            return propertyInfo;
        }

        public static void SetPropertyFrom(this object targetObj, object srcObj, Func<PropertyInfo, bool> func)
        {
            if (null == srcObj) return;
            PropertyInfo propertyInfo = null;
            object v = null;
            targetObj.ForeachProperty((pi, fieldType, fName, fValue) =>
            {
                if (!func(pi)) return true;
                propertyInfo = srcObj.find(fName);
                if (null == propertyInfo) return true;
                v = propertyInfo.GetValue(srcObj, null);
                if (null == v) return true;
                v = ConvertTo(v, pi.PropertyType);
                try
                {
                    pi.SetValue(targetObj, v, null);
                }
                catch { }
                return true;
            });
        }

        public static void SetPropertyFrom(this object targetObj, object srcObj)
        {
            SetPropertyFrom(targetObj, srcObj, pi =>
            {
                return true;
            });
        }

        /// <summary>
        /// 根据类型获取该数据对应的类型默认值
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static string getDefaultByType(Type type)
        {
            string s = "";
            string ganaricName = "IEnumerable";
            if (type == typeof(Guid))
            {
                s = Guid.Empty.ToString();
            }
            else if (type == typeof(DateTime))
            {
                s = DateTime.MinValue.ToString("yyyy/MM/dd hh:mm:ss");
            }
            else if (type == typeof(bool))
            {
                s = "false";
            }
            else if (type == typeof(string))
            {
                s = "\"\"";
            }
            else if (null != type.GetInterface(ganaricName))
            {
                s = "null";
            }
            else
            {
                string tn = type.TypeToString();
                s = "default(" + tn + ")";
            }
            return s;
        }

        public static object DefaultValue(this Type type)
        {
            object v = null;
            string ganaricName = "IEnumerable";
            if (type == typeof(Guid))
            {
                v = Guid.Empty;
            }
            else if (type == typeof(DateTime))
            {
                v = DateTime.MinValue;
            }
            else if (type == typeof(bool))
            {
                v = false;
            }
            else if (type == typeof(string))
            {
                v = null;
            }
            else if (typeof(int) == type
                || typeof(double) == type
                || typeof(float) == type
                || typeof(Single) == type
                || typeof(decimal) == type
                || typeof(byte) == type
                || typeof(double) == type)
            {
                v = 0;
            }
            else if (null != type.GetInterface(ganaricName))
            {
                v = null;
            }
            else
            {
                v = null;
            }
            return v;
        }

        /// <summary>
        /// 根据参数标识(@ : ?)获取参数类名称
        /// </summary>
        /// <param name="dbType">参数标识(@ : ?)</param>
        /// <param name="AssemblyName"></param>
        /// <returns></returns>
        public static string GetParamertClassNameByDbTag1(string dbType, ref string AssemblyName)
        {
            string pcn = "";
            switch (dbType.Trim())
            {
                case "@": //sql server @ParameterName
                    pcn = "System.Data.SqlClient.SqlParameter";
                    AssemblyName = "System.Data.dll";
                    break;
                case ":": //oracle :ParameterName
                    pcn = "Data.OracleClient.OracleParameter";
                    AssemblyName = "Data.OracleClient.dll";
                    break;
                case "?": //mysql ?ParameterName
                    pcn = "MySql.Data.MySqlClient.MySqlParameter";
                    AssemblyName = "MySql.Data.dll";
                    break;
            }
            return pcn;
        }

        /// <summary>
        /// 根据参数标识(@ : ?)获取参数类名称
        /// </summary>
        /// <param name="dbType">参数标识(@ : ?)</param>
        /// <returns></returns>
        public static string GetParamertClassNameByDbTag1(string dbType)
        {
            string AssemblyName = "";
            return GetParamertClassNameByDbTag1(dbType, ref AssemblyName);
        }

        public static string ExtFormat(this string formatStr, params string[] arr)
        {
            string s1 = formatStr;
            if (null == arr) return s1;
            if (0 == arr.Length) return s1;

            int n = 0;
            foreach (string item in arr)
            {
                s1 = s1.Replace("{" + n + "}", item);
                n++;
            }
            return s1;
        }

        public static string GetDllRootPath(string rootPath)
        {
            string root_path = rootPath;
            string[] dlls = Directory.GetFiles(root_path, "*.dll");
            string[] exes = Directory.GetFiles(root_path, "*.exe");
            string[] dirs = null;
            string dirName = "bin";
            int n = 0;
            while ((0 == dlls.Length && 0 == exes.Length) && 10 > n)
            {
                root_path = Path.Combine(root_path, dirName);
                if (!Directory.Exists(root_path)) break;
                dlls = Directory.GetFiles(root_path, "*.dll");
                exes = Directory.GetFiles(root_path, "*.exe");
                if (0 < dlls.Length || 0 < exes.Length) break;
                dirs = Directory.GetDirectories(root_path);
                if (0 == dirs.Length) break;
                dirName = new DirectoryInfo(dirs[0]).Name;
                n++;
            }

            dlls = Directory.GetFiles(root_path, "*.dll");
            exes = Directory.GetFiles(root_path, "*.exe");
            if (0 == dlls.Length && 0 == exes.Length)
            {
                string s = "无效的根路径<{0}>";
                s = s.ExtFormat(root_path);
                _err = s;
            }

            return root_path;
        }

        static List<string> GetDllPathCollection(string rootPath, string[] excludes)
        {
            List<string> anList = new List<string>();

            string[] dlls = Directory.GetFiles(rootPath, "*.dll");
            string[] exes = Directory.GetFiles(rootPath, "*.exe");

            excludes = null == excludes ? new string[] { } : excludes;
            Assembly assembly = null;
            AssemblyName assemblyName = null;
            byte[] bt = null;
            bool mbool = false;
            string s = "";
            foreach (var item in dlls)
            {
                mbool = false;
                s = item.ToLower();
                foreach (var item1 in excludes)
                {
                    mbool = -1 != s.IndexOf(item1.ToLower());
                    if (mbool) break;
                }
                if (mbool) continue;

                try
                {
                    assembly = Assembly.LoadFrom(item);
                    if (null == assembly) continue;
                    assemblyName = assembly.GetName();
                    bt = assemblyName.GetPublicKeyToken();
                    if (0 != bt.Length) continue;
                    anList.Add(item);
                }
                catch (Exception ex)
                {
                    _err = ex.ToString();
                    //throw;
                }
            }

            if (0 < exes.Length) anList.Add(exes[0]);

            string[] dirs = Directory.GetDirectories(rootPath);
            if (0 < dirs.Length)
            {
                List<string> list = null;
                foreach (var item in dirs)
                {
                    list = GetDllPathCollection(item, excludes);
                    if (0 < list.Count)
                    {
                        list.ForEach(e =>
                        {
                            anList.Add(e);
                        });
                    }
                }
            }

            return anList;
        }

        public static List<Assembly> GetAssemblyCollection(string rootPath)
        {
            return GetAssemblyCollection(rootPath, null);
        }

        public static List<Assembly> GetAssemblyCollection(string rootPath, string[] excludes)
        {
            List<Assembly> assemblies = new List<Assembly>();
            string root_path = GetDllRootPath(rootPath);
            List<string> dllPathCollection = GetDllPathCollection(root_path, excludes);

            Assembly asse = null;
            foreach (string item in dllPathCollection)
            {
                try
                {
                    asse = Assembly.LoadFrom(item);
                    assemblies.Add(asse);
                }
                catch { }
            }

            return assemblies;
        }

        public static string GetClassName(Type type)
        {
            string name = GetClassName(type, false);
            return name;
        }

        public static string GetClassName(Type type, bool isFullName)
        {
            string name = type.Name;

            if (null != type.FullName)
            {
                if (-1 != type.FullName.IndexOf("+"))
                {
                    string ns = type.Namespace + ".";
                    string s1 = type.FullName.Substring(0, type.FullName.IndexOf("+"));
                    s1 = s1.Substring(ns.Length);
                    name = s1 + "." + type.Name;
                }
            }

            Regex rg = new Regex(@"(?<typeName>.+)`1$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(name))
            {
                name = rg.Match(name).Groups["typeName"].Value;
            }

            if (isFullName)
            {
                name = type.Namespace + "." + name;
            }

            Type[] genericTypes = type.GetGenericArguments();
            if (0 < genericTypes.Length)
            {
                string gts = "";
                foreach (Type item in genericTypes)
                {
                    gts += "," + GetClassName(item, isFullName);
                }
                gts = gts.Substring(1);
                name += "<" + gts + ">";
            }

            return name;
        }

        public static bool IsImplementInterface(this Type instanceType, Type interfaceType)
        {
            bool mbool = false;

            string interfaceName = GetClassName(interfaceType, true);
            Type[] genericTypes = instanceType.GetInterfaces();
            string itName = "";
            foreach (Type item in genericTypes)
            {
                itName = GetClassName(item, true);
                if (itName.Equals(interfaceName))
                {
                    mbool = true;
                    break;
                }
            }

            return mbool;
        }

        public static bool isWeb { get; set; }

        public static string RootPath
        {
            get
            {
                string rootPath = "";
                object webCurrent = System.Web.HttpContext.Current;
                if (null != webCurrent)
                {
                    object request = null;
                    webCurrent.ForeachProperty((pi, piType, fn, fv) =>
                    {
                        if (fn.Equals("Request"))
                        {
                            if (piType.FullName.Equals("System.Web.HttpRequest"))
                            {
                                request = fv;
                                return false;
                            }
                        }
                        
                        return true;
                    });

                    if (null != request)
                    {
                        try
                        {
                            object v = request.GetType().InvokeMember("MapPath", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, request, new object[] { "~/" });
                            rootPath = v.ToString();
                        }
                        catch (Exception)
                        {

                            //throw;
                        }                        
                                            
                    }
                    //rootPath = null.Request.MapPath("~/");
                    if ("\\" == rootPath.Substring(rootPath.Length - 1))
                    {
                        rootPath = rootPath.Substring(0, rootPath.Length - 1);
                    }

                    string s = rootPath + "\\bin";
                    if (Directory.Exists(s))
                    {
                        isWeb = true;
                    }
                }

                if (string.IsNullOrEmpty(rootPath))
                {
                    Assembly asse = Assembly.GetExecutingAssembly();
                    string path = asse.Location;
                    FileInfo fileInfo = new FileInfo(path);
                    DirectoryInfo dri = fileInfo.Directory;
                    rootPath = dri.FullName;
                }

                return rootPath;
            }
        }

        public static string GetParaTagByDbDialect(db_dialect dialect)
        {
            string dbTag = "";
            switch (DataAdapter.dbDialect)
            {
                case db_dialect.sqlserver:
                    dbTag = "@";
                    break;
                case db_dialect.oracle:
                    dbTag = ":";
                    break;
                case db_dialect.mysql:
                    dbTag = "?";
                    break;
            }
            return dbTag;
        }

        public static string toArrayString(this string[] arr)
        {
            string s = "null";
            if (null == arr) return s;
            if (0 == arr.Length) return s;
            s = "";
            foreach (string item in arr)
            {
                s += ", \"" + item + "\"";
            }
            s = s.Substring(1);
            s = "new string[] {" + s + " }";
            return s;
        }

        public static string TypeToString(this Type type)
        {
            return type.TypeToString(true);
        }

        public static string TypeToString(this Type type, bool isFullName)
        {
            string s = "";
            string tn = "";
            Regex rg = new Regex(@"(?<TypeName>[a-z0-9_\.]+)[^a-z0-9_\.]", RegexOptions.IgnoreCase);
            Type[] types = type.GetGenericArguments();

            Func<Type[], string> func = (_types) =>
            {
                string _s1 = "";
                foreach (Type item in _types)
                {
                    if (item.IsGenericType || null == types[0].FullName)
                    {
                        _s1 += ", " + item.Name;
                    }
                    else
                    {
                        _s1 += ", " + item.TypeToString(isFullName);
                    }
                }
                _s1 = _s1.Substring(2);
                string _tn = "<" + _s1 + ">";
                return _tn;
            };

            if (typeof(IEnumerable) == type.GetInterface("IEnumerable") && typeof(string) != type.BaseType)
            {
                tn = isFullName ? type.FullName : type.Name;
                if (string.IsNullOrEmpty(tn)) tn = type.Name;

                if (rg.IsMatch(tn))
                {
                    tn = rg.Match(tn).Groups["TypeName"].Value;
                }

                if (typeof(IDictionary) == type.GetInterface("IDictionary"))
                {
                    tn += "<" + types[0].TypeToString(isFullName) + ", " + types[1].TypeToString(isFullName) + ">";
                }
                else if (type.IsArray)
                {
                    tn = isFullName ? type.FullName : type.Name;
                }
                else if (typeof(IList) == type.GetInterface("IList"))
                {
                    if (types[0].IsGenericType || null == types[0].FullName)
                    {
                        tn += "<" + types[0].Name + ">";
                    }
                    else
                    {
                        tn += "<" + types[0].TypeToString(isFullName) + ">";
                    }                    
                }
                else if (0 < types.Length)
                {
                    tn += func(types);                    
                }
                s = tn;
            }
            else if (type.BaseType == typeof(System.MulticastDelegate))
            {
                tn = isFullName ? type.FullName : type.Name;
                if (rg.IsMatch(tn))
                {
                    tn = rg.Match(tn).Groups["TypeName"].Value;
                }

                if (0 < types.Length)
                {
                    tn += func(types); ;
                }
                s = tn;
            }
            else if (type.IsGenericType || null == type.FullName)
            {
                //泛型
                s = type.Name;
            }
            else if (IsBaseType(type))
            {
                s = isFullName ? type.FullName : type.Name;
            }
            else
            {
                if (0 < types.Length)
                {
                    tn = isFullName ? type.FullName : type.Name;
                    if (rg.IsMatch(tn))
                    {
                        tn = rg.Match(tn).Groups["TypeName"].Value;
                    }                    
                    tn += func(types); ;
                    s = tn;
                }
                else
                {
                    s = isFullName ? type.FullName : type.Name;
                    if (type.IsByRef)
                    {
                        rg = new Regex(".+[^a-z0-9]$", RegexOptions.IgnoreCase);
                        if (rg.IsMatch(s))
                        {
                            s = s.Substring(0, s.Length - 1);
                        }
                    }
                }
            }
            return s;
        }

        public static Type GetClassTypeByPath(this string classPath)
        {
            Type classType = null;
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            Assembly item = null;
            int num = 0;
            int len = asses.Length;
            while (num < len)
            {
                try
                {
                    item = asses[num];
                    if (string.IsNullOrEmpty(item.Location)) continue;
                    classType = item.GetType(classPath);
                    if (null != classType) break;
                }
                catch (Exception ex)
                {
                    _err = ex.ToString();
                    //throw;
                }
                finally
                {
                    num++;
                }
            }
            return classType;
        }

        public static DataEntity<DataElement> GetDynamicEntityBy(this DataRow dataRow)
        {
            DataEntity<DataElement> dataElements = new DataEntity<DataElement>();
            if (null == dataRow) return dataElements;
            foreach (DataColumn dc in dataRow.Table.Columns)
            {
                dataElements.Add(dc.ColumnName, dataRow[dc.ColumnName]);
            }
            return dataElements;
        }

        public static List<DataEntity<DataElement>> GetDynamicEntitiesBy(this DataTable dataTable)
        {
            List<DataEntity<DataElement>> dataElements = new List<DataEntity<DataElement>>();
            if (null == dataTable) return dataElements;
            DataEntity<DataElement> dataElements1 = null;
            foreach (DataRow item in dataTable.Rows)
            {
                dataElements1 = item.GetDynamicEntityBy();
                dataElements.Add(dataElements1);
            }
            return dataElements;
        }

        public static object createArrayByType(Type type, int length)
        {
            object arr = null;
            if (false == type.IsArray) return arr;

            try
            {
                arr = type.InvokeMember("Set", BindingFlags.CreateInstance, null, arr, new object[] { length });
            }
            catch { }

            return arr;
        }

        public static void arrayAdd(object arrObj, object arrElement, int eleIndex)
        {
            if (null == arrObj) return;

            Type type = arrObj.GetType();
            if (false == type.IsArray) return;

            Array array = (Array)arrObj;

            try
            {
                array.SetValue(arrElement, eleIndex);
            }
            catch { }
        }

        public static object createListByType(Type type)
        {
            object list = null;
            if (null == type.GetInterface("IList")) return list;

            Type[] types = type.GetGenericArguments();
            string asseName = type.Assembly.GetName().Name;
            string dicTypeName = type.FullName;
            string ClsPath = "";
            string s = @"\[(?<ClsPath>[a-z0-9_\.]+)\s*\,\s*[^\[\]]+\]";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            if (rg.IsMatch(dicTypeName))
            {
                int n = 0;
                int len = types.Length;
                string txt = "";
                Type ele = null;
                MatchCollection mc = rg.Matches(dicTypeName);
                foreach (Match item in mc)
                {
                    if (n == len) break;
                    ele = types[n];
                    ClsPath = item.Groups["ClsPath"].Value;
                    s = ele.FullName;
                    s += ", " + ele.Assembly.GetName().Name;
                    s += ", Version=" + ele.Assembly.GetName().Version.ToString();
                    s += ", Culture=neutral";
                    s += ", PublicKeyToken=null";
                    s = "[" + s + "]";
                    txt = item.Groups[0].Value;
                    dicTypeName = dicTypeName.Replace(txt, s);
                    n++;
                }
            }

            //Type t = GetClassTypeByPath(ClsPath);
            //if (null == t) return list;

            object v = null;
            try
            {
                v = Activator.CreateInstance(asseName, dicTypeName) as ObjectHandle;
            }
            catch { }
            if (null == v) return list;
            list = ((ObjectHandle)v).Unwrap();
            return list;
        }

        public static void listAdd(object list, object listElement)
        {
            if (null == list) return;

            Type listType = list.GetType();
            if (null == listType.GetInterface("IList")) return;

            MethodInfo methodInfo = listType.GetMethod("Add");
            if (null == methodInfo) return;

            try
            {
                methodInfo.Invoke(list, new object[] { listElement });
            }
            catch { }
        }

        public static object createDictionaryByType(Type type)
        {
            object dic = null;
            if (null == type.GetInterface("IDictionary")) return dic;

            Type[] types = type.GetGenericArguments();
            Type dicType = typeof(Dictionary<string, int>);
            string asseName = dicType.Assembly.GetName().Name;
            string dicTypeName = dicType.FullName;
            string ClsPath = "";
            string s = @"\[(?<ClsPath>[a-z0-9_\.]+)\s*\,\s*[^\[\]]+\]";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            if (rg.IsMatch(dicTypeName))
            {
                int n = 0;
                int len = types.Length;
                string txt = "";
                Type ele = null;
                MatchCollection mc = rg.Matches(dicTypeName);
                foreach (Match item in mc)
                {
                    if (n == len) break;
                    ClsPath = item.Groups["ClsPath"].Value;
                    ele = types[n];
                    s = ele.FullName;
                    s += ", " + ele.Assembly.GetName().Name;
                    s += ", Version=" + ele.Assembly.GetName().Version.ToString();
                    s += ", Culture=neutral";
                    s += ", PublicKeyToken=null";
                    s = "[" + s + "]";
                    txt = item.Groups[0].Value;
                    dicTypeName = dicTypeName.Replace(txt, s);
                    n++;
                }
            }

            //Type t = GetClassTypeByPath(ClsPath);
            //if (null == t) return dic;

            object v = null;
            try
            {
                v = Activator.CreateInstance(asseName, dicTypeName) as ObjectHandle;
            }
            catch { }
            if (null == v) return dic;
            dic = ((ObjectHandle)v).Unwrap();

            return dic;
        }

        public static void dictionaryAdd(object dic, string key, object val)
        {
            if (null == dic) return;

            Type dicType = dic.GetType();
            if (null == dicType.GetInterface("IDictionary")) return;

            MethodInfo methodInfo = dicType.GetMethod("Add");
            if (null == methodInfo) return;

            try
            {
                methodInfo.Invoke(dic, new object[] { key, val });
            }
            catch { }
        }

        private static object _entity = null;
        private static Dictionary<string, PropertyInfo> _entityPropertyDic = new Dictionary<string, PropertyInfo>();
        private static void initPropertyDic(object entity)
        {
            _entity = null == _entity ? entity : _entity;
            if ((false == entity.Equals(_entity)) || (0 == _entityPropertyDic.Count))
            {
                _entity = entity;
                _entityPropertyDic.Clear();
                entity.ForeachProperty((propertyInfo, type, fn, fv) =>
                {
                    _entityPropertyDic.Add(fn.ToLower(), propertyInfo);
                });
            }
        }

        public static T GetPropertyValue<T>(this object entity, string propertyName)
        {
            initPropertyDic(entity);
            T v = default(T);
            PropertyInfo pi = null;
            string fn1 = propertyName.ToLower();            
            _entityPropertyDic.TryGetValue(fn1, out pi);
            if (null == pi) return v;
            object _obj = pi.GetValue(entity, null);

            if (null == _obj) return v;
            if (IsBaseType(pi.PropertyType))
            {
                bool mbool = false;
                object o = ConvertTo(_obj, typeof(T), ref mbool);
                if (mbool) v = (T)o;
            }
            else
            {
                try
                {
                    v = (T)_obj;
                }
                catch (Exception ex)
                {
                    _err = ex.ToString();
                    //throw;
                }
            }
            return v;
        }

        public static void SetPropertyValue(this object entity, string propertyName, object propertyValue)
        {
            initPropertyDic(entity);
            PropertyInfo pi = null;   
            string fn1 = propertyName.ToLower();
            _entityPropertyDic.TryGetValue(fn1, out pi);
            if (null == pi) return;
            object v = null;

            if (pi.PropertyType.IsEnum)
            {
                string s = "";
                int n = 0;
                if (null != propertyValue) s = propertyValue.ToString().Trim();
                Array arr = Enum.GetValues(pi.PropertyType);
                string[] ens = Enum.GetNames(pi.PropertyType);

                Regex rg = new Regex(@"(^[0-9]$)|(^[0-9][0-9]*[0-9]$)", RegexOptions.IgnoreCase);
                if (rg.IsMatch(s))
                {
                    int.TryParse(s, out n);
                    int x = 0;
                    foreach (var item in arr)
                    {
                        x = (int)item;
                        if (x == n)
                        {
                            v = item;
                            break;
                        }
                    }
                }
                else
                {
                    s = s.ToLower();
                    n = 0;
                    foreach (string item in ens)
                    {
                        if (item.ToLower().Equals(s))
                        {
                            v = arr.GetValue(n);
                            break;
                        }
                        n++;
                    }
                }
            }
            else if (IsBaseType(pi.PropertyType))
            {
                bool isSuccess = false;
                v = ConvertTo(propertyValue, pi.PropertyType, ref isSuccess);
                if (!isSuccess)
                {
                    v = pi.PropertyType.DefaultValue();
                }
            }
            else
            {
                v = propertyValue;
            }

            try
            {
                pi.SetValue(entity, v, null);
            }
            catch (Exception)
            {

                //throw;
            }
        }

        public static bool InitDirectory(this string path, bool isDirectory)
        {
            if (string.IsNullOrEmpty(path)) return false;
            bool isSuccess = true;
            path = path.Replace("/", "\\");
            if (-1 == path.IndexOf("\\")) return isSuccess;

            string[] arr = path.Split('\\');
            int len = arr.Length;
            string f = arr[0];
            for (int i = 1; i < len; i++)
            {
                if (!isDirectory)
                {
                    if ((i + 1) == len)
                    {
                        if (-1 != arr[i].IndexOf(".")) break;
                    }
                }

                f += "\\" + arr[i];
                if (!Directory.Exists(f))
                {
                    try
                    {
                        Directory.CreateDirectory(f);
                    }
                    catch (Exception ex)
                    {
                        isSuccess = false;
                        _err = ex.ToString();
                        //throw;
                    }
                }

                if (!isSuccess) break;
            }
            return isSuccess;
        }

        public static bool InitDirectory(this string path)
        {
            return InitDirectory(path, false);
        }

        private static string _err = "";
        public static string err
        {
            get
            {
                string s = _err;
                _err = "";
                return s;
            }
            set { _err = value; }
        }

        private static bool _IsDebug(Type type)
        {
            Assembly assembly = Assembly.GetAssembly(type);
            bool debug = false;
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                if (attribute.GetType() == typeof(System.Diagnostics.DebuggableAttribute))
                {
                    if (((System.Diagnostics.DebuggableAttribute)attribute)
                        .IsJITTrackingEnabled)
                    {
                        debug = true;
                        break;
                    }
                }
            }
            return debug;
        }

        public static bool IsDebug<T>()
        {
            return _IsDebug(typeof(T));
        }

        public static bool IsDebug(Type type)
        {
            return _IsDebug(type);
        }

        public static T GetSourceInstance<T>(this object newInstance)
        {
            T v = default(T);
            if (newInstance.GetType().IsInterface) return v;
            v = newInstance.GetPropertyValue<T>(TempImpl.InterfaceInstanceType);
            return v;
        }
    }
}
