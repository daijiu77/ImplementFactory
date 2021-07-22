using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.Framework
{
    public class CalculateImpl : ICalculate
    {
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
