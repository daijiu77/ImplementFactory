using System.DJ.ImplementFactory.Pipelines;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public class MixedCalculate : IMixedCalculate
    {
        private Regex bracketRg = null;
        /// <summary>
        /// 按运算符的优先级顺序来做匹配(数组元素的先后顺序决定运算符的优先级)
        /// </summary>
        private Regex[] regexes = null;

        private const int max = 100;

        private string err = "";

        string IMixedCalculate.Err {  get { return err; } }

        public MixedCalculate()
        {
            bracketRg = new Regex(@"\((?<CalculateExp>[^\(\)]+)\)", RegexOptions.IgnoreCase);
            regexes = new Regex[]
            {
                new Regex(@"(?<CalcChar>\+\+)(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), // ++number 运算
                new Regex(@"(?<Num1>[0-9\.]+)(?<CalcChar>\+\+)", RegexOptions.IgnoreCase), //number++ 运算
                new Regex(@"(?<CalcChar>\-\-)(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), // --number 运算
                new Regex(@"(?<Num1>[0-9\.]+)(?<CalcChar>\-\-)", RegexOptions.IgnoreCase), //number-- 运行
                new Regex(@"(?<CalcChar>[\~])(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), //按位取反运算符
                new Regex(@"(?<Num1>[\-\+]?[0-9\.]+)\s*(?<CalcChar>[\/\*\%])\s*(?<Num2>[\-\+]?[0-9\.]+)", RegexOptions.IgnoreCase), //除|乘|余
                new Regex(@"(?<Num1>[\-\+]?[0-9\.]+)\s*(?<CalcChar>[\+\-])\s*(?<Num2>[\-\+]?[0-9\.]+)", RegexOptions.IgnoreCase), //加|减
                new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>((\<\<)|(\>\>)))\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), //左位移|右位移
                new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>[\&])\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), //按位与
                new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>[\^])\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase), //按位异或
                new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>[\|])\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase)  //按位或
            };
        }

        T IMixedCalculate.Exec<T>(string expression)
        {
            err = "";
            T val = default(T);
            if (string.IsNullOrEmpty(expression)) return val;
            BracketCalculate(ref expression);
            foreach (Regex item in regexes)
            {
                Level_Calculate(item, ref expression);
            }
            string s = expression.Trim();
            object vObj = ConvertTo<T>(s);
            if (null != vObj) val = (T)vObj;
            return val;
        }

        private object ConvertTo<T>(string vs)
        {
            Type type = typeof(T);
            object vObj = null;
            if (typeof(int) == type)
            {
                int n = 0;
                int.TryParse(vs, out n);
                vObj = n;
            }
            else if (typeof(Int64) == type)
            {
                Int64 fNum = 0;
                Int64.TryParse(vs, out fNum);
                vObj = fNum;
            }
            else if (typeof(long) == type)
            {
                long fNum = 0;
                long.TryParse(vs, out fNum);
                vObj = fNum;
            }
            else if (typeof(Int16) == type)
            {
                Int16 fNum = 0;
                Int16.TryParse(vs, out fNum);
                vObj = fNum;
            }
            else if (typeof(float) == type)
            {
                float fNum = 0;
                float.TryParse(vs, out fNum);
                vObj = fNum;
            }
            else if (typeof(double) == type)
            {
                double dbNum = 0;
                double.TryParse(vs, out dbNum);
                vObj = dbNum;
            }
            else if (typeof(decimal) == type)
            {
                decimal dbNum = 0;
                decimal.TryParse(vs, out dbNum);
                vObj = dbNum;
            }
            return vObj;
        }

        private void Level_Calculate(Regex rg, ref string expression)
        {
            string s = expression.Trim();
            if (string.IsNullOrEmpty(s)) return;
            int n = 0;
            string Num1 = "";
            string Num2 = "";
            string CalcChar = "";
            float n1 = 0;
            float n2 = 0;
            float num = 0;
            Match m = null;
            while (rg.IsMatch(s) && max > n)
            {
                m = rg.Match(s);
                Num1 = m.Groups["Num1"].Value;
                Num2 = m.Groups["Num2"].Value;
                CalcChar = m.Groups["CalcChar"].Value;

                n1 = float.MinValue;
                n2 = float.MinValue;
                if (!string.IsNullOrEmpty(Num1)) n1 = Convert.ToSingle(Num1);
                if (!string.IsNullOrEmpty(Num2)) n2 = Convert.ToSingle(Num2);
                num = calculat(n1, n2, CalcChar);
                s = s.Replace(m.Groups[0].Value, num.ToString());
                n++;
            }
            expression = s;
        }

        private void BracketCalculate(ref string expression)
        {
            string s = expression.Trim();
            if (string.IsNullOrEmpty(s)) return;
            string CalculateExp = "";
            int n = 0;
            Match m = null;
            while (bracketRg.IsMatch(s) && max > n)
            {
                m = bracketRg.Match(s);
                CalculateExp = m.Groups["CalculateExp"].Value;
                foreach (var item in regexes)
                {
                    Level_Calculate(item, ref CalculateExp);
                }
                s = s.Replace(m.Groups[0].Value, CalculateExp);
                n++;
            }
            expression = s;
        }

        private float calculat(float num1, float num2, string CalculateChar)
        {
            float v1 = 0;
            if (string.IsNullOrEmpty(CalculateChar)) return v1;

            CalculateChar = CalculateChar.Trim();
            switch (CalculateChar)
            {
                case "+":
                    v1 = num1 + num2;
                    break;
                case "-":
                    v1 = num1 - num2;
                    break;
                case "*":
                    v1 = num1 * num2;
                    break;
                case "/":
                    if (0 == num2)
                    {
                        err = "The number can not be zero in the expression '{0}/{1}'".ExtFormat(num1, num2);
                        return v1;
                    }
                    v1 = num1 / num2;
                    break;
                case "%":
                    v1 = num1 % num2;
                    break;
                case "|":
                    v1 = Convert.ToInt32(num1) | Convert.ToInt32(num2);
                    break;
                case "&":
                    v1 = Convert.ToInt32(num1) & Convert.ToInt32(num2);
                    break;
                case "^":
                    v1 = Convert.ToInt32(num1) ^ Convert.ToInt32(num2);
                    break;
                case "<<":
                    v1 = Convert.ToInt32(num1) << Convert.ToInt32(num2);
                    break;
                case ">>":
                    v1 = Convert.ToInt32(num1) >> Convert.ToInt32(num2);
                    break;
                case "~":
                    v1 = ~Convert.ToInt32(num2);
                    break;
                case "++":
                    if (float.MinValue != num1)
                    {
                        num1++;
                        v1 = num1;
                    }
                    else if (float.MinValue != num2)
                    {
                        v1 = ++num2;
                    }
                    break;
                case "--":
                    if (float.MinValue != num1)
                    {
                        num1--;
                        v1 = num1;
                    }
                    else if (float.MinValue != num2)
                    {
                        v1 = --num2;
                    }
                    break;
            }
            return v1;
        }
    }
}
