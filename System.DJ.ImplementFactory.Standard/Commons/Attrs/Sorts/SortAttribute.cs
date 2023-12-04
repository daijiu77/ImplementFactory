using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortAttribute : Attribute, IComparable<SortAttribute>
    {
        private int index = 0;
        private OrderByRule orderByRule = OrderByRule.Asc;

        /// <summary>
        /// Specifies that a class attribute participates in sorting, which defaults to asc sorting
        /// </summary>
        public SortAttribute() { }

        /// <summary>
        /// Specify a class property to participate in sorting, you can set the order of the participating sorting fields, or you can set the collation of each sort field
        /// </summary>
        /// <param name="index">set the order of the participating sorting fields</param>
        /// <param name="orderByRule">set the collation(Asc\Desc) of each sort field</param>
        public SortAttribute(int index, OrderByRule orderByRule)
        {
            this.index = index;
            this.orderByRule = orderByRule;
        }

        /// <summary>
        /// Specify a class property to participate in sorting, you can set the collation of each sort field
        /// </summary>
        /// <param name="orderByRule">set the collation(Asc\Desc) of each sort field</param>
        public SortAttribute(OrderByRule orderByRule)
        {
            this.orderByRule = orderByRule;
        }

        /// <summary>
        /// Specify a class property to participate in sorting, you can set the order of the participating sorting fields
        /// </summary>
        /// <param name="index">set the order of the participating sorting fields</param>
        public SortAttribute(int index)
        {
            this.index = index;
        }

        public OrderByRule GetOrderByRule
        {
            get { return orderByRule; }
        }

        public string FieldName { get; set; }

        int IComparable<SortAttribute>.CompareTo(SortAttribute other)
        {
            int v = 0;
            if (index < other.index)
            {
                v = -1;
            }
            else if (index > other.index)
            {
                v = 1;
            }
            return v;
        }
    }
}
