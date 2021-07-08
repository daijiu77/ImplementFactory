using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Framework
{
    public class CalculateImpl : ICalculate
    {
        int ICalculate.Sum(int a, int b)
        {
            return a + b;
        }
    }
}
