using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortSecond : SortAttribute
    {
        public SortSecond() : base(2) { }

        public SortSecond(OrderByRule orderByRule) : base(2, orderByRule) { }
    }
}
