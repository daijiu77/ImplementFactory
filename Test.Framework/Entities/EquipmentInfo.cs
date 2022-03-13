using System;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;

namespace Test.Framework.Entities
{
    [Condition]
    public class EquipmentInfo
    {
        public Guid id { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string equipmentName { get; set; }
        public string code { get; set; }
        public DateTime cdatetime { get; set; }
    }
}
