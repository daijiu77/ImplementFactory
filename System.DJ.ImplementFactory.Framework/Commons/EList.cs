using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public enum OrderBy { asc, desc }

    /// <summary>
    /// 维护及变更： 
    /// 1. 2020-04-27 [查询速度及add速度优化]数据存储机制由单体集合变更为多个集合体,多任务查询,
    /// 以提高查询速度,由于是多个集合体模式,所以必须重构循环体,重新实现 IEnumerable《T》 接口
    /// Author: DJ - Allan
    /// QQ: 564343162
    /// Email: 564343162@qq.com
    /// CreateDate: 2020-03-05
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EList<T> : IEnumerable<T> where T : CKeyValue
    {
        private Dictionary<string, T> _dictionary = new Dictionary<string, T>();
        private List<T> _list = new List<T>();

        private object _obj = new object();

        public EList() : base() { }

        public T this[string key]
        {
            get
            {
                lock (_obj)
                {
                    T t = default(T);
                    _dictionary.TryGetValue(key, out t);
                    return t;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_obj)
                {
                    T t = default(T);
                    if (index < _list.Count)
                    {
                        t = _list[index];
                    }
                    return t;
                }
            }
        }

        public void Add(T cKeyValue)
        {
            lock (_obj)
            {
                if (null == cKeyValue) return;
                string key = cKeyValue.Key;
                if (string.IsNullOrEmpty(key)) return;
                if (_dictionary.ContainsKey(key))
                {
                    T t = _dictionary[key];
                    _list.Remove(t);
                    _dictionary.Remove(key);
                }
                _dictionary.Add(key, cKeyValue);
                _list.Add(cKeyValue);
                _list.Sort();
            }
        }

        public int Count
        {
            get
            {
                lock (_obj)
                {
                    return _list.Count;
                }
            }
        }

        public void Clear(string key)
        {
            lock (_obj)
            {
                if (_dictionary.ContainsKey(key))
                {
                    T t = _dictionary[key];
                    _list.Remove(t);
                    _dictionary.Remove(key);
                }
            }
        }

        public void Clear()
        {
            lock (_obj)
            {
                _dictionary.Clear();
                _list.Clear();
            }
        }

        public void ForEach(Func<T, int, bool> func)
        {            
            int n = 0;
            bool mbool = false;
            foreach (var item in _list)
            {
                mbool = func(item, n);
                if (!mbool) break;
                n++;
            }
        }

        public void ForEach(Action<T> action)
        {
            ForEach((o, index) =>
            {
                action(o);
                return true;
            });
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator<T>, IEnumerator
        {
            private int index = 0;
            private EList<T> elist = null;

            public Enumerator(EList<T> elist)
            {
                this.elist = elist;
            }

            object IEnumerator.Current => current;

            T IEnumerator<T>.Current => (T)current;

            void IDisposable.Dispose()
            {
                index = 0;
            }

            bool IEnumerator.MoveNext()
            {
                if (index >= elist.Count) return false;
                current = elist[index];
                index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                index = 0;
            }

            public object current { get; set; }
        }
    }

    public class CKeyValue : IComparable<CKeyValue>
    {
        static Regex isNum = null;
        string key = "";

        static CKeyValue()
        {
            isNum = new Regex("[0-9]");
        }

        public static CKeyValue KV(string key, object val)
        {
            CKeyValue kv = new CKeyValue();
            kv.key = key;
            kv.Value = val;
            return kv;
        }

        public static CKeyValue KV(string key, object val, object other)
        {
            CKeyValue kv = new CKeyValue();
            kv.key = key;
            kv.Value = val;
            kv.other = other;
            return kv;
        }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                if (string.IsNullOrEmpty(this.key) || isReset)
                {
                    this.key = value;
                    isReset = false;
                }
            }
        }

        public bool isReset { get; set; }
        public object Value { get; set; }

        public object other { get; set; }

        public Type ValueType { get; set; }

        public Type otherType { get; set; }

        public int orderBy { get; set; }

        int IComparable<CKeyValue>.CompareTo(CKeyValue other)
        {
            int n = key.CompareTo(other.key);

            return n;
        }

    }
}
