using System;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.Pipelines
{
    public interface IDbSqlScheme
    {
        int Count();
        DataTable ToDataTable();
        IList<T> ToList<T>();
        IList<object> ToList(Type modelType);
        T DefaultFirst<T>();
        object DefaultFirst(Type modelType);
        AbsDataModel parentModel { get; set; }
        int Update();
        int AppendUpdate(Dictionary<string, object> keyValue);
        int Insert();
        int AppendInsert(Dictionary<string, object> keyValue);
        int Delete();
        DbSqlBody dbSqlBody { get; }
        string error { get; }
    }
}
