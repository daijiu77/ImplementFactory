using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Threading.Tasks;

namespace Test.Framework.MSVisitor
{
    [MicroServiceRoute("route1", "/UserInfo")]
    public interface IUserInfo
    {
        [RequestMapping("/GetUserName?name={name}", MethodTypes.Post)]
        Task<string> UserName(string name);
    }

}
