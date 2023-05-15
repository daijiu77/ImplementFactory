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
        public DataTypes DataType { get; set; }
        public object Data { get; set; }
    }
}
