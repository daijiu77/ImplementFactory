using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.NetCore.Entities
{
    public class SysConfig
    {
        private string _Recomplie = "是否启用重新编译机制, false(不启用), true(启用), 默认为 false";
        /// <summary>
        /// 是否启用重新编译机制
        /// </summary>
        public bool Recomplie { get; set; } = false;

        private string _IsShowCode = "显示所有参与编译的代码, false(不显示), true(显示)";
        /// <summary>
        /// 显示所有参与编译的代码
        /// </summary>
        public bool IsShowCode { get; set; } = false;
    }
}
