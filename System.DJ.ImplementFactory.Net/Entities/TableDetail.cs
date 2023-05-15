using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace System.DJ.ImplementFactory.Entities
{
    public class TableDetail
    {
        private Dictionary<string, TableFieldInfo> dic = new Dictionary<string, TableFieldInfo>();
        private List<string> list = new List<string>();
        private List<string> fields = new List<string>();

        public TableFieldInfo this[string fieldName]
        {
            get
            {
                TableFieldInfo field = null;
                string key = fieldName.Trim().ToLower();
                dic.TryGetValue(key, out field);
                return field;
            }
        }

        public TableFieldInfo this[int index]
        {
            get
            {
                TableFieldInfo field = null;
                if (index < list.Count)
                {
                    string key = list[index];
                    field = dic[key];
                }
                return field;
            }
        }

        /// <summary>
        /// Table Name
        /// </summary>
        public string Name { get; private set; }

        public int Count { get { return list.Count; } }

        public TableDetail SetName(string name)
        {
            this.Name = name;
            return this;
        }

        public TableDetail Add(string fieldName, string fieldType, int valueLength)
        {
            string key = fieldName.ToLower();
            if (!dic.ContainsKey(key))
            {
                TableFieldInfo tableFieldInfo = new TableFieldInfo();
                tableFieldInfo.SetTableName(Name)
                    .SetName(fieldName)
                    .SetValueType(fieldType)
                    .SetLength(valueLength);
                dic[key] = tableFieldInfo;
                list.Add(key);
                fields.Add(fieldName);
            }
            return this;
        }

        public void Foreach(Func<string, string, int, bool> func)
        {
            if (null == func) return;
            Foreach(fInfo =>
            {
                return func(fInfo.Name, fInfo.ValueType, fInfo.Length);
            });
        }

        public void Foreach(Action<string, string, int> action)
        {
            if (null == action) return;
            Foreach(fInfo =>
            {
                action(fInfo.Name, fInfo.ValueType, fInfo.Length);
                return true;
            });
        }

        public void Foreach(Func<TableFieldInfo, bool> func)
        {
            if (null == func) return;
            foreach (var item in dic)
            {
                if (!func(item.Value)) break;
            }
        }

        public void Foreach(Action<TableFieldInfo> action)
        {
            if (null == action) return;
            Foreach(fInfo =>
            {
                action(fInfo);
                return true;
            });
        }

        public string[] FieldNames
        {
            get { return fields.ToArray(); }
        }
    }
}
