using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.Text;
using System.Threading.Tasks;
using Test.NetCore.Entities;

namespace Test.NetCore
{
    [MicroServiceRoute("route1", "UserInfo")]
    public interface IApiUserInfo
    {
        Task<UserInfo> GetUserInfo(string id);
    }
}
