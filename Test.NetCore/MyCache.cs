using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DCache;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.NetCore
{
    public class MyCache : DataCachePool
    {
        public override object GetValueByKey(string key1, string key2)
        {
            return base.GetValueByKey(key1, key2);
        }
    }
}
