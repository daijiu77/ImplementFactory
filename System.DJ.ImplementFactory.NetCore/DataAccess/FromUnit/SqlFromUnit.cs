using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class SqlFromUnit
    {
        private string objErr = "'dataModel' must be a subclass of AbsDataModel.";
        private string typeErr = "Type 'modelType' must be a subclass of AbsDataModel.";
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

        public SqlFromUnit From(object dataModel)
        {
            if (null == (dataModel as AbsDataModel))
            {
                throw new Exception(objErr);
            }
            this.dataModel = (AbsDataModel)dataModel;
            modelType = dataModel.GetType();
            return this;
        }

        public SqlFromUnit From<T>() where T : AbsDataModel
        {
            modelType = typeof(T);
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            return this;
        }

        public SqlFromUnit From(Type modelType)
        {
            if (!typeof(AbsDataModel).IsAssignableFrom(modelType))
            {
                throw new Exception(typeErr);
            }
            this.modelType = modelType;
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, string alias) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            this.alias = alias;
            modelType = typeof(T);
            return this;
        }

        public SqlFromUnit From(object dataModel, string alias)
        {
            if (null == (dataModel as AbsDataModel))
            {
                throw new Exception(objErr);
            }
            this.dataModel = (AbsDataModel)dataModel;
            modelType = dataModel.GetType();
            this.alias = alias;
            return this;
        }

        public SqlFromUnit From<T>(string alias) where T : AbsDataModel
        {
            this.alias = alias;
            modelType = typeof(T);
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            return this;
        }

        public SqlFromUnit From(Type modelType, string alias)
        {
            if (!typeof(AbsDataModel).IsAssignableFrom(modelType))
            {
                throw new Exception(typeErr);
            }
            this.modelType = modelType;
            this.alias = alias;
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            return this;
        }

        public SqlFromUnit From<T>(T dataModel, string alias, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.dataModel = dataModel;
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From(object dataModel, string alias, params ConditionItem[] conditions)
        {
            if (null == (dataModel as AbsDataModel))
            {
                throw new Exception(objErr);
            }
            this.dataModel = (AbsDataModel)dataModel;
            this.alias = alias;
            this.conditions = conditions;
            modelType = dataModel.GetType();
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From<T>(string alias, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From(Type modelType, string alias, params ConditionItem[] conditions)
        {
            if (!typeof(AbsDataModel).IsAssignableFrom(modelType))
            {
                throw new Exception(typeErr);
            }
            this.alias = alias;
            this.conditions = conditions;
            this.modelType = modelType;
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            InitConditionItem(modelType, alias, conditions);
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
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From(object dataModel, string alias, Func<object, bool> funcCondition, params ConditionItem[] conditions)
        {
            if (null == (dataModel as AbsDataModel))
            {
                throw new Exception(objErr);
            }
            this.dataModel = (AbsDataModel)dataModel;
            this.alias = alias;
            this.conditions = conditions;
            modelType = dataModel.GetType();
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition(dm); };
            }
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From<T>(string alias, Func<T, bool> funcCondition, params ConditionItem[] conditions) where T : AbsDataModel
        {
            this.alias = alias;
            this.conditions = conditions;
            modelType = typeof(T);
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            InitConditionItem(modelType, alias, conditions);
            return this;
        }

        public SqlFromUnit From(Type modelType, string alias, Func<object, bool> funcCondition, params ConditionItem[] conditions)
        {
            if (!typeof(AbsDataModel).IsAssignableFrom(modelType))
            {
                throw new Exception(typeErr);
            }
            this.alias = alias;
            this.conditions = conditions;
            this.modelType = modelType;
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition(dm); };
            }
            InitConditionItem(modelType, alias, conditions);
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

        public SqlFromUnit From(object dataModel, Func<object, bool> funcCondition)
        {
            if (null == (dataModel as AbsDataModel))
            {
                throw new Exception(objErr);
            }
            this.dataModel = (AbsDataModel)dataModel;
            modelType = dataModel.GetType();
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition(dm); };
            }
            return this;
        }

        public SqlFromUnit From<T>(Func<T, bool> funcCondition) where T : AbsDataModel
        {
            modelType = typeof(T);
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition((T)dm); };
            }
            return this;
        }

        public SqlFromUnit From(Type modelType, Func<object, bool> funcCondition)
        {
            if (!typeof(AbsDataModel).IsAssignableFrom(modelType))
            {
                throw new Exception(typeErr);
            }
            this.modelType = modelType;
            this.dataModel = (AbsDataModel)Activator.CreateInstance(modelType);
            if (null != funcCondition)
            {
                this.funcCondition = dm => { return funcCondition(dm); };
            }
            return this;
        }

        private void InitConditionItem(Type mType, string alias, ConditionItem[] conditions)
        {
            if (string.IsNullOrEmpty(alias)) return;
            if (null == conditions) return;
            Dictionary<string, string> kvDic = new Dictionary<string, string>();
            mType.ForeachProperty((pi, pt, fn) =>
            {
                kvDic[fn.ToLower()] = fn;
            });
            string s = "^(" + alias + @")\.(([a-z0-9_]+)|([\[\`""][a-z0-9_]+)[\]\`""])";
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            foreach (ConditionItem item in conditions)
            {
                if (!kvDic.ContainsKey(item.FieldName.ToLower())) continue;
                if (rg.IsMatch(item.FieldName)) continue;
                item.FieldName = alias + "." + item.FieldName;
            }
        }

        public AbsDataModel dataModel { get; set; }
        public string alias { get; set; }
        public ConditionItem[] conditions { get; set; }
        public Type modelType { get; set; }
        public Func<AbsDataModel, bool> funcCondition { get; set; }
    }
}
