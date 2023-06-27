using System;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;

namespace Test.NetCore.Entities
{
    [Condition]
    public class EquipmentInfo: AbsDataModel
    {
        [IgnoreField(IgnoreField.IgnoreType.Insert|IgnoreField.IgnoreType.Update)]
        public virtual Guid id { get; set; }
        public virtual int height { get; set; }
        public virtual int width { get; set; }
        public virtual string equipmentName { get; set; }
        public virtual string code { get; set; }

        [IgnoreField(IgnoreField.IgnoreType.Insert | IgnoreField.IgnoreType.Update)]
        public DateTime cdatetime { get; set; }

        public virtual Guid PlanId { get; set; }

        [Constraint(foreignKey: "PlanId", refrenceKey: "id")]
        public virtual Plan PlanData { get; set; }
    }
}
