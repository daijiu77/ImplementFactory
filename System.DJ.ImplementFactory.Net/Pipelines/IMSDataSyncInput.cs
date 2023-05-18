using System.DJ.ImplementFactory.Entities;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSDataSyncInput : IMSDataSyncBase
    {
        bool Insert(string dataSyncsName, DataSyncItem item);
        bool Update(string dataSyncsName, DataSyncItem item);
        bool Delete(string dataSyncsName, DataSyncItem item);
    }
}
