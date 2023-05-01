using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Pipelines
{
    public delegate void changeEnabled(DateTime startTime, DateTime endTime, string contractKey);
    public delegate void registerIP(string ipAddr);
    public interface IMSService
    {
        List<string> IPAddrSources();
        bool SaveIPAddr(string IPAddr, string contractKey);
        bool SetEnabledTime(DateTime startTime, DateTime endTime, string contractKey);

        event changeEnabled ChangeEnabled;
        event registerIP RegisterIP;
    }
}
