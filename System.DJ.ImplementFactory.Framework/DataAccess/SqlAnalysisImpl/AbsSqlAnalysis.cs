using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public abstract class AbsSqlAnalysis
    {
        protected Regex specialRg = new Regex(@"(^[0-9]$)|(^[1-9][0-9]*[0-9]$)|(^[\-\+][0-9]$)|(^[\-\+][1-9][0-9]*[0-9]$)|(^[0-9]\.[0-9]*[0-9]$)|(^[1-9][0-9]+\.[0-9]*[0-9]$)|(^[\-\+][0-9]\.[0-9]*[0-9]$)|(^[\-\+][1-9][0-9]+\.[0-9]*[0-9]$)|(^true$)|(^false$)|(^null$)", RegexOptions.IgnoreCase);

        protected bool IsAliasField(string chars, ref string alias, ref string field)
        {
            bool mbool = false;
            if (string.IsNullOrEmpty(chars)) return mbool;
            string s = chars.Trim();
            Regex rg = new Regex(@"^[^a-z0-9_].+[^a-z0-9_]$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s)) return mbool;
            rg = new Regex(@"^(?<alias>[a-z0-9_]+)\.((?<field>[a-z0-9_]+)|([\[\`""](?<field>[a-z0-9_]+)[\]\`""]))$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s))
            {
                mbool = true;
                Match m = rg.Match(s);
                alias = m.Groups["alias"].Value;
                field = m.Groups["field"].Value;
            }
            return mbool;
        }

        protected string LegalName(string name)
        {
            string dbn = name;
            const int size = 60;
            if (size < name.Length)
            {
                string s = "";
                Regex rg = new Regex(@"[A-Z0-9]+");
                if (rg.IsMatch(name))
                {
                    MatchCollection mc = rg.Matches(name);
                    foreach (Match item in mc)
                    {
                        s += item.Groups[0].Value;
                    }
                }
                int n = size - s.Length - 1;
                int len = name.Length;
                dbn = s + "_" + name.Substring(len - n);
            }
            return dbn;
        }

        protected string GetWhere(string wherePart, bool IgnoreWhereChar)
        {
            wherePart = wherePart.Trim();
            if (string.IsNullOrEmpty(wherePart)) return wherePart;

            Regex rg = new Regex(@"^where\s+(?<ConditionStr>.+)", RegexOptions.IgnoreCase);
            if (IgnoreWhereChar)
            {
                if (rg.IsMatch(wherePart))
                {
                    wherePart = rg.Match(wherePart).Groups["ConditionStr"].Value;
                }
            }
            else
            {
                if (!rg.IsMatch(wherePart))
                {
                    int n = 0;
                    const int maxNum = 100;
                    Regex rg1 = new Regex(@"^((and)|(or))\s+(?<where_str>.+)");
                    while (rg1.IsMatch(wherePart) && (n < maxNum))
                    {
                        wherePart = rg1.Match(wherePart).Groups["where_str"].Value.Trim();
                        if (string.IsNullOrEmpty(wherePart)) break;
                        n++;
                    }
                    if (!string.IsNullOrEmpty(wherePart)) wherePart = "where " + wherePart;
                }
            }
            if (!string.IsNullOrEmpty(wherePart)) wherePart = " " + wherePart;
            return wherePart;
        }

        protected string GetWhere(string wherePart)
        {
            return GetWhere(wherePart, false);
        }

    }
}
