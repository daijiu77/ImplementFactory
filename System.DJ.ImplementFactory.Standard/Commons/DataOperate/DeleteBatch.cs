using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons.DataOperate
{
    class DeleteBatch: AbsBatch
    {
        string CharTag = "";

        public override string Step01_GetTableName(string sql)
        {
            Regex rg = new Regex(@"((^delete)|([^\s]\s+delete))(\s+from)?\s+(?<TableName>[a-z0-9_]+)", RegexOptions.IgnoreCase);
            string tbName = rg.Match(sql).Groups["TableName"].Value.ToLower();
            return tbName;
        }

        public override bool Step02_InvalidBatch(string sql)
        {
            sql = sql.Trim();
            Regex rg = new Regex(@"((^delete)|((?<CharTag>[^\s])\s+delete))(\s+from)?\s+[a-z0-9_]+", RegexOptions.IgnoreCase);
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
            return base.Step03_GetSqlExpressionOfParamPart(sql, dbTag, index);
        }

        public override string Step04_SplitCharOfSqlUnit()
        {
            return ";";
        }

        public override string Step05_GetParameterName(string fieldName, int index)
        {
            return base.Step05_GetParameterName(fieldName, index);
        }
    }
}
