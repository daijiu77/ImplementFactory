using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Entities
{
    public class FieldItem
    {
        public string Name { get; set; }
        public string Alias { get; set; }
    }

    public class FieldItemList<T> : List<T> where T : FieldItem
    {
        private Dictionary<string, FieldItem> dic = new Dictionary<string, FieldItem>();

        public FieldItem this[string fieldName]
        {
            get
            {
                FieldItem item = null;
                if (string.IsNullOrEmpty(fieldName)) return item;
                string fLower = fieldName.Trim().ToLower();
                dic.TryGetValue(fLower, out item);
                return item;
            }
            set
            {
                if (string.IsNullOrEmpty(fieldName)) return;
                string kLower = fieldName.Trim().ToLower();
                if (string.IsNullOrEmpty(kLower)) return;
                if (dic.ContainsKey(kLower))
                {
                    T fieldItem = (T)dic[kLower];
                    base.Remove(fieldItem);
                    dic.Remove(kLower);
                }
                base.Add((T)value);
                dic[kLower] = value;
            }
        }

        public new void Add(FieldItem item)
        {
            if (null == item) return;
            if (string.IsNullOrEmpty(item.Name)) return;

            string key = item.Name.Trim();
            if (string.IsNullOrEmpty(key)) return;

            string kLower = key.ToLower();
            if (dic.ContainsKey(kLower)) return;

            base.Add((T)item);
            dic[kLower] = item;
        }

        public new int Count
        {
            get
            {
                return dic.Count;
            }
        }

        public new bool Contains(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return false;
            string fName = fieldName.Trim().ToLower();
            return dic.ContainsKey(fName);
        }
    }
}
