using System.Reflection;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IInstanceCodeCompiler
    {
        void SetRootPath(string rootPath);
        Assembly TranslateCode(string[] references, string dotNetVersion, string code, ref string err);
        string SavePathOfDll { get; set; }
    }
}
