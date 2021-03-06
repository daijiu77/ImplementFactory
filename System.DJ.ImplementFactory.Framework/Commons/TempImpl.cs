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
    public class TempImpl
    {
        public static string dirName = "TempImpl";
        public static string libName = "bin";
        static string rootPath = "";

        static string[] dllArr = new string[] { "System.dll", "System.Xml.dll" };
        static int instanceCount = 0;

        public void SetRootPath(string rootPath)
        {
            init(rootPath);
        }

        public Type NewImplement(Type interfaceType, Type implementType, AutoCall autoCall_Impl, bool isShowCode)
        {
            Type implType = null;

            string dir = Path.Combine(rootPath, dirName);
            if (!Directory.Exists(dir)) return implType;
            DynamicCodeTempImpl dynamicCode = new DynamicCodeTempImpl(dirName);
            string classPath = "";
            string code = dynamicCode.GetCodeByImpl(interfaceType, implementType, autoCall_Impl, ref classPath);
            if (!string.IsNullOrEmpty(code))
            {
                string fn = Guid.NewGuid().ToString().Replace("-", "_");
                string fn1 = libName + "\\" + fn + ".dll";

                string dllFilePath = Path.Combine(dir, fn1);
                dllFilePath.InitDirectory();

                string err = "";
                if (null == codeCompiler)
                {
                    err = "请提供代码编译器";
                    autoCall_Impl.ExecuteException(interfaceType, null, null, null, new Exception(err));
                    return null;
                }
                codeCompiler.SavePathOfDll = dllFilePath;
                Assembly assObj = codeCompiler.TranslateCode(dllArr, "v4.0", code, ref err);

                if (string.IsNullOrEmpty(err))
                {
                    try
                    {
                        implType = assObj.GetType(classPath);
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

                isShowCode = dynamicCode.IsDataInterface ? IsShowCodeOfDataResourceDLL : isShowCode;
                isShowCode = IsShowCodeOfAll ? IsShowCodeOfAll : isShowCode;

                if (isShowCode)
                {
                    if (!string.IsNullOrEmpty(err))
                    {
                        string s = "\r\n/******************\r\n" + err;
                        s += "\r\n********/";
                        code += s;
                    }
                    string fn2 = fn + ".cs";
                    string f2 = Path.Combine(dir, fn2);
                    try
                    {
                        File.WriteAllText(f2, code);
                    }
                    catch { }
                }
                else
                {
                    if (!string.IsNullOrEmpty(err))
                    {
                        autoCall_Impl.e(err);
                    }
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

        public static string InterfaceInstanceType => DynamicCodeTempImpl.InterfaceInstanceType;

        void init(string rootPath)
        {
            instanceCount = 0;
            if (string.IsNullOrEmpty(rootPath)) return;

            TempImpl.rootPath = rootPath;

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

    }
}
