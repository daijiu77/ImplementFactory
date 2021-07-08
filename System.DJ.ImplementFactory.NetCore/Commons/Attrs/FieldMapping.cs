using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class FieldMapping : Attribute
    {
        string fieldName = "";
        public FieldMapping(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public string FieldName
        {
            get
            {
                return fieldName;
            }
        }

        public static string GetFieldMapping(PropertyInfo propertyInfo)
        {
            string field = "";
            object[] arr = propertyInfo.GetCustomAttributes(typeof(FieldMapping), true);
            if (null != arr)
            {
                foreach (var mp in arr)
                {
                    if (null != (mp as FieldMapping))
                    {
                        field = ((FieldMapping)mp).FieldName;
                        break;
                    }
                }
            }
            return field;
        }
    }
}
