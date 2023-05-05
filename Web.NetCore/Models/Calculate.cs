using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.NetCore.Models
{
    public class Calculate: ICalculate
    {
        private string _str = "";
        public virtual string GetStr()
        {
            return _str;
        }

        string ICalculate.append(string value)
        {
            if(string.IsNullOrEmpty(_str))
            {
                _str = value;
            }
            else
            {
                _str += "\r\n" + value;
            }
            return _str;
        }

        string[] ICalculate.Lines()
        {
            string txt = _str;
            List<string> lines = new List<string>();
            if (string.IsNullOrEmpty(txt)) return lines.ToArray();
            string s = txt;
            string tg = "\r\n";
            int tN = tg.Length;
            string line1 = "";
            int n = s.IndexOf(tg);
            while (-1 != n)
            {
                line1 = s.Substring(0, n);
                lines.Add(line1);
                s = s.Substring(n + tN);
                n = s.IndexOf(tg);
            }
            lines.Add(s);
            return lines.ToArray();
        }
    }
}