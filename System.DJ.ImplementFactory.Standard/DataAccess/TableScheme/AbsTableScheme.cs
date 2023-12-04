using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;

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

        protected string getFieldType(FieldMapping fieldMapping, Func<Type, string> func)
        {
            string ft = "";
            if (null == fieldMapping.FieldType) return ft;
            if (0 >= fieldMapping.Length) fieldMapping.Length = 100;

            if (null == func) func = (tp) => { return null; };
            string ft1 = func(fieldMapping.FieldType);
            if (!string.IsNullOrEmpty(ft1)) return ft1;

            if (typeof(string) == fieldMapping.FieldType)
            {
                ft = "varchar({0})";
                ft = ft.ExtFormat(fieldMapping.Length.ToString());
            }
            else if (typeof(Guid) == fieldMapping.FieldType || typeof(Guid?) == fieldMapping.FieldType)
            {
                ft = "varchar(50)";
            }
            else if (typeof(float) == fieldMapping.FieldType || typeof(float?) == fieldMapping.FieldType)
            {
                ft = "float";
            }
            else if (typeof(decimal) == fieldMapping.FieldType || typeof(decimal?) == fieldMapping.FieldType)
            {
                ft = "decimal(18, {0})";
                ft = ft.ExtFormat(fieldMapping.Length.ToString());
            }
            else if (typeof(bool) == fieldMapping.FieldType || typeof(bool?) == fieldMapping.FieldType)
            {
                ft = "bit";
            }
            else if (typeof(DateTime) == fieldMapping.FieldType || typeof(DateTime?) == fieldMapping.FieldType)
            {
                ft = "datetime";
            }
            else if (typeof(int) == fieldMapping.FieldType || typeof(int?) == fieldMapping.FieldType || fieldMapping.FieldType.IsEnum)
            {
                ft = "int";
            }
            else if (typeof(Int64) == fieldMapping.FieldType
                 || typeof(Int64?) == fieldMapping.FieldType
                  || typeof(long) == fieldMapping.FieldType
                   || typeof(long?) == fieldMapping.FieldType)
            {
                ft = "bigint";
            }
            else if (typeof(double) == fieldMapping.FieldType || typeof(double?) == fieldMapping.FieldType)
            {
                ft = "money";
            }
            else if (typeof(byte[]) == fieldMapping.FieldType)
            {
                ft = "varbinary({0})";
                ft = ft.ExtFormat(fieldMapping.Length.ToString());
            }
            return ft;
        }

        protected string getFieldType(FieldMapping fieldMapping)
        {
            return getFieldType(fieldMapping, ft => { return null; });
        }
    }
}
