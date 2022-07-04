using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class SqlFromUnit
    {
        protected SqlFromUnit() { }

        public static SqlFromUnit New { get { return new SqlFromUnit(); } }

        public static SqlFromUnit Me { get { return new SqlFromUnit(); } }

        public static SqlFromUnit Instance { get { return new SqlFromUnit(); } }

        public SqlFromUnit From<T>(T dataModel) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>() where T : AbsDataModel
        {
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, string alias) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            this.alias = alias;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>(string alias) where T : AbsDataModel
        {
            this.alias = alias;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, string alias, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>(string alias, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, string alias, Func<T, bool> funcCondition, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            return this;
        }

        public SqlFromUnit From<T>(string alias, Func<T, bool> funcCondition, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, Func<T, bool> funcCondition) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            modelType = typeof(T);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            return this;
        }

        public SqlFromUnit From<T>(Func<T, bool> funcCondition) where T : AbsDataModel
        {
            modelType = typeof(T);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            return this;
        }

        public AbsDataModel dataModel { get; set; }
        public string alias { get; set; }
        public ConditionItem[] conditions { get; set; }
        public Type modelType { get; set; }
        public Func<AbsDataModel, bool> funcCondition { get; set; }
    }
}
