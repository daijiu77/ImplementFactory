using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons.DataOperate
{
    abstract class AbsBatch : IDisposable
    {
        Dictionary<string, DataEle> deDic = new Dictionary<string, DataEle>();

        string dbTag = "";
        int maxNum = 20;
        int waitSecond = 10;
        bool isRun = false;

        IDbHelper dbHelper = null;
        AutoCall autoCall = null;

        class DataEle
        {
            List<DbParameter> _parameters = new List<DbParameter>();
            string _sqlExpress = "";
            int _eleNum = 0;
            int _nTime = 0;

            public AbsBatch absBatch { get; set; }
            public IDbHelper dbHelper { get; set; }
            public string dbTag { get; set; }
            public int maxNum { get; set; }

            /// <summary>
            /// 参数：sql, tableName, DbParameterCollection
            /// </summary>
            public Action<string, string, List<DbParameter>> action { get; set; }

            /// <summary>
            /// 批量最大 sql 表达式长度
            /// </summary>
            public int sql_length { get; set; } = 50000;

            public string Key { get; set; }

            public List<DbParameter> parameters
            {
                get { return _parameters; }
            }

            public string sqlExpress
            {
                get { return _sqlExpress; }
            }

            public void Add(string sql, List<DbParameter> paras)
            {
                if (maxNum == _eleNum || sql_length <= _sqlExpress.Length)
                {
                    toBuffer();
                }

                _eleNum++;
                _nTime = 0;
                string fn = "";
                Regex rg1 = null;
                if (string.IsNullOrEmpty(_sqlExpress))
                {
                    _sqlExpress = sql;
                }
                else
                {
                    string vs = absBatch.Step03_GetSqlExpressionOfParamPart(sql, dbTag, _eleNum);

                    if (!string.IsNullOrEmpty(vs))
                    {
                        string s = _sqlExpress.TrimEnd();
                        s = s.Substring(s.Length - 1);
                        if (s.Equals(",") || s.Equals(";"))
                        {
                            int n = _sqlExpress.LastIndexOf(s);
                            _sqlExpress = _sqlExpress.Substring(0, n);
                        }
                        
                        _sqlExpress += absBatch.Step04_SplitCharOfSqlUnit() + vs;
                    }
                }

                if (null == paras) return;

                DbParameter para = null;
                rg1 = new Regex(@"(?<FieldName>[a-z0-9_]+)", RegexOptions.IgnoreCase);
                paras.ForEach(e =>
                {
                    fn = e.ParameterName;
                    if (rg1.IsMatch(fn)) fn = rg1.Match(fn).Groups["FieldName"].Value;
                    fn = absBatch.Step05_GetParameterName(fn, _eleNum);
                    para = dbHelper.dataServerProvider.CreateDbParameter(fn, e.Value);
                    parameters.Add(para);
                });
            }

            public void toBuffer()
            {
                if (string.IsNullOrEmpty(_sqlExpress)) return;
                action(_sqlExpress, Key, parameters);
                clear();
            }

            public bool Enabled
            {
                get { return false == string.IsNullOrEmpty(_sqlExpress); }
            }

            public int nTime
            {
                get { return _nTime; }
                set { _nTime = value; }
            }

            void clear()
            {
                _eleNum = 0;
                _nTime = 0;
                _sqlExpress = "";
                parameters.Clear();
            }
        }

        public AbsBatch()
        {
            dbTag = DJTools.GetParaTagByDbDialect(DataAdapter.dbDialect);

            new Task(() =>
            {
                int pulse = 1000;
                isRun = true;
                while (isRun)
                {
                    if (0 < deDic.Count)
                    {
                        foreach (KeyValuePair<string, DataEle> item in deDic)
                        {
                            if (false == item.Value.Enabled) continue;
                            item.Value.nTime++;
                            if (waitSecond == item.Value.nTime)
                            {
                                item.Value.toBuffer();
                            }
                        }
                    }
                    Thread.Sleep(pulse);
                }
            }).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbHelper"></param>
        /// <param name="autoCall"></param>
        /// <param name="sql"></param>
        /// <param name="dbParameters"></param>
        /// <param name="action">参数：sql, tableName, DbParameterCollection</param>
        public void analysis(IDbHelper dbHelper, AutoCall autoCall, string sql, List<DbParameter> dbParameters, Action<string, string, List<DbParameter>> action)
        {
            if (null == this.dbHelper) this.dbHelper = dbHelper;
            if (null == this.autoCall) this.autoCall = autoCall;

            if (null == dbParameters) dbParameters = new List<DbParameter>();

            string tbName = Step01_GetTableName(sql);
            if (Step02_InvalidBatch(sql))
            {
                action(sql, tbName, dbParameters);
                return;
            }

            DataEle de = null;
            deDic.TryGetValue(tbName, out de);

            if (null == de)
            {
                de = new DataEle()
                {
                    absBatch = this,
                    dbHelper = dbHelper,
                    dbTag = dbTag,
                    maxNum = maxNum,
                    action = action,
                    sql_length = sqlMaxLengthForBatch,
                    Key = tbName
                };
                deDic.Add(tbName, de);
            }

            de.Add(sql, dbParameters);
        }

        /// <summary>
        /// 批量操作最大数量
        /// </summary>
        public int optByBatchMaxNumber
        {
            get { return maxNum; }
            set { maxNum = value; }
        }

        /// <summary>
        /// 批量操作时等待时间(秒), 如果等于该时间则强制执行
        /// </summary>
        public int optByBatchWaitSecond
        {
            get { return waitSecond; }
            set { waitSecond = value; }
        }

        /// <summary>
        /// insert/update/delete 批量操作 sql 表达式最大长度
        /// </summary>
        public int sqlMaxLengthForBatch { get; set; } = 50000;

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual string Step01_GetTableName(string sql)
        {
            return "";
        }

        /// <summary>
        /// 无效的批量, 判断 sql 语句是否符合批量, true(无效的), false(有效的), 默认 false
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual bool Step02_InvalidBatch(string sql)
        {
            return false;
        }

        /// <summary>
        /// 多项 sql 语句时, 需要重新生成除第一项外的 sql 项中的参数名称，每一项中的参数名称必须是唯一的
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string Step03_GetSqlExpressionOfParamPart(string sql, string dbTag, int index)
        {
            return ResetParamName(sql, index);
        }

        /// <summary>
        /// 多项 sql 语句时, 项与项之间的分隔符, 默认为英文逗号. 例: insert into user(name,age) values (@name,@age),(@name1,@age1),(@name2,@age2)
        /// </summary>
        /// <returns></returns>
        public virtual string Step04_SplitCharOfSqlUnit()
        {
            return ",";
        }

        /// <summary>
        /// 多项 sql 语句时, 因为 sql 语句中的参数名称被重新命名，所以参数列表里的参数名也必须与之一一对应
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string Step05_GetParameterName(string fieldName, int index)
        {
            string paraName = fieldName;
            paraName = ParaFormat(paraName, index);
            return paraName;
        }

        protected string ParaFormat(string fieldName, int index)
        {
            string fn = fieldName;
            if (1 < index)
            {
                fn += "_" + index;
            }
            return fn;
        }

        protected string ResetParamName(string sql, int index)
        {
            string ValueStr1 = sql;
            string sql1 = sql;
            Regex rg = null;

            string FieldName = "";
            string fn1 = "";
            string LeftSign = "";
            string EndSign = "";
            string s = "";
            rg = DynamicCodeChange.rgParaField;
            MatchCollection mc = rg.Matches(ValueStr1);
            string v = "";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Match item = null;
            int num = 0;
            while (rg.IsMatch(sql1) && 1000 > num)
            {
                item = rg.Match(sql1);
                LeftSign = item.Groups["LeftSign"].Value;
                FieldName = item.Groups["FieldName"].Value;
                EndSign = item.Groups["EndSign"].Value;
                v = null;
                fn1 = FieldName.ToLower();
                dic.TryGetValue(fn1, out v);
                if (false == string.IsNullOrEmpty(v)) continue;

                dic.Add(fn1, fn1);
                fn1 = ParaFormat(FieldName, index);
                s = LeftSign + dbTag + FieldName + EndSign;
                fn1 = LeftSign + dbTag + fn1 + EndSign;
                ValueStr1 = ValueStr1.Replace(s, fn1);
                sql1 = sql1.Replace(s, EndSign);
                num++;
            }

            return ValueStr1;
        }

        void IDisposable.Dispose()
        {
            isRun = false;
        }
    }
}
