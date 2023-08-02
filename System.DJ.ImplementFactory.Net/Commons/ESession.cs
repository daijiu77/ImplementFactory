using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class ESession
    {
        private static Dictionary<string, SrcIPData> kvDic = new Dictionary<string, SrcIPData>();
        private const int defMinute = 3;

        static ESession()
        {
            Task.Run(() =>
            {
                DateTime dt = DateTime.Now;

                while (true)
                {
                    dt = DateTime.Now;
                    CheckEnabled(dt);
                    Thread.Sleep(2000);
                }
            });
        }

        private static object _ESessionLock = new object();

        private static void CheckEnabled(DateTime dt)
        {
            lock (_ESessionLock)
            {
                List<string> keys = new List<string>();
                List<Type> types = new List<Type>();
                foreach (var item in kvDic)
                {
                    types.Clear();
                    item.Value.Foreach(ipData =>
                    {
                        keys.Clear();
                        ipData.Foreach(gd =>
                        {
                            if (dt >= gd.end)
                            {
                                keys.Add(gd.key);
                            }
                        });

                        if (0 < keys.Count)
                        {
                            types.Add(ipData.SrcType);
                        }
                        RemoveGData(keys, ipData);
                    });
                    RemoveIPData(types, item.Value);
                }
                RemoveKvDic();
            }
        }

        private static void RemoveKvDic()
        {
            lock (_ESessionLock)
            {
                int n = 0;
                int size = kvDic.Count;
                List<string> list = new List<string>();
                foreach (var item in kvDic)
                {
                    list.Add(item.Key);
                }

                string key = "";
                while (n < size)
                {
                    key = list[n];
                    if (0 == kvDic[key].Count)
                    {
                        list.Remove(key);
                        kvDic.Remove(key);
                        size = kvDic.Count;
                        n = 0;
                    }
                    else
                    {
                        n++;
                    }
                }
            }
        }

        private static void RemoveIPData(List<Type> types, SrcIPData srcIPData)
        {
            lock (_ESessionLock)
            {
                if (null == types) return;
                if (0 == types.Count) return;
                foreach (var item in types)
                {
                    srcIPData.Remove(item);
                }
            }
        }

        private static void RemoveGData(List<string> keys, IPData ipData)
        {
            lock (_ESessionLock)
            {
                if (null == keys) return;
                if (0 == keys.Count) return;
                foreach (string key in keys)
                {
                    ipData.Remove(key);
                }
            }
        }

        public static void Add(HttpContext httpContext, string key, object value, int liveCycle_second)
        {
            lock (_ESessionLock)
            {
                string client_ip = AbsActionFilterAttribute.GetIP(httpContext);
                SrcIPData srcIPData = null;
                kvDic.TryGetValue(client_ip, out srcIPData);
                if (null == srcIPData)
                {
                    srcIPData = new SrcIPData();
                    kvDic.Add(client_ip, srcIPData);
                }

                if (0 >= liveCycle_second) liveCycle_second = 60 * defMinute;
                Type srcType = GetSrcType();
                DateTime dt = DateTime.Now;

                GData gd = new GData(key, value);
                gd.SetStart(dt)
                    .SetEnd(dt.AddSeconds(liveCycle_second))
                    .SetIP(client_ip)
                    .SetSrcType(srcType);

                srcIPData.Add(srcType, gd);
            }
        }

        public static void Add(HttpContext httpContext, string key, object value)
        {
            int liveCycle = 60 * defMinute;
            Add(httpContext, key, value, liveCycle);
        }

        public static GData Get<T>(HttpContext httpContext, string key)
        {
            lock (_ESessionLock)
            {
                return Get(httpContext, typeof(T), key);
            }
        }

        public static GData Get(HttpContext httpContext, Type dataFromClass, string key)
        {
            lock (_ESessionLock)
            {
                GData gd = null;
                string client_ip = AbsActionFilterAttribute.GetIP(httpContext);
                SrcIPData srcIPData = null;
                kvDic.TryGetValue(client_ip, out srcIPData);
                if (null == srcIPData) return gd;
                IPData iPData = srcIPData[dataFromClass];
                if (null == iPData) return gd;
                gd = iPData[key];

                iPData.Remove(key);
                srcIPData.Remove(dataFromClass);
                if (0 == srcIPData.Count)
                {
                    kvDic.Remove(client_ip);
                }
                return gd;
            }
        }

        private static Type GetSrcType()
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = null;
            MethodBase mb = null;
            Type meType = typeof(ESession);
            Type pt = null;
            Type srcType = null;
            const int maxNum = 10;
            int num = 0;
            while (num <= maxNum)
            {
                stackFrame = trace.GetFrame(num);
                if (null == stackFrame) break;
                mb = stackFrame.GetMethod();
                if (null == mb) break;
                pt = mb.DeclaringType;
                if (null == pt) break;
                if (pt != meType)
                {
                    srcType = pt;
                    break;
                }
                num++;
            }

            return srcType;
        }

        class IPData
        {
            /// <summary>
            /// key: Id, value: data
            /// </summary>
            private Dictionary<string, GData> gdDic = new Dictionary<string, GData>();

            public GData this[string key]
            {
                get
                {
                    GData gd = null;
                    string k = key.ToLower().Trim();
                    gdDic.TryGetValue(k, out gd);
                    return gd;
                }
            }

            public void Add(string key, GData data)
            {
                string k = key.ToLower().Trim();
                if (gdDic.ContainsKey(k)) gdDic.Remove(k);
                gdDic.Add(k, data);
            }

            public void Foreach(Action<GData> action)
            {
                foreach (var item in gdDic)
                {
                    action(item.Value);
                }
            }

            public int Count
            {
                get
                {
                    return gdDic.Count;
                }
            }

            public void Remove(string key)
            {
                if (null == key) return;
                string kn = key.ToLower().Trim();
                gdDic.Remove(kn);
            }

            public Type SrcType { get; private set; }

            public IPData SetSrcType(Type srcType)
            {
                SrcType = srcType;
                return this;
            }
        }

        class SrcIPData
        {
            /// <summary>
            /// key: Sets the class in which the data resides, value: IPData
            /// </summary>
            private Dictionary<string, IPData> dic = new Dictionary<string, IPData>();

            public IPData this[Type type]
            {
                get
                {
                    return GetIPDataBy(type);
                }
            }

            public IPData GetIPDataBy(Type type)
            {
                IPData srcIPData = null;
                if (null == type) return srcIPData;
                string key = type.FullName;
                dic.TryGetValue(key, out srcIPData);
                return srcIPData;
            }

            public void Add(Type srcType, GData data)
            {
                string key = srcType.FullName;
                IPData srcIPData = null;
                dic.TryGetValue(key, out srcIPData);
                if (null == srcIPData)
                {
                    srcIPData = new IPData();
                    srcIPData.SetSrcType(srcType);
                    dic.Add(key, srcIPData);
                }

                srcIPData.Add(data.key, data);
            }

            public void Foreach(Action<IPData> callback)
            {
                foreach (var item in dic)
                {
                    callback(item.Value);
                }
            }

            public int Count
            {
                get
                {
                    return dic.Count;
                }
            }

            public void Remove(Type type)
            {
                string key = type.FullName;
                if (dic.ContainsKey(key))
                {
                    IPData srcIPData = dic[key];
                    if (0 == srcIPData.Count)
                    {
                        dic.Remove(key);
                    }
                }
            }

        }
    }

    public class GData
    {
        public GData(string key, object val)
        {
            this.key = key;
            data = val;
            start = DateTime.Now;
            end = start.AddHours(1);
        }

        public GData SetStart(DateTime start)
        {
            this.start = start;
            return this;
        }

        public GData SetEnd(DateTime end)
        {
            this.end = end;
            return this;
        }

        public GData SetIP(string ip)
        {
            this.ip = ip;
            return this;
        }

        public GData SetSrcType(Type srcType)
        {
            this.srcType = srcType;
            return this;
        }

        public override string ToString()
        {
            if (null != data)
            {
                return data.ToString();
            }
            else
            {
                return string.Format("{0} , {1}", ip, key);
            }
        }

        public string key { get; private set; }
        public object data { get; private set; }
        public string ip { get; private set; }
        public Type srcType { get; private set; }
        public DateTime start { get; private set; }
        public DateTime end { get; private set; }
    }
}
