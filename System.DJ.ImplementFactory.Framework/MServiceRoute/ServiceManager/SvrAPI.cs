using System.Collections.Generic;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class SvrAPI
    {
        public string ServiceName { get; set; }

        public int index { get; set; }

        public SvrAPIOption GetSvrAPIOption()
        {
            SvrAPIOption option = null;
            if (0 == _items.Count) return option;
            option = _items[index];
            index++;
            index = index % _items.Count;
            return option;
        }

        private List<SvrAPIOption> _items = new List<SvrAPIOption>();
        public List<SvrAPIOption> Items { get { return _items; } }
    }
}
