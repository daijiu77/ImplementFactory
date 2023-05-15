using System;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.TableScheme
{
    public abstract class AbsTableScheme
    {
        protected const string fName = "FNAME";
        protected const string fType = "FTYPE";
        protected const string fLen = "FLEN";
        protected const string fIsNull = "FISNULL";

        protected AutoCall autoCall = new AutoCall();

        protected List<FieldInformation> GetFieldInfos(string sql)
        {
            IDbHelper dbHelper = ImplementAdapter.DbHelper;
            string err = "";
            DataTable dt = dbHelper.query(autoCall, sql, false, null, ref err);
            List<FieldInformation> list = new List<FieldInformation>();
            if (null == dt) return list;
            if (0 == dt.Rows.Count) return list;
            string fn = "";
            string ft = "";
            string f_len = "";
            int len = 0;
            string f_isNull = "";
            bool isNull = true;
            foreach (DataRow item in dt.Rows)
            {
                if (System.DBNull.Value == item[fName]) continue;
                fn = item[fName].ToString();
                ft = System.DBNull.Value == item[fType] ? "" : item[fType].ToString();
                f_len = System.DBNull.Value == item[fLen] ? "" : item[fLen].ToString();
                f_isNull = System.DBNull.Value == item[fIsNull] ? "yes" : item[fIsNull].ToString();
                f_isNull = f_isNull.Trim().ToLower();
                len = 0;
                int.TryParse(f_len, out len);
                isNull = f_isNull.Equals("yes");
                list.Add(new FieldInformation()
                {
                    Name = fn,
                    ValueType = ft,
                    Length = len,
                    IsNull = isNull,
                });
            }
            ImplementAdapter.Destroy(dbHelper);
            return list;
        }
    }
}
