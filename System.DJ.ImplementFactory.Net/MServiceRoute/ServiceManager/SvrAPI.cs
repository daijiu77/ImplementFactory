using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPI
    {        
        public string ServiceName { get; set; }

        public int index { get; set; }

        private List<SvrAPIOption> _items = new List<SvrAPIOption>();
        public List<SvrAPIOption> Items { get { return _items; } }
    }
}
