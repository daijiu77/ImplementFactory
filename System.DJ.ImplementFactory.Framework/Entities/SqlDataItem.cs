using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Entities
{
    public class SqlDataItem
    {
        public string sql { get; set; }
        private List<DbParameter> paras = new List<DbParameter>();
        public IList<DbParameter> parameters { get { return paras; } }
        public object model { get; set; }
    }
}
