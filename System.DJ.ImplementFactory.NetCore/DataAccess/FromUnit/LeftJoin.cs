namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class LeftJoin : SqlFromUnit
    {
        Func<AbsDataModel, bool> func1 = null;

        private LeftJoin() { }

        public static LeftJoin Me
        {
            get { return new LeftJoin(); }
        }

        public static LeftJoin Instance
        {
            get { return new LeftJoin(); }
        }

        public SqlFromUnit From(AbsDataModel dataModel)
        {
            return Set(dataModel);
            //throw new NotImplementedException();
        }

        public SqlFromUnit From(AbsDataModel dataModel, Func<AbsDataModel, bool> condition)
        {
            return Set(dataModel, null, condition);
            //throw new NotImplementedException();
        }

        public SqlFromUnit From(AbsDataModel dataModel, string alias)
        {
            return Set(dataModel, alias);
            //throw new NotImplementedException();
        }

        public SqlFromUnit From(AbsDataModel dataModel, string alias, params ConditionItem[] conditions)
        {
            return Set(dataModel, alias, conditions);
            //throw new NotImplementedException();
        }

        public SqlFromUnit From(AbsDataModel dataModel, string alias, Func<AbsDataModel, bool> funcCondition, params ConditionItem[] conditions)
        {
            return Set(dataModel, alias, funcCondition, conditions);
            //throw new NotImplementedException();
        }

        public SqlFromUnit From(AbsDataModel dataModel, string alias, Func<AbsDataModel, bool> condition)
        {
            return Set(dataModel, alias, condition);
            //throw new NotImplementedException();
        }
    }
}
