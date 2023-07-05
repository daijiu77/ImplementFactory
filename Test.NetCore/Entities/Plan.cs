using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.NetCore.Entities
{
    public class Plan : BaseModel
    {
        public virtual string PName { get; set; }
        public virtual string Detail { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        public virtual int num { get; set; }

        public virtual Plan plan { get; set; }
        public virtual List<Plan> plans { get; set; }

        public virtual Guid UserInfoId { get; set; }

        [Constraint(foreignKey: "UserInfoId", refrenceKey: "id")]
        public virtual UserInfo userInfoData { get; set; }

        [Constraint(foreignKey: "id", refrenceKey: "PlanId")]
        public virtual List<EquipmentInfo> equipmentInfos { get; set; }
    }
}
