using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines
{
    public interface ISqlAnalysis
    {
        string GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValue);
        string GetConditionOfCollection(string fieldName, ConditionRelation relation, ICollection fieldValue);
        string GetConditionOfDbBody(string fieldName, ConditionRelation relation, DbBody fieldValue);
        string GetOrderByItem(string fieldName, OrderByRule orderByRule);
        string GetOrderBy(string orderByItems);
        string GetGroupBy(string groupByFields);
        string GetPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber);
        string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top);
        string GetCount(string fromPart, string wherePart, string groupPart);
        
        string GetLeftJoin(string tableName, string alias, string wherePart);
        string GetRightJoin(string tableName, string alias, string wherePart);
        string GetInnerJoin(string tableName, string alias, string wherePart);
    }
}
