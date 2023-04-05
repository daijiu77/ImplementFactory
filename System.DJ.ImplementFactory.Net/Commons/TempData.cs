using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    class TempData
    {
        private TempData() { }

        public static TempData Instance
        {
            get { return new TempData(); }
        }
        public IDbHelper dbHelper { get; set; }
        public AutoCall autoCall { get; set; }
        public string sql { get; set; }

        List<DbParameter> _paras = new List<DbParameter>();
        public List<DbParameter> parameters
        {
            get { return _paras; }
            set
            {
                _paras.Clear();
                value.ForEach(e =>
                {
                    _paras.Add(e);
                });
            }
        }

        public Action<object> resultOfOpt { get; set; }

        public Func<DbCommand, object> dataOpt { get; set; }

    }
}
