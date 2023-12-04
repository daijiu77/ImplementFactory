using System;
using System.Collections.Generic;
using System.Text;
using Test.NetCore.Entities;

namespace Test.NetCore.DTO
{
    public class UserInfo_Insert
    {
        public virtual string name { get; set; }

        public virtual int age { get; set; }

        public virtual string address { get; set; }

        public virtual int userType { get; set; }
    }
}
