using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return v;
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
