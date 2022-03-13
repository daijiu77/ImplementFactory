using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.NetCore.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class MultiTablesExec : IMultiTablesExec, IDisposable
    {
        /// <summary>
        /// key: tableName_lower, value: List<string>
        /// </summary>
        private static Dictionary<string, object> tbDic = new Dictionary<string, object>();
        private Dictionary<string, ThreadOpt> threadDic = new Dictionary<string, ThreadOpt>();
        private AutoCall autoCall = new AutoCall();
        private static DbInfo dbInfo = null;
        private static IDbHelper dbHelper = null;
        private DataTable queryDatas = new DataTable();
        private DbAdapter dbAdapter = DbAdapter.Instance;
        private CreateNewTable createNewTable = null;
        private int OptDatas = 0;

        private string leftStr = "|#";
        private string rightStr = "#|";

        public MultiTablesExec() { }

        public MultiTablesExec(IDbHelper dbHelper)
        {
            MultiTablesExec.dbHelper = dbHelper;
        }

        public MultiTablesExec(DbInfo dbInfo, IDbHelper dbHelper)
        {
            if (0 < tbDic.Count) return;
            
            MultiTablesExec.dbInfo = dbInfo;
            MultiTablesExec.dbHelper = dbHelper;

            string rule = getRule(dbInfo);
            string sql = "";
            if (dbInfo.DatabaseType.Equals("sqlserver"))
            {
                //select TABLE_NAME from {0}.dbo.sysobjects where type='U'
                sql = "select name as TABLE_NAME from sysobjects where type='U'";
            }
            else if (dbInfo.DatabaseType.Equals("sqlserver"))
            {
                //select TABLE_NAME from information_schema.tables where table_schema='{0}';
                sql = "select TABLE_NAME from information_schema.tables where LOWER(TABLE_SCHEMA)<>'information_schema' and LOWER(TABLE_SCHEMA)<>'mysql';";
            }
            else if (dbInfo.DatabaseType.Equals("sqlserver"))
            {
                //SELECT TABLE_NAME FROM all_tables WHERE OWNER='{0}'
                sql = "SELECT TABLE_NAME FROM all_tables";
                /*
             获取所有的表字段，类型，长度和注释

select distinct a.TABLE_NAME,a.COLumn_name,a.data_type,a.data_length,c.comments from all_tab_columns a
inner join all_tables b on a.TABLE_NAME=b.TABLE_NAME and a.OWNER=b.OWNER

inner join all_col_comments c on a.TABLE_NAME=c.TABLE_NAME and a.COLumn_name=c.COLumn_name
where b.OWNER=‘数据库名称‘ order by a.TABLE_NAME;
             */
            }

            initDictionary(rule, sql);
        }

        private void initBasicExecForSQL(DbAdapter dbAdapter, IDbHelper dbHelper)
        {
            dbAdapter.dbConnectionString = dbHelper.connectString;
            dbAdapter.dbConnectionState = dbHelper.dbConnectionState;
            dbAdapter.disposableAndClose = dbHelper.disposableAndClose;
            dbAdapter.dataServerProvider = dbHelper.dataServerProvider;
        }

        private string getRule(DbInfo dbInfo)
        {
            string rule = dbInfo.splitTable.Rule;
            string Field = "";
            Regex rg = new Regex(@"(?<Field>\{[0-9]\})\#");
            Match m = null;
            if (rg.IsMatch(rule))
            {
                m = rg.Match(rule);
                Field = m.Groups["Field"].Value;
                rule = rule.Replace(m.Groups[0].Value, "[0-9]" + Field);
            }

            rule = rule.Replace("#", "[0-9]");
            return rule;
        }

        private bool isSplitTable(string rule, string tableName, ref string srcTableName)
        {
            srcTableName = "";
            bool isEnable = false;
            string s = rule.Replace("$", "(?<srcTableName>[a-z_]+)");
            s = "^" + s;
            Regex rg = new Regex(s, RegexOptions.IgnoreCase);
            isEnable = rg.IsMatch(tableName);
            if (isEnable)
            {
                srcTableName = rg.Match(tableName).Groups["srcTableName"].Value;
            }
            return isEnable;
        }

        private DataTable GetDataTable(string sql)
        {
            DataTable dt = null;
            string err = "";

            initBasicExecForSQL(dbAdapter, dbHelper);
            dbAdapter.ExecSql(autoCall, sql, null, ref err, vObj =>
            {
                if (null == vObj) return;
                dt = vObj as DataTable;
            }, cmd =>
            {
                DataTable dt1 = new DataTable();
                try
                {
                    Data.Common.DataAdapter da = dbHelper.dataServerProvider.CreateDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (0 < ds.Tables.Count) dt1 = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    autoCall.e(ex.ToString(), ErrorLevels.severe);
                    //throw;
                }
                return dt1;
            });
            return dt;
        }

        private void initDictionary(string rule, string sql)
        {
            DataTable dt = GetDataTable(sql);

            if (null == dt) return;
            List<TableInfo> list = null;
            TableInfo tableInfo = null;
            string tbName = "";
            string tbn = "";
            string srcTableName = "";
            foreach (DataRow item in dt.Rows)
            {
                tbName = item["TABLE_NAME"].ToString();
                if (!isSplitTable(rule, tbName, ref srcTableName)) continue;
                tbn = srcTableName.ToLower();
                if (tbDic.ContainsKey(tbn))
                {
                    list = (List<TableInfo>)tbDic[tbn];
                }
                else
                {
                    list = new List<TableInfo>();
                    tableInfo = new TableInfo();
                    tableInfo.tbName = srcTableName;
                    //tableInfo.recordQuantity = RecordCount(srcTableName);
                    list.Add(tableInfo);
                    tbDic.Add(tbn, list);
                }
                tableInfo = new TableInfo();
                tableInfo.tbName = tbName;
                //tableInfo.recordQuantity = RecordCount(tbName);
                list.Add(tableInfo);
            }
        }

        object ISingleInstance.Instance { get; set; }

        private string[] getTableNamesWithSql(string sql, string leftStr, string rightStr, ref string new_sql)
        {
            //dic tableName key:Lower, value:self
            Dictionary<string, string> dic = new Dictionary<string, string>();            
            string s1 = "";
            string newSql = sql;
            string _sql = sql;
            foreach (KeyValuePair<string, object> item in tbDic)
            {
                s1 += @"|(\s" + item.Key + @"\s)";
            }
            s1 = s1.Substring(1);
            s1 = "(?<TbName>(" + s1 + "))";

            Regex rg1 = new Regex(s1, RegexOptions.IgnoreCase);
            Action<Regex> action = _rg =>
            {
                if (_rg.IsMatch(_sql))
                {
                    string s = null;
                    string ss = "";
                    string tb = "";
                    string tbn = "";
                    MatchCollection mc1 = _rg.Matches(_sql);
                    MatchCollection mc2 = null;
                    foreach (Match m1 in mc1)
                    {
                        s = m1.Groups[0].Value;
                        ss = s;
                        _sql = _sql.Replace(s, "");
                        if (!rg1.IsMatch(s)) continue;
                        mc2 = rg1.Matches(s);
                        foreach (Match m2 in mc2)
                        {
                            tb = m2.Groups["TbName"].Value.Trim();
                            tbn = tb.ToLower();
                            ss = ss.Replace(m2.Groups["TbName"].Value, " " + leftStr + tbn + rightStr + " ");
                            if (dic.ContainsKey(tbn)) continue;
                            dic.Add(tbn, tb);
                        }
                        newSql = newSql.Replace(s, ss);
                    }
                }
            };

            Regex rg2 = new Regex(@"from\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)).)+\s((where)|(group)|(order))\s", RegexOptions.IgnoreCase);
            action(rg2);

            rg2 = new Regex(@"from\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)(?!\()(?!\))).)+\)", RegexOptions.IgnoreCase);
            action(rg2);

            rg2 = new Regex(@"from\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)).)+", RegexOptions.IgnoreCase);
            action(rg2);

            new_sql = newSql;
            string[] arr = new string[dic.Count];
            int n = 0;
            foreach (var item in dic)
            {
                arr[n] = item.Key;
                n++;
            }
            return arr;
        }

        private List<string> getSqlByTables(string sql, string leftStr, string rightStr)
        {
            //newSql 带有替换标识符的sql语句,例: select * from |#UserInfo#| order by createdate desc;
            string newSql = "";
            string[] arr = getTableNamesWithSql(sql, leftStr, rightStr, ref newSql);

            List<string> sqlList = new List<string>();
            Action<string, string> action1 = (tbn1, tbn2) =>
            {
                string sql1 = "", sql2;
                string k1 = "";
                List<TableInfo> list1 = tbDic[tbn1] as List<TableInfo>;
                List<TableInfo> list2 = null;
                if (!string.IsNullOrEmpty(tbn2))
                {
                    list2 = tbDic[tbn2] as List<TableInfo>;
                }

                foreach (TableInfo item1 in list1)
                {
                    k1 = leftStr + tbn1 + rightStr;
                    sql1 = newSql.Replace(k1, item1.tbName);
                    if (null != list2)
                    {
                        foreach (TableInfo item2 in list2)
                        {
                            k1 = leftStr + tbn2 + rightStr;
                            sql2 = sql1.Replace(k1, item2.tbName);
                            sqlList.Add(sql2);
                        }
                    }
                    else
                    {
                        sqlList.Add(sql1);
                    }
                }
            };

            int nlen = arr.Length;
            string key1 = "";
            string key2 = "";
            for (int i = 0; i < nlen; i++)
            {
                key1 = arr[i];
                if ((i + 1) < nlen)
                {
                    for (int ii = i + 1; ii < nlen; ii++)
                    {
                        key2 = arr[ii];
                        action1(key1, key2);
                    }
                }
                else if (0 == i)
                {
                    action1(key1, null);
                }
            }
            return sqlList;
        }

        private void DataOpt(object autoCall, string sql, List<DbParameter> parameters, Action<object> resultAction, ref string err)
        {
            OptDatas = 0;
            threadDic.Clear();
            if (0 == tbDic.Count) return;

            List<string> sqlList = getSqlByTables(sql, leftStr, rightStr);

            ThreadOpt threadOpt = null;
            foreach (string item in sqlList)
            {
                threadOpt = new ThreadOpt(this);
                threadDic.Add(threadOpt.ID, threadOpt);
                threadOpt.oparete(autoCall, sql, parameters);
            }

            Task.Run(() =>
            {
                while (0 < threadDic.Count)
                {
                    Thread.Sleep(100);
                }
                resultAction(OptDatas);
            });
        }

        void IMultiTablesExec.Delete(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            DataOpt(autoCall, sql, parameters, action, ref err);
        }

        bool IMultiTablesExec.ExistMultiTables(string sql)
        {
            throw new NotImplementedException();
        }

        void IMultiTablesExec.Insert(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            initBasicExecForSQL(dbAdapter, dbHelper);
            if (null == createNewTable) createNewTable = new CreateNewTable(autoCall, dbInfo, dbAdapter, dbHelper);
            createNewTable.SplitTable(sql);
            if (false == string.IsNullOrEmpty(createNewTable.SrcTableName) 
                && false == string.IsNullOrEmpty(createNewTable.NewTableName))
            {
                string tb = createNewTable.SrcTableName.ToLower();
                List<TableInfo> list = null;
                if (tbDic.ContainsKey(tb))
                {
                    list = (List<TableInfo>)tbDic[tb];
                }
                else
                {
                    list = new List<TableInfo>();
                    list.Add(new TableInfo()
                    {
                        tbName = createNewTable.SrcTableName
                    });
                    tbDic.Add(tb, list);
                }
                list.Add(new TableInfo()
                {
                    tbName = createNewTable.NewTableName
                });
            }
            string err1 = "";
            dbAdapter.ExecSql((AutoCall)autoCall, sql, parameters, ref err1, val =>
                {
                    int n = Convert.ToInt32(val);
                    action(n);
                }, cmd =>
             {
                 return cmd.ExecuteNonQuery();
             });
            err = err1;
        }

        void IMultiTablesExec.Update(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            DataOpt(autoCall, sql, parameters, action, ref err);
        }

        void IMultiTablesExec.Query(AutoCall autoCall, string sql, List<DbParameter> parameters, ref string err, Action<object> action, Func<DbCommand, object> func)
        {
            queryDatas.Rows.Clear();
            queryDatas.Columns.Clear();
            threadDic.Clear();
            if (0 == tbDic.Count) return;

            List<string> sqlList = getSqlByTables(sql, leftStr, rightStr);

            ThreadOpt threadOpt = null;
            foreach (string item in sqlList)
            {
                threadOpt = new ThreadOpt(this);
                threadDic.Add(threadOpt.ID, threadOpt);
                threadOpt.query(autoCall, item, parameters);
            }

            while (0 < threadDic.Count)
            {
                Thread.Sleep(100);
            }
            action(queryDatas);
        }

        private object _QueryResult = new object();
        private void QueryResult(ThreadOpt threadOpt, object data)
        {
            lock (_QueryResult)
            {
                if (null != data)
                {
                    DataTable dt = data as DataTable;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (0 == queryDatas.Columns.Count)
                        {
                            foreach (DataColumn dc in dt.Columns)
                            {
                                queryDatas.Columns.Add(dc.ColumnName, dc.DataType);
                            }
                        }
                        DataRow dr = queryDatas.NewRow();
                        foreach (DataColumn dc in dt.Columns)
                        {
                            dr[dc.ColumnName] = item[dc.ColumnName];
                        }
                        queryDatas.Rows.Add(dr);
                    }
                }

                string id = threadOpt.ID;
                ((IDisposable)threadOpt).Dispose();
                threadDic.Remove(id);
            }
        }

        private object _OperateResult = new object();
        private void OperateResult(ThreadOpt threadOpt, object data)
        {
            lock (_OperateResult)
            {
                if (null != data)
                {
                    int n = (int)data;
                    OptDatas += n;
                }
                string id = threadOpt.ID;
                ((IDisposable)threadOpt).Dispose();
                threadDic.Remove(id);
            }
        }

        void IDisposable.Dispose()
        {
            if (null == dbAdapter) return;
            ((IDisposable)dbAdapter).Dispose();
        }

        ~MultiTablesExec()
        {
            ((IDisposable)this).Dispose();
        }

        class ThreadOpt : IDisposable
        {
            private MultiTablesExec multiTablesExec = null;
            private DbAdapter dbAdapter = DbAdapter.Instance;
            private string _ID = null;

            public ThreadOpt(MultiTablesExec multiTablesExec)
            {
                _ID = Guid.NewGuid().ToString();
                this.multiTablesExec = multiTablesExec;
                multiTablesExec.initBasicExecForSQL(dbAdapter, MultiTablesExec.dbHelper);
            }

            public string ID { get { return _ID; } }

            public void query(object autoCall, string sql, List<DbParameter> parameters)
            {
                Task.Run(() =>
                {
                    DataTable dt = null;
                    AutoCall autoCall1 = autoCall as AutoCall;
                    string _err = "";
                    dbAdapter.ExecSql(autoCall1, sql, parameters, ref _err, val =>
                    {
                        dt = val as DataTable;
                    }, cmd =>
                    {
                        DataTable dataTable = null;
                        DataSet ds = new DataSet();
                        Data.Common.DataAdapter da = MultiTablesExec.dbHelper.dataServerProvider.CreateDataAdapter(cmd);
                        da.Fill(ds);
                        if (0 < ds.Tables.Count) dataTable = ds.Tables[0];
                        return dataTable;
                    });
                    err = _err;
                    this.multiTablesExec.QueryResult(this, dt);
                });
            }

            public void oparete(object autoCall, string sql, List<DbParameter> parameters)
            {
                Task.Run(() =>
                {
                    int num = 0;
                    AutoCall autoCall1 = autoCall as AutoCall;
                    string _err = "";
                    dbAdapter.ExecSql(autoCall1, sql, parameters, ref _err, val =>
                       {
                           num = Convert.ToInt32(val);
                       }, cmd =>
                        {
                            return cmd.ExecuteNonQuery();
                        });
                    err = _err;
                    multiTablesExec.OperateResult(this, num);
                });
            }

            public string err { get; set; }

            void IDisposable.Dispose()
            {
                if (null == dbAdapter) return;
                ((IDisposable)dbAdapter).Dispose();
            }

            ~ThreadOpt()
            {
                ((IDisposable)this).Dispose();
            }
        }

        class TableInfo
        {
            public override string ToString()
            {
                return tbName;
            }

            public string tbName { get; set; }

            public int recordQuantity { get; set; }
        }
    }
}
