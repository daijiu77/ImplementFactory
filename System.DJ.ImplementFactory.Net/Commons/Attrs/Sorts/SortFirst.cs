using System.DJ.ImplementFactory.DataAccess;

namespace System.DJ.ImplementFactory.Commons.Attrs.Sorts
{
    public class SortFirst : SortAttribute
    {
        public SortFirst() : base(1) { }

        public SortFirst(OrderByRule orderByRule) : base(1, orderByRule) { }
    }
}
