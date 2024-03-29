﻿using System.Data.Common;
using System.Data.SqlClient;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.Framework.CodeCompiler
{
    public class MSDataServerProvider : IDataServerProvider
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
