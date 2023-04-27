using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public class MSqlAnalysis : AbsSqlAnalysis, ISqlAnalysis
    {
        public MSqlAnalysis()
        {
            dataType = db_dialect.sqlserver;
            leftTag = '[';
            rightTag = ']';
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
            return GetPageChange(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, pageSize1, pageNumber1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber)
                {
                    if (string.IsNullOrEmpty(orderByPart)) throw new Exception("A field for sorting is required.");
                    Random rnd = new Random();
                    string tb = "tb_" + DateTime.Now.ToString("HHmmss") + "_" + rnd.Next(1, 99);
                    string sql = "select {0} from (select ROW_NUMBER() OVER({1}) rowNumber,{0} from {2}{3}{4}) {5} where {5}.rowNumber>{6} and {5}.rowNumber<={7}";
                    ISqlAnalysis sqlAnalysis = this;
                    sqlAnalysis.PageSizeSignOfSql = "{0}.rowNumber<=".ExtFormat(tb);
                    sqlAnalysis.StartQuantitySignOfSql = "{0}.rowNumber>".ExtFormat(tb);
                    sql = sql.ExtFormat(selectPart, orderByPart, fromPart, wherePart, groupPart, tb, (pageSize * (pageNumber - 1)).ToString(), (pageSize * pageNumber).ToString());
                    return sql;
                });
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int top1)
        {
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, top1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top)
                {
                    string sql = "select * from (select row_number() over({5}) rowNum, {1} from {2}{3}{4}) tb where tb.rowNum<={0} and tb.rowNum>0";
                    if (!string.IsNullOrEmpty(orderByPart))
                    {
                        sql = sql.ExtFormat(top.ToString(), selectPart, fromPart, wherePart, groupPart, orderByPart);
                        ISqlAnalysis sqlAnalysis = this;
                        sqlAnalysis.PageSizeSignOfSql = "tb.rowNum<=";
                        sqlAnalysis.StartQuantitySignOfSql = "tb.rowNum>";
                    }
                    else
                    {
                        sql = "select top {0} {1} from {2}{3}{4}{5}";
                        sql = sql.ExtFormat(top.ToString(), selectPart, fromPart, wherePart, groupPart);
                    }

                    return sql;
                });
        }

        string ISqlAnalysis.GetTop(string selectPart1, string fromPart1, string wherePart1, string groupPart1, string orderByPart1, int startNumber1, int length1)
        {
            return GetTop(selectPart1, fromPart1, wherePart1, groupPart1, orderByPart1, startNumber1, length1,
                delegate (string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length)
                {
                    int end = startNumber + length;
                    string sql = "select * from (select row_number() over({4}) rowNum,{0} from {1}{2}{3}{4}) tb where tb.rowNum>={5} and tb.rowNum<{6}";
                    sql = sql.ExtFormat(selectPart, fromPart, wherePart, groupPart, orderByPart, startNumber.ToString(), end.ToString());
                    ISqlAnalysis sqlAnalysis = this;
                    sqlAnalysis.PageSizeSignOfSql = "tb.rowNum<";
                    sqlAnalysis.StartQuantitySignOfSql = "tb.rowNum>=";
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
            Regex rg = new Regex(@"(?<SqlLeft>^insert\s+(into\s+)?[a-z0-9_\[\]]+\s*\([^\(\)]+\))\s+(?<SqlRight>values\s*\([^\(\)]+\))", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(sql)) return sql;
            Match m = rg.Match(sql);
            string SqlLeft = m.Groups["SqlLeft"].Value;
            string SqlRight = m.Groups["SqlRight"].Value;
            string output = " ";
            if (null != primaryKeys)
            {
                output = "";
                ISqlAnalysis sqlAnalysis = this;
                foreach (var item in primaryKeys)
                {
                    output += ",Inserted." + sqlAnalysis.GetFieldName(item);
                }

                if (0 < primaryKeys.Count)
                {
                    int n = output.IndexOf(",") + 1;
                    output = output.Substring(n);
                    output = " output " + output + " ";
                }
                else
                {
                    output = " ";
                }
            }
            string s = SqlLeft + output + SqlRight;
            return s;
        }

        bool ISqlAnalysis.IsLegalCaseDefaultValueWhenInsert(string tableName, object fieldValue, PropertyInfo propertyInfo, FieldMapping fieldMapping, ref object defaultValue)
        {
            defaultValue = fieldValue;
            if (null == fieldMapping) return true;
            if (!string.IsNullOrEmpty(fieldMapping.DefualtValue)) return false;
            return true;
        }

        string ISqlAnalysis.GetLegalName(string name)
        {
            return LegalName(name);
        }

    }
}
