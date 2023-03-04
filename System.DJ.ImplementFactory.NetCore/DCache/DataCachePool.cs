using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess.AnalysisDataModel;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.DCache
{
    public class DataCachePool : IDataCache
    {
        private static Dictionary<string, MethodItem> cacheDic = new Dictionary<string, MethodItem>();
        private static List<DataItem> idList = new List<DataItem>();
        private static int cacheTime = 0;
        private static bool execState = false;

        static DataCachePool()
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

                            if (false == execState)
                            {
                                if (!string.IsNullOrEmpty(item.Value.GetId()))
                                {
                                    idList.Add(item.Value);
                                }
                            }
                        }

                        if (false == execState)
                        {
                            execState = 0 < idList.Count;
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

            Task.Run(() =>
            {
                PersistenceCache persistence = new PersistenceCache();
                while (true)
                {
                    if (execState)
                    {
                        foreach (var item in idList)
                        {
                            persistence.UpdateTime(item.GetId(), item.GetStart(), item.GetEnd());
                        }
                        execState = false;
                        idList.Clear();
                    }
                    Thread.Sleep(5000);
                }
            });
        }

        private string GetMethodPath(MethodInfo methodBase)
        {
            string namespace1 = methodBase.DeclaringType.Namespace;
            string clsName = methodBase.DeclaringType.Name;
            string methodName = methodBase.Name;
            Regex rg = new Regex(@"^(?<cName>[a-z0-9_]+)_[0-9]{14}_[0-9]+", RegexOptions.IgnoreCase);
            if (rg.IsMatch(clsName))
            {
                clsName = rg.Match(clsName).Groups["cName"].Value;
            }
            return namespace1 + "-" + clsName + "-" + methodName;
        }

        object IDataCache.Get(MethodInfo method, string key)
        {
            string methodPath = GetMethodPath(method);
            object vObj = GetValueByKey(methodPath, key);
            if (null != vObj) return vObj;

            PersistenceCache.task.Wait();
            if (!cacheDic.ContainsKey(methodPath)) return null;
            return cacheDic[methodPath].GetValue(key);
        }

        void IDataCache.Set(MethodInfo method, string key, object value)
        {
            ((IDataCache)this).Set(method, key, value, cacheTime, false);
        }

        void IDataCache.Set(MethodInfo method, string key, object value, int cacheTime)
        {
            ((IDataCache)this).Set(method, key, value, cacheTime, false);
        }

        void IDataCache.Set(MethodInfo method, string key, object value, int cacheTime, bool persistenceCache)
        {
            string methodPath = GetMethodPath(method);
            bool mbool = SetValue(methodPath, key, value, cacheTime, persistenceCache);
            string id = "";
            if (persistenceCache)
            {
                PersistenceCache persistence = new PersistenceCache();
                Type tp = value.GetType();
                Type type = null;
                object vObj = null;
                if (typeof(IList).IsAssignableFrom(tp))
                {
                    Type[] ts = tp.GetGenericArguments();
                    type = ts[0];
                    IList list = (IList)value;
                    vObj = list[0];
                }
                else if (!tp.IsBaseType())
                {
                    type = tp;
                    vObj = value;
                }

                if (null != type)
                {
                    type.ForeachProperty((pi, pt, fn) =>
                    {
                        if (fn.Equals(OverrideModel.CopyParentModel))
                        {
                            tp = vObj.GetPropertyValue<Type>(fn);
                            return false;
                        }
                        return true;
                    });
                }
                Guid guid = persistence.Set(methodPath, key, value, tp, cacheTime, DateTime.Now, DateTime.Now.AddSeconds(cacheTime));
                if (Guid.Empty != guid)
                {
                    id = guid.ToString();
                }
            }
            if (mbool) return;

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

            mItem.Set(key, value).SetDataType(value.GetType());
            mItem[key].SetId(id);
        }

        public void Put(string id, string methodPath, string key, object value, string dataType, int cacheTime, DateTime start, DateTime end)
        {
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
                Type type = Type.GetType(dataType);
                mItem = new MethodItem(methodPath, cacheTime);
                mItem.SetDataType(type);
                cacheDic.Add(methodPath, mItem);
            }

            mItem.Set(key, value);

            mItem[key]
                .SetId(id)
                .SetStartTime(start)
                .SetEnd(end);
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

        public virtual object GetValueByKey(string methodPath, string key)
        {
            return null;
        }

        public virtual bool SetValue(string methodPath, string key, object value, int cacheCycle_second, bool persistenceCache)
        {
            return false;
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
            private Type dataType = null;
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

            public MethodItem Set(string key, object value)
            {
                lock (this)
                {
                    if (dic.ContainsKey(key))
                    {
                        ((IDisposable)dic[key]).Dispose();
                        dic.Remove(key);
                    }
                    dic[key] = new DataItem(key, value, cacheTime);
                    return this;
                }
            }

            public object GetValue(string key)
            {
                lock (this)
                {
                    return dic[key].GetValue();
                }
            }

            public MethodItem Remove(string key)
            {
                lock (this)
                {
                    string id = dic[key].GetId();
                    if (!string.IsNullOrEmpty(id))
                    {
                        PersistenceCache persistence = new PersistenceCache();
                        persistence.Remove(id);
                    }
                    dic.Remove(key);
                    return this;
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

            public MethodItem SetDataType(Type dataType)
            {
                this.dataType = dataType;
                return this;
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
            private string id = "";
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

            public DataItem SetStartTime(DateTime start)
            {
                this.start = start;
                return this;
            }

            public DataItem SetEnd(DateTime end)
            {
                this.end = end;
                return this;
            }

            public DataItem SetId(string id)
            {
                this.id = id;
                return this;
            }

            public DateTime GetStart()
            {
                return start;
            }

            public DateTime GetEnd()
            {
                return end;
            }

            public string GetId()
            {
                return id;
            }

            void IDisposable.Dispose()
            {
                if (null == value) return;
                if (typeof(IDisposable).IsAssignableFrom(value.GetType()))
                {
                    ((IDisposable)value).Dispose();
                }
                value = null;
            }
        }
    }
}
