using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class LogsRange
    {
        private string _upperLimit = "可以是数字或英文名称, 为数字时 upperLimit 应小于 lowerLimit";
        /// <summary>
        /// 上限值
        /// </summary>
        public string upperLimit { get; set; } = "severe";

        private string _lowerLimit = "可以是数字或英文名称, 为数字时 lowerLimit 应大于 upperLimit";
        /// <summary>
        /// 下限值
        /// </summary>
        public string lowerLimit { get; set; } = "debug";
    }
}
