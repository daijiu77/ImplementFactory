using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.Entities
{
    public class RouteAttr
    {
        public string Name { get; set; }
        public string Uri { get; set; } = "";
        public string RegisterAddr { get; set; }
        public string TestAddr { get; set; }
        public string ContractKey { get; set; } = "";

        private MethodTypes _method = MethodTypes.Get;
        public MethodTypes RegisterActionType
        {
            get { return _method; }
            set { _method = value; }
        }
    }
}
