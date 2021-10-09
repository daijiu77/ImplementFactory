using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Text;
using Test.NetCore.Entities;

namespace Test.NetCore.DataInterface
{
    public interface IUserInfo
    {
        [AutoSelect("select * from UserInfo where name like '%{name}%'")]
        List<T> query<T>(T data, string name);

        [AutoSelect("select top 1 * from UserInfo order by cdatetime desc")]
        T query<T>();

        /// <summary>
        /// 当表字段不确定的情况，采用动态数据集合获取数据
        /// </summary>
        /// <returns></returns>
        [AutoSelect("select top 1 * from UserInfo order by cdatetime desc")]
        DataEntity<DataElement> dynamicQuery();

        [AutoInsert("insert into UserInfo {userInfo}",
            fields: new string[] { "id", "cdatetime" },
            fieldsType: FieldsType.Exclude,
            EnabledBuffer: true)]
        int insert(UserInfo userInfo);

        [AutoInsert("insert into UserInfo {userInfo}",
            fields: new string[] { "id", "cdatetime" },
            fieldsType: FieldsType.Exclude,
            EnabledBuffer: true)]
        int insert(List<UserInfo> userInfos);

        [AutoUpdate("update UserInfo set {userInfo} where id=@id",
            fields: new string[] { "id", "cdatetime" },
            fieldsType: FieldsType.Exclude)]
        int update(UserInfo userInfo);

        [AutoDelete("delete from UserInfo where id=@id")]
        int delete(Guid id);

    }
}
