using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.NetCore.Commons;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using static System.DJ.ImplementFactory.NetCore.Commons.Attrs.Condition;

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

        public static object ConvertTo(this object value, Type type, ref bool isSuccess)
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
                Guid guid = Guid.Empty;
                isSuccess = Guid.TryParse(sv, out guid);
                obj = guid;
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

        public static object ConvertTo(this object value, Type type)
        {
            bool isSuccess = false;
            return ConvertTo(value, type, ref isSuccess);
        }

        public static T ConvertTo<T>(this object value)
        {
            bool isSuccess = false;
            T t = default(T);
            Type type = typeof(T);
            object v = ConvertTo(value, type, ref isSuccess);
            if (null != v) t = (T)v;
            return t;
        }

        public static bool IsBaseType(this Type type)
        {
            byte[] arr = type.Assembly.GetName().GetPublicKeyToken();
            if (0 == arr.Length) return false;
            bool mbool = ((typeof(ValueType) == type.BaseType) || (typeof(string) == type));
            if (!mbool)
            {
                mbool = typeof(Guid) == type || typeof(Guid?) == type || typeof(DateTime) == type || typeof(DateTime?) == type;
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

        public static void ForeachProperty(this object obj, bool isAll, Func<PropertyInfo, Type, string, object, bool> func)
        {
            if (null == obj) return;
            Type type = obj.GetType();
            PropertyInfo[] piArr = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            object v = null;
            bool mbool = false;
            Attribute att = null;
            foreach (var item in piArr)
            {
                att = item.GetCustomAttribute(typeof(IgnoreForeachProp));
                if (null != att) continue;
                if (!isAll)
                {
                    if (item.DeclaringType != type) continue;
                }
                v = item.GetValue(obj);
                mbool = func(item, item.PropertyType, item.Name, v);
                if (false == mbool) break;
            }
        }

        public static void ForeachProperty(this object obj, Func<PropertyInfo, Type, string, object, bool> func)
        {
            bool isAll = true;
            obj.ForeachProperty(isAll, func);
        }

        public static void ForeachProperty(this object obj, Action<PropertyInfo, Type, string, object> action)
        {
            obj.ForeachProperty((pi, fieldType, fName, fValue) =>
            {
                action(pi, fieldType, fName, fValue);
                return true;
            });
        }

        public static void ForeachProperty(this object obj, bool isAll, Action<PropertyInfo, Type, string, object> action)
        {
            obj.ForeachProperty(isAll, (pi, fieldType, fName, fValue) =>
            {
                action(pi, fieldType, fName, fValue);
                return true;
            });
        }

        public static void ForeachProperty(this Type objType, bool isAll, Func<PropertyInfo, Type, string, bool> func)
        {
            if (null == objType) return;
            PropertyInfo[] piArr = objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            bool mbool = false;
            Attribute att = null;
            foreach (var item in piArr)
            {
                att = item.GetCustomAttribute(typeof(IgnoreForeachProp));
                if (null != att) continue;
                if (!isAll)
                {
                    if (item.DeclaringType != objType) continue;
                }
                mbool = func(item, item.PropertyType, item.Name);
                if (false == mbool) break;
            }
        }

        public static void ForeachProperty(this Type objType, Func<PropertyInfo, Type, string, bool> func)
        {
            bool isAll = true;
            objType.ForeachProperty(isAll, func);
        }

        public static void ForeachProperty(this Type objType, Action<PropertyInfo, Type, string> action)
        {
            objType.ForeachProperty((pi, fieldType, fName) =>
            {
                action(pi, fieldType, fName);
                return true;
            });
        }

        public static void ForeachProperty(this Type objType, bool isAll, Action<PropertyInfo, Type, string> action)
        {
            objType.ForeachProperty(isAll, (pi, fieldType, fName) =>
            {
                action(pi, fieldType, fName);
                return true;
            });
        }

        public static PropertyInfo find(this object obj, string fieldName)
        {
            PropertyInfo propertyInfo = null;
            if (string.IsNullOrEmpty(fieldName)) return propertyInfo;
            string fn = fieldName.Trim().ToLower();
            obj.GetType().ForeachProperty((pi, fieldType, fName) =>
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
            targetObj.ForeachProperty(true, (pi, fieldType, fName, fValue) =>
            {
                if (!func(pi)) return true;
                propertyInfo = srcObj.find(fName);
                if (null == propertyInfo) return true;
                v = propertyInfo.GetValue(srcObj, null);
                if (null == v) return true;
                if (typeof(ICollection).IsAssignableFrom(fieldType) && (typeof(string) == propertyInfo.PropertyType))
                {
                    string[] arr = v.ToString().Split(',');
                    object collection = null;
                    Type eleType = null;
                    if (fieldType.IsArray)
                    {
                        string s = fieldType.TypeToString(true);
                        s = s.Replace("[]", "");
                        eleType = Type.GetType(s);
                        if (null == eleType) return true;
                        collection = createArrayByType(null, arr.Length);
                    }
                    else if (typeof(IList).IsAssignableFrom(fieldType))
                    {
                        eleType = fieldType.GetGenericArguments()[0];
                        collection = createListByType(eleType);
                    }

                    if (null == collection) return true;
                    object sv = "";
                    int n = 0;
                    foreach (var item in arr)
                    {
                        sv = item.Trim();
                        sv = ConvertTo(sv, eleType);
                        if (fieldType.IsArray)
                        {
                            arrayAdd(collection, sv, n);
                        }
                        else
                        {
                            listAdd(collection, sv);
                        }
                        n++;
                    }
                    v = collection;
                }
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

        public static void SetCurrentPropertyFrom<T>(this T targetObj, object srcObj, Func<PropertyInfo, bool> func)
        {
            if (null == srcObj || null == targetObj) return;
            PropertyInfo propertyInfo = null;
            PropertyInfo pInfo = null;
            object v = null;
            Type tp = typeof(T);
            tp.ForeachProperty(false, (pi, type, fn) =>
            {
                if (!func(pi)) return;
                pInfo = targetObj.GetType().GetProperty(pi.Name);
                if (null == pInfo) return;
                if (!pInfo.CanWrite) return;
                propertyInfo = srcObj.find(pi.Name);
                if (null == propertyInfo) return;
                v = propertyInfo.GetValue(srcObj);
                if (null != v) v = v.ConvertTo(pInfo.PropertyType);
                try
                {
                    pInfo.SetValue(targetObj, v);
                }
                catch (Exception) { }
            });
        }

        public static void SetCurrentPropertyFrom<T>(this T targetObj, object srcObj)
        {
            targetObj.SetCurrentPropertyFrom<T>(srcObj, pi => { return true; });
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

        public static T GetInstanceByType<T>(string likeName)
        {
            if (null != likeName) likeName = likeName.ToLower();
            object tObj = null;
            Type type = typeof(T);
            Type[] types = null;
            List<Assembly> assemblies = GetAssemblyCollection(RootPath);
            foreach (Assembly asse in assemblies)
            {
                types = asse.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    if (!t.IsClass) continue;
                    if (!string.IsNullOrEmpty(likeName))
                    {
                        if (-1 == t.Name.ToLower().IndexOf(likeName)) continue;
                    }
                    if (type.IsAssignableFrom(t))
                    {
                        try
                        {
                            tObj = (T)Activator.CreateInstance(t);
                            break;
                        }
                        catch (Exception ex)
                        {

                            //throw;
                        }
                    }
                }
                if (null != tObj) break;
            }
            if (null == tObj) return default(T);
            return (T)tObj;
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
                    _s1 += ", " + item.TypeToString(isFullName);
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
                    tn += "<" + types[0].TypeToString(isFullName) + ">";
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
                if (null == tn) tn = type.Name;
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

        private static Type getTypeFromAssemblies(this string typeName)
        {
            Type type = Type.GetType(typeName);
            if (null != type) return type;
            List<Assembly> assemblies = GetAssemblyCollection(RootPath);
            foreach (Assembly item in assemblies)
            {
                type = item.GetType(typeName);
                if (null != type) break;
            }
            return type;
        }

        public static Type GetClassTypeByPath(this string classPath)
        {
            Type classType = Type.GetType(classPath);
            if (null != classType) return classType;
            if (-1 != classPath.IndexOf("+"))
            {
                string[] arr = classPath.Split('+');
                Type tp = arr[0].getTypeFromAssemblies();
                if (null == tp) return classType;
                classType = tp.GetNestedType(arr[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else
            {
                classType = classPath.getTypeFromAssemblies();
            }
            return classType;
        }

        public static Type GetTypeByFullName(this string fullName)
        {
            return fullName.GetClassTypeByPath();
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
            Type listType = null;
            if (null == type.GetInterface("IList"))
            {
                listType = typeof(List<>);
                listType = listType.MakeGenericType(type);
            }
            else
            {
                listType = type;
            }

            object v = null;
            try
            {
                v = Activator.CreateInstance(listType);
                return v;
            }
            catch (Exception)
            {

                //throw;
            }
            Type[] types = listType.GetGenericArguments();
            string asseName = listType.Assembly.GetName().Name;
            string dicTypeName = listType.FullName;
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

            try
            {
                v = Activator.CreateInstance(asseName, dicTypeName) as ObjectHandle;
            }
            catch { }
            object list = null;
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
            try
            {
                dic = Activator.CreateInstance(type);
                return dic;
            }
            catch (Exception)
            {

                //throw;
            }

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

        private static Type _entityType = null;
        private static Dictionary<string, PropertyInfo> _entityPropertyDic = new Dictionary<string, PropertyInfo>();
        private static object _initPropertyDic = new object();
        private static void initPropertyDic(object entity)
        {
            if (null == entity) return;
            lock (_initPropertyDic)
            {
                _entityType = null == _entityType ? entity.GetType() : _entityType;
                if ((_entityType != entity.GetType()) || (0 == _entityPropertyDic.Count))
                {
                    _entityType = entity.GetType();
                    _entityPropertyDic.Clear();
                    entity.GetType().ForeachProperty((propertyInfo, type, fn) =>
                    {
                        if (_entityPropertyDic.ContainsKey(fn.ToLower())) return;
                        _entityPropertyDic.Add(fn.ToLower(), propertyInfo);
                    });
                }
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

        public static PropertyInfo GetPropertyInfo(this object entity, string propertyName)
        {
            initPropertyDic(entity);
            PropertyInfo pi = null;
            string fn1 = propertyName.ToLower();
            _entityPropertyDic.TryGetValue(fn1, out pi);
            return pi;
        }

        public static List<object> DataTableToList(this DataTable dataTable, Type type)
        {
            List<object> list = new List<object>();
            if (null == dataTable) return list;
            if (0 == dataTable.Rows.Count) return list;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DataColumn item in dataTable.Columns)
            {
                dic.Add(item.ColumnName.ToLower(), item.ColumnName);
            }

            object ele = null;
            object vObj = null;
            string field = "";
            string fn1 = "";
            Attribute att = null;
            foreach (DataRow dr in dataTable.Rows)
            {
                ele = Activator.CreateInstance(type);
                type.ForeachProperty((pi, tp, fn) =>
                {
                    if (!IsBaseType(tp)) return;
                    field = "";
                    fn1 = fn.ToLower();
                    if (dic.ContainsKey(fn1)) field = dic[fn1];
                    if (string.IsNullOrEmpty(field))
                    {
                        att = pi.GetCustomAttribute(typeof(FieldMapping));
                        if (null != att)
                        {
                            fn1 = ((FieldMapping)att).FieldName.ToLower();
                            if (dic.ContainsKey(fn1)) field = dic[fn1];
                        }
                    }
                    if (string.IsNullOrEmpty(field)) return;
                    vObj = dr[field];
                    if (DBNull.Value == vObj) return;
                    if (null == vObj) return;
                    vObj = ConvertTo(vObj, tp);
                    pi.SetValue(ele, vObj);
                });
                list.Add(ele);
            }
            return list;
        }

        public static List<T> DataTableToList<T>(this DataTable dataTable)
        {
            Type type = typeof(T);
            List<T> datas = new List<T>();
            List<object> list = dataTable.DataTableToList(type);
            foreach (var item in list)
            {
                datas.Add((T)item);
            }
            return datas;
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

        public static string ByteToStr(this byte[] dt)
        {
            if (null == dt) return "";
            string v = Encoding.UTF8.GetString(dt);
            if (-1 != v.IndexOf("\0")) v = v.Substring(0, v.IndexOf("\0"));
            return v;
        }

        public static byte[] StrToByte(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private static int headSize = 6;
        private static string headFlag = "@";
        private static string CollectSign = "IEnumerable";
        public static byte[] ObjectToByteArray(this object dataObj)
        {
            byte[] result = null;
            if (null == dataObj) return result;
            Type type = dataObj.GetType();

            if (DJTools.IsBaseType(type))
            {
                result = dataObj.ToString().StrToByte();
            }
            else if (null != (dataObj as IEnumerable))
            {
                IEnumerable enumerable = dataObj as IEnumerable;
                List<byte[]> list = new List<byte[]>();
                int size = 0;
                byte[] buffer = null;
                string prop = "";
                foreach (var item in enumerable)
                {
                    buffer = item.ObjectToByteArray();
                    size += buffer.Length;
                    prop += "," + buffer.Length;
                    list.Add(buffer);
                }
                if (0 == list.Count) return result;

                prop = prop.Substring(1);
                byte[] propertyData = prop.StrToByte();

                int AllSize = propertyData.Length + size;
                string typeName = type.TypeToString(true);
                typeName += ":" + CollectSign + ":" + AllSize + ":" + propertyData.Length;
                byte[] headBuffer = typeName.StrToByte();

                string headStr = headFlag + headBuffer.Length;
                buffer = headStr.StrToByte();
                AllSize += (headSize + headBuffer.Length);

                int pos = 0;
                result = new byte[AllSize];
                Array.Copy(buffer, 0, result, pos, buffer.Length);
                pos += headSize;

                Array.Copy(headBuffer, 0, result, pos, headBuffer.Length);
                pos += headBuffer.Length;

                Array.Copy(propertyData, 0, result, pos, propertyData.Length);
                pos += propertyData.Length;

                foreach (var item in list)
                {
                    Array.Copy(item, 0, result, pos, item.Length);
                    pos += item.Length;
                }
            }
            else
            {
                result = dataObj.EntityToByteArray();
            }
            return result;
        }

        public static byte[] EntityToByteArray(this object entity)
        {
            if (null == entity) return null;
            byte[] dt = null;
            byte[] buffer = null;
            if (DJTools.IsBaseType(entity.GetType()))
            {
                dt = entity.ToString().StrToByte();
                return dt;
            }

            string paras = "";
            int dataSize = 0;
            Dictionary<string, byte[]> dic = new Dictionary<string, byte[]>();
            entity.ForeachProperty((pi, type, fn, fv) =>
            {
                buffer = null;
                if (null != fv)
                {
                    if (typeof(byte[]) == type)
                    {
                        buffer = (byte[])fv;
                    }
                    else if (DJTools.IsBaseType(type))
                    {
                        buffer = fv.ToString().StrToByte();
                    }
                    else
                    {
                        buffer = fv.ObjectToByteArray();
                    }
                }

                if (null == buffer) buffer = new byte[] { };
                dic.Add(fn, buffer);
                dataSize += buffer.Length;
                paras += "," + fn + ":" + type.TypeToString(true) + ":" + buffer.Length;
            });

            if (!string.IsNullOrEmpty(paras))
            {
                paras = paras.Substring(1);
            }

            paras = entity.GetType().TypeToString(true) + "#" + paras;
            byte[] infoDatas = paras.StrToByte();
            int AllSize = infoDatas.Length + dataSize;

            string sizeInfo = "object:" + AllSize + ",property:" + infoDatas.Length;
            byte[] hdData = sizeInfo.StrToByte();

            string headStr = headFlag + hdData.Length;
            byte[] headBuffer = headStr.StrToByte();
            AllSize += (headSize + hdData.Length);

            int pos = 0;
            dt = new byte[AllSize];
            Array.Copy(headBuffer, 0, dt, pos, headBuffer.Length);
            pos += headSize;

            Array.Copy(hdData, 0, dt, pos, hdData.Length);
            pos += hdData.Length;

            Array.Copy(infoDatas, 0, dt, pos, infoDatas.Length);
            pos += infoDatas.Length;

            foreach (var item in dic)
            {
                Array.Copy(item.Value, 0, dt, pos, item.Value.Length);
                pos += item.Value.Length;
            }

            return dt;
        }

        public static T ByteArrayToEntity<T>(this byte[] data)
        {
            Type type = typeof(T);
            return (T)data.ByteArrayToEntity(type);
        }

        public static T ByteArrayToObject<T>(this byte[] data)
        {
            Type type = typeof(T);
            return (T)data.ByteArrayToObject(type);
        }

        public static object ByteArrayToObject(this byte[] data, Type type)
        {
            object result = null;
            if (null == data) return result;
            if (0 == data.Length) return result;

            string s = "";
            if (DJTools.IsBaseType(type))
            {
                s = data.ByteToStr();
                result = DJTools.ConvertTo(s, type);
                return result;
            }

            if (headSize > data.Length) return result;
            byte[] buffer = new byte[headSize];
            Array.Copy(data, 0, buffer, 0, headSize);
            s = buffer.ByteToStr();
            if (string.IsNullOrEmpty(s)) return result;
            if (!s.Substring(0, 1).Equals(headFlag)) return result;

            s = s.Substring(1);
            int len = Convert.ToInt32(s);
            buffer = new byte[len];
            Array.Copy(data, headSize, buffer, 0, len);
            s = buffer.ByteToStr();

            string sign = ":" + CollectSign + ":";
            if (-1 != s.IndexOf(sign))
            {
                string[] arr = s.Split(':');
                int propSize = Convert.ToInt32(arr[3]);

                int pos = headSize + len;
                buffer = new byte[propSize];
                Array.Copy(data, pos, buffer, 0, propSize);
                pos += propSize;

                s = buffer.ByteToStr();
                arr = s.Split(',');
                len = arr.Length;
                Type[] types = type.GenericTypeArguments;
                Type paraType = null;
                bool isArr = false;
                if (0 == types.Length)
                {
                    result = DJTools.createArrayByType(type, len);
                    s = type.FullName;
                    s = s.Replace("[]", "");
                    paraType = s.GetClassTypeByPath();
                    isArr = true;
                }
                else
                {
                    result = DJTools.createListByType(type);
                    paraType = types[0];
                }
                if (null == paraType) return result;

                int size = 0;
                object vObj = null;
                for (int i = 0; i < len; i++)
                {
                    size = Convert.ToInt32(arr[i]);
                    buffer = new byte[size];
                    Array.Copy(data, pos, buffer, 0, size);
                    pos += size;
                    vObj = buffer.ByteArrayToObject(paraType);
                    if (isArr)
                    {
                        DJTools.arrayAdd(result, vObj, i);
                    }
                    else
                    {
                        DJTools.listAdd(result, vObj);
                    }
                }
            }
            else
            {
                result = data.ByteArrayToEntity(type);
            }
            return result;
        }

        public static object ByteArrayToEntity(this byte[] data, Type type)
        {
            object dt = null;
            if (null == data) return dt;
            if (0 == data.Length) return dt;
            Func<byte[], Type, object> func = (_para, _paraType) =>
            {
                string s = _para.ByteToStr();
                return DJTools.ConvertTo(s, _paraType);
            };

            if (headSize > data.Length)
            {
                dt = func(data, type);
                return dt;
            }
            byte[] buffer = new byte[headSize];
            Array.Copy(data, 0, buffer, 0, headSize);
            string headStr = buffer.ByteToStr();
            if (string.IsNullOrEmpty(headStr)) return dt;
            if (!headStr.Substring(0, 1).Equals(headFlag)) return dt;

            headStr = headStr.Substring(1);
            int len = Convert.ToInt32(headStr);
            buffer = new byte[len];
            Array.Copy(data, headSize, buffer, 0, len);
            headStr = buffer.ByteToStr();
            if (string.IsNullOrEmpty(headStr)) return dt;

            Regex rg = new Regex(@"object\s*\:\s*(?<ObjectSize>[0-9]+)\s*\,\s*property\s*\:\s*(?<PropertySize>[0-9]+)", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(headStr))
            {
                dt = func(data, type);
                return dt;
            }

            int ObjectSize = 0;
            int PropertySize = 0;

            Match match = rg.Match(headStr);
            string sv = match.Groups["ObjectSize"].Value;
            int.TryParse(sv, out ObjectSize);

            sv = match.Groups["PropertySize"].Value;
            int.TryParse(sv, out PropertySize);

            int pos = headSize + len;
            buffer = new byte[PropertySize];
            Array.Copy(data, pos, buffer, 0, PropertySize);
            string prop = buffer.ByteToStr();

            pos = prop.IndexOf("#");
            string typeName = prop.Substring(0, pos);
            prop = prop.Substring(pos + 1);

            if (null == type) type = typeName.GetClassTypeByPath();
            if (null == type) throw new Exception("Object type '" + typeName + "' is not exist.");

            dt = Activator.CreateInstance(type);
            PropertyInfo pi = null;

            int size = 0;
            string[] arr = prop.Split(',');
            string[] arr1 = null;
            string fn = "";
            object fv = null;
            //Type ft = null;
            pos = headSize + len + PropertySize;
            foreach (var item in arr)
            {
                size = 0;
                arr1 = item.Split(':');
                fn = arr1[0].Trim();
                //ft = arr1[1].GetClassTypeByPath();
                //if (null == ft) continue;
                int.TryParse(arr1[2].Trim(), out size);
                buffer = new byte[size];
                Array.Copy(data, pos, buffer, 0, buffer.Length);
                pos += buffer.Length;

                pi = dt.GetType().GetProperty(fn);
                if (null == pi) continue;
                if (pi.PropertyType == typeof(byte[]))
                {
                    pi.SetValue(dt, buffer);
                }
                else if (DJTools.IsBaseType(pi.PropertyType))
                {
                    fv = buffer.ByteToStr();
                    fv = DJTools.ConvertTo(fv, pi.PropertyType);
                    pi.SetValue(dt, fv);
                }
                else
                {
                    fv = buffer.ByteArrayToObject(pi.PropertyType);
                    if (null != fv) pi.SetValue(dt, fv);
                }
            }

            return dt;
        }

        public static byte[] SerializableObjectToByteArray(this object entity)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            byte[] dt = null;
            using (MemoryStream ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, entity);
                dt = ms.ToArray();
            }
            return dt;
        }

        public static object ByteArrayToSerializableObject(this byte[] data)
        {
            object vObj = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                vObj = binaryFormatter.Deserialize(ms);
            }
            return vObj;
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

        /// <summary>
        /// 创建人:DJ
        /// 日  期:2021-12-02
        /// 根据实体对象获取 where 条件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="startChar">条件开始字符</param>
        /// <param name="enableFun">有效性判断,true为有效的条件,false无效的条件</param>
        /// <returns></returns>
        public static string GetWhere(this object entity, string startChar, Func<PropertyInfoExt, Condition, bool> enableFun)
        {
            string whereStr = "";
            Func<string> resultFn = () =>
            {
                return startChar + whereStr;
            };
            if (null == entity) return resultFn();
            if (false == entity.GetType().IsClass) return resultFn();
            if (entity.GetType().IsAbstract) return resultFn();

            Func<object, PropertyInfo, PropertyInfoExt> PropFunc = (_obj, _pi) =>
            {
                object v = _pi.GetValue(_obj);
                return new PropertyInfoExt()
                {
                    PropertyType = _pi.PropertyType,
                    DeclaringType = _pi.DeclaringType,
                    Name = _pi.Name,
                    Value = v,
                    Property = _pi
                };
            };

            Action<PropertyInfoExt, Condition> initCondition = (_pi, _cond) =>
            {
                Type _t = _pi.PropertyType;
                if (_t == typeof(string) || _t == typeof(Guid))
                {
                    _cond.where_igrones = WhereIgrons.igroneEmptyNull;
                }
                else if (_t == typeof(bool))
                {
                    _cond.where_igrones = WhereIgrons.igroneFalse;
                }
                else if (_t == typeof(DateTime))
                {
                    _cond.where_igrones = WhereIgrons.igroneMinMaxDate;
                }
                else if (_t == typeof(int)
                           || _t == typeof(Int16)
                           || _t == typeof(Int64)
                           || _t == typeof(double)
                           || _t == typeof(float)
                           || _t == typeof(decimal))
                {
                    _cond.where_igrones = WhereIgrons.igroneZero;
                }
                else
                {
                    _cond.where_igrones = WhereIgrons.igroneNull;
                }
            };

            bool mbool = null != enableFun;
            if (null == enableFun) enableFun = (pi, cond) => { return true; };
            Condition condition = new Condition();
            Type type = entity.GetType();
            bool isCollect = false;
            Type attType = typeof(Condition);
            Attribute attr = type.GetCustomAttribute(attType);
            Attribute at = null;
            PropertyInfo[] piArr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfoExt propertyExt = null;
            string sw = "";
            foreach (PropertyInfo pi in piArr)
            {
                at = pi.GetCustomAttribute(attType);
                isCollect = (null != pi.PropertyType.GetInterface("System.Collections.ICollection"));
                if (null != at && false == isCollect)
                {
                    propertyExt = PropFunc(entity, pi);
                    if (!enableFun(propertyExt, (Condition)at)) continue;
                    sw = ((Condition)at).Unit(propertyExt);
                    if (string.IsNullOrEmpty(sw)) continue;
                    whereStr += " " + sw;
                }
                else if (null != attr && false == isCollect)
                {
                    propertyExt = PropFunc(entity, pi);
                    if (null == at) initCondition(propertyExt, (Condition)attr);
                    if (!enableFun(propertyExt, (Condition)attr)) continue;
                    ((Condition)attr).FieldMapping = "";
                    sw = ((Condition)attr).Unit(propertyExt);
                    if (string.IsNullOrEmpty(sw)) continue;
                    whereStr += " " + sw;
                }
                else if (mbool)
                {
                    if (isCollect)
                    {
                        if (null == pi.PropertyType.GetInterface("System.Collections.IDictionary")) continue;
                        object vObj = pi.GetValue(entity);
                        if (null == vObj) continue;
                        ICollection collection = (ICollection)vObj;
                        Type[] ts = vObj.GetType().GenericTypeArguments;
                        foreach (var item in collection)
                        {
                            string key = item.GetType().GetProperty("Key").GetValue(item).ToString();
                            object val = item.GetType().GetProperty("Value").GetValue(item);
                            propertyExt = new PropertyInfoExt()
                            {
                                Name = key,
                                Value = val,
                                PropertyType = ts[1],
                                DeclaringType = pi.PropertyType
                            };
                            condition.FieldMapping = propertyExt.Name;
                            initCondition(propertyExt, condition);
                            if (!enableFun(propertyExt, condition)) continue;
                            sw = condition.Unit(propertyExt);
                            if (string.IsNullOrEmpty(sw)) continue;
                            whereStr += " " + sw;
                        }
                    }
                    else
                    {
                        propertyExt = PropFunc(entity, pi);
                        condition.FieldMapping = propertyExt.Name;
                        initCondition(propertyExt, condition);
                        if (!enableFun(propertyExt, condition)) continue;
                        sw = condition.Unit(propertyExt);
                        if (string.IsNullOrEmpty(sw)) continue;
                        whereStr += " " + sw;
                    }
                }
            }

            return resultFn();
        }

        /// <summary>
        /// 创建人:DJ
        /// 日  期:2021-12-02
        /// 根据实体对象获取 where 条件
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetWhere(this object entity)
        {
            return entity.GetWhere("", (pi, condition) => { return true; });
        }

        /// <summary>
        /// 创建人:DJ
        /// 日  期:2021-12-02
        /// 根据实体对象获取 where 条件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="startChar">条件开始字符</param>
        /// <returns></returns>
        public static string GetWhere(this object entity, string startChar)
        {
            return entity.GetWhere(startChar, null);
        }

        /// <summary>
        /// 创建人:DJ
        /// 日  期:2021-12-02
        /// 根据实体对象获取 where 条件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="enableFun"></param>
        /// <returns></returns>
        public static string GetWhere(this object entity, Func<PropertyInfoExt, Condition, bool> enableFun)
        {
            return entity.GetWhere("", enableFun);
        }

        public static T GetSourceInstance<T>(this object newInstance)
        {
            T v = default(T);
            if (newInstance.GetType().IsInterface) return v;
            v = newInstance.GetPropertyValue<T>(TempImpl.InterfaceInstanceType);
            return v;
        }

        /// <summary>
        /// 拆解按位或
        /// </summary>
        /// <param name="bitwiseOR">按位或的值</param>
        /// <returns></returns>
        public static int[] UnBitwiseOR(this int bitwiseOR)
        {
            List<int> list = new List<int>();
            int n = bitwiseOR;
            int num = 100;
            int x = 0;
            while (2 <= n && x < num)
            {
                int a = n / 2;
                int b = n % (2 * a);
                n = a;
                list.Add(b);
                x++;
            }

            if (1 == n) list.Add(n);

            List<int> list1 = new List<int>();
            n = list.Count;
            n--;
            x = 0;
            for (int i = n; i >= 0; i--)
            {
                num = list[i] * (int)Math.Pow(2, i);
                if (0 < num) list1.Add(num);
                x++;
            }

            return list1.ToArray();
        }

        public static object JsonToObject(string json)
        {
            JsonToEntity toEntity = new JsonToEntity();
            return toEntity.GetObject(json);
        }
    }
}
