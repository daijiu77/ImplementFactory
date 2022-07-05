using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public abstract class AbsDataModel
    {
        public T GetValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return default(T);
            string field = propertyName.Trim().ToLower();
            object fv = default(T);
            object dataModel = this;
            Type tp = dataModel.GetType();
            tp.ForeachProperty((pi, type, fn) =>
            {
                if (!fn.ToLower().Equals(field)) return true;                
                fv = pi.GetValue(dataModel);
                return false;
            });
            return (T)fv;
        }
    }
}
