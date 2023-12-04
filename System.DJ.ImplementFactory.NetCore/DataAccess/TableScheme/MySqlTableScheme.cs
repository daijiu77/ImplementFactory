using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.DataAccess.SqlAnalysisImpl;

namespace System.DJ.ImplementFactory.DataAccess.TableScheme
{
    public class MySqlTableScheme : AbsTableScheme, IDbTableScheme
    {
        private ISqlAnalysis sqlAnalysis = new MySqlAnalysis();

        ISqlAnalysis IDbTableScheme.sqlAnalysis => sqlAnalysis;

        private string getFieldScheme(FieldMapping fieldMapping)
        {
            string sql = sqlAnalysis.GetFieldName(fieldMapping.FieldName);
            sql += " " + getFieldType(fieldMapping);

            //if (!string.IsNullOrEmpty(fieldMapping.DefualtValue))
            //{
            //    if (typeof(string) == fieldMapping.FieldType
            //        || typeof(Guid) == fieldMapping.FieldType
            //        || typeof(Guid?) == fieldMapping.FieldType
            //        || typeof(DateTime) == fieldMapping.FieldType
            //        || typeof(DateTime?) == fieldMapping.FieldType)
            //    {
            //        sql += " default '{0}'";
            //    }
            //    else
            //    {
            //        sql += " default {0}";
            //    }
            //    sql = sql.ExtFormat(fieldMapping.DefualtValue);
            //}

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
            string sql = "alter table {0} add column";
            tableName = sqlAnalysis.GetTableName(tableName);
            sql = sql.ExtFormat(tableName);
            if (0 >= fieldMapping.Length) fieldMapping.Length = 100;
            sql += " " + getFieldScheme(fieldMapping);

            if (fieldMapping.IsPrimaryKey)
            {
                sql += ",alter table {0} add primary key({1});";
                string fn = sqlAnalysis.GetFieldName(fieldMapping.FieldName);
                sql = sql.ExtFormat(tableName, fn);
            }
            else
            {
                sql += ";";
            }
            return sql;
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
            tableName = sqlAnalysis.GetTableName(tableName);
            string sql = "create table " + tableName;
            DJTools.append(ref sql, "(");
            string field = "";
            string fn = "";
            int n = 0;
            int size = fieldMappings.Count - 1;
            foreach (FieldMapping item in fieldMappings)
            {
                field = getFieldScheme(item);
                if (n < size)
                {
                    DJTools.append(ref sql, field + ",");
                    if (item.IsPrimaryKey)
                    {
                        fn = sqlAnalysis.GetFieldName(item.FieldName);
                        DJTools.append(ref sql, "primary key({0}),", fn);
                    }
                }
                else
                {
                    if (item.IsPrimaryKey)
                    {
                        DJTools.append(ref sql, field + ",");
                        fn = sqlAnalysis.GetFieldName(item.FieldName);
                        DJTools.append(ref sql, "primary key({0})", fn);
                    }
                    else
                    {
                        DJTools.append(ref sql, field);
                    }
                }
                n++;
            }
            DJTools.append(ref sql, ") character set utf8;");
            return sql;
        }
    }
}
