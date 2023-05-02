using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.Entities
{
    public class MServiceManager : RouteAttr
    {
        public string ServiceManagerAddr { get; set; }

        private MethodTypes _method1 = MethodTypes.Get;
        public MethodTypes ServiceManagerActionType
        {
            get { return _method1; }
            set { _method1 = value; }
        }
    }
}
