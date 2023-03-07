using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public abstract class AbsSqlAnalysis
    {
        protected Regex specialRg= new Regex(@"(^[0-9]$)|(^[1-9][0-9]*[0-9]$)|(^[\-\+][0-9]$)|(^[\-\+][1-9][0-9]*[0-9]$)|(^[0-9]\.[0-9]*[0-9]$)|(^[1-9][0-9]+\.[0-9]*[0-9]$)|(^[\-\+][0-9]\.[0-9]*[0-9]$)|(^[\-\+][1-9][0-9]+\.[0-9]*[0-9]$)|(^true$)|(^false$)|(^null$)", RegexOptions.IgnoreCase);
        
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
    }
}
