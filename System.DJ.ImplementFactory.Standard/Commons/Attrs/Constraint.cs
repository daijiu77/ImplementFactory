using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class Constraint : Attribute
    {
        public Constraint(string foreignKey, string refrenceKey)
        {
            this.ForeignKey = foreignKey;
            this.RefrenceKey = refrenceKey;
        }
        public string ForeignKey { get; set; }
        public string RefrenceKey { get; set; }
    }
}
