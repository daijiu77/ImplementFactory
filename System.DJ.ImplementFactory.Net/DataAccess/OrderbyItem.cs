using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public enum OrderByRule
    {
        Asc,
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
