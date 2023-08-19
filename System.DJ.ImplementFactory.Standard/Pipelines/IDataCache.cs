using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Entities;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDataCache
    {
        void Set(MethodInfo method, string key, object value);
        void Set(MethodInfo method, string key, object value, int cacheTime);
        void Set(MethodInfo method, string key, object value, int cacheTime, bool persistenceCache);
        object Get(MethodInfo method, string key);
        object Get(MethodInfo method, string key, ref RefOutParams refOutParams);
    }
}
