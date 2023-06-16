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
        protected Dictionary<string, string> aliasDic = new Dictionary<string, string>();
        protected bool isUseConstraintLoad = false;
        public IDbSqlScheme CreateSqlFrom(bool isUseConstraintLoad, params SqlFromUnit[] sqlFromUnits)
        {
            startNumber = 0;
            DbSqlScheme dbSqlStructure = new DbSqlScheme();
            dbSqlStructure.isUseConstraintLoad = isUseConstraintLoad;
            if (null != sqlFromUnits)
            {
                string alias = "";
                foreach (var item in sqlFromUnits)
                {
                    dbSqlStructure.fromUnits.Add(item);
                    if (!string.IsNullOrEmpty(item.alias))
                    {
                        alias = item.alias.Trim();
                        if (!string.IsNullOrEmpty(alias))
                        {
                            aliasDic[alias.ToLower()] = alias;
                        }
                    }
                }
            }
            return dbSqlStructure;
        }

        public IDbSqlScheme CreateSqlFrom(params SqlFromUnit[] sqlFromUnits)
        {
            return CreateSqlFrom(true, sqlFromUnits);
        }

        void IDisposable.Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
