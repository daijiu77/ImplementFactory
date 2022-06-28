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

        public void And(string fieldName, ConditionRelation relation, object fieldValue)
        {
            IsOr = false;
            FieldName = fieldName;
            Relation = relation;
            FieldValue = fieldValue;
        }

        public void AndUnit(string fieldName, ConditionRelation relation, DbBody dbBody)
        {
            IsOr = false;
            FieldName = fieldName;
            Relation = relation;
            this.dbBody = dbBody;
        }

        public void And(ConditionItem conditionItem)
        {
            IsOr = false;
            this.conditionItem = conditionItem;
        }

        public void Or(string fieldName, ConditionRelation relation, object fieldValue)
        {
            IsOr = true;
            FieldName = fieldName;
            Relation = relation;
            FieldValue = fieldValue;
        }

        public void OrUnit(string fieldName, ConditionRelation relation, DbBody dbBody)
        {
            IsOr = true;
            FieldName = fieldName;
            Relation = relation;
            this.dbBody = dbBody;
        }

        public void Or(ConditionItem conditionItem)
        {
            IsOr = true;
            this.conditionItem = conditionItem;
        }

        public bool IsOr { get; set; }
        public string FieldName { get; set; }
        public ConditionRelation Relation { get; set; }
        public object FieldValue { get; set; }
        public ConditionItem conditionItem { get; set; }
        public DbBody dbBody { get; set; }

    }
}
