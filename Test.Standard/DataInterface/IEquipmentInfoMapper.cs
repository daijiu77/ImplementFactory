using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using Test.Standard.Entities;

namespace Test.Standard.DataInterface
{
    public interface IEquipmentInfoMapper
    {
        [AutoSelect("select * from EquipmentInfo where {equipmentInfo} order by cdatetime desc")]
        List<EquipmentInfo> query(EquipmentInfo equipmentInfo);

        [AutoSelect("select * from EquipmentInfo where id=@id")]
        EquipmentInfo query(string id);

        /// <summary>
        /// The return value is a dynamic data collection type
        /// </summary>
        /// <returns></returns>
        [AutoSelect("select * from EquipmentInfo order by cdatetime desc")]
        DataEntity<DataElement> query1();

        [EquipInsert("insert into EquipmentInfo values {equipmentInfo}")]
        int insert(EquipmentInfo equipmentInfo);

        /// <summary>
        /// Parameter is dynamic data collection type.
        /// </summary>
        /// <param name="dataElements"></param>
        /// <returns></returns>
        [EquipInsert("insert into EquipmentInfo values {equipmentInfo}")]
        int insert(DataEntity<DataElement> dataElements);

        /// <summary>
        /// 数据实体属性加 IgnoreField 属性，所以可以省略如下参数: 
        /// fields: new string[] { "id", "cdatetime" }, fieldsType:FieldsType.Exclude
        /// </summary>
        /// <param name="equipmentInfo"></param>
        /// <returns></returns>
        [AutoUpdate(updateExpression: "update EquipmentInfo set {equipmentInfo} where id=@id", EnabledBuffer: true)]
        int update(EquipmentInfo equipmentInfo);
    }

    class EquipInsert: AutoInsert
    {
        public EquipInsert(string sqlExpression): base(sqlExpression, new string[] { "id", "cdatetime" },
            fieldsType: FieldsType.Exclude, EnabledBuffer: true) { }
    }
}
