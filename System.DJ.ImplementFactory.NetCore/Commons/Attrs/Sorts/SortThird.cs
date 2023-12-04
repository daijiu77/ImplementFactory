using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortThird : SortAttribute
    {
        public SortThird() : base(3) { }

        public SortThird(OrderByRule orderByRule) : base(3, orderByRule) { }
    }
}
