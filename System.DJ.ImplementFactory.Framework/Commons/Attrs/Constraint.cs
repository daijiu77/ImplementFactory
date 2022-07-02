using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class Constraint : Attribute
    {
        public Constraint(string foreignKey, string refrenceKey, params string[] foreign_refrenceKeys)
        {
            this.ForeignKey = foreignKey;
            this.RefrenceKey = refrenceKey;
            this.Foreign_refrenceKeys = foreign_refrenceKeys;
        }
        public string ForeignKey { get; set; }
        public string RefrenceKey { get; set; }
        public string[] Foreign_refrenceKeys { get; set; }
    }
}
