using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IEntityCopy
    {
        Type CopyParentModelType { get; }
        /// <summary>
        /// 赋值开关,当为 true 时表示仅赋值,不做数据变更操作,且每次给数据模型属性赋值后,AssignmentNo 都会被重置为 false
        /// </summary>
        bool AssignmentNo { get; set; }
    }
}
