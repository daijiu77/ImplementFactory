using System;
using System.DJ.ImplementFactory.DataAccess;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using Test.Framework.DataInterface;

namespace Test.Framework.Entities
{
    public enum UserType
    {
        None,
        Normal,
        VIP,
        TOP
    }

    public class UserInfo : AbsDataModel
    {
        public Guid id { get; set; }

        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public string name { get; set; }

        [Condition("=", Condition.WhereIgrons.igroneZero)]
        public int age { get; set; }

        //Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull 
        //��ͬ�� Condition.WhereIgrons.igroneEmptyNull
        [Condition("like", Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull)]
        public string address { get; set; }

        public UserType userType { get; set; }

        public DateTime cdatetime { get; set; }
    }
}
