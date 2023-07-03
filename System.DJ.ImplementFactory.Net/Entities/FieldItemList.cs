using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;

namespace System.DJ.ImplementFactory.Entities
{
    public class FieldItem
    {
        public static FieldItem Me
        {
            get
            {
                return new FieldItem();
            }
        }

        public static FieldItem Instance
        {
            get
            {
                return new FieldItem();
            }
        }

        public void Set(string fieldName, string fieldAlias)
        {
            NameOrSelectFrom = fieldName;
            Alias = fieldAlias;
        }

        public void Set(DbSqlBody dbSqlBody, string alias)
        {
            NameOrSelectFrom = dbSqlBody;
            Alias = alias;
        }

        public object NameOrSelectFrom { get; set; }
        public string Alias { get; set; }
    }

    public class FieldItemList<T> : List<T> where T : FieldItem
    {
        private Dictionary<string, FieldItem> dic = new Dictionary<string, FieldItem>();
        private Dictionary<DbSqlBody, FieldItem> bodyDic = new Dictionary<DbSqlBody, FieldItem>();

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
            if (null == item.NameOrSelectFrom) return;

            if (null == (item.NameOrSelectFrom as DbSqlBody))
            {
                string key = item.NameOrSelectFrom.ToString().Trim();
                if (string.IsNullOrEmpty(key)) return;

                string kLower = key.ToLower();
                if (dic.ContainsKey(kLower)) return;

                base.Add((T)item);
                dic[kLower] = item;
            }
            else
            {
                bodyDic.Add(((DbSqlBody)item.NameOrSelectFrom), item);
            }
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
