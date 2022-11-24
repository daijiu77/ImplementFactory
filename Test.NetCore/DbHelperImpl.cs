using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace Test.NetCore
{
    public class DbHelperImpl : IDb_Helper, ISingleInstance
    {
        object ISingleInstance.Instance { get; set; }
        string IDb_Helper.ConStr { get; set; }

        int IDb_Helper.ExecuteSql(string sql, IEnumerable<DbParameter> dbs, ref string err)
        {
            int num = 0;
            return num;
            //throw new NotImplementedException();
        }
    }
}
