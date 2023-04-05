using System.Collections.Generic;
using System.Reflection;

namespace System.DJ.ImplementFactory.Commons
{
    internal class AssembliesData
    {
        private Dictionary<string, List<Assembly>> dic = new Dictionary<string, List<Assembly>>();

        public List<Assembly> this[string key]
        {
            get
            {
                List<Assembly> list = null;
                dic.TryGetValue(key, out list);
                return list;
            }
        }

        public void Add(string key, List<Assembly> list)
        {
            if(dic.ContainsKey(key))
            {
                dic[key].Clear();
                dic.Remove(key);
            }

            dic.Add(key, list);
        }

        public List<Assembly> Get(string key)
        {
            return this[key];
        }
    }
}
