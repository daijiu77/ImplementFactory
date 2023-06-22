using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Threading.Tasks;

namespace Test.NetCore.MSVisitor
{
    [MicroServiceRoute("route1", "/UserInfo")]
    public interface IMSUserInfo
    {
        [RequestMapping("/GetUserName?name={name}", MethodTypes.Post)]
        Task<string> UserName(string name);
    }

}
