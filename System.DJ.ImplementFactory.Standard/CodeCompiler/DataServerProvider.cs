using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.DotNetCore.CodeCompiler
{
    public class DataServerProvider : IDataServerProvider
    {
        DataAdapter IDataServerProvider.CreateDataAdapter(DbCommand dbCommand)
        {
            return new SqlDataAdapter((SqlCommand)dbCommand);
        }

        DbCommand IDataServerProvider.CreateDbCommand(string sql, DbConnection connection)
        {
            return new SqlCommand(sql, (SqlConnection)connection);
        }

        DbCommand IDataServerProvider.CreateDbCommand()
        {
            return new SqlCommand();
        }

        DbConnection IDataServerProvider.CreateDbConnection(string connectString)
        {
            return new SqlConnection(connectString);
        }

        DbParameter IDataServerProvider.CreateDbParameter(string fieldName, object fieldValue)
        {
            return new SqlParameter(fieldName, fieldValue);
        }
    }
}
