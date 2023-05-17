using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataSyncMessage
    {
        private Dictionary<string, Guid> serviceFlagDic = new Dictionary<string, Guid>();
        public Dictionary<string, Guid> ServiceFlagDic { get { return serviceFlagDic; } }
        public string ResourceKey { get; private set; }
        /// <summary>
        /// Corresponds to the Name property value of the DataSyncs node in the MicroServiceRoute.xml configuration file
        /// </summary>
        public string DataSyncsName { get; private set; }
        public DataSyncItem DataSyncOption { get; set; }

        public DataSyncMessage SetResourceKey(string resourceKey)
        {
            this.ResourceKey = resourceKey;
            return this;
        }

        public DataSyncMessage SetDataSyncsName(string name)
        {
            this.DataSyncsName = name;
            return this;
        }
    }
}
