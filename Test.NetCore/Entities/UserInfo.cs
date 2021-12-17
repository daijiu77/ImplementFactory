using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;
using System.Text;

namespace Test.NetCore.Entities
{
    public class UserInfo
    {
        public Guid id { get; set; }
        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public string name { get; set; }
        [Condition("=", Condition.WhereIgrons.igroneZero)]
        public int age { get; set; }
        [Condition("like", Condition.WhereIgrons.igroneEmptyNull)]
        public string address { get; set; }
        public DateTime cdatetime { get; set; }
    }
}
