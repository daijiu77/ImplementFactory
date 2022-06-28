using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class SqlFromUnit
    {
        protected SqlFromUnit() { }

        public static SqlFromUnit Me
        {
            get { return new SqlFromUnit(); }
        }

        public static SqlFromUnit Instance
        {
            get { return new SqlFromUnit(); }
        }

        protected SqlFromUnit Set(AbsDataModel dataModel)
        {
            this.dateModel = dateModel;
            return this;
        }
        protected SqlFromUnit Set(AbsDataModel dataModel, string alias)
        {
            this.dateModel = dateModel;
            this.alias = alias;
            return this;
        }
        protected SqlFromUnit Set(AbsDataModel dataModel, string alias, params ConditionItem[] conditions)
        {
            this.dateModel = dateModel;
            this.alias = alias;
            this.conditions = conditions;
            return this;
        }
        protected SqlFromUnit Set(AbsDataModel dataModel, string alias, Func<AbsDataModel, bool> funcCondition, params ConditionItem[] conditions)
        {
            this.dateModel = dateModel;
            this.alias = alias;
            this.conditions = conditions;
            this.funcCondition = funcCondition;
            return this;
        }
        public AbsDataModel dateModel { get; set; }
        public string alias { get; set; }
        public ConditionItem[] conditions { get; set; }
        public Func<AbsDataModel, bool> funcCondition { get; set; }
    }
}
