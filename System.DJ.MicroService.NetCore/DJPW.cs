using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.MicroService.NetCore
{
    public class DJPW
    {
        public static string GetPW(int numberOf4Char)
        {
            char[] arr = new char[] { 'a', 'b', 'c', 'd' };
            int nlen = arr.Length;
            if (nlen > numberOf4Char.ToString().Length) return numberOf4Char.ToString();
            string pw = "";
            char[] charArr = numberOf4Char.ToString().ToCharArray();
            Dictionary<char, int> dic = new Dictionary<char, int>();

            for (int i = 0; i < nlen; i++)
            {
                dic.Add(arr[i], i);
            }
            char[] guidArr = null;
            int[] indexArr = new int[4];
            int num = 0;
            int n = 0;
            int x1 = 0;
            //随机抽取排序
            while (0 < dic.Count && 3 > x1)
            {
                guidArr = Guid.NewGuid().ToString().ToLower().ToCharArray();
                foreach (char c in guidArr)
                {
                    if (dic.ContainsKey(c))
                    {
                        num = dic[c];
                        dic.Remove(c);
                        indexArr[n] = num;
                        n++;
                    }
                    if (0 == dic.Count) break;
                }
                x1++;
            }

            //如果字典里存在未随机的数字,侧按序获取
            if (0 < dic.Count)
            {
                foreach (KeyValuePair<char, int> item in dic)
                {
                    indexArr[n] = item.Value;
                    n++;
                }
            }

            n = 0;
            num = 0;
            string s = "";
            int n1 = 0;
            foreach (int item in indexArr)
            {
                n++;
                s = charArr[item].ToString();
                n1 = Convert.ToInt32(s);
                //序号+1 + 当前序号对应的数字按位取反
                pw += (item + 1).ToString() + (~n1);
                if (n % 2 == 0)
                {
                    //当前取值顺序为偶数时: 当前数字相加
                    num += n1;
                }
                else
                {
                    //当前取值顺序为奇数时: 当前数字相减
                    num -= n1;
                }
            }
            if (0 > num) num = 0 - num;
            pw = pw.Replace("-", "");

            //序号+1 + 数字按位取反, 最后加上运算后的值
            pw += num.ToString("D2");

            num = 0;
            double db = 0;
            for (int i = 0; i < nlen; i++)
            {
                s = charArr[i].ToString();
                n1 = Convert.ToInt32(s);
                n = i + 1;
                db = ((n1 + n) % 10) * Math.Pow(10, n);
                num += Convert.ToInt32(db);
            }

            string txt = num.ToString();
            if (nlen > txt.Length)
            {
                int ii = 0;
                int x = 0;
                num = 1;
                while (nlen > txt.Length)
                {
                    if ((ii + num) < nlen)
                    {
                        n = ii + num;
                    }
                    else
                    {
                        ii = 0;
                        num++;
                        n = ii + num;
                    }
                    s = charArr[ii].ToString();
                    n1 = Convert.ToInt32(s);
                    s = charArr[n].ToString();
                    x = Convert.ToInt32(s);
                    txt += (x | n1).ToString();
                    ii++;
                }
            }
            txt = txt.Substring(0, 4);
            pw += txt;
            return pw;
        }
    }
}
