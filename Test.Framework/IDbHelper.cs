using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Test.Framework
{
    public interface IDbHelper
    {
        int ExecuteSql(string sql, IEnumerable<DbParameter> dbs, ref string err);
    }
}
