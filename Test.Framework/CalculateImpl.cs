using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.Framework
{
    public class CalculateImpl : ICalculate
    {
        private event getData GetData;

        event getData ICalculate.GetData
        {
            add
            {
                GetData += value;
            }
            remove
            {
                GetData -= value;
            }
        }

        private string pi = "";
        string ICalculate.PropertyTest
        {
            get { return pi; }
            set { pi = value; }
        }

        int ICalculate.Division(int a, int b, AutoCall autoCall)
        {
            int c = 0;
            try
            {
                c = a / b;
            }
            catch (Exception ex)
            {
                autoCall.e(ex.ToString(), System.DJ.ImplementFactory.Commons.ErrorLevels.dangerous);
                //throw;
            }
            return c;
        }

        int ICalculate.Sum(int a, int b)
        {
            return a + b;
        }
    }
}
