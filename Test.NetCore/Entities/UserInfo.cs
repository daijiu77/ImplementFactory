using System;
using System.Collections.Generic;
using System.Text;

namespace Test.NetCore.Entities
{
    public class UserInfo
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public string address { get; set; }
        public DateTime cdatetime { get; set; }
    }
}
