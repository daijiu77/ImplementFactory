using System.DJ.ImplementFactory.MServiceRoute.Attrs;

namespace Test.Framework.MSVisitor
{
    [MicroServiceRoute("WebSiteService", "Home")]
    public interface IHomeController
    {
        object Test();
    }
}
