using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Exts
{
    public static class ExtCollection
    {
        private static object _createArrayByType = new object();
        public static object createArrayByType(this Type type, int length)
        {
            lock (_createArrayByType)
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
        }

        private static object _arrayAdd = new object();
        public static void arrayAdd(this object arrObj, object arrElement, int eleIndex)
        {
            lock (_arrayAdd)
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
        }

        private static object _createListByType = new object();
        public static object createListByType(this Type type)
        {
            lock (_createListByType)
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
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                return v;
            }
        }

        private static object _listAdd = new object();
        public static void listAdd(this object list, object listElement)
        {
            lock (_listAdd)
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
        }

        private static object _createDictionaryByType = new object();
        public static object createDictionaryByType(this Type type)
        {
            lock (_createDictionaryByType)
            {
                object dic = null;
                try
                {
                    dic = Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                return dic;
            }
        }

        private static object _dictionaryAdd = new object();
        public static void dictionaryAdd(this object dic, string key, object val)
        {
            lock (_dictionaryAdd)
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
}
