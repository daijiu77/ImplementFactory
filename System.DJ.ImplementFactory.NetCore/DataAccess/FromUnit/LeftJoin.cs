using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class LeftJoin
    {
        Func<AbsDataModel, bool> func1 = null;
        public static SqlFromUnit From(AbsDataModel dataModel)
        {
            return new SqlFromUnit(dataModel);
            //throw new NotImplementedException();
        }

        public static SqlFromUnit From(AbsDataModel dataModel, Func<AbsDataModel, bool> condition)
        {
            return new SqlFromUnit(dataModel, null, null, condition);
            //throw new NotImplementedException();
        }

        public static SqlFromUnit From(AbsDataModel dataModel, string alias)
        {
            return new SqlFromUnit(dataModel, alias);
            //throw new NotImplementedException();
        }

        public static SqlFromUnit From(AbsDataModel dataModel, string alias, string condition)
        {
            return new SqlFromUnit(dataModel, alias, condition);
            //throw new NotImplementedException();
        }

        public static SqlFromUnit From(AbsDataModel dataModel, string alias, string condition, Func<AbsDataModel, bool> funcCondition)
        {
            return new SqlFromUnit(dataModel, alias, condition, funcCondition);
            //throw new NotImplementedException();
        }

        public static SqlFromUnit From(AbsDataModel dataModel, string alias, Func<AbsDataModel, bool> condition)
        {
            return new SqlFromUnit(dataModel, alias, null, condition);
            //throw new NotImplementedException();
        }
    }
}
