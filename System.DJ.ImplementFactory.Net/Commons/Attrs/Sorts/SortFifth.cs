using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortFifth : SortAttribute
    {
        public SortFifth() : base(5) { }
        public SortFifth(OrderByRule orderByRule) : base(5, orderByRule) { }
    }
}
