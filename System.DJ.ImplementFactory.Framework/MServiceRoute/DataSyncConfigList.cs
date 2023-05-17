using System.Collections.Generic;
using System.DJ.ImplementFactory.Entities;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class DataSyncConfigList<T> where T : DataSyncConfig
    {
        private Dictionary<string, T> _configDic = new Dictionary<string, T>();

        public T this[string routeName]
        {
            get
            {
                T tObj = default(T);
                if (string.IsNullOrEmpty(routeName)) return tObj;
                string key = routeName.Trim().ToLower();
                _configDic.TryGetValue(key, out tObj);
                return tObj;
            }
        }

        public void Add(T dataSyncConfig)
        {
            if (null == dataSyncConfig) return;
            DataSyncConfig dsc = (DataSyncConfig)dataSyncConfig;
            string key = dsc.Name.Trim().ToLower();
            if (_configDic.ContainsKey(key)) return;
            _configDic.Add(key, dataSyncConfig);
        }

        public void Remove(string routeName)
        {
            if (string.IsNullOrEmpty(routeName)) return;
            string key = routeName.Trim().ToLower();
            _configDic.Remove(key);
        }

        public void Clear()
        {
            _configDic.Clear();
        }

        public bool Contains(string routeName)
        {
            if (string.IsNullOrEmpty(routeName)) return false;
            string key = routeName.Trim().ToLower();
            return _configDic.ContainsKey(key);
        }

        public void Foreach(Action<T> action)
        {
            foreach (var item in _configDic)
            {
                action(item.Value);
            }
        }

        public int Count
        {
            get { return _configDic.Count; }
        }

    }
}
