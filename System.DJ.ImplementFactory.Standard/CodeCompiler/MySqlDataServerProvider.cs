using MySql.Data.MySqlClient;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.Standard.CodeCompiler
{
    public class MySqlDataServerProvider : IDataServerProvider
    {
        DataAdapter IDataServerProvider.CreateDataAdapter(DbCommand dbCommand)
        {
            return new MySqlDataAdapter((MySqlCommand)dbCommand);
        }

        DbCommand IDataServerProvider.CreateDbCommand(string sql, DbConnection connection)
        {
            return new MySqlCommand(sql, (MySqlConnection)connection);
        }

        DbCommand IDataServerProvider.CreateDbCommand()
        {
            return new MySqlCommand();
        }

        DbConnection IDataServerProvider.CreateDbConnection(string connectString)
        {
            return new MySqlConnection(connectString);
        }

        DbParameter IDataServerProvider.CreateDbParameter(string fieldName, object fieldValue)
        {
            return new MySqlParameter(fieldName, fieldValue);
        }
    }
}
