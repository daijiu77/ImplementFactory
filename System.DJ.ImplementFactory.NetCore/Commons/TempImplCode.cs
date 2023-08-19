using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons
{
    public class TempImplCode
    {
        public const string dirName = "TempImpl";
        public const string libName = "bin";
        public const string msProjectName = "System.DJ.MicroService";
        static string rootPath = "";

        static string[] dllArr = new string[] { "System.dll", "System.Xml.dll" };
        static int instanceCount = 0;

        public void SetRootPath(string rootPath)
        {
            init(rootPath);
        }

        public Type NewImplement(Type interfaceType, Type implementType, AutoCall autoCall_Impl, bool isShowCode, bool isSingleInstance)
        {
            Type implType = null;

            string dir = Path.Combine(rootPath, dirName);
            dir.InitDirectory(true);
            if (!Directory.Exists(dir)) return implType;
            DynamicCodeTempImpl dynamicCode = new DynamicCodeTempImpl(dirName);
            string classPath = "";
            string code = dynamicCode.GetCodeByImpl(interfaceType, implementType, autoCall_Impl, ref classPath);
            if (!string.IsNullOrEmpty(code))
            {
                string tg = "\\";
                if (-1 != dir.IndexOf("/")) tg = "/";
                string fn = Guid.NewGuid().ToString().Replace("-", "_");
                string fn1 = libName + tg + fn + ".dll";

                string dllFilePath = Path.Combine(dir, fn1);
                dllFilePath.InitDirectory();

                string err = "";
                if (null == codeCompiler)
                {
                    err = "请提供代码编译器";
                    autoCall_Impl.ExecuteException(interfaceType, null, null, null, new Exception(err));
                    return null;
                }

                if (isSingleInstance)
                {
                    codeCompiler.SavePathOfDll = null;
                }
                else
                {
                    codeCompiler.SavePathOfDll = dllFilePath;
                }
                Assembly assObj = null; // codeCompiler.TranslateCode(dllArr, "v4.0", code, ref err);
                assObj = ShareCodeCompiler.TranslateCode(dllFilePath, dllArr, "v4.0", code, ref err);
                if (string.IsNullOrEmpty(err))
                {
                    try
                    {
                        implType = assObj.GetType(classPath);
                        DJTools.AddDynamicType(implType);
                        instanceCount++;
                    }
                    catch (Exception ex)
                    {
                        err = "加载程序集时出错:\r\n" + ex.ToString();
                    }
                }
                else
                {
                    autoCall_Impl.ExecuteException(interfaceType, null, null, null, new Exception(err));
                }

                if (ImplementAdapter.dbInfo1.IsShowCode)
                {
                    if (!string.IsNullOrEmpty(err))
                    {
                        string s = "\r\n/******************\r\n" + err;
                        s += "\r\n********/";
                        code += s;
                    }
                    PrintCode(code, fn);
                }
            }

            autoCall_Impl.CreateInstanceByInterface(interfaceType, implType, autoCall_Impl, instanceCount);

            return implType;
        }

        static bool _IsShowCodeOfDataResourceDLL = false;
        public bool IsShowCodeOfDataResourceDLL
        {
            get { return _IsShowCodeOfDataResourceDLL; }
            set { _IsShowCodeOfDataResourceDLL = value; }
        }

        public IInstanceCodeCompiler codeCompiler { get; set; }

        public bool IsShowCodeOfAll { get; set; } = false;

        void init(string rootPath)
        {
            instanceCount = 0;
            if (string.IsNullOrEmpty(rootPath)) return;

            TempImplCode.rootPath = rootPath;

            Regex rg = new Regex(@".+\.((dll)|(exe))$", RegexOptions.IgnoreCase);
            List<string> list = new List<string>();
            list.Add("System.dll");
            list.Add("System.Data.dll");
            list.Add("System.Xml.dll");
            string[] fs = Directory.GetFiles(rootPath);
            foreach (string f in fs)
            {
                if (rg.IsMatch(f))
                {
                    list.Add(f);
                }
            }

            dllArr = list.ToArray();

            string dir = Path.Combine(rootPath, dirName);
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    FileAttributes fileAttributes = File.GetAttributes(dir);
                    File.SetAttributes(dir, fileAttributes | FileAttributes.Hidden);
                }
                catch { }
            }

            if (!Directory.Exists(dir)) return;

            fs = Directory.GetFiles(dir);
            foreach (string f in fs)
            {
                try
                {
                    File.Delete(f);
                }
                catch { }
            }
        }

        private static Dictionary<string, string> printDic = new Dictionary<string, string>();
        private static object TempImplCodeLock = new object();
        public static void PrintCode(string code, string fileName)
        {
            lock (TempImplCodeLock)
            {
                if (!ImplementAdapter.dbInfo1.IsShowCode) return;
                if (printDic.ContainsKey(fileName)) return;
                printDic.Add(fileName, fileName);
                string fn = fileName + ".cs";
                string f = Path.Combine(DJTools.RootPath, TempImplCode.dirName);
                if (!Directory.Exists(f))
                {
                    try
                    {
                        Directory.CreateDirectory(f);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                }
                f = Path.Combine(f, fn);
                File.WriteAllText(f, code);
            }
        }

        public static string GetSrcInterfaceInstancePropName
        {
            get { return DynamicCodeTempImpl.InterfaceInstanceType; }
        }
    }
}
