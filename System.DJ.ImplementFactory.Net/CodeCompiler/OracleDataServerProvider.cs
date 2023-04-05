using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.Net.CodeCompiler
{
    public class OracleDataServerProvider : IDataServerProvider
    {
        DataAdapter IDataServerProvider.CreateDataAdapter(DbCommand dbCommand)
        {
            return new OracleDataAdapter((OracleCommand)dbCommand);
            //throw new NotImplementedException();
        }

        DbCommand IDataServerProvider.CreateDbCommand(string sql, DbConnection connection)
        {
            return new OracleCommand(sql, (OracleConnection)connection);
            //throw new NotImplementedException();
        }

        DbCommand IDataServerProvider.CreateDbCommand()
        {
            return new OracleCommand();
            //throw new NotImplementedException();
        }

        DbConnection IDataServerProvider.CreateDbConnection(string connectString)
        {
            return new OracleConnection(connectString);
            //throw new NotImplementedException();
        }

        DbParameter IDataServerProvider.CreateDbParameter(string fieldName, object fieldValue)
        {
            return new OracleParameter(fieldName, fieldValue);
            //throw new NotImplementedException();
        }
    }
}
