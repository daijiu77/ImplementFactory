using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.Framework.CodeCompiler
{
    public class CodeCompiler : IInstanceCodeCompiler
    {
        string rootPath = "";

        void IInstanceCodeCompiler.SetRootPath(string rootPath)
        {
            this.rootPath = rootPath;
        }

        Assembly IInstanceCodeCompiler.TranslateCode(string[] references, string dotNetVersion, string code, ref string err)
        {
            //CompilerParameters cp = new CompilerParameters(references, dllFilePath, false);
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            List<string> fs = new List<string>();
            foreach (var item in asses)
            {
                if (string.IsNullOrEmpty(item.Location)) continue;
                fs.Add(item.Location);
            }
            references = fs.ToArray();
            CompilerParameters cp = new CompilerParameters(references);
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            dotNetVersion = string.IsNullOrEmpty(dotNetVersion) ? "v4.0" : dotNetVersion;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("CompilerVersion", dotNetVersion);
            CSharpCodeProvider provider = new CSharpCodeProvider(dic);

            CompilerResults cr = null;
            string f = ((IInstanceCodeCompiler)this).SavePathOfDll;
            if (!string.IsNullOrEmpty(f))
            {
                cp.OutputAssembly = f;
            }

            try
            {                
                cr = provider.CompileAssemblyFromSource(cp, code);
            }
            catch (Exception ex)
            {
                cr = null;
                err += "\r\n信息:" + ex.Message + "\r\n\r\n";
                //throw;
            }

            if (null != cr)
            {
                StringCollection strArr = cr.Output;
                int n = 0;
                foreach (string item in strArr)
                {
                    err += "\r\n" + item;
                    n++;
                }
            }

            Assembly asse = null;
            if (string.IsNullOrEmpty(err)) asse = cr.CompiledAssembly;

            return asse;
        }

        string IInstanceCodeCompiler.SavePathOfDll { get; set; }

    }
}
