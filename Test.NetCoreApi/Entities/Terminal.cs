using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace Test.NetCoreApi.Entities
{
    public class Terminal : AbsDataModel
    {
        [FieldMapping("id", typeof(Guid), 0, "newid()", true, true)]
        public Guid id { get; set; }

        public string tname { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public int posX { get; set; }

        [FieldMapping("cdatetime", typeof(DateTime), 0, "getdate()", false)]
        public DateTime cdatetime { get; set; }
    }
}
