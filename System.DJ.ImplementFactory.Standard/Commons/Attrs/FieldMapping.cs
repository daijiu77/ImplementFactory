using System.Reflection;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    /// <summary>
    /// Table field names are associated with attribute mappingsProperty identification.
    /// </summary>
    public class FieldMapping : Attribute
    {
        private string fieldName = "";
        private Type fieldType = null;
        private int length = 0;
        private string defaultValue = "";
        private bool isPrimaryKey = false;
        private bool noNull = false;
        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        public FieldMapping(string fieldName)
        {
            this.fieldName = fieldName;
        }

        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        /// <param name="fieldType">The data type of the data table field</param>
        public FieldMapping(string fieldName, Type fieldType)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }

        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        /// <param name="fieldType">The data type of the data table field</param>
        /// <param name="length">The data length of the data table field, if exist</param>
        public FieldMapping(string fieldName, Type fieldType, int length)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
        }

        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        /// <param name="fieldType">The data type of the data table field</param>
        /// <param name="length">The data length of the data table field, if exist</param>
        /// <param name="defaultValue">The initial default value for the data table field</param>
        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        /// <param name="fieldType">The data type of the data table field</param>
        /// <param name="length">The data length of the data table field, if exist</param>
        /// <param name="defaultValue">The initial default value for the data table field.</param>
        /// <param name="IsPrimaryKey">Whether this datasheet field is a primary key</param>
        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue, bool IsPrimaryKey)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
            this.isPrimaryKey = IsPrimaryKey;
        }

        /// <summary>
        /// Table field names are associated with attribute mappingsProperty identification.
        /// </summary>
        /// <param name="fieldName">Data table field names.</param>
        /// <param name="fieldType">The data type of the data table field</param>
        /// <param name="length">The data length of the data table field, if exist</param>
        /// <param name="defaultValue">The initial default value for the data table field.</param>
        /// <param name="IsPrimaryKey">Whether this datasheet field is a primary key</param>
        /// <param name="NoNull">Whether this datasheet field is not allowed to be null</param>
        public FieldMapping(string fieldName, Type fieldType, int length, string defaultValue, bool IsPrimaryKey, bool NoNull)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
            this.length = length;
            this.defaultValue = defaultValue;
            this.isPrimaryKey = IsPrimaryKey;
            this.noNull = NoNull;
        }

        /// <summary>
        /// Data table field names.
        /// </summary>
        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set { fieldName = value; }
        }

        /// <summary>
        /// The data type of the data table field
        /// </summary>
        public Type FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }

        /// <summary>
        /// The data length of the data table field, if exist.
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// The initial default value for the data table field.
        /// </summary>
        public string DefualtValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// Whether this datasheet field is a primary key.
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set { isPrimaryKey = value; }
        }

        /// <summary>
        /// Whether this datasheet field is not allowed to be null
        /// </summary>
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
