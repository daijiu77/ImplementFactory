using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class TableFieldInfo
    {
        public string TableName { get; set; }

        /// <summary>
        /// Field Name
        /// </summary>
        public string Name { get; private set; }
        public string ValueType { get; private set; }
        public int Length { get; private set; }

        public bool IsNull { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public TableFieldInfo SetTableName(string tableName)
        {
            TableName = tableName;
            return this;
        }

        public TableFieldInfo SetName(string name)
        {
            this.Name = name;
            return this;
        }

        public TableFieldInfo SetValueType(string valueType)
        {
            this.ValueType = valueType;
            return this;
        }

        public TableFieldInfo SetLength(int length)
        {
            this.Length = length;
            return this;
        }

        public TableFieldInfo SetIsNull(bool isNull)
        {
            IsNull = isNull;
            return this;
        }

        public TableFieldInfo SetIsPrimaryKey(bool isPrimaryKey)
        {
            IsPrimaryKey = isPrimaryKey;
            return this;
        }
    }
}
