using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.DJ.MicroService.Pipelines;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.DJ.MicroService.NetCore
{
    public class CreateServerInfo: ImplementAdapter
    {
        private string ipAddress = "";
        private int portNumber = 0;
        private string key = "";

        public CreateServerInfo(JToken jToken)
        {
            object v = jToken["ipAddress"];
            ipAddress = null == v ? "" : v.ToString();

            v = jToken["portNumber"];
            int.TryParse(v.ToString(), out portNumber);

            v = jToken["key"];
            key = null == v ? "" : v.ToString();
        }

        public IManageSvrInfo GetManageSvrInfo()
        {
            if (string.IsNullOrEmpty(ipAddress) || 0 >= portNumber || string.IsNullOrEmpty(key)) return null;
            IManageSvrInfo manageSvrInfo = null;

            List<string> fList = new List<string>();
            string fPath = Path.Combine(DJTools.RootPath, ServerFile);
            if (File.Exists(fPath))
            {
                string[] fArr = File.ReadAllLines(fPath);
                foreach (var item in fArr)
                {
                    fList.Add(item);
                }
                File.Delete(fPath);
            }

            string code = "";
            EList<CKeyValue> list = new EList<CKeyValue>();
            list.Add(CKeyValue.KV("System.DJ.MicroService.Pipelines", ""));
            list.Add(CKeyValue.KV("System", ""));
            list.Add(CKeyValue.KV("System.Text", ""));

            string np = typeof(IManageSvrInfo).Namespace;
            MethodInformation mi = new MethodInformation();
            mi.append(ref code, LeftSpaceLevel.one, "{#import}");
            mi.append(ref code, LeftSpaceLevel.one, "");
            mi.append(ref code, LeftSpaceLevel.one, "namespce {0}.{1}.{2}", np, TempImpl.dirName, TempImpl.libName);
            mi.append(ref code, LeftSpaceLevel.one, "{");
            mi.append(ref code, LeftSpaceLevel.two, "public class ManageSvrInfoImpl : IManageSvrInfo");
            mi.append(ref code, LeftSpaceLevel.two, "{");

            mi.append(ref code, LeftSpaceLevel.one, "");
            mi.append(ref code, LeftSpaceLevel.three, "string IManageSvrInfo.ipAddress");
            mi.append(ref code, LeftSpaceLevel.three, "{");
            mi.append(ref code, LeftSpaceLevel.four, "get { return \"{0}\"; }", ipAddress);
            mi.append(ref code, LeftSpaceLevel.three, "}");

            mi.append(ref code, LeftSpaceLevel.one, "");
            mi.append(ref code, LeftSpaceLevel.three, "int IManageSvrInfo.portNumber");
            mi.append(ref code, LeftSpaceLevel.three, "{");
            mi.append(ref code, LeftSpaceLevel.four, "get { return {0}; }", portNumber.ToString());
            mi.append(ref code, LeftSpaceLevel.three, "}");

            mi.append(ref code, LeftSpaceLevel.one, "");
            mi.append(ref code, LeftSpaceLevel.three, "string IManageSvrInfo.key");
            mi.append(ref code, LeftSpaceLevel.three, "{");
            mi.append(ref code, LeftSpaceLevel.four, "get { return \"{0}\"; }", key);
            mi.append(ref code, LeftSpaceLevel.three, "}");

            mi.append(ref code, LeftSpaceLevel.two, "{");
            mi.append(ref code, LeftSpaceLevel.one, "}");

            string[] arr = new string[] { };
            string err = "";
            string import = "";
            foreach (CKeyValue item in list)
            {
                if (string.IsNullOrEmpty(import))
                {
                    import = "using " + item.Key + ";";
                }
                else
                {
                    import += "\r\nusing " + item.Key + ";";
                }
            }

            code = code.Replace("{#import}", import);

            string f = Path.Combine(DJTools.RootPath, TempImpl.dirName);
            f = Path.Combine(f, TempImpl.libName);
            f = Path.Combine(f, Guid.NewGuid().ToString().ToLower() + ".dll");
            fList.Add(f);

            File.WriteAllLines(fPath, fList.ToArray());

            codeCompiler.SetRootPath(f);
            Assembly asse = codeCompiler.TranslateCode(arr, "v4.0", code, ref err);

            if (null != asse)
            {
                string typePath = np + "." + TempImpl.dirName + "." + TempImpl.libName + ".ManageSvrInfoImpl";
                Type type = asse.GetType(typePath);
                if (null != type)
                {
                    try
                    {
                        object o = Activator.CreateInstance(type);
                        manageSvrInfo = o as IManageSvrInfo;
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                }
            }

            return manageSvrInfo;
        }

    }
}
