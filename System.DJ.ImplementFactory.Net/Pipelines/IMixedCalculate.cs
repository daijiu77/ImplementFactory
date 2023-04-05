using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    /// <summary>
    /// 四则混合运算
    /// </summary>
    public interface IMixedCalculate
    {
        T Exec<T>(string expression);
    }
}
