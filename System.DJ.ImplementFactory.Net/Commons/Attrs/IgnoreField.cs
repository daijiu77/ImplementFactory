using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Commons.Attrs
{
    public class IgnoreField : Attribute
    {
        public enum IgnoreType
        {
            none = 0,
            Insert = 1,
            Update = 2,
            Procedure = 4,
            All = (1 | 2 | 4)
        }

        public IgnoreType ignoreType { get; set; } = IgnoreType.none;

        public IgnoreField(IgnoreType ignoreType)
        {
            this.ignoreType = ignoreType;
        }
    }
}
