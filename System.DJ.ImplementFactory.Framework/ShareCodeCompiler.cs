using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory
{
    public class ShareCodeCompiler
    {
        public static Assembly TranslateCode(string SavePath_dll, string[] references, string dotNetVersion, string code, ref string err)
        {
            Assembly assembly = null;
            if (null == ImplementAdapter.codeCompiler) return assembly;
            Type type = ImplementAdapter.codeCompiler.GetType();
            object _obj = null;
            try
            {
                _obj = Activator.CreateInstance(type);
            }
            catch (Exception)
            {

                //throw;
            }
            if (null == _obj) return assembly;
            IInstanceCodeCompiler instanceCodeCompiler = (IInstanceCodeCompiler)_obj;
            instanceCodeCompiler.SavePathOfDll = SavePath_dll;
            assembly = instanceCodeCompiler.TranslateCode(references, dotNetVersion, code, ref err);
            return assembly;
        }
    }
}
