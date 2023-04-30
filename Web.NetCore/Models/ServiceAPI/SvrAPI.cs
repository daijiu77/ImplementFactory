using System.Collections.Generic;

namespace Web.NetCore.Models.ServiceAPI
{
    public class SvrAPI
    {
        private List<SvrUri> svrUris = new List<SvrUri>();
        public string ContractKey { get; set; }
        public string ServiceName { get; set; }
        public List<SvrUri> SvrUris { get { return svrUris; } }
    }
}
