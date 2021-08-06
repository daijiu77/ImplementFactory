using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMicroServiceMethod
    {
        Type GetMS(Type interfaceType);
    }
}
