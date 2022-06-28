using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines
{
    public interface ISqlAnalysis
    {
        string GetCondition(string fieldName, ConditionRelation relation, object fieldValue);
        string GetOrderByItem(string fieldName, OrderByRule orderByRule);
        string GetOrderBy(string orderByItems);
        string GetGroupBy(string groupByFields);
        string GetPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber);
        string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top);
        string GetCount(string fromPart, string wherePart, string groupPart);
        string GetUpdate(string updatePart, string tableName, string wherePart);
        string GetDelete(string tableName, string wherePart);
        string GetInsert(string tableName, string selectPart, string valuesPart);
    }
}
