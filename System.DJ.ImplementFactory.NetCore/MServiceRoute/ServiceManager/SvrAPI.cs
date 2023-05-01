using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPI
    {
        private List<SvrUri> svrUris = new List<SvrUri>();
        public string ContractKey { get; set; }
        public string ServiceName { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public List<SvrUri> SvrUris { get { return svrUris; } }
    }
}
