using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    internal class SpaceManage
    {
        private SpaceManage Child = null;
        private SpaceManage Parent = null;

        private int MyNumber = 0;
        private string MySpace = "";

        public SpaceManage InitLine(ref string line)
        {
            string s = line.Trim();
            string sp = DJTools.UnitSpace;
            int sLen = sp.Length;
            SpaceManage sm = this;
            if (!string.IsNullOrEmpty(s))
            {
                if (s.Equals("{") || s.Substring(s.Length - 1) == "{")
                {
                    if (null == Child)
                    {
                        Child = new SpaceManage();
                        Child.MyNumber = MyNumber + 1;
                        Child.MySpace = MySpace + sp;
                        Child.Parent = this;
                    }
                    sm = Child;
                    line = MySpace + line.TrimStart();
                }
                else if (s.Substring(0, 1).Equals("}"))
                {
                    if (null != Parent)
                    {
                        sm = Parent;
                    }
                    line = sm.MySpace + line.TrimStart();
                }
                else
                {
                    if (0 == MyNumber)
                    {
                        string s1 = s.Substring(0, 1);
                        int n = line.IndexOf(s1);
                        s = line.Substring(0, n);
                        n = s.Length;
                        MyNumber = n / sLen;
                        MySpace = s;
                    }
                    s = MySpace + line.TrimStart();
                    line = s;
                }
            }
            return sm;
        }
    }
}
