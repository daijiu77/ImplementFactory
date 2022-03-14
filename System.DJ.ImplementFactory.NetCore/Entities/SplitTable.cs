using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class SplitTable
    {
        private string _Rule = "$符号代替表名,#符号表示日期时间,{8}#表示仅年月日,#{6}表示仅时分秒";
        public string Rule { get; set; } = "$_{8}#_#{2}";

        private string _RecordQuantity = "单表最大记录数量";
        public long RecordQuantity { get; set; } = 10000000;

        private string _Enabled = "分表存储，分表查询";
        public bool Enabled { get; set; } = false;

        private string _MaxWaitIntervalOfS = "最大等待间隔_秒";
        public int MaxWaitIntervalOfS { get; set; }
    }
}
