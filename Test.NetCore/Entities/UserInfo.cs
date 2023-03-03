using System;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using Test.NetCore.DataInterface;

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
        public Guid id { get; set; }

        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public string name { get; set; }

        [Condition("=", Condition.WhereIgrons.igroneZero)]
        public int age { get; set; }

        //Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull 
        //µÈÍ¬ÓÚ Condition.WhereIgrons.igroneEmptyNull
        [Condition("like", Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull)]
        public string address { get; set; }

        public UserType userType { get; set; }

        public DateTime cdatetime { get; set; }
    }
}
