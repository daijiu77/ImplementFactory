using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess
{


    public class DbBody : DbVisitor
    {
        private List<ConditionItem> conditionItems = new List<ConditionItem>();
        private Dictionary<string, object> dicPara = new Dictionary<string, object>();

        public DbBody() { }

        public DbBody Where(params ConditionItem[] conditionItems)
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

        public DbBody PageSize(int pageSize)
        {
            this.pageSize = pageSize;
            return this;
        }

        public DbBody Skip(int pageNumber)
        {
            this.pageNumber = pageNumber;
            return this;
        }

        public DbBody Top(int top)
        {
            this.top = top;
            return this;
        }

        private List<OrderbyItem> orderbyItems = new List<OrderbyItem>();
        public DbBody Orderby(params OrderbyItem[] orderbyItems)
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
        public DbBody Select(object field, string alias)
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

        public DbBody Group(string field)
        {
            groupFields.Remove(field);
            groupFields.Add(field);
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
        public int pageNumber { get; set; }
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
                if (null != (item.Value as DbBody))
                {
                    s = ((DbBody)item.Value).GetSql();
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
                    selectPart += ", " + s + " " + item.Key;
                }
            }
            if (!string.IsNullOrEmpty(selectPart)) selectPart = selectPart.Substring(1);
            selectPart = selectPart.Trim();
            if (string.IsNullOrEmpty(selectPart)) selectPart = "*";
            return selectPart;
        }

        private string GetFromPart(ref string wherePart)
        {
            string fromPart = "";
            string s = "";
            string s1 = "";
            string ConditionBody = "";
            bool mbool = false;
            Regex rg = new Regex(@"^\s+((or)|(and))\s+(?<ConditionBody>.+)", RegexOptions.IgnoreCase);
            Attribute att = null;
            Func<ConditionItem[], string> func = (conditions) =>
            {
                string cdt = "";
                string cnts = "";
                if (null == conditions) return cdt;
                foreach (var item in conditions)
                {
                    cnts = " and ".ToUpper();
                    if (item.IsOr) cnts = " or ".ToUpper();
                    cdt += cnts + sqlAnalysis.GetCondition(item.FieldName, item.Relation, item.FieldValue);
                }
                return cdt;
            };

            foreach (SqlFromUnit item in fromUnits)
            {
                if (null == item.dateModel) continue;
                if (null != (item.dateModel as DbBody))
                {
                    s = ((DbBody)item.dateModel).GetSql();
                    s = "(" + s + ")";
                    if (!string.IsNullOrEmpty(item.alias)) s += " " + item.alias;
                    if (null != item.conditions)
                    {
                        wherePart += func(item.conditions);                        
                    }
                }
                else
                {
                    att = item.dateModel.GetType().GetCustomAttribute(typeof(TableAttribute));
                    if (null != att)
                    {
                        s = ((TableAttribute)att).Name;
                    }
                    else
                    {
                        s = item.dateModel.GetType().Name;
                    }

                    mbool = false;
                    if (null != (item as LeftJoin))
                    {
                        s = "left join ".ToUpper() + s;
                        mbool = true;
                    }
                    else if (null != (item as RightJoin))
                    {
                        s = "right join ".ToUpper() + s;
                        mbool = true;
                    }
                    else if (null != (item as InnerJoin))
                    {
                        s = "inner join ".ToUpper() + s;
                        mbool = true;
                    }

                    if (mbool)
                    {
                        if (!string.IsNullOrEmpty(item.alias)) s += " " + item.alias;
                        if (null!= item.conditions)
                        {
                            s1 = func(item.conditions);
                            if (rg.IsMatch(s1))
                            {
                                ConditionBody = rg.Match(s).Groups["ConditionBody"].Value;
                                s += " on ".ToUpper() + ConditionBody;
                            }
                        }
                    }
                    else if (null != item.conditions)
                    {
                        wherePart += func(item.conditions);
                    }
                }
                fromPart += ", " + s;
            }
            if (!string.IsNullOrEmpty(fromPart)) fromPart = fromPart.Substring(1);
            return fromPart;
        }

        private string GetWherePart()
        {
            string wherePart = "";
            string s = "";
            foreach (var item in conditionItems)
            {
                s = " and ".ToUpper();
                if (item.IsOr) s = " or ".ToUpper();
                wherePart += s + sqlAnalysis.GetCondition(item.FieldName, item.Relation, item.FieldValue);
            }

            if (!string.IsNullOrEmpty(wherePart))
            {
                Regex rg = new Regex(@"^((and)|or)\s+(?<wherePart>.+)", RegexOptions.IgnoreCase);
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

        public string GetSql()
        {
            string selectPart = "";
            string fromPart = "";
            string wherePart = "";
            string groupPart = "";
            string orderbyPart = "";

            selectPart = GetSelectPart();
            fromPart = GetFromPart(ref wherePart);
            wherePart += GetWherePart();
            groupPart = GetGroupPart();
            orderbyPart = GetOrderbyPart();

            orderbyPart = sqlAnalysis.GetOrderBy(orderbyPart);
            groupPart = sqlAnalysis.GetGroupBy(groupPart);

            wherePart = wherePart.Trim();
            groupPart = groupPart.Trim();
            orderbyPart = orderbyPart.Trim();

            string sql = "";
            if (0 < pageSize && 0 < pageNumber)
            {
                sql = sqlAnalysis.GetPageChange(selectPart, fromPart, wherePart, groupPart, orderbyPart, pageSize, pageNumber);
            }
            else if (0 < top)
            {
                sql = sqlAnalysis.GetTop(selectPart, fromPart, wherePart, groupPart, orderbyPart, top);
            }
            else
            {
                if(!string.IsNullOrEmpty(wherePart)) wherePart = " where " + wherePart;                
                sql = "select {0} from {1}{2}{3}{4}";
                sql = string.Format(sql, selectPart, fromPart, wherePart, groupPart, orderbyPart);
            }
            return sql;
        }
    }
}
