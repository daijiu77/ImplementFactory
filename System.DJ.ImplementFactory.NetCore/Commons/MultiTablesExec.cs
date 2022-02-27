using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.NetCore.Entities;
using System.DJ.ImplementFactory.NetCore.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.NetCore.Commons
{
    public class MultiTablesExec : IMultiTablesExec
    {
        private static Dictionary<string, object> tbDic = new Dictionary<string, object>();
        private Dictionary<string, object> threadDic = new Dictionary<string, object>();
        private AutoCall autoCall = new AutoCall();
        private DbInfo dbInfo = null;
        private DataTable queryDatas = new DataTable();
        private IDbHelper dbHelper = null;
        private int OptDatas = 0;

        private delegate void ExecResult(ThreadOpt threadOpt, object data);

        public MultiTablesExec(IDbHelper dbHelper)
        {
            this.dbHelper = dbHelper;
        }

        public MultiTablesExec(DbInfo dbInfo, IDbHelper dbHelper)
        {
            if (0 < tbDic.Count) return;
            this.dbInfo = dbInfo;
            this.dbHelper = dbHelper;
            
            string rule = getRule(dbInfo);
            string sql = "";
            if (dbInfo.DatabaseType.Equals("sqlserver"))
            {
                //select TABLE_NAME from {0}.dbo.sysobjects where type='U'
                sql = "select TABLE_NAME from {0}.dbo.sysobjects where type='U'";
            }
            else if (dbInfo.DatabaseType.Equals("sqlserver"))
            {
                //select TABLE_NAME from information_schema.tables where table_schema='{0}';
                sql = "select TABLE_NAME from information_schema.tables;";
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

        private void initBasicExecForSQL(BasicExecForSQL basicExecForSQL, IDbHelper dbHelper)
        {
            basicExecForSQL.dbConnectionString = dbHelper.connectString;
            basicExecForSQL.dbConnectionState = dbHelper.dbConnectionState;
            basicExecForSQL.disposableAndClose = dbHelper.disposableAndClose;
            basicExecForSQL.dataServerProvider = dbHelper.dataServerProvider;
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
            BasicExecForSQL basicExecForSQL = BasicExecForSQL.Instance;
            initBasicExecForSQL(basicExecForSQL, dbHelper);
            basicExecForSQL.Exec(autoCall, sql, null, ref err, vObj =>
            {
                if (null == vObj) return;
                dt = vObj as DataTable;
            }, cmd =>
            {
                DataTable dt = new DataTable();
                try
                {
                    Data.Common.DataAdapter da = dbHelper.dataServerProvider.CreateDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (0 < ds.Tables.Count) dt = ds.Tables[0];
                }
                catch (Exception ex)
                {
                    autoCall.e(ex.ToString(), ErrorLevels.severe);
                    //throw;
                }
                return dt;
            });
            return dt;
        }

        private void initDictionary(string rule, string sql)
        {
            DataTable dt = GetDataTable(sql);

            if (null == dt) return;
            List<string> list = null;
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
                    list = (List<string>)tbDic[tbn];
                }
                else
                {
                    list = new List<string>();
                    tbDic.Add(tbn, list);
                }
                list.Add(tbName);
            }
        }

        object ISingleInstance.Instance { get; set; }

        int IMultiTablesExec.Delete(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }

        bool IMultiTablesExec.ExistMultiTables(string sql)
        {
            throw new NotImplementedException();
        }

        int IMultiTablesExec.Insert(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
        }

        DataTable IMultiTablesExec.Query(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<DataTable> resultAction, ref string err)
        {
            if (0 == tbDic.Count) return null;
            //dic key:Lower, value:self
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string newSql = sql;
            string _sql = sql;
            string leftStr = "|#";
            string rightStr = "#|";

            string s1 = "";
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
            return null;
        }

        int IMultiTablesExec.Update(object autoCall, string sql, List<DbParameter> parameters, bool EnabledBuffer, Action<int> resultAction, ref string err)
        {
            throw new NotImplementedException();
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
                        queryDatas.Rows.Add(item);
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

        class ThreadOpt : IDisposable
        {
            private BasicExecForSQL basicExecForSQL1 = BasicExecForSQL.Instance;
            private string _ID = null;

            public ThreadOpt(MultiTablesExec multiTablesExec)
            {
                _ID = Guid.NewGuid().ToString();
                multiTablesExec.initBasicExecForSQL(basicExecForSQL1, multiTablesExec.dbHelper);
            }

            public string ID { get { return _ID; } }

            void IDisposable.Dispose()
            {
                if (null == basicExecForSQL1) return;
                ((IDisposable)basicExecForSQL1).Dispose();
            }

            ~ThreadOpt()
            {
                ((IDisposable)this).Dispose();
            }
        }
    }
}
