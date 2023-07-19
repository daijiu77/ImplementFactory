using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.DJ.ImplementFactory.Commons.Exts
{
    public static class ExtCollection
    {        
        public static object createListByType(this Type type)
        {
            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("createList", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return null;
            method = method.MakeGenericMethod(type);
            object list = null;
            try
            {
                list = method.Invoke(cc, null);
            }
            catch (Exception ex)
            {
                //throw;
            }
            return list;
        }

        public static void listAdd(this object list, object listElement)
        {
            if (null == list) return;

            Type tp = list.GetType();
            if ((false == typeof(IList).IsAssignableFrom(tp)) && (typeof(IList) != tp)) return;
            Type eleType = tp.GetGenericArguments()[0];
            if (null != listElement)
            {
                if (listElement.GetType() != eleType) return;
            }
            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("listAdd", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return;
            method = method.MakeGenericMethod(eleType);
            try
            {
                method.Invoke(cc, new object[] { list, listElement });
            }
            catch (Exception ex)
            {

                // throw;
            }
        }

        public static object createArrayByType(this Type type, int length)
        {
            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("createArray", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return null;
            method = method.MakeGenericMethod(type);
            object arr = null;
            try
            {
                arr = method.Invoke(cc, new object[] { length });
            }
            catch (Exception ex)
            {

                //throw;
            }
            return arr;
        }

        public static void arrayAdd(this object arrObj, object arrElement, int eleIndex)
        {
            if (null == arrObj) return;
            if (!arrObj.GetType().IsArray) return;
            Type srcTp = arrObj.GetType().GetTypeForArrayElement();
            if (null != arrElement)
            {
                Type eleTp = arrElement.GetType();
                if (!srcTp.Equals(eleTp)) return;
            }

            Type type = srcTp;
            if (null == type) return;

            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("arrayAdd", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return;
            method = method.MakeGenericMethod(type);
            try
            {
                method.Invoke(cc, new object[] { arrObj, arrElement, eleIndex });
            }
            catch (Exception ex)
            {

                //throw;
            }
        }

        public static object listToArray(this object list)
        {
            if (null == list) return null;
            Type tp = list.GetType();
            if (!tp.IsList()) return null;
            Type eleType = tp.GetGenericArguments()[0];

            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("listToArray", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return null;
            method = method.MakeGenericMethod(eleType);
            object arr = null;
            try
            {
                arr = method.Invoke(cc, new object[] { list });
            }
            catch (Exception ex)
            {

                //throw;
            }
            return arr;
        }

        public static object createDictionaryByType(this Type type)
        {
            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("createDictionary", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return null;
            method = method.MakeGenericMethod(typeof(string), type);
            object dic = null;
            try
            {
                dic = method.Invoke(cc, null);
            }
            catch (Exception ex)
            {

                //throw;
            }
            return dic;
        }

        public static void dictionaryAdd(this object dic, string key, object val)
        {
            if (null == dic) return;
            if (string.IsNullOrEmpty(key)) return;
            Type[] ts = dic.GetType().GetGenericArguments();
            if (2 != ts.Length) return;
            if (ts[0] != typeof(string)) return;
            if (null != val)
            {
                if (ts[1] != val.GetType()) return;
            }

            CreateCollection cc = new CreateCollection();
            MethodInfo method = cc.GetType().GetMethod("dictionaryAdd", BindingFlags.Instance | BindingFlags.Public);
            if (null == method) return;
            method = method.MakeGenericMethod(ts);
            try
            {
                method.Invoke(cc, new object[] { dic, key, val });
            }
            catch (Exception ex)
            {

                //throw;
            }
        }

        class CreateCollection
        {
            public T[] createArray<T>(int length)
            {
                return new T[length];
            }

            public void arrayAdd<T>(T[] arr, T ele, int index)
            {
                if (arr.Length <= index) return;
                arr[index] = ele;
            }

            public IList<T> createList<T>()
            {
                return new List<T>();
            }

            public void listAdd<T>(IList<T> list, T ele)
            {
                list.Add(ele);
            }

            public T[] listToArray<T>(IList<T> list)
            {
                return ((List<T>)list).ToArray();
            }

            public Dictionary<T, TT> createDictionary<T, TT>()
            {
                return new Dictionary<T, TT>();
            }

            public void dictionaryAdd<T, TT>(Dictionary<T, TT> dic, T key, TT val)
            {
                if (dic.ContainsKey(key)) dic.Remove(key);
                dic.Add(key, val);
            }
        }
    }
}
