using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static bool ExistConstraint(Type modelType)
        {
            bool isExist = false;
            if (null == modelType) return isExist;
            if (modelType.IsBaseType()) return isExist;
            Attribute attr = null;
            modelType.ForeachProperty((pi, pt, fn) =>
            {
                attr = pi.GetCustomAttribute(typeof(System.DJ.ImplementFactory.Commons.Attrs.Constraint));
                if (null != attr)
                {
                    isExist = true;
                    return;
                }
            });
            return isExist;
        }
    }
}
