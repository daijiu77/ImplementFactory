using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;
using Test.NetCore.Entities;

namespace Test.NetCore
{
    [MicroServiceRoute("route1")]
    public interface IApiUserInfo
    {
        UserInfo GetUserInfo(string id);
    }
}
