using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbVisitor : ImplementAdapter, IDisposable
    {
        public static ISqlAnalysis sqlAnalysis = null;

        protected int startNumber = 0;
        protected List<SqlFromUnit> fromUnits = new List<SqlFromUnit>();
        public IDbSqlScheme CreateSqlFrom(params SqlFromUnit[] sqlFromUnits)
        {
            startNumber = 0;
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
