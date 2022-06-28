using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.NetCore.DataAccess.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public class DbVisitor : AbsDataModel
    {
        public static ISqlAnalysis sqlAnalysis = null;

        protected List<SqlFromUnit> fromUnits = new List<SqlFromUnit>();
        public DbSqlStructure CreateSqlFrom(params SqlFromUnit[] sqlFromUnits)
        {
            DbSqlStructure dbSqlStructure = new DbSqlStructure();
            if (null != sqlFromUnits)
            {
                foreach (var item in sqlFromUnits)
                {
                    dbSqlStructure.AddSqlFromUnit(item);
                }
            }
            return dbSqlStructure;
        }

        protected void AddSqlFromUnit(SqlFromUnit sqlFromUnit)
        {
            if (null == sqlFromUnit) return;
            fromUnits.Add(sqlFromUnit);
        }

    }
}
