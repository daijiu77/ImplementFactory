using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbSqlBody : DbVisitor
    {
        private List<ConditionItem> conditionItems = new List<ConditionItem>();
        private Dictionary<string, object> dicPara = new Dictionary<string, object>();

        protected const string EnDH = "{#$}";

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

        protected OrderbyList<OrderbyItem> orderbyItems = new OrderbyList<OrderbyItem>();
        public OrderbyList<OrderbyItem> OrderbyItemList { get { return orderbyItems; } }

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

        private Dictionary<string, OrderbyList<OrderbyItem>> lazyOrderbyDic = new Dictionary<string, OrderbyList<OrderbyItem>>();
        public Dictionary<string, OrderbyList<OrderbyItem>> LazyOrderbyDictionary { get { return lazyOrderbyDic; } }

        /// <summary>
        /// This method sets the properties of the current object, the rules for sorting of the object (when a lazy load is performed)
        /// </summary>
        /// <param name="fieldName">The current object property name</param>
        /// <param name="orderbyItems">Rules for sorting</param>
        /// <returns></returns>
        public DbSqlBody OrderbyLazy(string fieldName, params OrderbyItem[] orderbyItems)
        {
            if (string.IsNullOrEmpty(fieldName)) return this;
            if (null == orderbyItems) return this;
            string fn = fieldName.ToLower().Trim();
            OrderbyList<OrderbyItem> list = null;
            lazyOrderbyDic.TryGetValue(fn, out list);
            if (null == list)
            {
                list = new OrderbyList<OrderbyItem>();
                lazyOrderbyDic[fn] = list;
            }

            foreach (var item in orderbyItems)
            {
                list.Add(item);
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

        private Dictionary<Type, FieldItemList<FieldItem>> selectFieldDic = new Dictionary<Type, FieldItemList<FieldItem>>();
        public DbSqlBody Select<T>(params FieldItem[] selectFields)
        {
            if (null == selectFields) selectFields = new FieldItem[] { };

            Type mType = typeof(T);
            FieldItemList<FieldItem> fields = null;
            selectFieldDic.TryGetValue(mType, out fields);
            if (null == fields)
            {
                fields = new FieldItemList<FieldItem>();
                selectFieldDic.Add(mType, fields);
            }

            Type cmType = null;
            foreach (var item in fromUnits)
            {
                if (mType.Equals(item.modelType))
                {
                    cmType = item.modelType;
                    break;
                }
            }

            if (0 == selectFields.Length)
            {
                if (null == cmType)
                {
                    mType.ForeachProperty((pi, pt, fn) =>
                    {
                        fields.Add(new FieldItem()
                        {
                            NameOrSelectFrom = fn
                        });
                    });
                }
                else
                {
                    fields.Add(new FieldItem()
                    {
                        NameOrSelectFrom = "*"
                    });
                }

                return this;
            }

            SetSelect(selectFields, fields);
            return this;
        }

        public DbSqlBody Select(params FieldItem[] selectFields)
        {
            if (null == selectFields) return this;
            FieldItemList<FieldItem> fields = null;
            Type modeType = fromUnits[0].modelType;
            selectFieldDic.TryGetValue(modeType, out fields);
            if (null == fields)
            {
                fields = new FieldItemList<FieldItem>();
                selectFieldDic[modeType] = fields;
            }
            SetSelect(selectFields, fields);
            return this;
        }

        private void SetSelect(FieldItem[] selectFields, FieldItemList<FieldItem> fields)
        {
            string fName = "";
            foreach (FieldItem item in selectFields)
            {
                if (null == item) continue;
                if (null == item.NameOrSelectFrom) continue;
                if (null == (item.NameOrSelectFrom as DbSqlBody))
                {
                    fName = item.NameOrSelectFrom.ToString().Trim();
                    if (string.IsNullOrEmpty(fName)) continue;
                    item.NameOrSelectFrom = fName;
                }
                fields.Add(item);
            }
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

        public DbSqlBody SetParameter(string parameterName, object parameterValue)
        {
            if (dicPara.ContainsKey(parameterName)) dicPara.Remove(parameterName);
            dicPara.Add(parameterName, parameterValue);
            return this;
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

        private List<string> whereIgnoreConditions = new List<string>();
        public List<string> WhereIgnoreList { get { return whereIgnoreConditions; } }
        /// <summary>
        /// Automatically generate a where condition (including properties with a Condition identifier) by setting the ignore property by this method
        /// </summary>
        /// <param name="fieldNames">Field names</param>
        public DbSqlBody WhereIgnore(params string[] fieldNames)
        {
            if (null == fieldNames) return this;
            foreach (var item in fieldNames)
            {
                whereIgnoreConditions.Add(item);
            }
            return this;
        }

        protected Dictionary<string, List<string>> lazyIgnoreDic = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> LazyIgonreDictionary { get { return lazyIgnoreDic; } }

        /// <summary>
        /// Properties that ignore child objects when lazy loading is set by this method to generate a where condition (including properties with condition identifiers)
        /// </summary>
        /// <param name="fieldName">The current object property name</param>
        /// <param name="childFields">A collection of child object property names</param>
        public DbSqlBody WhereIgnoreLazy(string fieldName, params string[] childFields)
        {
            if (null == fieldName || null == childFields) return this;
            fieldName = fieldName.Trim();
            if (string.IsNullOrEmpty(fieldName)) return this;
            if (0 == fieldName.Length) return this;
            if (0 == childFields.Length) return this;

            string fnLower = fieldName.ToLower();

            List<string> fields = null;
            lazyIgnoreDic.TryGetValue(fnLower, out fields);
            if (null == fields) fields = new List<string>();

            string fn = "";
            foreach (var item in childFields)
            {
                if (null == item) continue;
                fn = item.Trim().ToLower();
                if (string.IsNullOrEmpty(fn)) continue;
                if (fields.Contains(fn)) continue;
                fields.Add(fn);
            }
            if (0 == fields.Count) return this;
            lazyIgnoreDic[fnLower] = fields;
            return this;
        }

        protected bool whereIgnoreAll = false;
        public DbSqlBody WhereIgnoreAll()
        {
            whereIgnoreAll = true;
            return this;
        }

        public bool GetWhereIgnoreAll()
        {
            return whereIgnoreAll;
        }

        private string GetSelectPart()
        {
            string selectPart = "";
            string s = "";
            Attribute att = null;
            Regex rg = new Regex(@"[^a-z0-9_\s\.\*]", RegexOptions.IgnoreCase);
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
                            s += ", " + sqlAnalysis.GetFieldName(((FieldMapping)att).FieldName);
                        }
                        else
                        {
                            s += ", " + sqlAnalysis.GetFieldName(fn);
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
                else if (rg.IsMatch(s))
                {
                    selectPart += ", {0} {1}".ExtFormat(s, item.Key);
                }
                else
                {
                    selectPart += ", " + sqlAnalysis.GetFieldAlias(s, item.Key);
                }
            }

            if (0 < selectFieldDic.Count)
            {
                Dictionary<Type, string> aliasDic = new Dictionary<Type, string>();
                foreach (SqlFromUnit item in fromUnits)
                {
                    aliasDic[item.modelType] = item.alias;
                }

                string tbAlias = "";
                string fAlias = "";
                string fName = "";
                foreach (KeyValuePair<Type, FieldItemList<FieldItem>> item in selectFieldDic)
                {
                    tbAlias = "";
                    aliasDic.TryGetValue(item.Key, out tbAlias);
                    if (!string.IsNullOrEmpty(tbAlias)) tbAlias += ".";
                    foreach (FieldItem fItem in item.Value)
                    {
                        fAlias = "";
                        if (!string.IsNullOrEmpty(fItem.Alias)) fAlias = " " + fItem.Alias;
                        if (null != (fItem.NameOrSelectFrom as DbSqlBody))
                        {
                            fName = ((DbSqlBody)fItem.NameOrSelectFrom).GetSql();
                            if (!string.IsNullOrEmpty(fName))
                            {
                                tbAlias = "";
                                fName = "(" + fName + ")";
                            }
                        }
                        else
                        {
                            fName = fItem.NameOrSelectFrom.ToString();
                        }

                        if (!string.IsNullOrEmpty(fName))
                        {
                            if (rg.IsMatch(fName))
                            {
                                selectPart += ", {0}{1}".ExtFormat(fName, fAlias);
                            }
                            else if (-1 == fName.IndexOf("."))
                            {
                                selectPart += ", {0}{1}{2}".ExtFormat(tbAlias, fName, fAlias);
                            }
                            else
                            {
                                selectPart += ", {0}{1}".ExtFormat(fName, fAlias);
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(selectPart)) selectPart = selectPart.Substring(1);
            selectPart = selectPart.Trim();
            if (string.IsNullOrEmpty(selectPart))
            {
                string alias = fromUnits[0].alias;
                if (!string.IsNullOrEmpty(alias)) alias += ".";
                selectPart = alias + "*";
            }
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

        private string GetConditionUnit(ConditionItem[] conditions, Dictionary<string, object> fieldDic)
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
                        s = GetConditionUnit(item.conditionItems, fieldDic);
                        s = s.Trim();
                        if (string.IsNullOrEmpty(s)) continue;
                        s = rg.Match(s).Groups["wherePart"].Value;
                        s = "(" + s + ")";
                        cdt += cnts + s;
                        continue;
                    }
                }
                if (null != (item.FieldValue as ICollection))
                {
                    s = sqlAnalysis.GetConditionOfCollection(item.FieldName, item.Relation, (ICollection)item.FieldValue);
                    if (!string.IsNullOrEmpty(s)) cdt += cnts + s;
                }
                else if (null != (item.FieldValue as DbSqlBody))
                {
                    sql = ((DbSqlBody)item.FieldValue).GetSql();
                    s = sqlAnalysis.GetConditionOfDbSqlBody(item.FieldName, item.Relation, sql);
                    if (!string.IsNullOrEmpty(s)) cdt += cnts + s;
                }
                else
                {
                    fv = GetValueByType(item);
                    s = sqlAnalysis.GetConditionOfBaseValue(item.FieldName, item.Relation, fv);
                    if (!string.IsNullOrEmpty(s)) cdt += cnts + s;
                    if (null != fieldDic)
                    {
                        if (fieldDic.ContainsKey(item.FieldName))
                        {
                            fieldDic.Add(item.FieldName, fv);
                        }
                    }
                }
            }
            return cdt;
        }

        private string GetWhereByProperty(DbSqlBody body, Dictionary<string, object> fieldDic)
        {
            string sw = "";
            if (null == body) return sw;
            if (null == body.fromUnits) return sw;
            if (0 == body.fromUnits.Count) return sw;
            string s = "";
            string alias = "";
            string fn = "";
            object dataMode = null;
            const string startStr = "";
            bool isSqlBody = false;
            foreach (SqlFromUnit item in body.fromUnits)
            {
                isSqlBody = false;
                if (null != item.dataModel)
                {
                    isSqlBody = null != (item.dataModel as DbSqlBody);
                }

                s = "";
                if (isSqlBody)
                {
                    s = GetWhereByProperty((DbSqlBody)item.dataModel, fieldDic);
                }
                else
                {
                    alias = item.alias;
                    if (null == alias) alias = "";
                    if (!string.IsNullOrEmpty(alias)) alias += ".";
                    dataMode = null;
                    if (null != item.dataModel) dataMode = item.dataModel;
                    if (null == dataMode)
                    {
                        try
                        {
                            dataMode = Activator.CreateInstance(item.modelType);
                        }
                        catch (Exception)
                        {
                            continue;
                            //throw;
                        }
                    }

                    if (!whereIgnoreAll)
                    {
                        s = dataMode.GetWhere(startStr, true, (propertyInfoExt, condition) =>
                        {
                            fn = alias + propertyInfoExt.Name;
                            if (fieldDic.ContainsKey(fn)
                            || fieldDic.ContainsKey(propertyInfoExt.Name)
                            || fieldDic.ContainsKey(fn.ToLower())
                            || fieldDic.ContainsKey(propertyInfoExt.Name.ToLower())) return false;
                            return true;
                        });
                    }

                    initWhereAlias(alias, ref s);
                    if (!string.IsNullOrEmpty(startStr))
                    {
                        if (0 == s.IndexOf(startStr))
                        {
                            s = s.Substring(startStr.Length).Trim();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(s)) sw += " " + s;
            }
            return sw;
        }

        private void initWhereAlias(string alias, ref string whereStr)
        {
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(whereStr)) return;
            whereStr = whereStr.Trim();
            Regex rg = new Regex(@"((^((and)|(or)))|(\s((and)|(or))))\s+(?<FieldName>[a-z0-9_]+)[\s\=\<\>\!]", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(whereStr)) return;
            MatchCollection mc = rg.Matches(whereStr);
            string FieldName = "";
            string fn = "";
            string s = "";
            alias = alias.Trim();
            if (!alias.Substring(alias.Length - 1).Equals(".")) alias += ".";
            foreach (Match item in mc)
            {
                s = item.Groups[0].Value;
                FieldName = item.Groups["FieldName"].Value;
                fn = alias + sqlAnalysis.GetLegalName(FieldName);
                s = s.Replace(FieldName, fn);
                whereStr = whereStr.Replace(item.Groups[0].Value, s);
            }
        }

        private string GetFromPart(ref string wherePart, Dictionary<string, object> fieldDic)
        {
            string fromPart = "";
            string s = "";
            string s1 = "";
            string ConditionBody = "";
            bool mbool = false;
            bool isSqlBody = false;
            Regex rg = new Regex(@"^\s+((or)|(and))\s+(?<ConditionBody>.+)", RegexOptions.IgnoreCase);
            Attribute att = null;

            foreach (SqlFromUnit item in fromUnits)
            {
                isSqlBody = false;
                mbool = false;
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
                        wherePart += GetConditionUnit(item.conditions, fieldDic);
                    }
                }
                else
                {
                    att = item.modelType.GetCustomAttribute(typeof(TableAttribute));
                    if (null != att)
                    {
                        s = sqlAnalysis.GetTableName(((TableAttribute)att).Name);
                    }
                    else
                    {
                        s = sqlAnalysis.GetTableName(item.modelType.Name);
                    }

                    //if ((null != (item as LeftJoin) || null != (item as RightJoin) || null != (item as InnerJoin)) && false == isFirst)
                    if ((null != (item as LeftJoin) || null != (item as RightJoin) || null != (item as InnerJoin)))
                    {
                        mbool = true;
                        ConditionBody = "";
                        if (null != item.conditions)
                        {
                            s1 = GetConditionUnit(item.conditions, fieldDic);
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
                        wherePart += GetConditionUnit(item.conditions, fieldDic);
                    }

                    if (!mbool)
                    {
                        s = sqlAnalysis.GetTableAilas(s, item.alias);
                    }
                }

                if (mbool)
                {
                    /*Left join/Right join/Inner join*/
                    fromPart += " " + s.Trim();
                }
                else
                {
                    fromPart += ", " + s;
                }
            }

            rg = new Regex(@"^\,\s", RegexOptions.IgnoreCase);
            if (rg.IsMatch(fromPart))
            {
                fromPart = fromPart.Substring(1);
            }
            fromPart = fromPart.Trim();
            return fromPart;
        }

        private string GetWherePart(string wherePart, Dictionary<string, object> fieldDic)
        {
            wherePart += GetConditionUnit(conditionItems.ToArray(), fieldDic);

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
                groupPart += ", " + sqlAnalysis.GetFieldName(item);
            }
            if (!string.IsNullOrEmpty(groupPart)) groupPart = groupPart.Substring(1);
            return groupPart;
        }

        private void InitWhereIgnore(Dictionary<string, object> dic)
        {
            string key = "";
            foreach (var item in whereIgnoreConditions)
            {
                if (string.IsNullOrEmpty(item)) continue;
                key = item.Trim();
                if (string.IsNullOrEmpty(item)) continue;
                dic[key.ToLower()] = key;
            }
        }

        private string GetOrderbyPart()
        {
            string orderbyPart = "";
            string OrderBy_Item = "";
            foreach (var item in orderbyItems)
            {
                OrderBy_Item = sqlAnalysis.GetOrderByItem(item.FieldName, item.Rule);
                if (null != OrderBy_Item) OrderBy_Item = OrderBy_Item.Trim();
                if (!string.IsNullOrEmpty(OrderBy_Item)) orderbyPart += ", " + OrderBy_Item;
            }
            if (!string.IsNullOrEmpty(orderbyPart)) orderbyPart = orderbyPart.Substring(1).Trim();
            return orderbyPart;
        }

        private void GetPropertyOfModel(AbsDataModel dataModel,
            string wherePart,
            Action<AbsDataModel, string, string> fromUnitAction,
            Action<string, object, Constraint, PropertyInfo> propertyAction,
            Action propEndAction, IgnoreField.IgnoreType ignoreType)
        {
            Attribute att = null;
            string tbName = "";
            string field = "";

            Func<Type, string> funcTbName = (_type) =>
            {
                if (null == _type) return "";
                Attribute _att = _type.GetCustomAttribute(typeof(TableAttribute));
                string _tbName = "";
                if (null != _att)
                {
                    _tbName = ((TableAttribute)_att).Name;
                }
                else
                {
                    _tbName = _type.Name;
                }
                return _tbName;
            };

            Action<object, string, object, string> actionSetVal = (_pObj, _pFn, _cObj, _cFn) =>
               {
                   PropertyInfo _pi = _pObj.find(_pFn);
                   object _pv = null;
                   if (null != _pi)
                   {
                       _pv = _pi.GetValue(_pObj);
                       if (null == _pv) return;
                       _pi = _cObj.find(_cFn);
                       if (null == _pi) return;
                       _pv = DJTools.ConvertTo(_pv, _pi.PropertyType);
                       if (null == _pv) return;
                       try
                       {
                           _pi.SetValue(_cObj, _pv);
                       }
                       catch (Exception)
                       {
                           try
                           {
                               _cObj.SetPropertyValue(_pi.Name, _pv);
                           }
                           catch (Exception)
                           {

                               //throw;
                           }
                           //throw;
                       }
                   }
               };

            Func<object, object, Constraint, string, string> funcItemWhere = (_pObj, _cObj, _constraint, _tName) =>
             {
                 string _tn = funcTbName(_cObj.GetType());
                 if (string.IsNullOrEmpty(_tn)) return "";
                 string _ws = "";
                 if (string.IsNullOrEmpty(wherePart))
                 {
                     _ws = _tName + "." + _constraint.ForeignKey + "=" + _tn + "." + _constraint.RefrenceKey;
                 }
                 else
                 {
                     _ws = wherePart + " and " + _tName + "." + _constraint.ForeignKey + "=" + _tn + "." + _constraint.RefrenceKey;
                 }
                 actionSetVal(_pObj, _constraint.ForeignKey, _cObj, _constraint.RefrenceKey);

                 if (null != _constraint.Foreign_refrenceKeys)
                 {
                     int _len = _constraint.Foreign_refrenceKeys.Length / 2;
                     if (0 < _len)
                     {
                         _len *= 2;
                         int _n = 0, _index = 0;
                         string _pn = "";
                         foreach (var _fr in _constraint.Foreign_refrenceKeys)
                         {
                             if (0 == _n)
                             {
                                 _n = 1;
                                 _ws += " and " + _tName + "." + _fr;
                                 _pn = _fr;
                             }
                             else
                             {
                                 _n = 0;
                                 _ws += "=" + _tn + "." + _fr;
                                 actionSetVal(_pObj, _pn, _cObj, _fr);
                             }
                             _index++;
                             if (_index == _len) break;
                         }
                     }
                 }
                 return _ws;
             };
            if (null != (dataModel as IEntityCopy))
            {
                tbName = funcTbName(((IEntityCopy)dataModel).CopyParentModelType);
            }
            else
            {
                tbName = funcTbName(dataModel.GetType());
            }
            fromUnitAction(dataModel, tbName, wherePart);
            if (null == propertyAction) return;
            Constraint constraint = null;
            Attribute attrIF = null;
            bool isCopy = false;
            if (null != (dataModel as IEntityCopy))
            {
                isCopy = true;
            }

            dataModel.ForeachProperty((pi, pt, fn) =>
            {
                if (isCopy)
                {
                    return pt.IsBaseType();
                }
                return true;
            }, (pi, type, fn, fv) =>
            {
                if (null == fv) return;
                object v2 = fv;
                if (typeof(byte[]) == type)
                {
                    v2 = (byte[])fv;
                }
                else if (!DJTools.IsBaseType(type))
                {
                    Type eleType = null;
                    if (typeof(IList).IsAssignableFrom(type))
                    {
                        Type[] types = type.GetGenericArguments();
                        if (0 < types.Length)
                        {
                            eleType = types[0];
                            if (!eleType.IsBaseType()) eleType = null;
                        }
                    }
                    else if (type.IsArray)
                    {
                        eleType = type.GetTypeForArrayElement();
                        if (null != eleType)
                        {
                            if (!eleType.IsBaseType()) eleType = null;
                        }
                    }

                    string ws = "";
                    if (null != eleType)
                    {
                        IEnumerable collect = (IEnumerable)fv;
                        string v = null;
                        foreach (var item in collect)
                        {
                            if (null == item) continue;
                            v = item.ToString().Replace(",", EnDH);
                            ws += "," + v;
                        }
                        if (!string.IsNullOrEmpty(ws)) ws = ws.Substring(1);
                        fv = ws;
                    }
                    else
                    {
                        att = dataModel.GetType().GetCustomAttribute(typeof(Constraint));
                        if (null == att) return;
                        constraint = (Constraint)att;

                        if (typeof(IEnumerable).IsAssignableFrom(type))
                        {
                            IEnumerable collect = (IEnumerable)fv;
                            foreach (var item in collect)
                            {
                                if (null == (item as AbsDataModel)) break;
                                ws = funcItemWhere(dataModel, item, constraint, tbName);
                                if (string.IsNullOrEmpty(ws)) break;
                                GetPropertyOfModel((AbsDataModel)item, ws, fromUnitAction, propertyAction, propEndAction, ignoreType);
                            }
                        }
                        else if (typeof(AbsDataModel).IsAssignableFrom(type))
                        {
                            ws = funcItemWhere(dataModel, fv, constraint, tbName);
                            if (string.IsNullOrEmpty(ws)) return;
                            GetPropertyOfModel((AbsDataModel)fv, ws, fromUnitAction, propertyAction, propEndAction, ignoreType);
                        }
                        return;
                    }
                }
                else if (type.IsEnum)
                {
                    v2 = (int)fv;
                }
                field = fn.ToLower();
                attrIF = pi.GetCustomAttribute(typeof(IgnoreField), true);

                if (0 < dicContains.Count)
                {
                    if (!dicContains.ContainsKey(field)) field = "";
                    attrIF = null;
                }
                if (string.IsNullOrEmpty(field)) return;

                if (0 < dicExcludes.Count)
                {
                    if (dicExcludes.ContainsKey(field)) field = "";
                }
                if (string.IsNullOrEmpty(field)) return;

                if (null != attrIF)
                {
                    if (ignoreType == (((IgnoreField)attrIF).ignoreType & ignoreType)) field = "";
                }
                if (string.IsNullOrEmpty(field)) return;

                propertyAction(fn, v2, constraint, pi);
            });
            if (null != propEndAction) propEndAction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUnitAction">tableName, where</param>
        /// <param name="propertyAction">fieldName, fieldValue</param>
        /// <param name="propEndAction">属性结束</param>
        private void CreateDataOpt(Dictionary<string, object> fieldDic,
            Action<AbsDataModel, string, string> fromUnitAction,
            Action<string, object, Constraint, PropertyInfo> propertyAction,
            Action propEndAction)
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = trace.GetFrame(1);
            MethodBase mb = stackFrame.GetMethod();
            string srcMethodName = mb.Name.ToLower();
            IgnoreField.IgnoreType dataOptType = IgnoreField.IgnoreType.none;
            if (-1 != srcMethodName.IndexOf("insert"))
            {
                dataOptType = IgnoreField.IgnoreType.Insert;
            }
            else if (-1 != srcMethodName.IndexOf("update"))
            {
                dataOptType = IgnoreField.IgnoreType.Update;
            }

            string wherePart = "";
            Regex rg = new Regex(@"^\s+((or)|(and))\s+(?<ConditionBody>.+)", RegexOptions.IgnoreCase);
            foreach (SqlFromUnit item in fromUnits)
            {
                if (IgnoreField.IgnoreType.none == dataOptType)
                {
                    if (null == item.dataModel && null != item.modelType)
                    {
                        item.dataModel = (AbsDataModel)Activator.CreateInstance(item.modelType);
                    }
                }
                if (null == item.dataModel) continue;
                if (null != (item.dataModel as DbSqlBody)) continue;

                wherePart = "";
                if (null != item.conditions)
                {
                    wherePart = GetConditionUnit(item.conditions, fieldDic);
                    if (rg.IsMatch(wherePart))
                    {
                        wherePart = rg.Match(wherePart).Groups["ConditionBody"].Value;
                    }
                }

                GetPropertyOfModel(item.dataModel, wherePart, fromUnitAction, propertyAction, propEndAction, dataOptType);
            }
        }

        private Dictionary<string, object> keyValUpdate = new Dictionary<string, object>();
        protected void SetAppendUpdate(Dictionary<string, object> keyValue)
        {
            keyValUpdate.Clear();
            foreach (var item in keyValue)
            {
                keyValUpdate.Add(item.Key, item.Value);
            }
        }

        private Dictionary<string, object> keyValInsert = new Dictionary<string, object>();
        protected void SetAppendInsert(Dictionary<string, object> keyValue)
        {
            keyValInsert.Clear();
            foreach (var item in keyValue)
            {
                keyValInsert.Add(item.Key, item.Value);
            }
        }

        protected List<SqlDataItem> GetUpdate()
        {
            sqlAnalysis.AliasDic = aliasDic;

            List<SqlDataItem> list = new List<SqlDataItem>();
            SqlDataItem dataItem = null;
            DbParameter para = null;
            FieldMapping fm = null;
            string dbTag = DJTools.GetParaTagByDbDialect(Commons.DbAdapter.dbDialect);
            Dictionary<string, DbParameter> dic = new Dictionary<string, DbParameter>();
            string sql = "";
            string sets = "";
            string where = "";

            Action<string, object> kvAction = (_fn, _fv) =>
            {
                if (dic.ContainsKey(_fn.ToLower()))
                {
                    para = dic[_fn.ToLower()];
                    dataItem.parameters.Remove(para);
                    para = ImplementAdapter.dataServerProvider.CreateDbParameter(_fn, _fv);
                    dataItem.parameters.Add(para);
                    return;
                }
                sets += ", " + sqlAnalysis.GetFieldName(_fn) + "=" + dbTag + _fn;
                para = ImplementAdapter.dataServerProvider.CreateDbParameter(_fn, _fv);
                dataItem.parameters.Add(para);
                dic.Add(_fn.ToLower(), para);
            };

            Dictionary<string, object> fieldDic = null;
            CreateDataOpt(fieldDic, (dataModel, tb, whereStr) =>
            {
                dataItem = new SqlDataItem()
                {
                    model = dataModel
                };
                sql = "update " + sqlAnalysis.GetTableName(tb) + " set ";
                where = whereStr;
                sets = "";
            }, (fn, fv, constraint, pi) =>
            {
                fm = pi.GetCustomAttribute(typeof(FieldMapping)) as FieldMapping;
                if (null != fm)
                {
                    if (!string.IsNullOrEmpty(fm.DefualtValue)) return;
                }
                kvAction(fn, fv);
            }, () =>
            {
                foreach (var item in keyValUpdate)
                {
                    kvAction(item.Key, item.Value);
                }
                keyValUpdate.Clear();

                if (!string.IsNullOrEmpty(sets))
                {
                    sets = sets.Substring(2);
                    where = GetWherePart(where, fieldDic);
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
            FieldMapping fm = null;
            Dictionary<string, DbParameter> dic = new Dictionary<string, DbParameter>();
            string dbTag = DJTools.GetParaTagByDbDialect(Commons.DbAdapter.dbDialect);
            string tableName = null;
            string sql = "";
            string fields = "";
            string vals = "";
            object dFv = null;
            List<string> pks = new List<string>();

            Action<string, object, PropertyInfo> kvAction = (_fn, _fv, _pi) =>
            {
                if (dic.ContainsKey(_fn.ToLower()))
                {
                    para = dic[_fn.ToLower()];
                    dataItem.parameters.Remove(para);
                    para = ImplementAdapter.dataServerProvider.CreateDbParameter(_fn, _fv);
                    dataItem.parameters.Add(para);
                    return;
                }

                fields += ", " + sqlAnalysis.GetFieldName(_fn);
                vals += ", " + dbTag + _fn;
                para = ImplementAdapter.dataServerProvider.CreateDbParameter(_fn, _fv);
                dataItem.parameters.Add(para);
                dic.Add(_fn.ToLower(), para);
            };

            Dictionary<string, object> fieldDic = null;
            CreateDataOpt(fieldDic, (dataModel, tb, whereStr) =>
            {
                dataItem = new SqlDataItem()
                {
                    model = dataModel
                };
                tableName = tb;
                sql = "insert into " + sqlAnalysis.GetTableName(tb) + "({0}) values({1})";
                fields = "";
                vals = "";
            }, (fn, fv, constraint, pi) =>
            {
                fm = pi.GetCustomAttribute(typeof(FieldMapping)) as FieldMapping;
                if (null != fm)
                {
                    if (fm.IsPrimaryKey)
                    {
                        if (!string.IsNullOrEmpty(fm.FieldName))
                        {
                            pks.Add(fm.FieldName);
                        }
                        else
                        {
                            pks.Add(fn);
                        }
                    }
                    if (!sqlAnalysis.IsLegalCaseDefaultValueWhenInsert(tableName, fv, pi, fm, ref dFv)) return;
                    fv = null == dFv ? fv : dFv;
                }
                kvAction(fn, fv, pi);
            }, () =>
            {
                PropertyInfo ppi = null;
                foreach (var item in keyValInsert)
                {
                    ppi = dataItem.model.GetPropertyInfo(item.Key);
                    kvAction(item.Key, item.Value, ppi);
                }
                keyValInsert.Clear();

                if (!string.IsNullOrEmpty(fields))
                {
                    fields = fields.Substring(2);
                    vals = vals.Substring(2);
                    sql = sql.ExtFormat(fields, vals);
                    //dataItem.sql = sqlAnalysis.GetPrimaryKeyValueScheme(sql, pks);
                    dataItem.sql = sql;
                    list.Add(dataItem);
                }
            });
            return list;
        }

        protected List<SqlDataItem> GetDelete()
        {
            sqlAnalysis.AliasDic = aliasDic;
            List<SqlDataItem> list = new List<SqlDataItem>();
            string sql = "";
            string whereStr = "";
            Dictionary<string, object> fieldDic = null;
            CreateDataOpt(fieldDic, (dataModel, tb, where) =>
            {
                sql = "delete from " + sqlAnalysis.GetTableName(tb);
                whereStr = GetWherePart(where, fieldDic);
                if (!string.IsNullOrEmpty(whereStr)) sql += " where " + whereStr;
                list.Add(new SqlDataItem()
                {
                    sql = sql,
                    model = dataModel
                });
            }, null, null);
            return list;
        }

        protected string GetCountSql()
        {
            sqlAnalysis.AliasDic = aliasDic;
            string wherePart = "";

            Dictionary<string, object> fieldDic = new Dictionary<string, object>();
            InitWhereIgnore(fieldDic);
            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart, fieldDic);
            wherePart = GetWherePart(wherePart, fieldDic);
            wherePart += GetWhereByProperty(this, fieldDic);
            string groupPart = GetGroupPart();

            string sql = sqlAnalysis.GetCount(fromPart, wherePart, groupPart);
            return sql;
        }

        protected void initOrderby(Action<OrderbyList<OrderbyItem>> action)
        {
            if (0 < orderbyItems.Count) return;
            if (db_dialect.sqlserver != DbAdapter.dbDialect) return;
            string keyName = "";
            string id = "";
            string dateName = "";
            string tbAlias = fromUnits[0].alias;
            if (null == tbAlias) tbAlias = "";
            tbAlias = tbAlias.Trim();
            if (!string.IsNullOrEmpty(tbAlias)) tbAlias += ".";
            FieldMapping fm = null;
            fromUnits[0].modelType.ForeachProperty((pi, pt, fn) =>
            {
                if (fn.ToLower().Equals("id")) id = fn;
                if (typeof(DateTime) == pt) dateName = fn;
                fm = pi.GetCustomAttribute<FieldMapping>();
                if (null == fm) return;
                if (fm.IsPrimaryKey)
                {
                    keyName = fn;
                }
            });

            if (!string.IsNullOrEmpty(keyName))
            {
                orderbyItems.Add(OrderbyItem.Me.Set(tbAlias + keyName, OrderByRule.Asc));
                if (null != action) action(orderbyItems);
            }
            else if (!string.IsNullOrEmpty(dateName))
            {
                orderbyItems.Add(OrderbyItem.Me.Set(tbAlias + dateName, OrderByRule.Asc));
                if (null != action) action(orderbyItems);
            }
            else if (!string.IsNullOrEmpty(id))
            {
                orderbyItems.Add(OrderbyItem.Me.Set(tbAlias + id, OrderByRule.Asc));
                if (null != action) action(orderbyItems);
            }
        }

        protected void initOrderby()
        {
            initOrderby((orderbyItems) => { });
        }

        protected string PageSizeSignOfSql { get; set; }
        protected string StartQuantitySignOfSql { get; set; }

        private string GetTop(int start, int top)
        {
            sqlAnalysis.AliasDic = aliasDic;
            string wherePart = "";
            Dictionary<string, object> fieldDic = new Dictionary<string, object>();
            InitWhereIgnore(fieldDic);

            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart, fieldDic);
            wherePart = GetWherePart(wherePart, fieldDic);
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
            sqlAnalysis.AliasDic = aliasDic;
            string wherePart = "";
            Dictionary<string, object> fieldDic = new Dictionary<string, object>();
            InitWhereIgnore(fieldDic);

            string selectPart = GetSelectPart();
            string fromPart = GetFromPart(ref wherePart, fieldDic);

            wherePart = GetWherePart(wherePart, fieldDic);
            wherePart += GetWhereByProperty(this, fieldDic);

            string groupPart = GetGroupPart();
            string orderbyPart = GetOrderbyPart();

            orderbyPart = sqlAnalysis.GetOrderBy(orderbyPart);
            groupPart = sqlAnalysis.GetGroupBy(groupPart);

            wherePart = wherePart.Trim();
            groupPart = groupPart.Trim();
            orderbyPart = orderbyPart.Trim();

            string sql = "";
            if (0 < pageSize)
            {
                if (1 > pageNumber) throw new Exception("The starting value of the parameter PageNumber is 1, " +
                    "which must be greater than or equal to 1, that is: 1<=PageNumber.");
                initOrderby(orderbyItems =>
                {
                    orderbyPart = GetOrderbyPart();
                    orderbyPart = sqlAnalysis.GetOrderBy(orderbyPart);
                    orderbyPart = orderbyPart.Trim();
                });
                sql = sqlAnalysis.GetPageChange(selectPart, fromPart, wherePart, groupPart, orderbyPart, pageSize, pageNumber);
                PageSizeSignOfSql = sqlAnalysis.PageSizeSignOfSql;
                StartQuantitySignOfSql = sqlAnalysis.StartQuantitySignOfSql;
            }
            else if (0 < top)
            {
                sql = sqlAnalysis.GetTop(selectPart, fromPart, wherePart, groupPart, orderbyPart, top);
                PageSizeSignOfSql = sqlAnalysis.PageSizeSignOfSql;
                StartQuantitySignOfSql = sqlAnalysis.StartQuantitySignOfSql;
            }
            else
            {
                if (null != (sqlAnalysis as AbsSqlAnalysis))
                {
                    AbsSqlAnalysis absAnalysis = (AbsSqlAnalysis)sqlAnalysis;
                    wherePart = absAnalysis.GetWhere(wherePart);
                }
                else if (!string.IsNullOrEmpty(wherePart))
                {
                    if (0 != wherePart.ToLower().IndexOf("where "))
                    {
                        wherePart = " where " + wherePart;
                    }
                }
                if (!string.IsNullOrEmpty(groupPart)) groupPart = " " + groupPart;
                if (!string.IsNullOrEmpty(orderbyPart)) orderbyPart = " " + orderbyPart;
                sql = "select {0} from {1}{2}{3}{4}";
                sql = string.Format(sql, selectPart, fromPart, wherePart, groupPart, orderbyPart);
            }
            return sql;
        }

        public bool IsDeleteRelation { get; set; }
    }
}
