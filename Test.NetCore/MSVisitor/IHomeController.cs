using System.DJ.ImplementFactory.MServiceRoute.Attrs;

namespace Test.NetCore.MSVisitor
{
    [MicroServiceRoute("WebSiteService", "Home")]
    public interface IHomeController
    {
        object Test();
    }
}
