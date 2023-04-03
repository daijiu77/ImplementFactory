using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMicroServiceMethod
    {
        Type GetMS(IInstanceCodeCompiler instanceCodeCompiler, AutoCall autoCall, MicroServiceRoute microServiceRoute, Type interfaceType);
    }
}
