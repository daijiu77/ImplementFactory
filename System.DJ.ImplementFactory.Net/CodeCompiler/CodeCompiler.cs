using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.DJ.Net.CodeCompiler
{
    public class CodeCompiler : IInstanceCodeCompiler
    {
        //private List<Assembly> _defaultAssemblies;
        string rootPath = "";

        public CodeCompiler()
        {
            //_defaultAssemblies = AssemblyLoadContext.Default.Assemblies
            //    .Where(assembly => !assembly.IsDynamic).ToList();
        }

        void IInstanceCodeCompiler.SetRootPath(string rootPath)
        {
            this.rootPath = rootPath;
        }

        Assembly IInstanceCodeCompiler.TranslateCode(string[] references, string dotNetVersion, string code, ref string err)
        {
            Assembly asse = null;

            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            List<MetadataReference> metadataReferences = new List<MetadataReference>();
            PortableExecutableReference portable = null;
            int len = asses.Length;
            int num = 0;
            Assembly item = null;
            while (num < len)
            {
                try
                {
                    item = asses[num];
                    if (string.IsNullOrEmpty(item.Location)) continue;
                    portable = MetadataReference.CreateFromFile(item.Location);
                    metadataReferences.Add(portable);
                }
                catch (Exception)
                {

                    //throw;
                }
                finally
                {
                    num++;
                }
            }

            var references1 = metadataReferences.ToArray(); //asses.Select(x => MetadataReference.CreateFromFile(x.Location));

            // 随机程序集名称
            string assemblyName = Path.GetRandomFileName();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            // 创建编译对象
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references1,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // 将编译后的IL代码放入内存中
                EmitResult result = compilation.Emit(ms);

                // 编译失败，提示
                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        //Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        err += string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()) + "\r\n";
                    }
                }
                else
                {
                    // 编译成功则从内存中加载程序集
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] dts = ms.ToArray();
                    asse = Assembly.Load(dts);
                    string f = ((IInstanceCodeCompiler)this).SavePathOfDll;
                    if (!string.IsNullOrEmpty(f))
                    {
                        if (File.Exists(f))
                        {
                            try
                            {
                                File.Delete(f);
                            }
                            catch (Exception)
                            {

                                //throw;
                            }
                        }

                        FileStream fs = null;
                        try
                        {
                            fs = File.OpenWrite(f);
                            fs.Write(dts, 0, dts.Length);
                        }
                        catch (Exception)
                        {

                            //throw;
                        }
                        finally
                        {
                            if (null != fs)
                            {
                                fs.Close();
                                fs.Dispose();
                            }
                        }

                    }
                }
            }

            return asse;
        }

        string IInstanceCodeCompiler.SavePathOfDll { get; set; }
    }
}
