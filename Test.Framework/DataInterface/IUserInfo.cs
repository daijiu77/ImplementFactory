using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Text;
using Test.Framework.Entities;

namespace Test.Framework.DataInterface
{
    public interface IUserInfo
    {
        /// <summary>
        /// {T} 泛型实体类名作为表名,
        /// 如果实体类加有 [Table("")] 属性，侧采用该属性值替换 {T} 标识
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [AutoSelect("select * from {T} where name like '%{name}%'")]
        List<T> query<T>(T data, string name);

        [AutoSelect("select top 1 * from {T} order by cdatetime desc")]
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
