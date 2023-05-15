using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataSyncMessage
    {
        private Dictionary<string, Guid> serviceFlagDic = new Dictionary<string, Guid>();
        public Dictionary<string, Guid> ServiceFlagDic { get { return serviceFlagDic; } }
        public string Key { get; set; }
        public DataSyncItem DataSyncOption { get; set; }
    }
}
