using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public class CommonMethods
    {
        public MethodBase GetSrcTypeMethod(params Type[] excludeTypes)
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = null;
            MethodBase methodBase = null;
            Type meType = typeof(CommonMethods);
            Type pt = null;
            Regex rg = new Regex(@"^\<\>.+", RegexOptions.IgnoreCase);
            Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
            if (null != excludeTypes)
            {
                foreach (var item in excludeTypes)
                {
                    dic[item] = item;
                }
            }
            const int maxNum = 50;
            const string threadSP = "System.Threading";
            Dictionary<string, string> kDic = new Dictionary<string, string>();
            kDic.Add(threadSP.ToLower(), threadSP);

            int spLen = threadSP.Length;
            string tpName = "";
            string spName = "";
            int num = 0;
            bool isExist = false;
            while (num <= maxNum)
            {
                stackFrame = trace.GetFrame(num);
                if (null == stackFrame) break;
                methodBase = stackFrame.GetMethod();
                if (null == methodBase) break;
                pt = methodBase.DeclaringType;
                if (null == pt) break;
                spName = pt.FullName;
                if (null == spName) spName = pt.Namespace;
                if (null == spName) spName = "";
                if (spName.Length >= spLen)
                {
                    spName = spName.Substring(0, spLen).ToLower();
                    if (kDic.ContainsKey(spName))
                    {
                        num++;
                        continue;
                    }
                }

                tpName = pt.Name;
                if (!string.IsNullOrEmpty(tpName))
                {
                    if (tpName.Equals("Task`1"))
                    {
                        num++;
                        continue;
                    }
                }
                isExist = dic.ContainsKey(pt);
                if ((false == isExist) && (pt != meType) && (false == rg.IsMatch(pt.Name)))
                {
                    break;
                }
                num++;
            }

            return methodBase;
        }

        public MethodBase GetSrcTypeMethod<T>()
        {
            Type t = typeof(T);
            return GetSrcTypeMethod(t);
        }

        public Type GetSrcType(params Type[] excludeTypes)
        {
            MethodBase methodBase = GetSrcTypeMethod(excludeTypes);
            if (null != methodBase) return methodBase.DeclaringType;
            return null;
        }

        public Type GetSrcType<T>()
        {
            MethodBase methodBase = GetSrcTypeMethod<T>();
            if (null != methodBase) return methodBase.DeclaringType;
            return null;
        }
    }
}
