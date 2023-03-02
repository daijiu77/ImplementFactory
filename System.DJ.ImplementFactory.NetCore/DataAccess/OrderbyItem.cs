using System;
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
}
