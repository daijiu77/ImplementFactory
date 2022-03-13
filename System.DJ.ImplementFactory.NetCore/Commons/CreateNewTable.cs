using System;
using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.NetCore.Commons
{
    class CreateNewTable
    {
        private AutoCall autoCall = null;
        private DbInfo dbInfo = null;
        private BasicExecForSQL basicExecForSQL = null;
        private MethodInformation mi = new MethodInformation();
        private IDbHelper dbHelper = null;
        public CreateNewTable(AutoCall autoCall, DbInfo dbInfo, BasicExecForSQL basicExecForSQL, IDbHelper dbHelper)
        {
            this.autoCall = autoCall;
            this.dbInfo = dbInfo;
            this.basicExecForSQL = basicExecForSQL;
            this.dbHelper = dbHelper;
        }

        private int getRecordCount(string tableName)
        {
            int ncount = 0;
            string sql = "select count(*) ncount from {0};";
            sql = string.Format(sql, tableName);
            string err = "";
            basicExecForSQL.Exec(autoCall, sql, null, ref err, val =>
            {
                ncount = Convert.ToInt32(val);
            }, cmd =>
            {
                DataTable dt = null;
                DataSet ds = new DataSet();
                Data.Common.DataAdapter da = dbHelper.dataServerProvider.CreateDataAdapter(cmd);
                da.Fill(ds);
                if (0 < ds.Tables.Count) dt = ds.Tables[0];
                int num = 0;
                if (null == dt) dt = new DataTable();
                if (0 < dt.Rows.Count)
                {
                    num = Convert.ToInt32(dt.Rows[0][0]);
                }
                return num;
            });
            return ncount;
        }

        private string getTableNameWithSql(string sql)
        {
            Regex rg = new Regex(@"(^insert\s+(into\s+)?(?<tbName>[a-z0-9_\-]+))|(\sinsert\s+(into\s+)?(?<tbName>[a-z0-9_\-]+))", RegexOptions.IgnoreCase);
            string tbName = rg.Match(sql).Groups["tbName"].Value;
            return tbName;
        }

        private string getTableNameByRule(string tableName)
        {
            string tbn = dbInfo.splitTable.Rule;
            tbn = tbn.Replace("$", tableName);

            Action<Regex, string> action = (_rg, _s) =>
              {
                  string Num = "";
                  int n = 0;
                  int len = 0;
                  if (_rg.IsMatch(tbn))
                  {
                      Match m = _rg.Match(tbn);
                      Num = m.Groups["Num"].Value;
                      n = Convert.ToInt32(Num);
                      
                      len = _s.Length;
                      n = len - n;
                      if (0 > n) n = 0;
                      _s = _s.Substring(n);
                      tbn = tbn.Replace(m.Groups[0].Value, _s);
                  }
              };

            Regex rg = new Regex(@"\{(?<Num>[1-8])\}\#", RegexOptions.IgnoreCase);
            string s = DateTime.Now.ToString("yyyyMMdd");
            action(rg, s);

            rg = new Regex(@"\#\{(?<Num>[1-8])\}", RegexOptions.IgnoreCase);
            s = DateTime.Now.ToString("HHmmss");
            action(rg, s);

            s = DateTime.Now.ToString("yyyyMMddHHmmss");
            tbn = tbn.Replace("#", s);

            return tbn;
        }

        /// <summary>
        /// 修改表名
        /// </summary>
        /// <param name="sql"></param>
        private void exec_sql(string sql)
        {
            string err = "";
            basicExecForSQL.Exec(autoCall, sql, null, ref err, val =>
                {
                    //
                }, cmd =>
             {
                 return cmd.ExecuteNonQuery();
             });
        }

        private DataTable dt_exec_sql(string sql)
        {
            DataTable dt = null;
            string err = "";
            basicExecForSQL.Exec(autoCall, sql, null, ref err, val =>
            {
                //
            }, cmd =>
            {
                DataSet ds = new DataSet();
                Data.Common.DataAdapter da = dbHelper.dataServerProvider.CreateDataAdapter(cmd);
                da.Fill(ds);
                if (0 < ds.Tables.Count) dt = ds.Tables[0];
                return dt;
            });
            return dt;
        }

        private string replaceBracket(string s)
        {
            s = s.Replace("(", "");
            s = s.Replace(")", "");
            return s;
        }

        private string getfield(DataRow dr)
        {
            string field = dr["field_name"].ToString();
            string ftype = dr["field_type"].ToString();
            string fLen = dr["field_length"].ToString();
            string dotLen = dr["dot_length"].ToString();
            string defVal = dr["default_value"].ToString();
            string is_null = dr["is_null"].ToString();
            string pk = dr["primary_key"].ToString();
            string ft = ftype.ToLower();
            if (-1 != ft.IndexOf("char"))
            {
                field += " " + ftype + "(" + fLen + ")";
                if (!string.IsNullOrEmpty(defVal))
                {
                    field += " default" + defVal;
                }
            }
            else if (-1 != ft.IndexOf("numeric")|| -1 != ft.IndexOf("decimal"))
            {
                field += " " + ftype + "(" + fLen + ", "+ dotLen + ")";
                if (!string.IsNullOrEmpty(defVal))
                {
                    defVal = replaceBracket(defVal);
                    field += " default(" + defVal+")";
                }
            }
            else if (-1 != ft.IndexOf("int"))
            {
                field += " " + ftype;
                if (!string.IsNullOrEmpty(defVal))
                {
                    defVal = replaceBracket(defVal);
                    field += " default(" + defVal + ")";
                }
            }
            else
            {
                field += " " + ftype;
                if (!string.IsNullOrEmpty(defVal))
                {
                    field += " default" + defVal;
                }
            }

            if (!string.IsNullOrEmpty(pk))
            {
                field += " primary key";
            }

            if (is_null.Equals("0"))
            {
                field += " not null";
            }
            else
            {
                field += " null";
            }
            return field;
        }

        private string getSqlStructure(DataTable dt, string tableName)
        {
            string sql = "";
            if (null == dt) return sql;
            mi.append(ref sql, LeftSpaceLevel.one, "create table {0}", tableName);
            mi.append(ref sql, "(");
            string s = "";
            int n = 0;
            foreach (DataRow item in dt.Rows)
            {
                if (0 != n) sql += ",";
                s = getfield(item);
                mi.append(ref sql, s);
                n++;
            }
            mi.append(ref sql, ")");
            return sql;
        }

        private string getSqlOfMSToCreateTable(string tableName)
        {
            string sql = "SELECT a.colorder field_order,a.name field_name,";
            mi.append(ref sql, "(case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then 'identity'else '' end) sign_name,");
            mi.append(ref sql, "(case when (SELECT count(*) FROM sysobjects");
            mi.append(ref sql, "WHERE (name in (SELECT name FROM sysindexes");
            mi.append(ref sql, "WHERE (id = a.id) AND (indid in ");
            mi.append(ref sql, "(SELECT indid FROM sysindexkeys");
            mi.append(ref sql, "WHERE (id = a.id) AND (colid in ");
            mi.append(ref sql, "(SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))");
            mi.append(ref sql, "AND (xtype = 'PK'))>0 then 'pk' else '' end) primary_key,");
            mi.append(ref sql, "b.name field_type, a.length byte_length,");
            mi.append(ref sql, "COLUMNPROPERTY(a.id,a.name,'PRECISION') as field_length,");
            mi.append(ref sql, "isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0) as dot_length,");
            mi.append(ref sql, "(case when a.isnullable=1 then '1'else '0' end) is_null,");
            mi.append(ref sql, "isnull(e.text,'') default_value,");
            mi.append(ref sql, "isnull(g.[value], ' ') AS descr ");
            mi.append(ref sql, "FROM  syscolumns a ");
            mi.append(ref sql, "left join systypes b on a.xtype=b.xusertype ");
            mi.append(ref sql, "inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties' ");
            mi.append(ref sql, "left join syscomments e on a.cdefault=e.id ");
            mi.append(ref sql, "left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id");
            mi.append(ref sql, "left join sys.extended_properties f on d.id=f.class and f.minor_id=0");
            mi.append(ref sql, LeftSpaceLevel.one, "WHERE d.name='{0}'", tableName);
            mi.append(ref sql, "order by a.colorder");

            DataTable dt = dt_exec_sql(sql);
            sql = "";
            if (null == dt) return sql;
            sql = getSqlStructure(dt, tableName);
            return sql;
        }

        private void sql_server(string sql)
        {
            string tbName = getTableNameWithSql(sql);
            int records = getRecordCount(tbName);
            if (records < dbInfo.splitTable.RecordQuantity) return;

            string sql0 = getSqlOfMSToCreateTable(tbName);
            string newTbName = getTableNameByRule(tbName);
            string sql1 = "exec sp_rename '{0}', '{1}';";
            sql1 = string.Format(sql1, tbName, newTbName);
            exec_sql(sql1);
            exec_sql(sql0);
        }

        private string getSqlOfMysqlToCreateTable(string tableName)
        {
            string sql = "SELECT COLUMN_NAME AS 'field_name',";
            mi.append(ref sql, "ORDINAL_POSITION AS 'field_order',");
            mi.append(ref sql, "COLUMN_DEFAULT AS 'default_value',");
            mi.append(ref sql, "(case UPPER(IS_NULLABLE) when 'NO' then 0 else 1 end) AS 'is_null',");
            mi.append(ref sql, "DATA_TYPE AS 'field_type',");
            mi.append(ref sql, "CHARACTER_MAXIMUM_LENGTH AS 'field_length',");
            mi.append(ref sql, "NUMERIC_PRECISION,"); //数值精度(最大位数)
            mi.append(ref sql, "NUMERIC_SCALE AS 'dot_length',");
            mi.append(ref sql, "COLUMN_TYPE AS 'columnType',");
            mi.append(ref sql, "(case UPPER(COLUMN_KEY) when 'PRI' then 'pk' else '' END) As 'primary_key',");
            mi.append(ref sql, "EXTRA AS 'descr',");
            mi.append(ref sql, "COLUMN_COMMENT AS 'annotation'");
            mi.append(ref sql, LeftSpaceLevel.one, "FROM information_schema.`COLUMNS` WHERE TABLE_NAME = '{0}'", tableName);
            mi.append(ref sql, "ORDER BY ORDINAL_POSITION;");

            DataTable dt = dt_exec_sql(sql);
            sql = "";
            if (null == dt) return sql;
            sql = getSqlStructure(dt, tableName);
            mi.append(ref sql, " default character set = 'utf8';");
            return sql;
        }

        private void mysql(string sql)
        {
            string tbName = getTableNameWithSql(sql);
            int records = getRecordCount(tbName);
            if (records < dbInfo.splitTable.RecordQuantity) return;

            string sql0 = "CREATE TABLE {0} LIKE {1};"; //getSqlOfMysqlToCreateTable(tbName)
            string newTbName = getTableNameByRule(tbName);

            string sql1 = "ALTER TABLE {0} RENAME TO {1};";
            sql1 = string.Format(sql1, tbName, newTbName); //把原表名更改为新表名
            exec_sql(sql1);

            sql0 = string.Format(sql0, tbName, newTbName); //根据新表名来创建原表名
            exec_sql(sql0);
        }

        private string getSqlOfOracleToCreateTable(string tableName)
        {
            string sql = "";
            return sql;
        }

        private void oracle(string sql)
        {
            string tbName = getTableNameWithSql(sql);
            int records = getRecordCount(tbName);
            if (records < dbInfo.splitTable.RecordQuantity) return;

            string sql0 = "CREATE TABLE {0} AS SELECT * FROM {1} WHERE ROWNUM=0;"; // getSqlOfOracleToCreateTable(tbName);
            string newTbName = getTableNameByRule(tbName);

            string sql1 = "ALTER TABLE {0} RENAME TO {1};";
            sql1 = string.Format(sql1, tbName, newTbName); //把原表名改为新表名
            exec_sql(sql1);

            sql0 = string.Format(sql0, tbName, newTbName); //根据新表名创建原名
            exec_sql(sql0);
        }

        public void SplitTable(string sql)
        {
            string dbType = dbInfo.DatabaseType.ToLower();
            if (dbType.Equals("sqlserver"))
            {
                sql_server(sql);
            }
            else if (dbType.Equals("mysql"))
            {
                mysql(sql);
            }
            else if (dbType.Equals("oracle"))
            {
                oracle(sql);
            }
        }
    }
}
