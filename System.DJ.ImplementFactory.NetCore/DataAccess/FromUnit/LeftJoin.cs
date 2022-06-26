using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class LeftJoin : AbsFromUnit
    {
        public void From(IDataModel dataModel)
        {
            //
        }

        public void From<T>(T dataModel, Func<T, bool> condition) where T : IDataModel
        {
            //
        }

        public void From(IDataModel dataModel, string alias)
        {
            //
        }

        public void From(IDataModel dataModel, string alias, string condition)
        {
            //
        }

        public void From<T>(T dataModel, string alias, Func<T, bool> condition) where T : IDataModel
        {
            //
        }
    }
}
