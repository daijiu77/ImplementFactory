using System.DJ.ImplementFactory.Pipelines;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public class MixedCalculate : IMixedCalculate
    {
        Regex bracketRg = null;
        /// <summary>
        /// 乘 - 除 - % 运算
        /// </summary>
        Regex calcLevel1 = null;
        /// <summary>
        /// 加 - 减 运算
        /// </summary>
        Regex calcLevel2 = null;

        const int max = 100;

        public MixedCalculate()
        {
            bracketRg = new Regex(@"\((?<CalculateExp>[^\(\)]+)\)", RegexOptions.IgnoreCase);
            calcLevel1 = new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>[\/\*\%])\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase);
            calcLevel2 = new Regex(@"(?<Num1>[0-9\.]+)\s*(?<CalcChar>[\+\-])\s*(?<Num2>[0-9\.]+)", RegexOptions.IgnoreCase);
        }

        T IMixedCalculate.Exec<T>(string expression)
        {
            T val = default(T);
            if (string.IsNullOrEmpty(expression)) return val;
            BracketCalculate(ref expression);
            Level_Calculate(calcLevel1, ref expression);
            Level_Calculate(calcLevel2, ref expression);
            string s = expression.Trim();
            object vObj = ConvertTo<T>(s);
            if (null != vObj) val = (T)vObj;
            return val;
        }

        object ConvertTo<T>(string vs)
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

        void Level_Calculate(Regex rg, ref string expression)
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

                n1 = Convert.ToSingle(Num1);
                n2 = Convert.ToSingle(Num2);
                num = calculat(n1, n2, CalcChar);
                s = s.Replace(m.Groups[0].Value, num.ToString());
                n++;
            }
            expression = s;
        }

        void BracketCalculate(ref string expression)
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
                Level_Calculate(calcLevel1, ref CalculateExp);
                Level_Calculate(calcLevel2, ref CalculateExp);
                s = s.Replace(m.Groups[0].Value, CalculateExp);
                n++;
            }
            expression = s;
        }

        float calculat(float num1, float num2, string CalculateChar)
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
                    v1 = num1 / num2;
                    break;
                case "%":
                    v1 = num1 % num2;
                    break;
            }
            return v1;
        }
    }
}
