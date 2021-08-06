using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMicroServiceMethod
    {
        Type GetMS(IInstanceCodeCompiler instanceCodeCompiler, AutoCall autoCall, Type interfaceType, string uri);
    }
}
