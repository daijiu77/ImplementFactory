using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory.Commons.DynamicCode;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    public class CommonMethods
    {
        public MethodBase GetSrcTypeMethod(params Type[] excludeTypes)
        {
            StackTrace trace = new StackTrace(true);
            StackFrame stackFrame = null;
            MethodBase methodBase = null;
            Type meType = typeof(CommonMethods);
            Type pt = null;
            Regex rg = new Regex(@"^\<\>.+", RegexOptions.IgnoreCase);
            Regex rg1 = new Regex(@"^(?<ClsName>.+)\+\<\>c__DisplayClass[0-9]+", RegexOptions.IgnoreCase);
            Regex rg2 = new Regex(@"^\<((?<ClsName>.+)\.)?(?<MethodName>[a-z0-9_]+)\>", RegexOptions.IgnoreCase);
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
            string ClsName1 = "";
            string ClsName2 = "";
            string MethodName2 = "";
            string mName = "";
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
                mName = methodBase.Name;
                if (null == mName) mName = "";
                /*当出现 Task 异步调用 lambda 时,例 Task.Run(() => { })
                 * spName: UserService.DataSources.Implements.PM_UserDataSourceImpl+<>c__DisplayClass10_0
                 * mName: <UserService.DataSources.IPM_UserDataSource.QueryByUserAccount>b__1 或 <get_User>b__0
                 **/
                if (rg1.IsMatch(spName) && rg2.IsMatch(mName))
                {
                    Type[] paramaterTypes = null;
                    PropertyInfo pi = pt.GetType().GetPropertyInfo("DeclaredFields");
                    if (null != pi)
                    {
                        var DeclaredFields = pi.GetValue(pt, null);
                        if (null != DeclaredFields)
                        {
                            ICollection collection = DeclaredFields as ICollection;
                            if (null != collection)
                            {
                                string fName = "";
                                FieldInfo field = null;
                                List<Type> typeList = new List<Type>();
                                Regex rg3 = new Regex(@"\<\>[a-z0-9_]+", RegexOptions.IgnoreCase);
                                foreach (var item in collection)
                                {
                                    if (null == (item as FieldInfo)) continue;
                                    field = item as FieldInfo;
                                    fName = field.Name;
                                    if (rg3.IsMatch(fName)) continue;

                                    typeList.Add(field.FieldType);
                                }
                                paramaterTypes = typeList.ToArray();
                            }
                        }
                    }
                    ClsName1 = rg1.Match(spName).Groups["ClsName"].Value;

                    Match m2 = rg2.Match(mName);
                    ClsName2 = m2.Groups["ClsName"].Value;
                    MethodName2 = m2.Groups["MethodName"].Value;
                    if (string.IsNullOrEmpty(ClsName2)) ClsName2 = ClsName1;
                    pt = DJTools.GetClassTypeByPath(ClsName2);
                    if (null != pt)
                    {
                        if (null == paramaterTypes) paramaterTypes = new Type[] { };
                        MethodInfo interfaceMethod = pt.GetMethod(MethodName2,
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, paramaterTypes, null);
                        if (null == interfaceMethod)
                        {
                            num++;
                            continue;
                        }

                        if (pt.IsInterface)
                        {
                            Type implType = DJTools.GetClassTypeByPath(ClsName1);
                            if (null == implType)
                            {
                                num++;
                                continue;
                            }

                            EMethodInfo eMethodInfo = new EMethodInfo(interfaceMethod);
                            methodBase = eMethodInfo.GetImplementMethodBy(interfaceMethod, implType);
                        }
                        else
                        {
                            methodBase = interfaceMethod;
                        }

                        if (null != methodBase)
                        {
                            break;
                        }
                        else
                        {
                            num++;
                            continue;
                        }
                    }
                }
                else if (spName.Length >= spLen)
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
