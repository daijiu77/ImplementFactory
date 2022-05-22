using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IDataOperateAttribute
    {
        DataOptType dataOptType { get; set; }
    }
}
