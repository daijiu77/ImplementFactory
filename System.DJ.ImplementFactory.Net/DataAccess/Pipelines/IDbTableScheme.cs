using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.Pipelines
{
    public interface IDbTableScheme
    {
        string GetTableScheme(string tableName, List<FieldMapping> fieldMappings);
        string GetAddFieldScheme(string tableName, FieldMapping fieldMapping);
        List<string> GetFields(string tableName);
        ISqlAnalysis sqlAnalysis { get; }
    }
}
