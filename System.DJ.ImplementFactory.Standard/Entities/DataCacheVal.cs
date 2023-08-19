using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataCacheVal
    {
        public object result { get; set; }
        public RefOutParams refOutParams { get; set; }
    }

    public class RefOutParams
    {
        private Dictionary<string, CKeyValue> dic = new Dictionary<string, CKeyValue>();
        public CKeyValue this[string key]
        {
            get
            {
                CKeyValue v = null;
                dic.TryGetValue(key, out v);
                return v;
            }
        }

        public void Add(string key, CKeyValue value)
        {
            if (dic.ContainsKey(key)) return;
            dic.Add(key, value);
        }

        public void Add(string key, object value, Type valueType)
        {
            if (dic.ContainsKey(key)) return;
            dic.Add(key, new CKeyValue()
            {
                Key = key,
                Value = value,
                ValueType = valueType
            });
        }

        public void Foreach(Func<CKeyValue, bool> func)
        {
            if (null == func) return;
            foreach (var item in dic)
            {
                if (!func(item.Value)) break;
            }
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);
            CKeyValue ckv = null;
            dic.TryGetValue(key, out ckv);
            bool mbool = false;
            if (null != ckv)
            {
                if (null != ckv.Value)
                {
                    try
                    {
                        value = (T)ckv.Value;
                        mbool = true;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }
            }
            return mbool;
        }
    }
}
