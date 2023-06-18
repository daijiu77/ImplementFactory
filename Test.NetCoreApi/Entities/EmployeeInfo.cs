using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace Test.NetCoreApi.Entities
{
    public class EmployeeInfo : AbsDataModel
    {
        public virtual int id { get; set; }
        public virtual string name { get; set; }
        public virtual string address { get; set; }
        public virtual string telphone { get; set; }
        [Constraint(foreignKey:"id", refrenceKey: "EmployeeInfoId")]
        public virtual List<WorkInfo> WorkInfos { get; set; }
    }
}
