using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.Pipelines
{
    public interface ISqlAnalysis
    {
        Dictionary<string, string> AliasDic { get; set; }
        string PageSizeSignOfSql { get; set; }
        string StartQuantitySignOfSql { get; set; }
        string GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValueOfBaseValue);
        string GetConditionOfCollection(string fieldName, ConditionRelation relation, ICollection fieldValueOfCollection);
        string GetConditionOfDbSqlBody(string fieldName, ConditionRelation relation, string fieldValueOfSql);
        string GetOrderByItem(string fieldName, OrderByRule orderByRule);
        string GetOrderBy(string orderByItems);
        string GetGroupBy(string groupByFields);
        string GetPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber);
        string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int length);
        string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length);
        string GetCount(string fromPart, string wherePart, string groupPart);
        
        string GetLeftJoin(string tableName, string alias, string wherePart);
        string GetRightJoin(string tableName, string alias, string wherePart);
        string GetInnerJoin(string tableName, string alias, string wherePart);

        string GetTableAilas(string tableName, string alias);
        string GetFieldAlias(string fieldName, string alias);
        string GetLegalName(string name);
        string GetTableName(string tableName);
        string GetFieldName(string fieldName);
        string GetFieldName(string alias, string fieldName);
        string GetPrimaryKeyValueScheme(string sql, List<string> primaryKeys);
        bool IsLegalCaseDefaultValueWhenInsert(string tableName, object fieldValue, PropertyInfo propertyInfo, FieldMapping fieldMapping, ref object defaultValue);
    }
}
