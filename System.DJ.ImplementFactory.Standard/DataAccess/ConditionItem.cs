using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public enum ConditionRelation
    {
        Equals,
        /// <summary>
        /// 不等于
        /// </summary>
        NoEquals,
        /// <summary>
        /// 大于
        /// </summary>
        Greader,
        /// <summary>
        /// 大于或等于
        /// </summary>
        GreaderOrEquals,
        /// <summary>
        /// 小于
        /// </summary>
        Less,
        /// <summary>
        /// 小于或等于
        /// </summary>
        LessOrEquals,
        /// <summary>
        /// 包含
        /// </summary>
        Contain,
        /// <summary>
        /// 左边包含
        /// </summary>
        LeftContain,
        /// <summary>
        /// 右边包含
        /// </summary>
        RightContain,
        /// <summary>
        /// 
        /// </summary>
        In
    }

    public class ConditionItem
    {
        protected ConditionItem() { }

        public static ConditionItem Me
        {
            get { return new ConditionItem(); }
        }

        public static ConditionItem Instance
        {
            get { return new ConditionItem(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="relation"></param>
        /// <param name="fieldValue">当为 数组 或 List 时采用in 例: and fieldName in (v1,v2)</param>
        /// <returns></returns>
        public ConditionItem And(string fieldName, ConditionRelation relation, object fieldValue)
        {
            IsOr = false;
            FieldName = fieldName;
            Relation = relation;
            FieldValue = fieldValue;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="relation"></param>
        /// <param name="dbSqlBody">例 and fieldName=(select top 1 userName from UserInfo)</param>
        /// <returns></returns>
        public ConditionItem AndUnit(string fieldName, ConditionRelation relation, DbSqlBody dbSqlBody)
        {
            IsOr = false;
            FieldName = fieldName;
            Relation = relation;
            this.dbSqlBody = dbSqlBody;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionItems">例 and (fieldName1=fieldValue1 or fieldName2=fieldValue2 or fieldName3=fieldValue3)</param>
        /// <returns></returns>
        public ConditionItem And(params ConditionItem[] conditionItems)
        {
            IsOr = false;
            this.conditionItems = conditionItems;
            return this;
        }

        public ConditionItem Or(string fieldName, ConditionRelation relation, object fieldValue)
        {
            IsOr = true;
            FieldName = fieldName;
            Relation = relation;
            FieldValue = fieldValue;
            return this;
        }

        public ConditionItem OrUnit(string fieldName, ConditionRelation relation, DbSqlBody dbSqlBody)
        {
            IsOr = true;
            FieldName = fieldName;
            Relation = relation;
            this.dbSqlBody = dbSqlBody;
            return this;
        }

        public ConditionItem Or(params ConditionItem[] conditionItems)
        {
            IsOr = true;
            this.conditionItems = conditionItems;
            return this;
        }

        public bool IsOr { get; set; }
        public string FieldName { get; set; }
        public ConditionRelation Relation { get; set; }
        public object FieldValue { get; set; }
        public ConditionItem[] conditionItems { get; set; }
        public DbSqlBody dbSqlBody { get; set; }

    }
}
