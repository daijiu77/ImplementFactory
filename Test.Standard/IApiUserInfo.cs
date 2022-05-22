using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;
using Test.Standard.Entities;

namespace Test.Standard
{
    [MicroServiceRoute("route1")]
    public interface IApiUserInfo
    {
        UserInfo GetUserInfo(string id);
    }
}
