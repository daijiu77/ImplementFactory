using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Test.NetCore
{
    public class DbHelperImpl : IDbHelper
    {
        int IDbHelper.ExecuteSql(string sql, IEnumerable<DbParameter> dbs, ref string err)
        {
            int num = 0;
            return num;
            //throw new NotImplementedException();
        }
    }
}
