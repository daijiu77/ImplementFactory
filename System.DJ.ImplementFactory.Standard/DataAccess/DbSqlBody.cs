using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlBody : DbVisitor
    {
        private List<ConditionItem> conditionItems = new List<ConditionItem>();
        private Dictionary<string, object> dicPara = new Dictionary<string, object>();

        public DbSqlBody() { }

        public DbSqlBody Where(params ConditionItem[] conditionItems)
        {
            if (null != conditionItems)
            {
                foreach (var item in conditionItems)
                {
                    this.conditionItems.Add(item);
                }
            }
            return this;
        }

        public DbSqlBody Skip(int pageNumber, int pageSize)
        {
            this.pageSize = pageSize;
            this.pageNumber = pageNumber;
            return this;
        }

        public DbSqlBody Top(int top)
        {
            this.top = top;
            return this;
        }

        private List<OrderbyItem> orderbyItems = new List<OrderbyItem>();
        public DbSqlBody Orderby(params OrderbyItem[] orderbyItems)
        {
            if (null != orderbyItems)
            {
                foreach (var item in orderbyItems)
                {
                    this.orderbyItems.Add(item);
                }
            }
            return this;
        }

        private Dictionary<string, string> dicSlt = new Dictionary<string, string>();
        public DbSqlBody Select(object field, string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                alias = Guid.NewGuid().ToString();
                dicSlt.Add(alias, alias);
            }
            if (dicSelect.ContainsKey(alias)) dicSelect.Remove(alias);
            dicSelect.Add(alias, field);
            return this;
        }

        public DbSqlBody Group(string field)
        {
            groupFields.Remove(field);
            groupFields.Add(field);
            return this;
        }

        private Dictionary<string, string> dicExcludes = new Dictionary<string, string>();
        public DbSqlBody DataOperateExcludes(params string[] fields)
        {
            if (null != fields)
            {
                foreach (var item in fields)
                {
                    dicExcludes.Add(item.Trim().ToLower(), item);
                }
            }
            return this;
        }

        private Dictionary<string, string> dicContains = new Dictionary<string, string>();
        public DbSqlBody DataOperateContains(params string[] fields)
        {
            if (null != fields)
            {
                foreach (var item in fields)
                {
                    dicContains.Add(item.Trim().ToLower(), item);
                }
            }
            return this;
        }

        private List<string> groupFields = new List<string>();
        public List<string> group
        {
            get { return groupFields; }
        }

        private Dictionary<string, object> dicSelect = new Dictionary<string, object>();
        public Dictionary<string, object> select
        {
            get { return dicSelect; }
        }

        public int pageSize { get; set; }
        public int pageNumber { get; set; } = -1;
        public int top { get; set; }

        public void SetParameter(string parameterName, object parameterValue)
        {
            if (dicPara.ContainsKey(parameterName)) dicPara.Remove(parameterName);
            dicPara.Add(parameterName, parameterValue);
        }

        public void Clear()
        {
            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo methodInfo = null;
            object v = null;
            foreach (var item in fieldInfos)
            {
                if (null == (item as System.Collections.ICollection)) continue;
                if (item.FieldType.IsArray) continue;
                v = item.GetValue(this);
                if (null == v) continue;
                methodInfo = item.FieldType.GetMethod("Clear");
                if (null == methodInfo) continue;
                try
                {
                    methodInfo.Invoke(v, null);
                }
                catch (Exception ex)
                {

                    //throw;
                }
            }
        }

        private string GetSelectPart()
        {
            string selectPart = "";
            string s = "";
            Attribute att = null;
            foreach (KeyValuePair<string, object> item in dicSelect)
            {
                if (null == item.Value) continue;
                if (null != (item.Value as DbSqlBody))
                {
                    s = ((DbSqlBody)item.Value).GetSql();
                    s = "(" + s + ")";
                }
                else if ((false == item.Value.GetType().IsBaseType()) && item.Value.GetType().IsClass)
                {
                    item.Value.ForeachProperty((pi, type, fn, fv) =>
                    {
                        att = type.GetCustomAttribute(typeof(FieldMapping));
                        if (null != att)
                        {
                            s += ", " + ((FieldMapping)att).FieldName;
                        }
                        else
                        {
                            s += ", " + fn;
                        }
                    });
                }
                else
                {
                    s = item.Value.ToString();
                }

                if (dicSlt.ContainsKey(item.Key))
                {
                    selectPart += ", " + s;
                }
                else
                {
                    selectPart += ", " + sqlAnalysis.GetFieldAlias(s, item.Key);
                }
            }
            if (!string.IsNullOrEmpty(selectPart)) selectPart = selectPart.Substring(1);
            selectPart = selectPart.Trim();
            if (string.IsNullOrEmpty(selectPart)) selectPart = "*";
            return selectPart;
        }

        private object GetValueByType(ConditionItem conditionItem)
        {
            if (null == conditionItem.ValueType) return conditionItem.FieldValue;
            object fv = conditionItem.FieldValue;
            fv = fv ?? "";
            Type type = conditionItem.ValueType;
            if ((typeof(DateTime) == type)
                || (typeof(DateTime?) == type)
                || (typeof(string) == type)
                || (typeof(Guid) == type)
                || (typeof(Guid?) == type))
            {
                fv = "'" + fv + "'";
            }
            return fv;
        }

        private string GetConditionUnit(ConditionItem[] conditions)
        {
            string cdt = "";
            string cnts = "";
            if (null == conditions) return cdt;
            string s = "";
            string sql = "";
            object fv = null;
            Regex rg = new Regex(@"^\s*((and)|or)\s+(?<wherePart>.+)", RegexOptions.IgnoreCase);
            foreach (ConditionItem item in conditions)
            {
                cnts = " and ";
                if (item.IsOr) cnts = " or ";
                if (null != item.conditionItems)
                {
                    if (0 < item.conditionItems.Length)
                    {
                        s = GetConditionUnit(item.conditionItems);
                        s = rg.Match(s).Groups["wherePart"].Value;
                        s = "(" + s + ")";
                        cdt += cnts + s;
                        continue;
                    }
                }
                if (null != (item.FieldValue as ICollection))
                {
                    cdt += cnts + sqlAnalysis.GetConditionOfCollection(item.FieldName, item.Relation, (ICollection)item.FieldValue);
                }
                else if (null != (item.FieldValue as DbSqlBody))
                {
                    sql = ((DbSqlBody)item.FieldValue).GetSql();
                    cdt += cnts + sqlAnalysis.GetConditionOfDbSqlBody(item.FieldName, item.Relation, sql);
                }
                else
                {
                    fv = GetValueByType(item);
                    cdt += cnts + sqlAnalysis.GetConditionOfBaseValue(item.FieldName, item.Relation, fv);
                }
            }
            return cdt;
        }

        private string GetFromPart(ref string wherePart)
        {
            string fromPart = "";
            string s = "";
            string s1 = "";
            string ConditionBody = "";
            bool mbool = false;
            bool isFirst = true;
            bool isSqlBody = false;
            Regex rg = new Regex(@"^\s+((or)|(and))\s+(?<ConditionBody>.+)", RegexOptions.IgnoreCase);
            Attribute att = null;

            foreach (SqlFromUnit item in fromUnits)
            {
                isSqlBody = false;
                if (null != item.dataModel)
                {
                    isSqlBody = null != (item.dataModel as DbSqlBody);
                }

                if (isSqlBody)
                {
                    s = ((DbSqlBody)item.dataModel).GetSql();
                    s = "(" + s + ")";
                    if (!string.IsNullOrEmpty(item.alias)) s += " " + item.alias;
                    if (null != item.conditions)
                    {
                        wherePart += GetConditionUnit(item.conditions);
                    }
                }
                else
                {
                    att = item.modelType.GetCustomAttribute(typeof(TableAttribute));
                    if (null != att)
                    {
                        s = ((TableAttribute)att).Name;
                    }
                    else
                    {
                        s = item.modelType.Name;
                    }

                    mbool = false;
                    if ((null != (item as LeftJoin) || null != (item as RightJoin) || null != (item as InnerJoin)) && false == isFirst)
                    {
                        mbool = true;
                    }
                    isFirst = false;

                    if (mbool)
                    {
                        ConditionBody = "";
                        if (null != item.conditions)
                        {
                            s1 = GetConditionUnit(item.conditions);
                            if (rg.IsMatch(s1))
                            {
                                ConditionBody = rg.Match(s1).Groups["ConditionBody"].Value;
                            }
                        }

                        if (null != (item as LeftJoin))
                        {
                            s = sqlAnalysis.GetLeftJoin(s, item.alias, ConditionBody);
                        }
                        else if (null != (item as RightJoin))
                        {
                            s = sqlAnalysis.GetRightJoin(s, item.alias, ConditionBody);
                        }
                        else if (null != (item as InnerJoin))
                        {
                            s = sqlAnalysis.GetInnerJoin(s, item.alias, ConditionBody);
                        }
                    }
                    else if (null != item.conditions)
                    {
                        wherePart += GetConditionUnit(item.conditions);
                    }

                    if (!mbool)
                    {
                        s = sqlAnalysis.GetTableAilas(s, item.alias);
                    }
                }
                fromPart += ", " + s;
            }
            if (!string.IsNullOrEmpty(fromPart)) fromPart = fromPart.Substring(2);
            return fromPart;
        }

        private string GetWherePart(string wherePart)
        {
            wherePart += GetConditionUnit(conditionItems.ToArray());

            if (!string.IsNullOrEmpty(wherePart))
            {
                Regex rg = new Regex(@"^\s*((and)|or)\s+(?<wherePart>.+)", RegexOptions.IgnoreCase);
                if (rg.IsMatch(wherePart))
                {
                    wherePart = rg.Match(wherePart).Groups["wherePart"].Value;
                }
            }
            return wherePart;
        }

        private string GetGroupPart()
        {
            string groupPart = "";
            foreach (var item in groupFields)
            {
                groupPart += ", " + item;
            }
            if (!string.IsNullOrEmpty(groupPart)) groupPart = groupPart.Substring(1);
            return groupPart;
        }

        private string GetOrderbyPart()
        {
            string orderbyPart = "";
            foreach (var item in orderbyItems)
            {
                orderbyPart += ", " + sqlAnalysis.GetOrderByItem(item.FieldName, item.Rule);
            }
            if (!string.IsNullOrEmpty(orderbyPart)) orderbyPart = orderbyPart.Substring(1);
            return orderbyPart;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUnitAction">tableName, where</param>
        /// <param name="propertyAction">fieldName, fieldValue</param>
        /// <param name="propEndAction">属性结束</param>
        private void CreateDataOpt(Action<string, string> fromUnitAction, Action<string, object> propertyAction, Action propEndAction)
        {
            string tbName = "";
            string wherePart = "";
            string field = "";
            Regex rg = new Regex(@"^\s+((or)|(and))\s+(?<ConditionBody>.+)", RegexOptions.IgnoreCase);
            Attribute att = null;
            foreach (SqlFromUnit item in fromUnits)
            {
                if (null == item.dataModel) continue;
                if (null != (item.dataModel as DbSqlBody)) continue;
                wherePart = "";
                att = item.dataModel.GetType().GetCustomAttribute(typeof(TableAttribute));
                if (null != att)
                {
                    tbName = ((TableAttribute)att).Name;
                }
                else
                {
                    tbName = item.dataModel.GetType().Name;
                }

                if (null != item.conditions)
                {
                    wherePart = GetConditionUnit(item.conditions);
                    if (rg.IsMatch(wherePart))
                    {
                        wherePart = rg.Match(wherePart).Groups["ConditionBody"].Value;
                    }
                }
                fromUnitAction(tbName, wherePart);
                if (null == propertyAction) continue;
                item.dataModel.ForeachProperty((pi, type, fn, fv) =>
                {
                    if (null == fv) return;
                    field = fn.ToLower();
                    if (0 < dicContains.Count)
                    {
                        if (!dicContains.ContainsKey(field)) field = "";
                    }
                    if (string.IsNullOrEmpty(field)) return;
                    if (0 < dicExcludes.Count)
                    {
                        if (dicExcludes.ContainsKey(field)) field = "";
                    }
                    if (string.IsNullOrEmpty(field)) return;
                    propertyAction(fn, fv);
                });
                if (null != propEndAction) propEndAction();
            }
        }

        protected List<SqlDataItem> GetUpdate()
        {
            List<SqlDataItem> list = new List<SqlDataItem>();
            SqlDataItem dataItem = null;
            DbParameter para = null;
            string dbTag = DJTools.GetParaTagByDbDialect(Commons.DataAdapter.dbDialect);
            string sql = "";
            string sets = "";
            string where = "";
            CreateDataOpt((tb, whereStr) =>
            {
                dataItem = new SqlDataItem();
                sql = "update " + tb + " set ";
                where = whereStr;
                sets = "";
            }, (fn, fv) =>
            {
                sets += ", " + fn + "=" + dbTag + fn;
                para = ImplementAdapter.dataServerProvider.CreateDbParameter(fn, fv);
                dataItem.parameters.Add(para);
            }, () =>
            {
                if (!string.IsNullOrEmpty(sets))
                {
                    sets = sets.Substring(2);
                    where = GetWherePart(where);
                    sql += sets;
                    if (!string.IsNullOrEmpty(where)) sql += " where " + where;
                    dataItem.sql = sql;
                    list.Add(dataItem);
                }
            });
            return list;
        }

        protected List<SqlDataItem> GetInsert()
        {
            List<SqlDataItem> list = new List<SqlDataItem>();
            SqlDataItem dataItem = null;
            DbParameter para = null;
            string dbTag = DJTools.GetParaTagByDbDialect(Commons.DataAdapter.dbDialect);
            string sql = "";
            string fields = "";
            string vals = "";
            CreateDataOpt((tb, whereStr) =>
            {
                dataItem = new SqlDataItem();
                sql = "insert into " + tb + "({0}) values({1})";
                fields = "";
                vals = "";
            }, (fn, fv) =>
            {
                fields += ", " + fn;
                vals += ", " + dbTag + fn;
                para = ImplementAdapter.dataServerProvider.CreateDbParameter(fn, fv);
                dataItem.parameters.Add(para);
            }, () =>
            {
                if (!string.IsNullOrEmpty(fields))
                {
                    fields = fields.Substring(2);
                    vals = vals.Substring(2);
                    sql = sql.ExtFormat(fields, vals);
                    dataItem.sql = sql;
                    list.Add(dataItem);
                }
            });
            return list;
        }

        protected List<SqlDataItem> GetDelete()
        {
            List<SqlDataItem> list = new List<SqlDataItem>();
            string sql = "";
            string whereStr = "";
            CreateDataOpt((tb, where) =>
            {
                sql = "delete from " + tb;
                whereStr = GetWherePart(where);
                if (!string.IsNullOrEmpty(whereStr)) sql += " where " + whereStr;
                list.Add(new SqlDataItem()
                {
                    sql = sql
                });
            }, null, null);
            return list;
        }

        protected string GetCountSql()
        {
            string wherePart = "";

            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart);
            wherePart = GetWherePart(wherePart);
            string groupPart = GetGroupPart();

            string sql = sqlAnalysis.GetCount(fromPart, wherePart, groupPart);
            return sql;
        }

        private string GetTop(int start, int top)
        {
            string wherePart = "";

            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart);
            wherePart = GetWherePart(wherePart);
            string groupPart = GetGroupPart();
            string orderbyPart = GetOrderbyPart();

            orderbyPart = sqlAnalysis.GetOrderBy(orderbyPart);
            groupPart = sqlAnalysis.GetGroupBy(groupPart);

            wherePart = wherePart.Trim();
            groupPart = groupPart.Trim();
            orderbyPart = orderbyPart.Trim();
            return sqlAnalysis.GetTop(selectPart, fromPart, wherePart, groupPart, orderbyPart, start, top);
        }

        protected string GetSql()
        {
            string wherePart = "";

            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart);
            wherePart = GetWherePart(wherePart);
            string groupPart = GetGroupPart();
            string orderbyPart = GetOrderbyPart();

            orderbyPart = sqlAnalysis.GetOrderBy(orderbyPart);
            groupPart = sqlAnalysis.GetGroupBy(groupPart);

            wherePart = wherePart.Trim();
            groupPart = groupPart.Trim();
            orderbyPart = orderbyPart.Trim();

            string sql = "";
            if (0 < pageSize && -1 < pageNumber)
            {
                sql = sqlAnalysis.GetPageChange(selectPart, fromPart, wherePart, groupPart, orderbyPart, pageSize, pageNumber);
            }
            else if (0 < top)
            {
                sql = sqlAnalysis.GetTop(selectPart, fromPart, wherePart, groupPart, orderbyPart, top);
            }
            else
            {
                if (!string.IsNullOrEmpty(wherePart)) wherePart = " where " + wherePart;
                sql = "select {0} from {1}{2}{3}{4}";
                sql = string.Format(sql, selectPart, fromPart, wherePart, groupPart, orderbyPart);
            }
            return sql;
        }
    }
}
