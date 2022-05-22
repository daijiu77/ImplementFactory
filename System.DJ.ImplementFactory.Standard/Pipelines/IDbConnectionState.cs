using System.Data;
using System.Data.Common;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDbConnectionState
    {
        void DbConnection_Created(DbConnection sender);
        void DbConnection_CreatedFail(Exception ex);
        void DbConnection_StateChange(DbConnection sender, StateChangeEventArgs e);
        void DbConnection_Disposed(DbConnection sender, EventArgs e);
    }
}
