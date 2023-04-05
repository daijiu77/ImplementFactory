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
        bool SaveIPAddr(string IPAddr, string contractKey);
        bool SetEnabledTime(DateTime startTime, DateTime endTime, string contractKey);
    }
}
