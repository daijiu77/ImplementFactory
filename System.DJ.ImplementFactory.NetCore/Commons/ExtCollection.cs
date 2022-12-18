using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public static class ExtCollection
    {

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

    }
}
