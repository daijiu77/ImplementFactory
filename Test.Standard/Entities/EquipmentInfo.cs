using System;
using System.DJ.ImplementFactory.NetCore.Commons.Attrs;

namespace Test.Standard.Entities
{
    [Condition]
    public class EquipmentInfo
    {
        [IgnoreField(IgnoreField.IgnoreType.Insert|IgnoreField.IgnoreType.Update)]
        public Guid id { get; set; }

        public int height { get; set; }
        public int width { get; set; }
        public string equipmentName { get; set; }
        public string code { get; set; }

        [IgnoreField(IgnoreField.IgnoreType.Insert | IgnoreField.IgnoreType.Update)]
        public DateTime cdatetime { get; set; }
    }
}
