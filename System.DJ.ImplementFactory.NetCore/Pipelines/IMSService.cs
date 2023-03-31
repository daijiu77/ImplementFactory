using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSService
    {
        List<string> IPAddrSources();
        void SaveIPAddr(string IPAddr);
    }
}
