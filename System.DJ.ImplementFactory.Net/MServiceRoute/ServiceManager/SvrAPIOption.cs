using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPIOption
    {
        public string ContractKey { get; set; }

        public string IP { get; set; }
        public string Port { get; set; }

        private List<SvrUri> svrUris = new List<SvrUri>();
        public List<SvrUri> SvrUris { get { return svrUris; } }
    }
}
