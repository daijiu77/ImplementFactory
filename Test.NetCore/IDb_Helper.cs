﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Test.NetCore
{
    public interface IDb_Helper
    {
        string ConStr { get; set; }
        int ExecuteSql(string sql, IEnumerable<DbParameter> dbs, ref string err);
    }
}
