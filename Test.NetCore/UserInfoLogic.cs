using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
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
            return userInfo.query<UserInfo>();
        }

        public DataEntity<DataElement> GetDynamicFieldData()
        {
            return userInfo.dynamicQuery();
        }

        public List<UserInfo> userInfos(string name)
        {
            return userInfo.query<UserInfo>(name, 0).Result;
        }

        public List<UserInfo> userInfos(UserInfo userInfo1)
        {
            return userInfo.query(userInfo1);
        }
    }
}
