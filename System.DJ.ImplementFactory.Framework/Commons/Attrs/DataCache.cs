using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    /// <summary>
    /// Enable data caching mechanism.
    /// </summary>
    public class DataCache : AbsDataCache
    {
        private int cacheTime = ImplementAdapter.dbInfo1.CacheTime_Second;
        private bool persistenceCache = false;

        /// <summary>
        /// Enable data caching mechanism.
        /// </summary>
        public DataCache() { }

        /// <summary>
        /// Enable data caching mechanism.
        /// </summary>
        /// <param name="cacheTime_Second">Data cache lifecycle, in seconds</param>
        public DataCache(int cacheTime_Second)
        {
            this.cacheTime = cacheTime_Second;
        }

        /// <summary>
        /// Enable data caching mechanism.
        /// </summary>
        /// <param name="cacheTime_Second">Data cache lifecycle, in seconds</param>
        /// <param name="persistenceCache">Whether to enable the data cache persistence mechanism</param>
        public DataCache(int cacheTime_Second, bool persistenceCache)
        {
            this.cacheTime = cacheTime_Second;
            this.persistenceCache = persistenceCache;
        }

        /// <summary>
        /// Enable data caching mechanism.
        /// </summary>
        /// <param name="persistenceCache">Whether to enable the data cache persistence mechanism</param>
        public DataCache(bool persistenceCache)
        {
            this.persistenceCache = persistenceCache;
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

        /// <summary>
        /// 数据缓存生命周期,单位:秒
        /// </summary>
        public int CacheTime { get { return cacheTime; } }

        /// <summary>
        /// 数据缓存持久化
        /// </summary>
        public bool PersistenceCache { get { return persistenceCache; } }
    }
}
