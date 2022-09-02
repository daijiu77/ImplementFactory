using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace Test.Framework.Entities
{
    public abstract class BaseModel : AbsDataModel
    {
        [FieldMapping("Id", typeof(Guid), 1, "newid()", true, true)]
        public Guid Id { get; set; }

        [FieldMapping("IsEnabled", typeof(bool), 0, "1")]
        public bool IsEnabled { get; set; }

        [FieldMapping("CreateDate", typeof(DateTime), 0, "getdate()")]
        public DateTime CreateDate { get; set; }
    }
}
