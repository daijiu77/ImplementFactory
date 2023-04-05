using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class MatchRule
    {
        private string _DllRelativePathOfImpl = "[可选] - 实例类所在dll文件的相对路径,如果为空,表示实例类和exe文件属同一dll文件";
        /// <summary>
        /// [可选] - 实例类所在dll文件的相对路径,
        /// 如果为空,表示实例类和exe文件属同一dll文件
        /// </summary>
        public string DllRelativePathOfImpl { get; set; }

        private string _ImplementNameSpace = "[可选] - 指定实现interface类的实例所在的namespace";
        /// <summary>
        /// [可选] - 指定实现interface类的实例所在的namespace
        /// </summary>
        public string ImplementNameSpace { get; set; }

        private string _MatchImplExpression = "[*必选*] - 匹配实现 interface 类的实例名称,可以是一个完整的类名称, 但不包含namespace。也可以是一个正则表达式";
        /// <summary>
        /// [*必选*] - 匹配实现 interface 类的实例名称,可以是一个完整的类名称, 但不包含namespace.
        /// 也可以是一个正则表达式
        /// </summary>
        public string MatchImplExpression { get; set; }

        private string _InterFaceName = "[*必选*] - 接口名称, 可以是一个 namespace.interfaceClassName 完整的接口名称, 也可是interfaceClassName";
        /// <summary>
        /// [*必选*] - 接口名称, 可以是一个 namespace.interfaceClassName 完整的接口名称, 
        /// 也可是interfaceClassName
        /// </summary>
        public string InterFaceName { get; set; }

        private string _IgnoreCase = "[可选] - 匹配 MatchImplExpression 时是否忽略大小写, 默认true[忽略大小写], false[区分大小写]";

        /// <summary>
        /// [可选] - 匹配 MatchImplExpression 时是否忽略大小写, 默认true[忽略大小写], false[区分大小写]
        /// </summary>
        public bool IgnoreCase { get; set; } = true;

        private string _IsShowCode = "是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]";
        /// <summary>
        /// 是否显示临时dll组件对应的代码, 默认false[不显示], true[显示]
        /// </summary>
        public bool IsShowCode { get; set; }
    }
}
