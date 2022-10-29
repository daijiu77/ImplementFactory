using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace Test.NetCore
{
    public class DbHelperImpl : IDbHelper, ISingleInstance
    {
        object ISingleInstance.Instance { get; set; }
        string IDbHelper.ConStr { get; set; }

        int IDbHelper.ExecuteSql(string sql, IEnumerable<DbParameter> dbs, ref string err)
        {
            int num = 0;
            return num;
            //throw new NotImplementedException();
        }
    }
}
