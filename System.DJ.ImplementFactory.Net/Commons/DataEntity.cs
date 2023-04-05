using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Linq;
using System.Reflection;

namespace System.DJ.ImplementFactory.Commons
{
    public enum AndOr
    {
        and,
        or
    }

    public class DataElement : AbsDataModel
    {
        public DataElement(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
        public string name { get; set; }

        public object value { get; set; }

        /// <summary>
        /// 参数条件比较的源表字段名称
        /// </summary>
        public string fieldNameOfSourceTable { get; set; }

        /// <summary>
        /// 比较符号：等于\不等于\大于\大于等于\小于\小于等于\like
        /// </summary>
        public string compareSign { get; set; }

        /// <summary>
        /// 与前一比较逻辑的连接符(and、or)
        /// </summary>
        public AndOr logicUnion { get; set; }

        /// <summary>
        /// 值是字符
        /// </summary>
        public bool valueIsChar { get; set; } = true;

        /// <summary>
        /// 通过外部判断当前条件单元是否有效(但不包含群组条件)
        /// </summary>
        public Func<DataElement, bool> IsEnabledContion { get; set; } = (dataElement) => { return true; };

        /// <summary>
        /// 群组条件与当前条件单元的连接符(and、or)
        /// </summary>
        public AndOr groupLogicUnion { get; set; }

        /// <summary>
        /// 群组条件
        /// </summary>
        private DataEntity<DataElement> _groupCondition = new DataEntity<DataElement>();
        public DataEntity<DataElement> groupCondition => _groupCondition;

        public override string ToString()
        {
            return null == value ? "" : value.ToString();
        }

        public int TryInt()
        {
            string s = ToString();
            int n = 0;
            int.TryParse(s, out n);
            return n;
        }

        public long TryLong()
        {
            string s = ToString();
            Int64 n = 0;
            Int64.TryParse(s, out n);
            return n;
        }

        public Int64 TryInt64()
        {
            return TryLong();
        }

        public float TryFloat()
        {
            string s = ToString();
            float n = 0;
            float.TryParse(s, out n);
            return n;
        }

        public double TryDouble()
        {
            string s = ToString();
            double n = 0;
            double.TryParse(s, out n);
            return n;
        }

        public bool TryBool()
        {
            string s = ToString();
            return s.Trim().ToLower().Equals("true");
        }

        public DateTime TryDateTime()
        {
            string s = ToString();
            DateTime n = DateTime.Now;
            DateTime.TryParse(s, out n);
            return n;
        }

        public decimal TryDecimal()
        {
            string s = ToString();
            decimal n = 0;
            decimal.TryParse(s, out n);
            return n;
        }

        public Guid TryGuid()
        {
            string s = ToString();
            Guid n = Guid.Empty;
            Guid.TryParse(s, out n);
            return n;
        }

        public T TryObject<T>()
        {
            string valueStr = ToString();
            object obj = null;
            T v = default(T);
            if (DJTools.IsBaseType(typeof(T)))
            {
                Type type = typeof(T);
                string s = type.ToString();
                string typeName = s.Substring(s.LastIndexOf(".") + 1);
                typeName = typeName.Replace("]", "");
                typeName = typeName.Replace("&", "");
                string methodName = "To" + typeName;
                try
                {
                    Type t = Type.GetType("System.Convert");
                    //执行Convert的静态方法
                    obj = t.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { valueStr });
                    v = (T)obj;
                }
                catch (Exception ex)
                {
                    //throw;
                }
            }
            else if (typeof(string) == typeof(T))
            {
                obj = value;
                v = (T)obj;
            }
            else if (null != value)
            {
                v = (T)value;
            }
            return v;
        }
    }

    public class DataEntity<T> : IEnumerable<T> where T : DataElement
    {
        private IEnumeratorImpl<T> enumeratorImpl = null;
        private Dictionary<string, object> dataDic = new Dictionary<string, object>();

        public DataEntity()
        {
            enumeratorImpl = new IEnumeratorImpl<T>(this);
        }

        public T this[string name]
        {
            get
            {
                DataElement de = null;
                if (dataDic.ContainsKey(name))
                {
                    object obj = dataDic[name];
                    de = new DataElement(name, obj);
                }                
                return (T)de;
            }
        }

        public bool ContainsKey(string key)
        {
            return dataDic.ContainsKey(key);
        }

        public bool Add(string name, object value)
        {
            object obj = null;
            dataDic.TryGetValue(name, out obj);
            if (null != obj) return false;

            dataDic.Add(name, value);
            return true;
        }

        public void Clear()
        {
            dataDic.Clear();
        }

        public void RemoveAt(int index)
        {
            if (dataDic.Count <= index) return;
            KeyValuePair<string, object> kv = dataDic.ElementAt(index);
            dataDic.Remove(kv.Key);
        }

        public void Remove(string name)
        {
            dataDic.Remove(name);
        }

        public int Count { get { return dataDic.Count; } }

        public void Foreach(Func<DataElement, bool> func)
        {
            foreach (DataElement item in this)
            {
                if (!func(item)) break;
            }
        }

        public void Foreach(Action<DataElement> action)
        {
            Foreach(dataElement =>
            {
                action(dataElement);
                return true;
            });
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return enumeratorImpl;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumeratorImpl;
        }

        class IEnumeratorImpl<TT> : IEnumerator<TT>, IEnumerator where TT : DataElement
        {
            private DataElement currentObj = null;
            private DataEntity<TT> dataEntity = null;
            private int index = 0;

            public IEnumeratorImpl(DataEntity<TT> dataEntity)
            {
                this.dataEntity = dataEntity;
            }

            TT IEnumerator<TT>.Current => (TT)currentObj;

            object IEnumerator.Current => currentObj;

            void IDisposable.Dispose()
            {
                //dataEntity.Clear();
                index = 0;
            }

            bool IEnumerator.MoveNext()
            {
                if (dataEntity.dataDic.Count <= index) return false;
                KeyValuePair<string, object> kv = dataEntity.dataDic.ElementAt(index);
                currentObj = new DataElement(kv.Key, kv.Value);
                index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                index = 0;
            }
        }
    }
}
