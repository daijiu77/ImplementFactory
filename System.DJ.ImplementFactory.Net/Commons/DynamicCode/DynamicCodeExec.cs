using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    public class DynamicCodeExec
    {
        private static IMixedCalculate mixedCalculate = new MixedCalculate();

        private static string[] calculate = new string[] 
        {
            "$calculate(",
            "$cal(",
            "$c(",
            "$execute(",
            "$exec(",
            "$e("
        };

        private static List<UnitEle> getUnit(string sql, string[] signs)
        {
            if (string.IsNullOrEmpty(sql)) return null;
            if (null == signs) return null;
            if (0 == signs.Length) return null;

            string s = "", s0 = "", s1 = "";
            foreach (var item in signs)
            {
                s1 = item.Trim();
                if (s1.Substring(0, 1).Equals("$")) s1 = s1.Substring(1);
                if (s1.Substring(s1.Length - 1).Equals("(")) s1 = s1.Substring(0, s1.Length - 1);
                s += @"|(\$(?<signStr>" + s1 + @")\((?<eleStr>[^\(\)]+)\))";
                s0 += @"|(\$" + s1 + @"\()";
            }

            s = s.Substring(1);
            s0 = s0.Substring(1);
            Regex rg0 = new Regex(s0, RegexOptions.IgnoreCase);
            if (!rg0.IsMatch(sql)) return null;
            Regex rg1 = new Regex(s, RegexOptions.IgnoreCase);
            Regex rg2 = new Regex(@"\([^\(\)]+\)", RegexOptions.IgnoreCase);

            string rp = "_cs[n]_";
            string signStr = "";
            string eleStr = "";
            Match m = null;
            List<UnitEle> unitEles = new List<UnitEle>();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            Func<string, string> func = (ele) =>
            {
                Regex rg3 = new Regex(@"_cs[0-9]+_");
                while (rg3.IsMatch(ele))
                {
                    string ele_str = rg3.Match(ele).Groups[0].Value;
                    string es = dic[ele_str];
                    ele = ele.Replace(ele_str, es);
                }
                return ele;
            };

            s1 = "";
            int n = 0;
            bool mbool = rg1.IsMatch(sql) | rg2.IsMatch(sql);
            while (mbool)
            {
                if (rg1.IsMatch(sql))
                {
                    m = rg1.Match(sql);
                    signStr = m.Groups["signStr"].Value;
                    eleStr = m.Groups["eleStr"].Value;
                    eleStr = func(eleStr);
                    s = m.Groups[0].Value;
                    unitEles.Add(new UnitEle()
                    {
                        oldStr = func(s),
                        eleStr = eleStr
                    });
                    sql = sql.Replace(s, "");
                }
                else
                {
                    n++;
                    m = rg2.Match(sql);
                    s = m.Groups[0].Value;
                    s1 = rp.Replace("[n]", n.ToString());
                    dic.Add(s1, s);
                    sql = sql.Replace(s, s1);
                }
                if (!rg0.IsMatch(sql)) break;
                mbool = rg1.IsMatch(sql) | rg2.IsMatch(sql);
            }
            return unitEles;
        }

        public static string Calculate(string sql)
        {
            List<UnitEle> unitEles = getUnit(sql, calculate);
            string s = "";
            if (null != unitEles)
            {
                unitEles.ForEach(item =>
                {
                    float f = mixedCalculate.Exec<float>(item.eleStr);
                    if (Convert.ToInt32(f) == f)
                    {
                        s = Convert.ToInt32(f).ToString();
                    }
                    else
                    {
                        s = f.ToString();
                    }
                    sql = sql.Replace(item.oldStr, s);
                });
            }
            return sql;
        }

        class UnitEle
        {
            public string oldStr { get; set; }
            public string eleStr { get; set; }
        }
    }
}
