using System.DJ.ImplementFactory.Entities;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSDataSync
    {
        bool Insert(DataSyncItem item);
        bool Update(DataSyncItem item);
        bool Delete(DataSyncItem item);
    }
}
