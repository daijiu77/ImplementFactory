using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons.DataOperate
{
    class InsertBatch : AbsBatch
    {
        string CharTag = "";

        public bool isNormalBatchInsert { get; set; }

        public override string Step01_GetTableName(string sql)
        {
            Regex rg = new Regex(@"((^insert)|([^\s]\s+insert))(\s+into)?\s+(?<TableName>[a-z0-9_]+)", RegexOptions.IgnoreCase);
            string tbName = rg.Match(sql).Groups["TableName"].Value.ToLower();
            return tbName;
        }

        public override bool Step02_InvalidBatch(string sql)
        {
            sql = sql.Trim();
            Regex rg = new Regex(@"((^insert)|((?<CharTag>[^\s])\s+insert))(\s+into)?\s+[a-z0-9_]+", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(sql))
            {
                return true;
            }

            Match m = rg.Match(sql);
            CharTag = m.Groups["CharTag"].Value;
            //if (false == string.IsNullOrEmpty(CharTag)) return true;

            return base.Step02_InvalidBatch(sql);
        }

        public override string Step03_GetSqlExpressionOfParamPart(string sql, string dbTag, int index)
        {
            string vs = "";
            if (isNormalBatchInsert)
            {
                string ValueStr1 = "";
                Regex rg = null;
                if (string.IsNullOrEmpty(CharTag))
                {
                    rg = new Regex(@"((^insert)|([^\s]\s+insert))(\s+into)?\s+[a-z0-9_]+\s*(\([^\(\)]+\))?\s+values\s*((\((?<ValueStr>((?!\)\s).)+)\)$)|(\((?<ValueStr>((?!\)\s).)+)\)\s*[^\s]))", RegexOptions.IgnoreCase);
                    ValueStr1 = rg.Match(sql).Groups["ValueStr"].Value;
                }
                else
                {
                    ValueStr1 = sql;
                }

                ValueStr1 = ResetParamName(ValueStr1, index);

                if (string.IsNullOrEmpty(CharTag))
                {
                    vs = "(" + ValueStr1 + ")";
                }
                else
                {
                    vs = ValueStr1;
                }
            }
            else
            {
                vs = ResetParamName(sql, index);
            }

            return vs;
        }

        public override string Step04_SplitCharOfSqlUnit()
        {            
            if (false == string.IsNullOrEmpty(CharTag) || false == isNormalBatchInsert) return ";";
            return base.Step04_SplitCharOfSqlUnit();
        }

        public override string Step05_GetParameterName(string fieldName, int index)
        {
            return base.Step05_GetParameterName(fieldName,index);
        }
    }
}
