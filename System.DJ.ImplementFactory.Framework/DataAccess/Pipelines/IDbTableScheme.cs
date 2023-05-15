using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;

namespace System.DJ.ImplementFactory.DataAccess.Pipelines
{
    public class FieldInformation
    {
        public string Name { get; set; }
        public string ValueType { get; set; }
        public int Length { get; set; }
        public bool IsNull { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
    public interface IDbTableScheme
    {
        string GetTableScheme(string tableName, List<FieldMapping> fieldMappings);
        string GetAddFieldScheme(string tableName, FieldMapping fieldMapping);
        List<FieldInformation> GetFields(string tableName);
        ISqlAnalysis sqlAnalysis { get; }
    }
}
