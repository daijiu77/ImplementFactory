using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public class SqlServerAnalysis : ISqlAnalysis
    {
        string ISqlAnalysis.GetCondition(string fieldName, ConditionRelation relation, object fieldValue)
        {
            string wherePart = "";
            if (null == fieldValue) return wherePart;

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

            Func<object, bool> funcIsChar = (fv) =>
            {
                bool mbool = false;
                if ((typeof(string) == fv.GetType())
                || (typeof(Guid) == fv.GetType()) || (typeof(Guid?) == fv.GetType())
                || (typeof(DateTime) == fv.GetType()) || (typeof(DateTime?) == fv.GetType()))
                {
                    mbool = true;
                }
                return mbool;
            };

            string s = "";
            sign = sign.ToUpper();
            if (null != (fieldValue as DbBody))
            {
                string sql = ((DbBody)fieldValue).GetSql();
                sql = "(" + sql + ")";
                sign = string.Format(sign, sql);
            }
            else if (null != (fieldValue as System.Collections.ICollection))
            {
                //bool isIN = -1 != sign.ToLower().IndexOf("in");
                bool? isChar = null;
                System.Collections.ICollection collection = (System.Collections.ICollection)fieldValue;
                foreach (var item in collection)
                {
                    if (null == item) continue;
                    if (null == isChar) isChar = funcIsChar(item);
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
                wherePart = fieldName + " IN (" + s + ")";
            }
            else if (funcIsChar(fieldValue))
            {
                if (-1 != sign.ToLower().IndexOf("like"))
                {
                    sign = string.Format(sign, fieldValue.ToString());
                }
                else if (-1 != sign.ToLower().IndexOf("in"))
                {
                    s = fieldValue.ToString();
                    if (2 <= s.Length)
                    {
                        if (s.Substring(0, 1).Equals("(") && s.Substring(s.Length - 1).Equals(")"))
                        {
                            s = s.Substring(1);
                            s = s.Substring(0, s.Length - 1);
                        }
                        fieldValue = s;
                    }

                    sign = string.Format(sign, "(" + fieldValue.ToString() + ")");
                }
                else
                {
                    sign = string.Format(sign, fieldValue.ToString());
                }
            }
            else
            {
                sign = string.Format(sign, fieldValue.ToString());
            }

            if (string.IsNullOrEmpty(wherePart)) wherePart = fieldName + " " + sign;
            return wherePart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetGroupBy(string groupByFields)
        {
            string groupPart = "Group by ".ToUpper() + groupByFields.Trim();
            return groupPart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetOrderBy(string orderByItems)
        {
            string orderbyPart = "order by ".ToUpper() + orderByItems;
            return orderbyPart;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetOrderByItem(string fieldName, OrderByRule orderByRule)
        {
            string orderbyPart = fieldName;
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

        private string GetWhere(string wherePart)
        {
            wherePart = wherePart.Trim();
            if (!string.IsNullOrEmpty(wherePart))
            {
                Regex rg = new Regex(@"^where\s+.+", RegexOptions.IgnoreCase);
                if (!rg.IsMatch(wherePart))
                {
                    wherePart = "where " + wherePart;
                }
                wherePart = " " + wherePart;
            }
            return wherePart;
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
            string sql = "select {0} from (select ROW_NUMBER() OVER({1}) rowNumber,{0} from {2}{3}{4}) {5} where {5}.rowNumber>{6} and {5}.rowNumber<={7}";
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
            string sql = "select top {0} {1} from {2}{3}{4}{5}";
            sql = sql.ExtFormat(top.ToString(), selectPart, fromPart, wherePart, groupPart, orderByPart);
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

        string ISqlAnalysis.GetDelete(string tableName, string wherePart)
        {
            if (null == wherePart) wherePart = "";
            wherePart = GetWhere(wherePart);
            string sql = "delete from {0}{1}";
            sql = sql.ExtFormat(tableName, wherePart);
            return sql;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetUpdate(string updatePart, string tableName, string wherePart)
        {
            if (null == wherePart) wherePart = "";
            wherePart = GetWhere(wherePart);
            string sql = "update {0} set {1}{2}";
            sql = sql.ExtFormat(updatePart, tableName, wherePart);
            return sql;
            //throw new NotImplementedException();
        }

        string ISqlAnalysis.GetInsert(string tableName, string selectPart, string valuesPart)
        {
            string sql = "insert into {0}{1} {2}";
            sql = sql.ExtFormat(tableName, selectPart, valuesPart);
            return sql;
            //throw new NotImplementedException();
        }
    }
}
