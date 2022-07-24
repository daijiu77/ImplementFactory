using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.DataAccess.TableScheme
{
    public class OracleTableScheme : IDbTableScheme
    {
        string IDbTableScheme.GetAddFieldScheme(string tableName, FieldMapping fieldMapping)
        {
            throw new NotImplementedException();
        }

        List<string> IDbTableScheme.GetFields(string tableName)
        {
            throw new NotImplementedException();
        }

        string IDbTableScheme.GetTableScheme(string tableName, List<FieldMapping> fieldMappings)
        {
            throw new NotImplementedException();
        }
    }
}
