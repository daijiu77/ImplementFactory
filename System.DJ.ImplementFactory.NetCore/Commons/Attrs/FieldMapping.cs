using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class FieldMapping : Attribute
    {
        private string fieldName = "";
        private Type fieldType = null;
        private int length = 0;
        private string defaultValue = "";
        private bool isPrimaryKey = false;
        private bool noNull = false;
        public FieldMapping(string fieldName)
        {
            this.fieldName = fieldName;
        }

        public FieldMapping(string fieldName, Type fieldType)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }

        public FieldMapping(string fieldName, Type fieldType, int length)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
        }

        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
        }

        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue, bool IsPrimaryKey)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
            this.isPrimaryKey = IsPrimaryKey;
        }

        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue, bool IsPrimaryKey, bool NoNull)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
            this.isPrimaryKey = IsPrimaryKey;
            this.noNull = NoNull;
        }

        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set { fieldName = value; }
        }

        public Type FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        public string DefualtValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set { isPrimaryKey = value; }
        }

        public bool NoNull
        {
            get { return noNull; }
            set { noNull = value; }
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
