using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using Test.Framework.DataInterface;
using Test.Framework.Entities;

namespace Test.Framework
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
            return userInfo.query<UserInfo>(name, 0, 5, 1).Result;
        }

        public List<UserInfo> userInfos(UserInfo userInfo1)
        {
            return userInfo.query(userInfo1);
        }
    }
}
