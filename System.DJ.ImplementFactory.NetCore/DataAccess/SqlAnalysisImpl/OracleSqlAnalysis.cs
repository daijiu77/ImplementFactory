using System.Collections;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public class OracleSqlAnalysis : ISqlAnalysis
    {
        private string GetRuleSign(ConditionRelation relation)
        {
            string sign = "";
            switch (relation)
            {
                case ConditionRelation.Equals:
                    sign = "= {0}";
                    break;
                case ConditionRelation.NoEquals:
                    sign = "<> {0}";
                    break;
                case ConditionRelation.Greader:
                    sign = "> {0}";
                    break;
                case ConditionRelation.GreaderOrEquals:
                    sign = ">= {0}";
                    break;
                case ConditionRelation.Less:
                    sign = "< {0}";
                    break;
                case ConditionRelation.LessOrEquals:
                    sign = "<= {0}";
                    break;
                case ConditionRelation.Contain:
                    sign = "like '%{0}%'";
                    break;
                case ConditionRelation.LeftContain:
                    sign = "like '%{0}'";
                    break;
                case ConditionRelation.RightContain:
                    sign = "like '{0}%'";
                    break;
                case ConditionRelation.In:
                    sign = "in {0}";
                    break;
            }
            return sign;
        }

        private bool IsChar(object fv)
        {
            bool mbool = false;
            if ((typeof(string) == fv.GetType())
            || (typeof(Guid) == fv.GetType()) || (typeof(Guid?) == fv.GetType())
            || (typeof(DateTime) == fv.GetType()) || (typeof(DateTime?) == fv.GetType()))
            {
                mbool = true;
            }
            return mbool;
        }

        string ISqlAnalysis.GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValueOfBaseValue)
        {
            string wherePart = "";
            if (null == fieldValueOfBaseValue) return wherePart;

            string s = "";
            string sign = GetRuleSign(relation);
            if (IsChar(fieldValueOfBaseValue))
            {
                if (-1 != sign.ToLower().IndexOf("like"))
                {
                    sign = string.Format(sign, fieldValueOfBaseValue.ToString());
                }
                else if (-1 != sign.ToLower().IndexOf("in"))
                {
                    s = fieldValueOfBaseValue.ToString();
                    if (2 <= s.Length)
                    {
                        if (s.Substring(0, 1).Equals("(") && s.Substring(s.Length - 1).Equals(")"))
                        {
                            s = s.Substring(1);
                            s = s.Substring(0, s.Length - 1);
                        }
                        fieldValueOfBaseValue = s;
                    }

                    sign = string.Format(sign, "(" + fieldValueOfBaseValue.ToString() + ")");
                }
                else
                {
                    sign = string.Format(sign, fieldValueOfBaseValue.ToString());
                }
            }
            else
            {
                sign = string.Format(sign, fieldValueOfBaseValue.ToString());
            }

            if (string.IsNullOrEmpty(wherePart)) wherePart = ((ISqlAnalysis)this).GetFieldName(fieldName) + " " + sign;
            return wherePart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetConditionOfCollection(string fieldName, ConditionRelation relation, ICollection fieldValueOfCollection)
        {
            string wherePart = "";
            if (null == fieldValueOfCollection) return wherePart;
            bool? isChar = null;
            string s = "";
            foreach (var item in fieldValueOfCollection)
            {
                if (null == item) continue;
                if (null == isChar) isChar = IsChar(item);
                if ((bool)isChar)
                {
                    s += ", '" + item.ToString() + "'";
                }
                else
                {
                    s += ", " + item.ToString();
                }
            }
            if (!string.IsNullOrEmpty(s)) s = s.Substring(2);
            wherePart = ((ISqlAnalysis)this).GetFieldName(fieldName) + " in (" + s + ")";
            return wherePart;
        }

        string ISqlAnalysis.GetConditionOfDbSqlBody(string fieldName, ConditionRelation relation, string fieldValueOfSql)
        {
            string wherePart = "";
            if (null == fieldValueOfSql) return wherePart;
            string sign = GetRuleSign(relation);
            string sql = "(" + fieldValueOfSql + ")";
            sign = string.Format(sign, sql);
            if (string.IsNullOrEmpty(wherePart)) wherePart = ((ISqlAnalysis)this).GetFieldName(fieldName) + " " + sign;
            return wherePart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetGroupBy(string groupByFields)
        {
            if (string.IsNullOrEmpty(groupByFields)) return "";
            string groupPart = "Group by " + groupByFields.Trim();
            return groupPart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetOrderBy(string orderByItems)
        {
            if (string.IsNullOrEmpty(orderByItems)) return "";
            string orderbyPart = "order by " + orderByItems;
            return orderbyPart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetOrderByItem(string fieldName, OrderByRule orderByRule)
        {
            if (string.IsNullOrEmpty(fieldName)) return "";
            string orderbyPart = ((ISqlAnalysis)this).GetFieldName(fieldName);
            if (OrderByRule.Asc == orderByRule)
            {
                orderbyPart += " asc".ToUpper();
            }
            else
            {
                orderbyPart += " desc".ToUpper();
            }
            return orderbyPart;
            //throw new NotImplementedException();
        }

        private string GetWhere(string wherePart, bool IgnoreWhereChar)
        {
            wherePart = wherePart.Trim();
            if (!string.IsNullOrEmpty(wherePart))
            {
                Regex rg = new Regex(@"^where\s+(?<ConditionStr>.+)", RegexOptions.IgnoreCase);
                if (!IgnoreWhereChar)
                {
                    if (!rg.IsMatch(wherePart))
                    {
                        wherePart = "where " + wherePart;
                    }
                }
                else
                {
                    if (rg.IsMatch(wherePart))
                    {
                        wherePart = rg.Match(wherePart).Groups["ConditionStr"].Value;
                    }
                }
                wherePart = " " + wherePart;
            }
            return wherePart;
        }

        private string GetWhere(string wherePart)
        {
            return GetWhere(wherePart, false);
        }

        string ISqlAnalysis.GetPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber)
        {
            if (null == orderByPart) orderByPart = "";
            orderByPart = orderByPart.Trim();
            if (string.IsNullOrEmpty(orderByPart)) throw new Exception("A field for sorting is required.");

            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;
            Random rnd = new Random();
            string tb = "tb_" + DateTime.Now.ToString("HHmmss") + "_" + rnd.Next(1, 99);
            string sql = "select {0} from (select {0} from {2}{3}{4}) {5} where {5}.ROWNUM>{6} and {5}.ROWNUM<={7}";
            sql = sql.ExtFormat(selectPart, orderByPart, fromPart, wherePart, groupPart, tb, (pageSize * (pageNumber - 1)).ToString(), (pageSize * pageNumber).ToString());
            return sql;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            if (null == orderByPart) orderByPart = "";

            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;

            orderByPart = orderByPart.Trim();
            if (!string.IsNullOrEmpty(orderByPart)) orderByPart = " " + orderByPart;
            string sql = "select * from (select {1} from {2}{3}{4}{5}) tb where ROWNUM<={0};";
            sql = sql.ExtFormat(top.ToString(), selectPart, fromPart, wherePart, groupPart, orderByPart);
            return sql;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            if (null == orderByPart) orderByPart = "";

            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;

            orderByPart = orderByPart.Trim();
            if (!string.IsNullOrEmpty(orderByPart)) orderByPart = " " + orderByPart;
            int end = startNumber + length;
            string sql = "select * from (select rownum,{0} from {1}{2}{3}{4}) tb where ROWNUM>={5} and ROWNUM<{6};";
            sql = sql.ExtFormat(selectPart, fromPart, wherePart, groupPart, orderByPart, startNumber.ToString(), end.ToString());
            return sql;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetCount(string fromPart, string wherePart, string groupPart)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;
            string sql = "select count(1) countNum from {0}{1}{2}";
            sql = sql.ExtFormat(fromPart, wherePart, groupPart);
            return sql;
            //throw new NotImplementedException();
        }

        private string GetJoin(string flag, string tableName, string alias, string wherePart)
        {
            string sql = flag + " " + ((ISqlAnalysis)this).GetTableName(tableName);
            sql = ((ISqlAnalysis)this).GetTableAilas(sql, alias);
            if (!string.IsNullOrEmpty(wherePart))
            {
                wherePart = GetWhere(wherePart, true);
                sql += " on " + wherePart.TrimStart();
            }
            return sql;
        }

        string ISqlAnalysis.GetLeftJoin(string tableName, string alias, string wherePart)
        {
            return GetJoin("Left join", tableName, alias, wherePart);
            //throw new NotImplementedException();
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
            string sql = ((ISqlAnalysis)this).GetTableName(tableName);
            string s = null == alias ? "" : alias;
            s = s.Trim();
            if (!string.IsNullOrEmpty(s)) sql += " " + s;
            return sql;
        }

        string ISqlAnalysis.GetFieldAlias(string fieldName, string alias)
        {
            string sql = ((ISqlAnalysis)this).GetFieldName(fieldName);
            string s = null == alias ? "" : alias;
            s = s.Trim();
            if (!string.IsNullOrEmpty(s)) sql += " " + s;
            return sql;
        }

        string ISqlAnalysis.GetTableName(string tableName)
        {
            if ("\"" == tableName.Substring(0, 1) && "\"" == tableName.Substring(tableName.Length - 1)) return tableName;
            return "\"" + tableName + "\"";
        }

        string ISqlAnalysis.GetFieldName(string fieldName)
        {
            if ("\"" == fieldName.Substring(0, 1) && "\"" == fieldName.Substring(fieldName.Length - 1)) return fieldName;
            return "\"" + fieldName + "\"";
        }
    }
}
