using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;
using Test.NetCore.DataInterface;
using Test.NetCore.Entities;

namespace Test.NetCore
{
    class UserInfoLogic: ImplementAdapter
    {
        [MyAutoCall]
        IUserInfo userInfo;

        public UserInfo GetLastUserInfo()
        {
            return userInfo.query();
        }

        public DataEntity<DataElement> GetDynamicFieldData()
        {
            return userInfo.dynamicQuery();
        }
    }
}
