using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl;

namespace System.DJ.ImplementFactory.DataAccess.TableScheme
{
    public class MSTableScheme : AbsTableScheme, IDbTableScheme
    {
        private ISqlAnalysis sqlAnalysis = new MSqlAnalysis();

        ISqlAnalysis IDbTableScheme.sqlAnalysis => sqlAnalysis;

        private string getFieldType(FieldMapping fieldMapping)
        {
            string ft = "";
            if (null == fieldMapping.FieldType) return ft;
            if (0 >= fieldMapping.Length) fieldMapping.Length = 100;
            if (typeof(string) == fieldMapping.FieldType)
            {
                ft = "varchar({0})";
                ft = ft.ExtFormat(fieldMapping.Length.ToString());
            }
            else if (typeof(Guid) == fieldMapping.FieldType || typeof(Guid?) == fieldMapping.FieldType)
            {
                ft = "uniqueidentifier";
            }
            else if (typeof(float) == fieldMapping.FieldType || typeof(float?) == fieldMapping.FieldType)
            {
                ft = "float";
            }
            else if (typeof(decimal) == fieldMapping.FieldType || typeof(decimal?) == fieldMapping.FieldType)
            {
                ft = "decimal(18, 0)";
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
                //ft = "varbinary({0})";
                //ft = ft.ExtFormat(fieldMapping.Length.ToString());
                ft = "image";
            }
            return ft;
        }

        private string getFieldScheme(FieldMapping fieldMapping)
        {
            string sql = sqlAnalysis.GetFieldName(fieldMapping.FieldName);
            sql += " " + getFieldType(fieldMapping);

            if (fieldMapping.IsPrimaryKey)
            {
                sql += " primary key";
            }

            if (!string.IsNullOrEmpty(fieldMapping.DefualtValue))
            {
                sql += " default({0})";
                sql = sql.ExtFormat(fieldMapping.DefualtValue);
            }

            if (fieldMapping.NoNull)
            {
                sql += " not null";
            }
            else
            {
                sql += " null";
            }
            return sql;
        }

        string IDbTableScheme.GetAddFieldScheme(string tableName, FieldMapping fieldMapping)
        {
            string sql = "alter table {0} add";
            tableName = sqlAnalysis.GetTableName(tableName);
            sql = string.Format(sql, tableName);
            if (0 >= fieldMapping.Length) fieldMapping.Length = 100;
            sql += " " + getFieldScheme(fieldMapping);
            return sql;
            //throw new NotImplementedException();
        }

        List<FieldInformation> IDbTableScheme.GetFields(string tableName)
        {
            string sql = "select COLUMN_NAME {1},DATA_TYPE {2},CHARACTER_MAXIMUM_LENGTH {3},IS_NULLABLE {4} from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='{0}';";
            sql = sql.ExtFormat(tableName, fName, fType, fLen, fIsNull);

            List<FieldInformation> list = GetFieldInfos(sql);
            return list;
        }

        string IDbTableScheme.GetTableScheme(string tableName, List<FieldMapping> fieldMappings)
        {
            if (null == fieldMappings) return "";
            if (0 == fieldMappings.Count) return "";
            string fn = sqlAnalysis.GetTableName(tableName);
            string sql = "create table " + fn;
            DJTools.append(ref sql, "(");
            string field = "";
            int n = 0;
            int size = fieldMappings.Count - 1;
            foreach (FieldMapping item in fieldMappings)
            {
                field = getFieldScheme(item);
                if (n < size)
                {
                    DJTools.append(ref sql, field + ",");
                }
                else
                {
                    DJTools.append(ref sql, field);
                }
                n++;
            }
            DJTools.append(ref sql, ")");
            return sql;
            //throw new NotImplementedException();
        }
    }
}
