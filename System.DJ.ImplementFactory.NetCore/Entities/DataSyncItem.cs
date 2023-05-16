namespace System.DJ.ImplementFactory.Entities
{
    public enum DataTypes
    {
        Add = 1 << 0,
        Change = 1 << 1,
        Delete = 1 << 2,
    }

    public class DataSyncItem
    {
        public string DataSyncsName { get; private set; }
        public DataTypes DataType { get; set; }
        public object Data { get; set; }

        public DataSyncItem SetDataSyncsName(string dataSyncsName)
        {
            this.DataSyncsName = dataSyncsName;
            return this;
        }
    }
}
