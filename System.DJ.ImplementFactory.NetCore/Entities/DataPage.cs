using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataPage
    {
        public int PageSize { get; set; }
        public int StartQuantity {  get; set; }
        public string PageSizeSignOfSql { get; set; }
        public string StartQuantitySignOfSql { get; set; }

        public string PageSizeDbParameterName { get; set; }
        public string StartQuantityDbParameterName { get; set; }
        
        private List<PageOrderBy> _orderby = new List<PageOrderBy>();
        public List<PageOrderBy> OrderBy { get {  return _orderby; } }

        public void InitSql(ref string sql, ref string tableName)
        {
            Regex rg = new Regex(@"\)\s+(?<tbFlag>[a-z0-9_]+)\s+where\s", RegexOptions.IgnoreCase);
            if (rg.IsMatch(sql))
            {
                tableName = rg.Match(sql).Groups["tbFlag"].Value;
                string tb = "";
                rg = new Regex(@"(?<tbFlag>[a-z0-9_]+)\.row[a-z0-9_]*\s*[\<\>\=\!]", RegexOptions.IgnoreCase);
                MatchCollection mc = rg.Matches(sql);
                foreach (Match m in mc)
                {
                    tb = m.Groups["tbFlag"].Value;
                    if (!tableName.Equals(tb))
                    {
                        sql = sql.Replace(tb + ".", tableName + ".");
                    }
                }
            }
        }

        public void InitSql(ref string sql)
        {
            string tableName = "";
            InitSql(ref sql, ref tableName);
        }

        public class PageOrderBy
        {
            public string FieldName { get; set; }
            public bool IsDesc { get; set; }
        }
    }
}
