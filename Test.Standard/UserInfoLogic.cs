using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using Test.Standard.DataInterface;
using Test.Standard.Entities;

namespace Test.Standard
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
            return userInfo.query(new UserInfo(), name);
        }

        public List<UserInfo> userInfos(UserInfo userInfo1)
        {
            return userInfo.query(userInfo1);
        }
    }
}
