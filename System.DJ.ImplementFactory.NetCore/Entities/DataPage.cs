using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataPage
    {
        public int PageSize { get; set; }
        public int PageNumber {  get; set; }
        public string PageSizeSignOfSql { get; set; }
        public string PageNumberSignOfSql { get; set; }

        public string PageSizeDbParameterName { get; set; }
        public string PageNumberDbParameterName { get; set; }
        
        private List<PageOrderBy> _orderby = new List<PageOrderBy>();
        public List<PageOrderBy> OrderBy { get {  return _orderby; } }

        public class PageOrderBy
        {
            public string FieldName { get; set; }
            public bool IsDesc { get; set; }
        }
    }
}
