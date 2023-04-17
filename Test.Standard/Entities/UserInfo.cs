using System;
using System.DJ.ImplementFactory.Commons.Attrs;

namespace Test.Standard.Entities
{
    public class UserInfo
    {
        public Guid id { get; set; }

        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public string name { get; set; }

        [Condition("=", Condition.WhereIgrons.igroneZero)]
        public int age { get; set; }

        //Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull 
        //µÈÍ¬ÓÚ Condition.WhereIgrons.igroneEmptyNull
        [Condition("like", Condition.WhereIgrons.igroneEmpty | Condition.WhereIgrons.igroneNull)]
        public string address { get; set; }

        public DateTime cdatetime { get; set; }
    }
}
