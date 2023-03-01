using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class DataCache : AbsDataCache
    {
        private int cacheTime = ImplementAdapter.dbInfo1.CacheTime_Second;

        public DataCache() { }

        public DataCache(int cacheTime_Second)
        {
            this.cacheTime = cacheTime_Second;
        }

        public static DataCache GetDataCache(MethodInfo methodInfo)
        {
            DataCache dataCache = null;
            if (null == methodInfo) return dataCache;
            object[] attributes = methodInfo.GetCustomAttributes(typeof(AbsDataCache), true);
            foreach (var item in attributes)
            {
                if (null != (item as DataCache))
                {
                    dataCache = (DataCache)item;
                    break;
                }
            }
            return dataCache;
        }

        public int CacheTime { get { return cacheTime; } }
    }
}
