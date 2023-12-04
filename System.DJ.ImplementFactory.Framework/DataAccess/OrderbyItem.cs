using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess
{
    /// <summary>
    /// Collation
    /// </summary>
    public enum OrderByRule
    {
        /// <summary>
        /// Positive ordering, for example: 1, 2, 3, 4
        /// </summary>
        Asc,
        /// <summary>
        /// Reverse order, for example: 4, 3, 2, 1
        /// </summary>
        Desc
    }

    public class OrderbyItem
    {
        public static OrderbyItem Me
        {
            get { return new OrderbyItem(); }
        }

        public static OrderbyItem Instance
        {
            get { return new OrderbyItem(); }
        }

        private OrderbyItem() { }

        public OrderbyItem(string fieldName, OrderByRule rule)
        {
            FieldName = fieldName;
            Rule = rule;
        }

        public OrderbyItem Set(string fieldName, OrderByRule rule)
        {
            FieldName = fieldName;
            Rule = rule;
            return this;
        }

        public string FieldName { get; set; }
        public OrderByRule Rule { get; set; }
    }

    public class OrderbyList<T> : IEnumerable<T> where T : OrderbyItem
    {
        private Dictionary<string, T> dic = new Dictionary<string, T>();
        private List<string> list = new List<string>();

        private ItemList itemList = null;

        public OrderbyList()
        {
            itemList = new ItemList(this);
        }

        public void Add(T orderbyItem)
        {
            if (null == orderbyItem) return;
            string fieldName = orderbyItem.FieldName;
            if (string.IsNullOrEmpty(fieldName)) return;
            string fn = fieldName.Trim();
            if (string.IsNullOrEmpty(fn)) return;
            fn = fn.ToLower();
            if (dic.ContainsKey(fn))
            {
                dic.Remove(fn);
                list.Remove(fn);
            }
            else
            {
                const string fname = "FName";
                Regex rg = new Regex(@"[a-z0-9_]+\.(?<FName>[a-z0-9_]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                string FName = "";
                if (rg.IsMatch(fn))
                {
                    FName = rg.Match(fn).Groups[fname].Value;
                    if (dic.ContainsKey(FName))
                    {
                        dic.Remove(FName);
                        list.Remove(FName);
                    }
                }
                else
                {
                    string key = "";
                    string s = "";
                    foreach (var item in dic)
                    {
                        s = item.Key;
                        if (rg.IsMatch(s))
                        {
                            FName = rg.Match(s).Groups[fname].Value;
                            if (FName.Equals(fn))
                            {
                                key = s;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(key))
                    {
                        dic.Remove(key);
                        list.Remove(key);
                    }
                }
            }
            dic.Add(fn, orderbyItem);
            list.Add(fn);
        }

        public int Count
        {
            get { return list.Count; }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return itemList;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return itemList;
        }

        class ItemList : IEnumerator, IEnumerator<T>
        {
            private OrderbyList<T> orderbyList = null;
            private int index = 0;
            private T item = default(T);

            public ItemList(OrderbyList<T> orderbyList)
            {
                this.orderbyList = orderbyList;
            }

            object IEnumerator.Current => item;

            T IEnumerator<T>.Current => item;

            void IDisposable.Dispose()
            {
                index = 0;
            }

            bool IEnumerator.MoveNext()
            {
                if (index >= orderbyList.Count) return false;
                string fn = orderbyList.list[index];
                item = orderbyList.dic[fn];
                index++;
                return true;
            }

            void IEnumerator.Reset()
            {
                index = 0;
            }
        }
    }
}
