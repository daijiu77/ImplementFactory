using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.Text;
using Test.Framework.Entities;

namespace Test.Framework
{
    [MicroServiceRoute("route1")]
    public interface IApiUserInfo
    {
        UserInfo GetUserInfo(string id);
    }
}
