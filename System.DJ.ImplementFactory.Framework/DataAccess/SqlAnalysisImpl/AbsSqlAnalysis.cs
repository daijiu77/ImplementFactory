using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl
{
    public abstract class AbsSqlAnalysis
    {
        protected db_dialect dataType = db_dialect.none;
        protected char leftTag = ' ';
        protected char rightTag = ' ';

        protected Regex specialRg = new Regex(@"(^[0-9]$)|(^[1-9][0-9]*[0-9]$)|(^[\-\+][0-9]$)|(^[\-\+][1-9][0-9]*[0-9]$)|(^[0-9]\.[0-9]*[0-9]$)|(^[1-9][0-9]+\.[0-9]*[0-9]$)|(^[\-\+][0-9]\.[0-9]*[0-9]$)|(^[\-\+][1-9][0-9]+\.[0-9]*[0-9]$)|(^true$)|(^false$)|(^null$)", RegexOptions.IgnoreCase);

        protected bool IsAliasField(string chars, ref string alias, ref string field)
        {
            bool mbool = false;
            if (string.IsNullOrEmpty(chars)) return mbool;
            string s = chars.Trim();
            Regex rg = new Regex(@"^[^a-z0-9_].+[^a-z0-9_]$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s)) return mbool;
            rg = new Regex(@"^(?<alias>[a-z0-9_]+)\.((?<field>[a-z0-9_]+)|([\[\`""](?<field>[a-z0-9_]+)[\]\`""]))$", RegexOptions.IgnoreCase);
            if (rg.IsMatch(s))
            {
                mbool = true;
                Match m = rg.Match(s);
                alias = m.Groups["alias"].Value;
                field = m.Groups["field"].Value;
            }
            return mbool;
        }

        protected string LegalName(string name)
        {
            string dbn = name;
            const int size = 60;
            if (size < name.Length)
            {
                string s = "";
                Regex rg = new Regex(@"[A-Z0-9]+");
                if (rg.IsMatch(name))
                {
                    MatchCollection mc = rg.Matches(name);
                    foreach (Match item in mc)
                    {
                        s += item.Groups[0].Value;
                    }
                }
                int n = size - s.Length - 1;
                int len = name.Length;
                dbn = s + "_" + name.Substring(len - n);
            }
            return dbn;
        }

        public string GetWhere(string wherePart, bool IgnoreWhereChar)
        {
            wherePart = wherePart.Trim();
            if (string.IsNullOrEmpty(wherePart)) return wherePart;

            Regex rg = new Regex(@"^where\s+(?<ConditionStr>.+)", RegexOptions.IgnoreCase);
            if (IgnoreWhereChar)
            {
                if (rg.IsMatch(wherePart))
                {
                    wherePart = rg.Match(wherePart).Groups["ConditionStr"].Value;
                }
            }
            else
            {
                if (!rg.IsMatch(wherePart))
                {
                    int n = 0;
                    const int maxNum = 100;
                    Regex rg1 = new Regex(@"^((and)|(or))\s+(?<where_str>.+)");
                    while (rg1.IsMatch(wherePart) && (n < maxNum))
                    {
                        wherePart = rg1.Match(wherePart).Groups["where_str"].Value.Trim();
                        if (string.IsNullOrEmpty(wherePart)) break;
                        n++;
                    }
                    if (!string.IsNullOrEmpty(wherePart)) wherePart = "where " + wherePart;
                }
            }
            if (!string.IsNullOrEmpty(wherePart)) wherePart = " " + wherePart;
            return wherePart;
        }

        public string GetWhere(string wherePart)
        {
            return GetWhere(wherePart, false);
        }

        #region 提取的代码
        protected string GetRuleSign(ConditionRelation relation)
        {
            string sign = "";
            switch (relation)
            {
                case ConditionRelation.Equals:
                    sign = "= {0}";
                    break;
                case ConditionRelation.NoEquals:
                    sign = "<> {0}";
                    if (db_dialect.mysql == dataType)
                    {
                        sign = "!= {0}";
                    }
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

        protected bool IsChar(object fv)
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

        protected string GetConditionOfBaseValue(string fieldName, ConditionRelation relation, object fieldValueOfBaseValue, Dictionary<string, string> aliasDic)
        {
            string wherePart = "";
            if (null == fieldValueOfBaseValue) return wherePart;

            string s = "";
            string sign = GetRuleSign(relation);
            if (IsChar(fieldValueOfBaseValue))
            {
                if (-1 != sign.ToLower().IndexOf("like"))
                {
                    s = fieldValueOfBaseValue.ToString();
                    if (0 == s.Trim().Length) return wherePart;
                    if (2 < s.Length)
                    {
                        if ((s.Substring(0, 1).Equals("'") && s.Substring(s.Length - 1).Equals("'"))
                            || (s.Substring(0, 1).Equals("\"") && s.Substring(s.Length - 1).Equals("\"")))
                        {
                            s = s.Substring(1);
                            s = s.Substring(0, s.Length - 1);
                        }
                    }
                    sign = string.Format(sign, s);
                }
                else if (-1 != sign.ToLower().IndexOf("in"))
                {
                    s = fieldValueOfBaseValue.ToString();
                    if (0 == s.Trim().Length) return wherePart;
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
                    s = fieldValueOfBaseValue.ToString();
                    if (string.IsNullOrEmpty(s)) return wherePart;
                    if (null == aliasDic) aliasDic = new Dictionary<string, string>();
                    string alias = "";
                    string field = "";
                    bool mbool = IsAliasField(s, ref alias, ref field);                    
                    if (mbool)
                    {
                        string aliasLower = alias.ToLower();
                        mbool = aliasDic.ContainsKey(aliasLower);
                    }
                    if (mbool)
                    {
                        s = ((ISqlAnalysis)this).GetFieldName(alias, field);
                    }
                    else if (!specialRg.IsMatch(s))
                    {
                        if (false == s.Substring(0, 1).Equals("'") && false == s.Substring(s.Length - 1).Equals("'"))
                        {
                            s = "'" + s + "'";
                        }
                    }
                    sign = string.Format(sign, s);
                }
            }
            else if (fieldValueOfBaseValue.GetType() == typeof(bool))
            {
                bool mbool = (bool)fieldValueOfBaseValue;
                sign = string.Format(sign, (mbool ? "1" : "0"));
            }
            else
            {
                sign = string.Format(sign, fieldValueOfBaseValue.ToString());
            }

            if (string.IsNullOrEmpty(wherePart)) wherePart = ((ISqlAnalysis)this).GetFieldName(fieldName) + " " + sign;
            return wherePart;
        }

        protected string GetConditionOfCollection(string fieldName, ConditionRelation relation, ICollection fieldValueOfCollection)
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

        protected string GetConditionOfDbSqlBody(string fieldName, ConditionRelation relation, string fieldValueOfSql)
        {
            string wherePart = "";
            if (null == fieldValueOfSql) return wherePart;
            string sign = GetRuleSign(relation);
            string sql = "(" + fieldValueOfSql + ")";
            sign = string.Format(sign, sql);
            if (string.IsNullOrEmpty(wherePart)) wherePart = ((ISqlAnalysis)this).GetFieldName(fieldName) + " " + sign;
            return wherePart;
        }

        protected string GetGroupBy(string groupByFields)
        {
            if (string.IsNullOrEmpty(groupByFields)) return "";
            string groupPart = "Group by " + groupByFields.Trim();
            return groupPart;
        }

        protected string GetOrderBy(string orderByItems)
        {
            if (string.IsNullOrEmpty(orderByItems)) return "";
            string orderbyPart = "order by " + orderByItems.Trim();
            return orderbyPart;
        }

        protected string GetOrderByItem(string fieldName, OrderByRule orderByRule)
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
        }

        protected delegate string DgtPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber);
        protected string GetPageChange(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int pageSize, int pageNumber, DgtPageChange pageChange)
        {
            if (null == orderByPart) orderByPart = "";
            orderByPart = orderByPart.Trim();

            if (!string.IsNullOrEmpty(orderByPart)) orderByPart = " " + orderByPart;
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;
            return pageChange(selectPart, fromPart, wherePart, groupPart, orderByPart, pageSize, pageNumber);
        }

        protected delegate string DgtGetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top);
        protected string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int top, DgtGetTop getTop)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            if (null == orderByPart) orderByPart = "";

            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;

            orderByPart = orderByPart.Trim();
            if (!string.IsNullOrEmpty(orderByPart)) orderByPart = " " + orderByPart;

            return getTop(selectPart, fromPart, wherePart, groupPart, orderByPart, top);
        }

        protected delegate string DgtGetTop1(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length);
        protected string GetTop(string selectPart, string fromPart, string wherePart, string groupPart, string orderByPart, int startNumber, int length, DgtGetTop1 dgtGet)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            if (null == orderByPart) orderByPart = "";

            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;

            orderByPart = orderByPart.Trim();
            if (!string.IsNullOrEmpty(orderByPart)) orderByPart = " " + orderByPart;
            return dgtGet(selectPart, fromPart, wherePart, groupPart, orderByPart, startNumber, length);
        }

        protected string GetCount(string fromPart, string wherePart, string groupPart)
        {
            if (null == wherePart) wherePart = "";
            if (null == groupPart) groupPart = "";
            wherePart = GetWhere(wherePart);

            groupPart = groupPart.Trim();
            if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;
            string sql = "select count(1) countNum from {0}{1}{2};";
            sql = sql.ExtFormat(fromPart, wherePart, groupPart);
            return sql;
            //throw new NotImplementedException();
        }

        protected string GetJoin(string flag, string tableName, string alias, string wherePart)
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

        protected string GetTableAilas(string tableName, string alias)
        {
            string sql = ((ISqlAnalysis)this).GetTableName(tableName);
            string s = null == alias ? "" : alias;
            s = s.Trim();
            if (!string.IsNullOrEmpty(s)) sql += " " + s;
            return sql;
        }

        protected string GetFieldAlias(string fieldName, string alias)
        {
            string sql = ((ISqlAnalysis)this).GetFieldName(fieldName);
            string s = null == alias ? "" : alias;
            s = s.Trim();
            if (!string.IsNullOrEmpty(s)) sql += " " + s;
            return sql;
        }

        protected string GetTableName(string tableName)
        {
            ISqlAnalysis sqlAnalysis = (ISqlAnalysis)this;
            string leftTag = this.leftTag.ToString();
            string rightTag = this.rightTag.ToString();
            if (leftTag == tableName.Substring(0, 1) && rightTag == tableName.Substring(tableName.Length - 1))
            {
                string s = tableName.Substring(1);
                s = s.Substring(0, s.Length - 1);
                tableName = sqlAnalysis.GetLegalName(s);
                tableName = leftTag + tableName + rightTag;
                return tableName;
            }
            tableName = sqlAnalysis.GetLegalName(tableName);
            return leftTag + tableName + rightTag;
        }

        protected string StandardFieldName(string fieldName)
        {
            return leftTag.ToString() + fieldName + rightTag.ToString();
        }

        protected string GetFieldName(string alias, string fieldName)
        {
            ISqlAnalysis sqlAnalysis = (ISqlAnalysis)this;
            if (!string.IsNullOrEmpty(alias))
            {
                fieldName = sqlAnalysis.GetLegalName(fieldName);
                return alias + "." + StandardFieldName(fieldName);
            }
            else
            {
                string field = "";
                string leftTag = this.leftTag.ToString();
                string rightTag = this.rightTag.ToString();
                bool mbool = IsAliasField(fieldName, ref alias, ref field);
                if (mbool)
                {
                    field = sqlAnalysis.GetLegalName(field);
                    return alias + "." + StandardFieldName(field);
                }
                else if (leftTag == fieldName.Substring(0, 1) && rightTag == fieldName.Substring(fieldName.Length - 1))
                {
                    string s = fieldName.Substring(1);
                    s = s.Substring(0, s.Length - 1);
                    fieldName = sqlAnalysis.GetLegalName(s);
                    fieldName = StandardFieldName(fieldName);
                    return fieldName;
                }
            }
            fieldName = sqlAnalysis.GetLegalName(fieldName);
            return StandardFieldName(fieldName);
        }

        /// <summary>
        /// 获取主键最大值的 sql 语句
        /// </summary>
        /// <param name="primaryKeyName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected delegate string DgtGetSqlOfPKV(string primaryKeyName, string tableName);
        protected bool IsLegalCaseDefaultValueWhenInsert(string tableName, object fieldValue, PropertyInfo propertyInfo, FieldMapping fieldMapping, ref object defaultValue, DgtGetSqlOfPKV sqlPKV)
        {
            defaultValue = null;
            if (null == fieldMapping) return true;
            if (!string.IsNullOrEmpty(fieldMapping.DefualtValue))
            {
                Regex rg = new Regex(@"[0-9]+", RegexOptions.IgnoreCase);
                if ((propertyInfo.PropertyType == typeof(int)) || (propertyInfo.PropertyType == typeof(long)))
                {
                    if (null != fieldValue)
                    {
                        if (propertyInfo.PropertyType == typeof(int))
                        {
                            int iNum = 0;
                            if (int.TryParse(fieldValue.ToString(), out iNum)) defaultValue = iNum;
                        }
                        else
                        {
                            long lNum = 0;
                            if (Int64.TryParse(fieldValue.ToString(), out lNum)) defaultValue = lNum;
                        }
                        if (null != defaultValue) return true;
                    }

                    if (rg.IsMatch(fieldMapping.DefualtValue))
                    {
                        string sNum = rg.Match(fieldMapping.DefualtValue).Groups[0].Value;
                        defaultValue = Convert.ToInt32(sNum);
                        return true;
                    }
                    AutoCall autoCall = new AutoCall();
                    long num = 0;
                    string err = "";
                    string sql = sqlPKV(propertyInfo.Name, tableName);
                    IDbHelper dbHelper = ImplementAdapter.DbHelper;
                    dbHelper.query(autoCall, sql, false, (dt) =>
                    {
                        if (null == dt) return;
                        if (0 == dt.Rows.Count) return;
                        object fv = DBNull.Value == dt.Rows[0][0] ? 0 : dt.Rows[0][0];
                        fv = DJTools.ConvertTo(fv, propertyInfo.PropertyType);
                        num = Convert.ToInt64(fv);
                    }, ref err);
                    ImplementAdapter.Destroy(dbHelper);
                    defaultValue = num + 1;
                }
                else if (propertyInfo.PropertyType == typeof(Guid))
                {
                    if (null != fieldValue)
                    {
                        Guid guid = Guid.Empty;
                        if (Guid.TryParse(fieldValue.ToString(), out guid))
                        {
                            if (Guid.Empty == guid) guid = Guid.NewGuid();
                            defaultValue = guid;
                        }
                        if (null != defaultValue) return true;
                    }
                    defaultValue = Guid.NewGuid();
                }
                else if (propertyInfo.PropertyType == typeof(DateTime))
                {
                    if (null != fieldValue)
                    {
                        DateTime dt = DateTime.Now;
                        if (DateTime.TryParse(fieldValue.ToString(), out dt))
                        {
                            if (DateTime.MinValue == dt) dt = DateTime.Now;
                            defaultValue = dt;
                        }
                        if (null != defaultValue) return true;
                    }
                    defaultValue = DateTime.Now;
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    if (rg.IsMatch(fieldMapping.DefualtValue))
                    {
                        string sNum = rg.Match(fieldMapping.DefualtValue).Groups[0].Value;
                        defaultValue = 1 == Convert.ToInt32(sNum);
                        return true;
                    }

                    if (null != fieldValue)
                    {
                        bool mbool = false;
                        if (bool.TryParse(fieldValue.ToString(), out mbool)) defaultValue = mbool;
                        if (null != defaultValue) return true;
                    }

                    if (-1 != fieldMapping.DefualtValue.ToLower().IndexOf("false"))
                    {
                        defaultValue = "false";
                    }
                    else if (-1 != fieldMapping.DefualtValue.ToLower().IndexOf("true"))
                    {
                        defaultValue = "true";
                    }
                }

                if (null == defaultValue) return false;
            }
            return true;
            //throw new NotImplementedException();
        }
        #endregion
    }
}
