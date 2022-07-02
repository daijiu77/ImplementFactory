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
        T DefaultFirst<T>();
        int Update();
        int Insert();
        int Delete();
        DbSqlBody dbSqlBody { get; }
        string error { get; }
    }
}
