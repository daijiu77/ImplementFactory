using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDataCache
    {
        void Set(string key, object value);
        void Set(string key, object value, int cacheTime);
        void Set(string key, object value, int cacheTime, bool persistenceCache);
        object Get(string key);
    }
}
