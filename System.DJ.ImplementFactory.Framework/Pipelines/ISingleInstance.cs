using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface ISingleInstance
    {
        object Instance { get; set; }
    }
}
