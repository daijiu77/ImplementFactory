using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Framework
{
    public class MyCache : DataCachePool
    {
        public override object GetValueByKey(string key1, string key2)
        {
            return base.GetValueByKey(key1, key2);
        }
    }
}
