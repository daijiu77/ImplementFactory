using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public class OracleSqlAnalysis : AbsSqlAnalysis, ISqlAnalysis
    {
        public OracleSqlAnalysis()
        {
            dataType = db_dialect.oracle;
            leftTag = '"';
            rightTag = '"';
        }

        Dictionary<string, string> ISqlAnalysis.AliasDic { get; set; }
        string ISqlAnalysis.PageSizeSignOfSql { get; set; }
        string ISqlAnalysis.StartQuantitySignOfSql { get; set; }

        char ISqlAnalysis.GetLeftTag { get { return leftTag; } }

        char ISqlAnalysis.GetRightTag { get { return rightTag; } }

        string ISqlAnalysis.GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValueOfBaseValue)
        {
            return GetConditionOfBaseValue(fieldName, relation, fieldValueOfBaseValue, ((ISqlAnalysis)this).AliasDic);
        }

        string ISqlAnalysis.GetConditionOfCollection(string fieldName, ConditionRelation relation, ICollection fieldValueOfCollection)
        {
            return GetConditionOfCollection(fieldName, relation, fieldValueOfCollection);
        }

        string ISqlAnalysis.GetConditionOfDbSqlBody(string fieldName, ConditionRelation relation, string fieldValueOfSql)
        {
            return GetConditionOfDbSqlBody(fieldName, relation, fieldValueOfSql);
        }

        string ISqlAnalysis.GetGroupBy(string groupByFields)
        {
            return GetGroupBy(groupByFields);
        }

        string ISqlAnalysis.GetOrderBy(string orderByItems)
        {
            return GetOrderBy(orderByItems);
        }

        string ISqlAnalysis.GetOrderByItem(string fieldName, OrderByRule orderByRule)
        {
            return GetOrderByItem(fieldName, orderByRule);
        }

        string ISqlAnalysis.GetPageChange(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int pageSize1, int pageNumber1)
        {
            return GetPageChange(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, pageSize1, pageNumber1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber)
                {
                    Random rnd = new Random();
                    rnd.Next(1, 99);
                    string tb = "tb" + DateTime.Now.ToString("HHmmss") + "_" + rnd.Next(1, 99);
                    string sql = "select {0} from (select ROWNUM row_num,{1} from {2}{3}{4}{5}) {6} where {6}.row_num>{7} and {6}.row_num<={8}";
                    ISqlAnalysis sqlAnalysis = this;
                    sqlAnalysis.PageSizeSignOfSql = "{0}.row_num<=".ExtFormat(tb);
                    sqlAnalysis.StartQuantitySignOfSql = "{0}.row_num>".ExtFormat(tb);
                    string newSelectPart = GetNewSelectPart(selectPart, null);
                    sql = sql.ExtFormat(newSelectPart, selectPart, fromPart, wherePart, groupPart, orderByPart, tb, (pageSize * (pageNumber - 1)).ToString(), (pageSize * pageNumber).ToString());
                    return sql;
                });
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int top1)
        {
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, top1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top)
                {
                    Random rnd = new Random();
                    rnd.Next(1, 99);
                    string tb = "tb" + DateTime.Now.ToString("HHmmss") + "_" + rnd.Next(1, 99);
                    string sql = "select * from (select ROWNUM row_num,{1} from {2}{3}{4}{5}) {6} where {6}.row_num<={0} and {6}.row_num>0;";
                    ISqlAnalysis sqlAnalysis = this;
                    sqlAnalysis.PageSizeSignOfSql = "{0}.row_num<=".ExtFormat(tb);
                    sqlAnalysis.StartQuantitySignOfSql = "{0}.row_num>".ExtFormat(tb);
                    sql = sql.ExtFormat(top.ToString(), selectPart, fromPart, wherePart, groupPart, orderByPart, tb);
                    return sql;
                });
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int startNumber1, int length1)
        {
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, startNumber1, length1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length)
                {
                    Random rnd = new Random();
                    rnd.Next(1, 99);
                    string tb = "tb" + DateTime.Now.ToString("HHmmss") + "_" + rnd.Next(1, 99);
                    int end = startNumber + length;
                    string sql = "select * from (select ROWNUM row_num,{0} from {1}{2}{3}{4}) {5} where {5}.row_num>={6} and {5}.row_num<{7};";
                    ISqlAnalysis sqlAnalysis = this;
                    sqlAnalysis.PageSizeSignOfSql = "{0}.row_num<".ExtFormat(tb);
                    sqlAnalysis.StartQuantitySignOfSql = "{0}.row_num>=".ExtFormat(tb);
                    sql = sql.ExtFormat(selectPart, fromPart, wherePart, groupPart, orderByPart, tb, startNumber.ToString(), end.ToString());
                    return sql;
                });
        }

        string ISqlAnalysis.GetCount(string fromPart, string wherePart, string groupPart)
        {
            return GetCount(fromPart, wherePart, groupPart);
        }

        string ISqlAnalysis.GetLeftJoin(string tableName, string alias, string wherePart)
        {
            return GetJoin("Left join", tableName, alias, wherePart);
        }

        string ISqlAnalysis.GetRightJoin(string tableName, string alias, string wherePart)
        {
            return GetJoin("Right join", tableName, alias, wherePart);
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetInnerJoin(string tableName, string alias, string wherePart)
        {
            return GetJoin("Inner join", tableName, alias, wherePart);
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetTableAilas(string tableName, string alias)
        {
            return GetTableAilas(tableName, alias);
        }

        string ISqlAnalysis.GetFieldAlias(string fieldName, string alias)
        {
            return GetFieldAlias(fieldName, alias);
        }

        string ISqlAnalysis.GetTableName(string tableName)
        {
            return GetTableName(tableName);
        }

        string ISqlAnalysis.GetFieldName(string alias, string fieldName)
        {
            return GetFieldName(alias, fieldName);
        }

        string ISqlAnalysis.GetFieldName(string fieldName)
        {
            return ((ISqlAnalysis)this).GetFieldName(null, fieldName);
        }

        string ISqlAnalysis.GetPrimaryKeyValueScheme(string sql, List<string> primaryKeys)
        {
            return sql;
        }

        bool ISqlAnalysis.IsLegalCaseDefaultValueWhenInsert(string tableName1, object fieldValue1, PropertyInfo propertyInfo1, FieldMapping fieldMapping1, ref object defaultValue1)
        {
            return IsLegalCaseDefaultValueWhenInsert(tableName1, fieldValue1, propertyInfo1, fieldMapping1, ref defaultValue1,
                delegate (string primaryKeyName, string tableName)
                {
                    string sql = "select {0} from {1} where rownum=1 order by {0} desc;";
                    sql = sql.ExtFormat(primaryKeyName, tableName);
                    return sql;
                });
        }

        string ISqlAnalysis.GetLegalName(string name)
        {
            return LegalName(name);
        }

    }
}
