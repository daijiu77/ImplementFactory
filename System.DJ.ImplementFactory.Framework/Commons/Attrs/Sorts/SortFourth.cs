using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortFourth : SortAttribute
    {
        public SortFourth() : base(4) { }

        public SortFourth(OrderByRule orderByRule) : base(4, orderByRule) { }
    }
}
