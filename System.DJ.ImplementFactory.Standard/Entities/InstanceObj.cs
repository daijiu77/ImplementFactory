using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Entities
{
    public class InstanceObj
    {
        public object newInstance { get; set; }
        public Type oldInstanceType { get; set; }
    }
}
