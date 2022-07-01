using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.AnalysisDataModel
{
    public class OverrideModel
    {
        public AbsDataModel CreateDataModel(AbsDataModel dataModel)
        {
            AbsDataModel dtModel = null;
            if (null == dataModel) return dtModel;
            dataModel.ForeachProperty((pi, type, fn, fv) =>
            {
                
            });
            return dtModel;
        }
    }
}
