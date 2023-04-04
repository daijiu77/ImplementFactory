using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace Test.Framework.MSVisitor
{
    [MicroServiceRoute("route1", "/UserInfo")]
    public interface IMSUserInfo
    {
        [RequestMapping("/GetUserName?name={name}", MethodTypes.Post)]
        Task<string> UserName(string name);
    }

}
