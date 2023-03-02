using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Commons
{
    internal class DllPathDataCollection
    {
        private Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

        public List<string> this[string key]
        {
            get
            {
                List<string> list = null;
                dic.TryGetValue(key, out list);
                return list;
            }
        }

        public void Add(string key, List<string> list)
        {
            if (dic.ContainsKey(key))
            {
                dic[key].Clear();
                dic.Remove(key);
            }

            dic.Add(key, list);
        }

        public List<string> Get(string key)
        {
            return this[key];
        }
    }
}
