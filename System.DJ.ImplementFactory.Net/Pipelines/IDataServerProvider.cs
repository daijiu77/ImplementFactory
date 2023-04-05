using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDataServerProvider
    {
        DbConnection CreateDbConnection(string connectString);

        DbParameter CreateDbParameter(string fieldName, object fieldValue);

        DbCommand CreateDbCommand(string sql, DbConnection connection);

        DbCommand CreateDbCommand();

        DataAdapter CreateDataAdapter(DbCommand dbCommand);
    }
}
