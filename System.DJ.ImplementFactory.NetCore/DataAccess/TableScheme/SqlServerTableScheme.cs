using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.DataAccess.TableScheme
{
    public class SqlServerTableScheme : IDbTableScheme
    {
        string IDbTableScheme.GetAddFieldScheme(string tableName, FieldMapping fieldMapping)
        {
            string sql = "alter table {0} add {1}";
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
