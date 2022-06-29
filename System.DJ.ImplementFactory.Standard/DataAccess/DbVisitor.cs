using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbVisitor : ImplementAdapter, IDisposable
    {
        public static ISqlAnalysis sqlAnalysis = null;

        protected List<SqlFromUnit> fromUnits = new List<SqlFromUnit>();
        public IDbSqlScheme CreateSqlFrom(params SqlFromUnit[] sqlFromUnits)
        {
            DbSqlScheme dbSqlStructure = new DbSqlScheme();
            if (null != sqlFromUnits)
            {
                foreach (var item in sqlFromUnits)
                {
                    dbSqlStructure.fromUnits.Add(item);
                }
            }
            return dbSqlStructure;
        }

        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
