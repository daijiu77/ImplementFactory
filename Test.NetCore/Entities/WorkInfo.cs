using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace Test.NetCore.Entities
{
    public class WorkInfo : AbsDataModel
    {
        public virtual int id { get; set; }
        public virtual int EmployeeInfoID { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string CompanyNameEn { get; set; }
        [Constraint(foreignKey: "EmployeeInfoID", refrenceKey: "id", "EmployeeInfoID", "id")]
        public virtual EmployeeInfo employeeInfo { get; set; }
    }
}
