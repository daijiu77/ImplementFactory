using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.NetCore.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
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
        private static Dictionary<string, string> _tableDic = new Dictionary<string, string>();
        private Dictionary<string, ThreadOpt> threadDic = new Dictionary<string, ThreadOpt>();
        private AutoCall autoCall = new AutoCall();
        private static DbInfo dbInfo = null;
        private static IDbHelper dbHelper = null;
        private DataTable queryDatas = null;
        private DbAdapter dbAdapter = DbAdapter.Instance;
        private CreateNewTable createNewTable = null;
        private int OptDatas = 0;
        private List<Task> tasks = new List<Task>();

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
            else if (dbInfo.DatabaseType.Equals("mysql"))
            {
                //select TABLE_NAME from information_schema.tables where table_schema='{0}';
                sql = "select TABLE_NAME from information_schema.tables where LOWER(TABLE_SCHEMA)<>'information_schema' and LOWER(TABLE_SCHEMA)<>'mysql' and LOWER(ENGINE)='innodb';";
            }
            else if (dbInfo.DatabaseType.Equals("oracle"))
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

        public static Dictionary<string, string> Tables
        {
            get
            {
                ImplementAdapter.task.Wait();
                return _tableDic;
            }
        }

        public static void SetTable(string tableName)
        {
            ImplementAdapter.task.Wait();
            string tb = tableName.ToLower();
            if (!_tableDic.ContainsKey(tb))
            {
                _tableDic.Add(tb, tableName);
            }
            set_tbDic(tableName, tableName);
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
            dbAdapter.IgnoreError = true;
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

        private static void set_tbDic(string srcTableName, string newTableName)
        {
            string tbn = srcTableName.ToLower();
            List<TableInfo> list = null;
            if (tbDic.ContainsKey(tbn))
            {
                list = (List<TableInfo>)tbDic[tbn];
            }
            else
            {
                list = new List<TableInfo>();
                //tableInfo.recordQuantity = RecordCount(srcTableName);
                list.Add(new TableInfo()
                {
                    tbName = srcTableName
                });
                tbDic.Add(tbn, list);
            }
            if (newTableName.ToLower().Equals(tbn)) return;
            list.Add(new TableInfo()
            {
                tbName = newTableName
            });
        }

        private void initDictionary(string rule, string sql)
        {
            DataTable dt = GetDataTable(sql);

            if (null == dt) return;
            string tbName = "";
            string tn = "";
            string srcTableName = "";
            foreach (DataRow item in dt.Rows)
            {
                tbName = item["TABLE_NAME"].ToString();
                tn = tbName.ToLower();
                if (!_tableDic.ContainsKey(tn))
                {
                    _tableDic.Add(tn, tbName);
                }
                //if (!isSplitTable(rule, tbName, ref srcTableName)) continue;
                if (!isSplitTable(rule, tbName, ref srcTableName)) srcTableName = tbName;
                set_tbDic(srcTableName, tbName);
            }
        }

        object ISingleInstance.Instance { get; set; }

        private string[] getTableNamesWithSql(string sql, string leftStr, string rightStr, Dictionary<string, string> AliasTbNameDic, ref string new_sql)
        {
            //dic tableName key:Lower, value:self
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string s1 = "";
            string newSql = sql;
            string _sql = sql;
            List<Regex> rgList = new List<Regex>();
            int n = 0;
            const int max = 10;
            foreach (KeyValuePair<string, object> item in tbDic)
            {
                s1 += @"|((\s|\,)[^a-z0-9_\s]?" + item.Key + @"[^a-z0-9_\s]?\s)";
                n++;
                if (max == n)
                {
                    s1 = s1.Substring(1);
                    s1 = "(?<TbName>(" + s1 + "))";
                    rgList.Add(new Regex(s1, RegexOptions.IgnoreCase));
                    s1 = "";
                    n = 0;
                }
            }

            if (!string.IsNullOrEmpty(s1))
            {
                s1 = s1.Substring(1);
                s1 = "(?<TbName>(" + s1 + "))";
                rgList.Add(new Regex(s1, RegexOptions.IgnoreCase));
                s1 = "";
            }

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
                    Regex rg3 = new Regex(@"^(?<LeftS>[^a-z0-9_\s])[a-z0-9_]+(?<RightS>[^a-z0-9_\s])$", RegexOptions.IgnoreCase);
                    Regex rg4 = null;
                    Match m3 = null;
                    string LeftS = "", RightS = "";
                    foreach (Match m1 in mc1)
                    {
                        s = m1.Groups[0].Value;
                        ss = s;
                        _sql = _sql.Replace(s, "");
                        foreach (Regex rg1 in rgList)
                        {
                            if (!rg1.IsMatch(s)) continue;
                            mc2 = rg1.Matches(s);
                            foreach (Match m2 in mc2)
                            {
                                tb = m2.Groups["TbName"].Value.Trim();
                                LeftS = "";
                                RightS = "";
                                if (rg3.IsMatch(tb))
                                {
                                    m3 = rg3.Match(tb);
                                    LeftS = m3.Groups["LeftS"].Value;
                                    RightS = m3.Groups["RightS"].Value;
                                    tb = tb.Substring(1);
                                    tb = tb.Substring(0, tb.Length - 1);
                                }
                                if (string.IsNullOrEmpty(LeftS))
                                {
                                    if (tb.Substring(0, 1).Equals(","))
                                    {
                                        LeftS = tb.Substring(0, 1);
                                        tb = tb.Substring(1);
                                    }
                                }
                                rg4 = new Regex(@"(\s|\,)" + tb + @"\s+(as\s+)?(?<tbAlias>[a-z0-9_]+)", RegexOptions.IgnoreCase);
                                if (rg4.IsMatch(s))
                                {
                                    string alias = rg4.Match(s).Groups["tbAlias"].Value.ToLower();
                                    if (!AliasTbNameDic.ContainsKey(alias))
                                    {
                                        AliasTbNameDic.Add(alias, tb);
                                    }
                                }
                                tbn = tb.ToLower();
                                ss = ss.Replace(m2.Groups["TbName"].Value, " " + LeftS + leftStr + tbn + rightStr + RightS + " ");
                                if (dic.ContainsKey(tbn)) continue;
                                dic.Add(tbn, tb);
                            }
                            newSql = newSql.Replace(s, ss);
                        }
                    }
                }
            };

            Regex rg2 = new Regex(@"\sfrom\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)).)+\s((where)|(group)|(order))\s", RegexOptions.IgnoreCase);
            action(rg2);

            rg2 = new Regex(@"\sfrom\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)(?!\()(?!\))).)+\)", RegexOptions.IgnoreCase);
            action(rg2);

            rg2 = new Regex(@"\sfrom\s+(((?!\sfrom\s)(?!\swhere\s)(?!\sgroup\s)(?!\sorder\s)).)+", RegexOptions.IgnoreCase);
            action(rg2);

            rg2 = new Regex(@"[a-z0-9_]+", RegexOptions.IgnoreCase);
            new_sql = newSql;
            string[] arr = new string[dic.Count];
            n = 0;
            foreach (var item in dic)
            {
                s1 = item.Key;
                if (rg2.IsMatch(s1))
                {
                    s1 = rg2.Match(s1).Groups[0].Value;
                }
                arr[n] = s1;
                n++;
            }
            return arr;
        }

        private List<string> getSqlByTables(string sql, string leftStr, string rightStr)
        {
            //newSql 带有替换标识符的sql语句,例: select * from |#UserInfo#| order by createdate desc;
            string newSql = "";
            Dictionary<string, string> AliasTbNameDic = new Dictionary<string, string>();
            string[] arr = getTableNamesWithSql(sql, leftStr, rightStr, AliasTbNameDic, ref newSql);

            TList tableOrderBies = new TList();
            List<string> sqlList = new List<string>();
            TableItem tableItem = null;
            List<TableInfo> list = null;
            string[] tbs = null;
            string now_Sql = newSql;
            int nlen = 0;
            int size = arr.Length;
            int num = 0;
            SqlItemList sqlItemList = new SqlItemList();
            foreach (var item in arr)
            {
                if (null != tableOrderBies[item])
                {
                    sqlItemList.Add(item, null);
                }
                list = tbDic[item] as List<TableInfo>;
                if (null == list) continue;
                if (1 == list.Count)
                {
                    now_Sql = now_Sql.Replace(leftStr + item + rightStr, list[0].tbName);
                    num++;
                    if (num == size) sqlList.Add(now_Sql);
                    if (null != sqlItemList[item]) sqlItemList[item].SqlItems.Add(new SqlItem() { TableName = item, IsDesc = tableOrderBies[item].IsDesc });
                }
                else if (1 < list.Count)
                {
                    nlen = list.Count;
                    tbs = new string[nlen];
                    for (int i = 0; i < nlen; i++)
                    {
                        tbs[i] = list[i].tbName;
                        if (null != sqlItemList[item]) sqlItemList[item].SqlItems.Add(new SqlItem() { TableName = tbs[i], IsDesc = tableOrderBies[item].IsDesc });
                    }

                    if (null == tableItem)
                    {
                        tableItem = new TableItem();
                    }
                    else
                    {
                        tableItem.Child = new TableItem();
                        tableItem.Child.Parent = tableItem;
                        tableItem = tableItem.Child;
                    }
                    tableItem.key = item;
                    tableItem.Tables = tbs;
                }
            }

            if (null != tableItem)
            {
                while (null != tableItem.Parent)
                {
                    tableItem = tableItem.Parent;
                }

                ForTableItem(tableItem, leftStr, rightStr, now_Sql, sqlList);
            }

            return sqlList;
        }

        private void ForTableItem(TableItem tableItem, string leftT, string rightT, string sql, List<string> sqlList)
        {
            int nlen = tableItem.Count;
            string sql1 = "";
            string key = "";
            for (int i = 0; i < nlen; i++)
            {
                key = tableItem.key;
                key = leftT + key + rightT;
                sql1 = sql.Replace(key, tableItem.Tables[i]);
                if (null != tableItem.Child)
                {
                    ForTableItem(tableItem.Child, leftT, rightT, sql1, sqlList);
                }
                else
                {
                    sqlList.Add(sql1);
                }
            }
        }

        class TableOrderBy
        {
            public string TableName { get; set; }
            public string Alias { get; set; }
            public bool IsDesc { get; set; }
        }

        class TList : List<TableOrderBy>
        {
            private Dictionary<string, int> dic = new Dictionary<string, int>();
            public TableOrderBy this[string tableName]
            {
                get
                {
                    TableOrderBy tableOrderBy = null;
                    string key = tableName.ToLower();
                    if (dic.ContainsKey(key))
                    {
                        int n = dic[key];
                        tableOrderBy = this[n];
                    }
                    return tableOrderBy;
                }
            }

            public void Add(string tableName, bool isDesc)
            {
                string key = tableName.ToLower();
                if (dic.ContainsKey(key)) return;
                dic.Add(key, this.Count);
                this.Add(new TableOrderBy()
                {
                    TableName = tableName,
                    IsDesc = isDesc
                });
            }
        }

        class SqlItemList
        {
            private Dictionary<string, SqlItemCollection> _dic = new Dictionary<string, SqlItemCollection>();
            public SqlItemCollection this[string tableName]
            {
                get
                {
                    SqlItemCollection sqlItemCollection = null;
                    string key = tableName.ToLower();
                    _dic.TryGetValue(key, out sqlItemCollection);
                    return sqlItemCollection;
                }
            }

            public void Add(string tableName, string sql)
            {
                SqlItemCollection sqlItemCollection = null;
                string key = tableName.ToLower();
                if (_dic.ContainsKey(key))
                {
                    sqlItemCollection = _dic[key];
                }
                else
                {
                    sqlItemCollection = new SqlItemCollection()
                    {
                        TableName = tableName
                    };
                    _dic.Add(key, sqlItemCollection);
                }

                sqlItemCollection.SqlItems.Add(new SqlItem()
                {
                    TableName = tableName,
                    Sql = sql
                });
            }

            public List<SqlItem> GetSqlItem()
            {
                List<SqlItem> sqlItems = new List<SqlItem>();
                foreach (var item in _dic)
                {
                    foreach (var item1 in item.Value.SqlItems)
                    {
                        sqlItems.Add(item1);
                    }
                }
                return sqlItems;
            }
        }

        class SqlItemCollection
        {
            private List<SqlItem> sqlList = new List<SqlItem>();
            public string TableName { get; set; }
            public List<SqlItem> SqlItems { get { return sqlList; } }
        }

        class SqlItem : IComparable<SqlItem>
        {
            public string TableName { get; set; }
            public string Sql { get; set; }

            public override string ToString()
            {
                return this.Sql;
            }

            public bool IsDesc { get; set; }

            int IComparable<SqlItem>.CompareTo(SqlItem other)
            {
                string[] arr = new string[] { TableName, other.TableName };
                string[] arr1 = null;
                if (IsDesc)
                {
                    arr1 = arr.OrderByDescending(x => x).ToArray();
                }
                else
                {
                    arr1 = arr.OrderBy(x => x).ToArray();
                }

                if (arr1[0].Equals(TableName))
                {
                    return -1;
                }
                else if (arr1[0].Equals(other.TableName))
                {
                    return 1;
                }
                return 0;
            }
        }

        class TableItem
        {
            private string[] tbs = null;

            public int Count { get; private set; }
            public string[] Tables
            {
                get { return tbs; }
                set
                {
                    tbs = value;
                    if (null != tbs)
                    {
                        Count = tbs.Length;
                    }
                }
            }

            public string key { get; set; }

            public TableItem Parent { get; set; }
            public TableItem Child { get; set; }
        }

        private void WaitExecResult(Action resultAction)
        {
            const int sleepNum = 100;
            int maxNum = 100;
            //int num = 0;
            if (0 < dbInfo.splitTable.MaxWaitIntervalOfS)
            {
                maxNum = dbInfo.splitTable.MaxWaitIntervalOfS * 1000 / sleepNum;
            }

            //while (0 < threadDic.Count && num <= maxNum)
            //{
            //    num++;
            //    Thread.Sleep(sleepNum);
            //}
            Task.WaitAll(tasks.ToArray());
            resultAction();
        }

        private TList getOrderBy(string sql, Dictionary<string, string> AliasTbNameDic)
        {
            TList tableOrderBies = new TList();
            return tableOrderBies;
        }

        private void DataOpt(object autoCall, string sql, List<DbParameter> parameters, Action<object> resultAction, ref string err)
        {
            OptDatas = 0;
            threadDic.Clear();
            tasks.Clear();
            ImplementAdapter.task.Wait();
            if (0 == tbDic.Count) return;

            List<string> sqlList = getSqlByTables(sql, leftStr, rightStr);
            if (0 == sqlList.Count)
            {
                sqlList.Add(sql);
            }

            ThreadOpt threadOpt = null;
            foreach (string item in sqlList)
            {
                threadOpt = new ThreadOpt(this);
                threadDic.Add(threadOpt.ID, threadOpt);
                threadOpt.oparete(autoCall, item, parameters);
                if (null != threadOpt.task) tasks.Add(threadOpt.task);
            }

            WaitExecResult(() =>
            {
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
            ImplementAdapter.task.Wait();
            initBasicExecForSQL(dbAdapter, dbHelper);
            if (null == createNewTable) createNewTable = new CreateNewTable(autoCall, dbInfo, dbAdapter, ImplementAdapter.DbHelper);
            createNewTable.SplitTable(sql);
            if (false == string.IsNullOrEmpty(createNewTable.SrcTableName)
                && false == string.IsNullOrEmpty(createNewTable.NewTableName))
            {
                set_tbDic(createNewTable.SrcTableName, createNewTable.NewTableName);
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
            queryDatas = new DataTable();
            threadDic.Clear();
            tasks.Clear();
            ImplementAdapter.task.Wait();
            if (0 == tbDic.Count) return;

            List<string> sqlList = getSqlByTables(sql, leftStr, rightStr);
            if (0 == sqlList.Count)
            {
                sqlList.Add(sql);
            }

            ThreadOpt threadOpt = null;
            foreach (string item in sqlList)
            {
                threadOpt = new ThreadOpt(this);
                threadDic.Add(threadOpt.ID, threadOpt);
                threadOpt.query(autoCall, item, parameters);
                if (null != threadOpt.task) tasks.Add(threadOpt.task);
            }

            WaitExecResult(() =>
            {
                action(queryDatas);
            });
        }

        private object _QueryResult = new object();
        private void QueryResult(ThreadOpt threadOpt, object data)
        {
            lock (_QueryResult)
            {
                string id = threadOpt.ID;
                if ((null != data) && threadDic.ContainsKey(id))
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
                multiTablesExec.initBasicExecForSQL(dbAdapter, dbHelper);
            }

            public string ID { get { return _ID; } }

            public Task task { get; set; }

            public void query(object autoCall, string sql, List<DbParameter> parameters)
            {
                task = Task.Run(() =>
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
                task = Task.Run(() =>
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
