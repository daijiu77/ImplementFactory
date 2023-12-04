using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class Sort1 : SortFirst
    {
        /// <summary>
        /// Set the order of class properties participating in sorting in the first place
        /// </summary>
        public Sort1() : base() { }

        /// <summary>
        /// Set the order of the class properties participating in the sorting in the first place, and set the collation
        /// </summary>
        /// <param name="orderByRule">Set the collation</param>
        public Sort1(OrderByRule orderByRule) : base(orderByRule) { }
    }
}
