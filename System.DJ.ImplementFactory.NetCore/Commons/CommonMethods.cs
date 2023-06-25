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
                 *        或 泛型类方法 <UserService.DataSources.IPM_UserDataSource<System.String>.QueryByUserAccount>b__1
                 **/
                if (rg1.IsMatch(spName) && rg2.IsMatch(mName))
                {
                    Type[] paramaterTypes = null;
                    PropertyInfo pi = pt.GetType().GetPropertyInfo("DeclaredFields");
                    Regex rg3 = null;
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
                                rg3 = new Regex(@"\<\>[a-z0-9_]+", RegexOptions.IgnoreCase);
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
                    rg3 = new Regex(@"^(?<ClsName>[a-z0-9_\.]+)\<(?<GType>[a-z0-9_\.\,\s]+)\>$", RegexOptions.IgnoreCase);
                    ClsName1 = rg1.Match(spName).Groups["ClsName"].Value;
                    Type[] genericTypes1 = null;
                    string GType = "";
                    Match match = null;
                    if (rg3.IsMatch(ClsName1))
                    {
                        //泛型类 UserService.DataSources.IPM_UserDataSource<System.String>
                        match = rg3.Match(ClsName1);
                        ClsName1 = match.Groups["ClsName"].Value + "`1";
                        GType = match.Groups["GType"].Value.Trim();
                        genericTypes1 = GenericTypes(GType);
                    }

                    Match m2 = rg2.Match(mName);
                    ClsName2 = m2.Groups["ClsName"].Value;
                    MethodName2 = m2.Groups["MethodName"].Value;
                    if (string.IsNullOrEmpty(ClsName2)) ClsName2 = ClsName1;
                    Type[] genericTypes2 = null;
                    if (rg3.IsMatch(ClsName2))
                    {
                        //泛型类 UserService.DataSources.IPM_UserDataSource<System.String>
                        match = rg3.Match(ClsName2);
                        ClsName2 = match.Groups["ClsName"].Value + "`1";
                        GType = match.Groups["GType"].Value.Trim();
                        genericTypes2 = GenericTypes(GType);
                    }
                    pt = DJTools.GetClassTypeByPath(ClsName2);
                    if (null != pt)
                    {
                        if (null != genericTypes2)
                        {
                            //创建泛型类型
                            pt = pt.MakeGenericType(genericTypes2);
                        }
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

                            if (null != genericTypes1)
                            {
                                //创建泛型类型
                                implType = implType.MakeGenericType(genericTypes1);
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
                    else
                    {
                        num++;
                        continue;
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

        private Type[] GenericTypes(string types)
        {
            Type[] genericTypes = null;
            if (string.IsNullOrEmpty(types)) return genericTypes;
            string[] names = types.Split(',');
            genericTypes = new Type[names.Length];
            int n = 0;
            foreach (var item in names)
            {
                genericTypes[n] = DJTools.GetTypeByFullName(item.Trim());
                n++;
            }
            return genericTypes;
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
