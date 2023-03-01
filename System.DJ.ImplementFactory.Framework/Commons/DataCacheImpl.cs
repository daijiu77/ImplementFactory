using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class DataCacheImpl : IDataCache
    {
        private static Dictionary<string, MethodItem> cacheDic = new Dictionary<string, MethodItem>();
        private static int cacheTime = 0;

        static DataCacheImpl()
        {
            cacheTime = ImplementAdapter.dbInfo1.CacheTime_Second;
            Task.Run(() =>
            {
                List<string> list1 = new List<string>();
                List<string> list2 = new List<string>();
                while (true)
                {
                    foreach (var method in cacheDic)
                    {
                        foreach (var item in method.Value.Children())
                        {
                            if (!item.Value.IsEnabled(1))
                            {
                                list2.Add(item.Key);
                            }
                        }

                        foreach (var k in list2)
                        {
                            ((IDisposable)method.Value[k]).Dispose();
                            method.Value.Remove(k);
                        }
                        list2.Clear();

                        if (0 == method.Value.Children().Count)
                        {
                            list1.Add(method.Key);
                        }
                    }

                    foreach (var k in list1)
                    {
                        cacheDic.Remove(k);
                    }
                    list1.Clear();
                    Thread.Sleep(1000);
                }
            });
        }

        private string GetMethodPath()
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(2);
            MethodBase methodBase = stackFrame.GetMethod();
            int n = 2;
            const int size = 10;
            Type typeMe = this.GetType();
            while (n < size)
            {
                stackFrame = trace.GetFrame(n);
                methodBase = stackFrame.GetMethod();
                if (methodBase.DeclaringType != typeMe) break;
                n++;
            }

            string namespace1 = methodBase.DeclaringType.Namespace;
            string clsName = methodBase.DeclaringType.Name;
            string methodName = methodBase.Name;
            return namespace1 + "-" + clsName + "-" + methodName;
        }

        object IDataCache.Get(string key)
        {
            string methodPath = GetMethodPath();
            if (!cacheDic.ContainsKey(methodPath)) return null;
            return cacheDic[methodPath].GetValue(key);
        }

        void IDataCache.Set(string key, object value)
        {
            ((IDataCache)this).Set(key, value, cacheTime);
        }

        void IDataCache.Set(string key, object value, int cacheTime)
        {
            string methodPath = GetMethodPath();
            MethodItem mItem = null;
            if (cacheDic.ContainsKey(methodPath))
            {
                mItem = cacheDic[methodPath];
                if (null != mItem[key])
                {
                    ((IDisposable)mItem[key]).Dispose();
                    mItem.Remove(key);
                }
            }
            else
            {
                mItem = new MethodItem(methodPath, cacheTime);
                cacheDic.Add(methodPath, mItem);
            }

            mItem.Set(key, value);
            //throw new NotImplementedException();
        }

        public EList<CKeyValue> GetParaNameList(MethodInfo methodInfo)
        {
            EList<CKeyValue> list = new EList<CKeyValue>();
            ParameterInfo[] args = methodInfo.GetParameters();
            string eleTypeName = null;
            Type eleType = null;
            foreach (ParameterInfo item in args)
            {
                if (item.ParameterType.BaseType == typeof(MulticastDelegate)) continue;
                if (typeof(IEnumerable).IsAssignableFrom(item.ParameterType) && typeof(string) != item.ParameterType)
                {
                    if (!item.ParameterType.IsArray) continue;
                    eleTypeName = item.ParameterType.FullName.Replace("[]", "");
                    eleType = Type.GetType(eleTypeName);
                    if (null == eleType) continue;
                    if (!eleType.IsBaseType()) continue;
                }
                if (-1 != item.ParameterType.FullName.IndexOf("&")) continue;
                list.Add(new CKeyValue()
                {
                    Key = item.Name,
                    ValueType = item.ParameterType
                });
            }
            if (0 == list.Count) list = null;
            return list;
        }

        public string GetParaKey(EList<CKeyValue> elist)
        {
            string key = "null";
            if (null != elist)
            {
                string s = "";
                foreach (var item in elist)
                {
                    GetKeyBy(item.ValueType, item.Key, item.Value, ref s);
                }

                if (!string.IsNullOrEmpty(s))
                {
                    key = s.Substring(1);
                }                
            }
            return key;
        }

        private void GetKeyBy(Type paraType, string fn, object dt, ref string s1)
        {
            if (null == dt) s1 = "";
            if (paraType.IsEnum)
            {
                s1 += "-" + fn + "=" + (int)dt;
            }
            else
            {
                s1 += "-" + fn + "=" + dt;
            }
        }

        private class MethodItem : IDisposable
        {
            private string methodPath = "";
            private int cacheTime = 0;
            private Dictionary<string, DataItem> dic = new Dictionary<string, DataItem>();

            public MethodItem(string methodPath, int cacheTime)
            {
                this.methodPath = methodPath;
                this.cacheTime = cacheTime;
            }

            public DataItem this[string key]
            {
                get
                {
                    lock (this)
                    {
                        return dic[key];
                    }
                }
            }

            public void Set(string key, object value)
            {
                lock (this)
                {
                    if (dic.ContainsKey(key))
                    {
                        ((IDisposable)dic[key]).Dispose();
                        dic.Remove(key);
                    }
                    dic[key] = new DataItem(key, value, cacheTime);
                }
            }

            public object GetValue(string key)
            {
                lock (this)
                {
                    return dic[key].GetValue();
                }
            }

            public void Remove(string key)
            {
                lock (this)
                {
                    dic.Remove(key);
                }
            }

            public Dictionary<string, DataItem> Children()
            {
                lock (this)
                {
                    return dic;
                }
            }

            public string MethodPath
            {
                get { return methodPath; }
            }

            void IDisposable.Dispose()
            {
                lock (this)
                {
                    foreach (var item in dic)
                    {
                        ((IDisposable)item.Value).Dispose();
                    }
                    dic.Clear();
                }
            }
        }

        private class DataItem : IDisposable
        {
            private string key = "";
            private object value = null;
            private int cacheTime = 0;

            private DateTime start = DateTime.MinValue;
            private DateTime end = DateTime.MinValue;

            public DataItem(string key, object value, int cacheTime)
            {
                this.key = key;
                this.value = value;
                this.cacheTime = cacheTime;
                start = DateTime.Now;
                end = start.AddSeconds(cacheTime);
            }

            public object GetValue()
            {
                start = DateTime.Now;
                end = start.AddSeconds(cacheTime);
                return value;
            }

            public bool IsEnabled(int num)
            {
                start = start.AddSeconds(num);
                return start < end;
            }

            void IDisposable.Dispose()
            {
                if (typeof(IDisposable).IsAssignableFrom(value.GetType()))
                {
                    ((IDisposable)value).Dispose();
                }
                value = null;
            }
        }
    }
}
