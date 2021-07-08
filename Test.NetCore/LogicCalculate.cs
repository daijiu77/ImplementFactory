using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.NetCore
{
    class LogicCalculate : ImplementAdapter
    {
        [AutoCall]
        private ICalculate calculate;

        public int testCalculate()
        {
            int n = calculate.Sum(2, 3);
            n = n % 2;
            return n;
        }
    }
}
