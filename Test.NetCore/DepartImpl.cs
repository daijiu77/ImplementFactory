using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Test.NetCore
{
    public class DepartImpl : IDepart
    {
        void IDepart.SetDepartName(string departName)
        {
            Trace.WriteLine(departName);
        }
    }
}
