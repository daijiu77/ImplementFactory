using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory
{
    public class ShareCodeCompiler
    {
        public static Assembly TranslateCode(string SavePath_dll, string[] references, string dotNetVersion, string code, ref string err)
        {
            Assembly assembly = null;
            if (null == ImplementAdapter.codeCompiler)
            {
                AutoCall.Instance.e("Create instance fially. [ImplementAdapter.codeCompiler] is empty\r\n" + SavePath_dll);
                return assembly;
            }
            Type type = ImplementAdapter.codeCompiler.GetType();
            object _obj = null;
            try
            {
                _obj = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                AutoCall.Instance.e("Create instance fially. \r\nException: " + ex.Message);
                //throw;
            }
            if (null == _obj) return assembly;
            IInstanceCodeCompiler instanceCodeCompiler = (IInstanceCodeCompiler)_obj;
            instanceCodeCompiler.SavePathOfDll = SavePath_dll;
            assembly = instanceCodeCompiler.TranslateCode(references, dotNetVersion, code, ref err);
            if (null == assembly)
            {
                AutoCall.Instance.e("Create instance fially.\r\n" + SavePath_dll + "\r\nError message: " + err);
            }
            return assembly;
        }
    }
}
