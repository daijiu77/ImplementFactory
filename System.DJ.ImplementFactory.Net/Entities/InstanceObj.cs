using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class InstanceObj
    {
        public object newInstance { get; set; }
        public Type newInstanceType { get; set; }
        public Type oldInstanceType { get; set; }
    }
}
