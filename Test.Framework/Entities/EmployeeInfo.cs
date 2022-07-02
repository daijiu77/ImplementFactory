using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace Test.Framework.Entities
{
    public class EmployeeInfo : AbsDataModel
    {
        public virtual int id { get; set; }
        public virtual string name { get; set; }
        public virtual string address { get; set; }
        public virtual string telphone { get; set; }
    }
}
