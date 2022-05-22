using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    /// <summary>
    /// 如果接口已存在对应的实例，侧直接使用该实例给成员变量赋值(无法拦截接口方法及异常信息)
    /// </summary>
    public class ExistCall: AutoCall
    {
    }
}
