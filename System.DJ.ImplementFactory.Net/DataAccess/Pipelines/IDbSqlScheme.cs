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
        IList<T> ToList<T>(object srcObj);
        IList<object> ToList(Type modelType);
        IList<object> ToList(Type modelType, object srcObj);

        T DefaultFirst<T>();
        T DefaultFirst<T>(object srcObj);
        object DefaultFirst(Type modelType);
        AbsDataModel parentModel { get; set; }
        int Update();
        int AppendUpdate(Dictionary<string, object> keyValue);
        int Insert();
        int AppendInsert(Dictionary<string, object> keyValue);
        int Delete();
        int RecordCount { get; }
        int PageCount { get; }
        DbSqlBody dbSqlBody { get; }
        string error { get; }
    }
}
