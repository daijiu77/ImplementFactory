using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.Text;
using Test.Framework.Entities;

namespace Test.Framework
{
    [MicroServiceRoute("route1", "UserInfo")]
    public interface IApiUserInfo
    {
        UserInfo GetUserInfo(string id);
    }
}
