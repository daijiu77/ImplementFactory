using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrUri
    {
        private List<ParameterItem> paremeterItems = new List<ParameterItem>();
        public string Name { get; set; }
        public string Uri { get; set; }
        public string MethodType { get; set; }
        public List<ParameterItem> ParameterItems { get { return paremeterItems; } }
    }
}
