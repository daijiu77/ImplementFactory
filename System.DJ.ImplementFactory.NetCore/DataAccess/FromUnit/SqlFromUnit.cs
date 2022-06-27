using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class SqlFromUnit
    {
        private SqlFromUnit() { }

        public SqlFromUnit(AbsDataModel dataModel)
        {
            this.dateModel = dateModel;
        }
        public SqlFromUnit(AbsDataModel dataModel, string alias)
        {
            this.dateModel = dateModel;
            this.alias = alias;
        }
        public SqlFromUnit(AbsDataModel dataModel, string alias, string condition)
        {
            this.dateModel = dateModel;
            this.alias = alias;
            this.condition = condition;
        }
        public SqlFromUnit(AbsDataModel dataModel, string alias, string condition, Func<AbsDataModel, bool> funcCondition)
        {
            this.dateModel = dateModel;
            this.alias = alias;
            this.condition = condition;
            this.funcCondition = funcCondition;
        }
        public AbsDataModel dateModel { get; set; }
        public string alias { get; set; }
        public string condition { get; set; }
        public Func<AbsDataModel, bool> funcCondition { get; set; }
    }
}
