﻿using System;
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
        private bool IsPrimaryKey = false;
        private bool NoNull = false;
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
            this.IsPrimaryKey = IsPrimaryKey;
        }

        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue, bool IsPrimaryKey, bool NoNull)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
            this.IsPrimaryKey = IsPrimaryKey;
            this.NoNull = NoNull;
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
