using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.Attrs.Sorts;
using System.DJ.ImplementFactory.DataAccess;

namespace Test.NetCore.Entities
{
    public enum UserType
    {
        None,
        Normal,
        VIP,
        TOP
    }

    public class ExtFieldMapping : FieldMapping
    {
        public ExtFieldMapping(string fieldName,
            Type fieldType,
            int length,
            string defaultVal,
            bool isPrimaryKey,
            bool NoNull) : base(fieldName, fieldType, length, defaultVal, isPrimaryKey, NoNull) { }
    }

    public class UserInfo : AbsDataModel
    {
        [ExtFieldMapping("id", typeof(Guid), 1, "", true, true)]
        public virtual Guid id { get; set; }

        public virtual Guid parentId { get; set; }

        [Constraint(foreignKey: "id", refrenceKey: "parentId")]
        public virtual IList<UserInfo> children { get; set; }

        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public virtual string name { get; set; }

        [Sort1]
        [Condition("=", Condition.WhereIgrons.igroneZero)]
        public virtual int age { get; set; }

        //Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull 
        //��ͬ�� Condition.WhereIgrons.igroneEmptyNull
        [Condition("like", Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull)]
        public virtual string address { get; set; }

        [Condition]
        public virtual bool IsDeleted { get; set; }

        [Condition]
        public virtual bool IsEnabled { get; set; } = true;

        public virtual UserType userType { get; set; }

        [Sort2]
        public virtual DateTime cdatetime { get; set; }

        [Constraint(foreignKey: "id", refrenceKey: "UserInfoId")]
        public virtual IList<Plan> Plans { get; set; }
    }
}
