using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Drawing.Printing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public class MySqlAnalysis : AbsSqlAnalysis, ISqlAnalysis
    {
        public MySqlAnalysis()
        {
            dataType = db_dialect.mysql;
            leftTag = '`';
            rightTag = '`';
        }

        string ISqlAnalysis.PageSizeSignOfSql { get; set; }
        string ISqlAnalysis.StartQuantitySignOfSql { get; set; }

        string ISqlAnalysis.GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValueOfBaseValue)
        {
            return GetConditionOfBaseValue(fieldName, relation, fieldValueOfBaseValue);
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
            ISqlAnalysis sqlAnalysis = this;
            return GetPageChange(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, pageSize1, pageNumber1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber)
                {
                    string sql = "select {0} from {1}{2}{3}{4} limit {5}, {6};";
                    sqlAnalysis.StartQuantitySignOfSql = (pageSize * (pageNumber - 1)).ToString();
                    sqlAnalysis.PageSizeSignOfSql = pageSize.ToString();
                    sql = sql.ExtFormat(selectPart, fromPart, wherePart, groupPart, orderByPart,
                        sqlAnalysis.StartQuantitySignOfSql, sqlAnalysis.PageSizeSignOfSql);
                    return sql;
                });
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int length1)
        {
            ISqlAnalysis sqlAnalysis = this;
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, length1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int length)
                {
                    string sql = "select {1} from {2}{3}{4}{5} limit {0};";
                    sqlAnalysis.StartQuantitySignOfSql = "0";
                    sqlAnalysis.PageSizeSignOfSql = length.ToString();
                    sql = sql.ExtFormat(length.ToString(), selectPart, fromPart, wherePart, groupPart, orderByPart);
                    return sql;
                });            
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int startNumber1, int length1)
        {
            ISqlAnalysis sqlAnalysis = this;
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, startNumber1, length1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length)
                {
                    string sql = "select row_number() over({4}) rowNum,{0} from {1}{2}{3}{4} limit {5}, {6};";
                    sqlAnalysis.StartQuantitySignOfSql = startNumber.ToString();
                    sqlAnalysis.PageSizeSignOfSql = length.ToString();
                    sql = sql.ExtFormat(selectPart, fromPart, wherePart, groupPart, orderByPart, startNumber.ToString(), length.ToString());
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
        }

        string ISqlAnalysis.GetInnerJoin(string tableName, string alias, string wherePart)
        {
            return GetJoin("Inner join", tableName, alias, wherePart);
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
            if (null == primaryKeys) return sql;
            if (0 == primaryKeys.Count) return sql;
            sql = sql.Trim();
            if (";" == sql.Substring(sql.Length - 1)) sql = sql.Substring(0, sql.Length - 1);
            string dbTag = DJTools.GetParaTagByDbDialect(Commons.DbAdapter.dbDialect);
            string s = "";
            string fn = "";
            ISqlAnalysis sqlAnalysis = this;
            foreach (var item in primaryKeys)
            {
                fn = sqlAnalysis.GetLegalName(item);
                s += "," + dbTag + fn + " " + sqlAnalysis.GetFieldName(item);
            }
            if (!string.IsNullOrEmpty(s)) s = s.Substring(1);
            sql += "; select " + s + ";";
            return sql;
        }

        bool ISqlAnalysis.IsLegalCaseDefaultValueWhenInsert(string tableName1, object fieldValue1, PropertyInfo propertyInfo1, FieldMapping fieldMapping1, ref object defaultValue1)
        {            
            return IsLegalCaseDefaultValueWhenInsert(tableName1, fieldValue1, propertyInfo1, fieldMapping1, ref defaultValue1,
                delegate (string primaryKeyName, string tableName)
                {
                    string sql = "select {0} from {1} order by {0} desc limit 1;";
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
