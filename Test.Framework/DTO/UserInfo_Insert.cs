using System;
using System.Collections.Generic;
using System.Text;
using Test.Framework.Entities;

namespace Test.Framework.DTO
{
    public class UserInfo_Insert
    {
        public virtual string name { get; set; }

        public virtual int age { get; set; }

        public virtual string address { get; set; }

        public virtual int userType { get; set; }
    }
}
