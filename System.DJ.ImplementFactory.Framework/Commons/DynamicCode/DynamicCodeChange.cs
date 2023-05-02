using System.Collections.Generic;
using System.Data;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.DynamicCode
{
    class DynamicCodeChange
    {
        public string procParaSign = "-&#-";

        /// <summary>
        /// 最大参数数量
        /// </summary>
        private static int paraMaxQuantity = 1000;

        /// <summary>
        /// (?<LeftSign>.)?(?<DbTag>[\@\:\?])(?<FieldName>[a-z0-9_]+)(?<EndSign>[^a-z0-9_])?
        /// </summary>
        public static Regex rgParaField => new Regex(@"(?<LeftSign>.)?(?<DbTag>[\@\:\?])(?<FieldName>[a-z0-9_]+)(?<EndSign>[^a-z0-9_])?", RegexOptions.IgnoreCase);

        bool baseTypeWithResult(MethodInformation method)
        {
            bool mbool = (typeof(bool) == method.methodComponent.ResultType || typeof(int) == method.methodComponent.ResultType);
            return mbool;
        }

        public static bool isEnabledField(string LeftSign)
        {
            bool mbool = true;
            string ss = "@:?";
            if (!string.IsNullOrEmpty(LeftSign))
            {
                if (-1 != ss.IndexOf(LeftSign))
                {
                    mbool = false;
                }
            }
            return mbool;
        }

        /// <summary>
        /// 获取 sql 表达式所包含的参数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="func"></param>
        /// <param name="action">fiald, dbTag</param>
        public static void GetSqlParameter(ref string sql, Func<Match, bool> func, Action<string, string> action)
        {
            Regex rg = rgParaField;
            string sql1 = sql;
            if (rg.IsMatch(sql))
            {
                string LeftSign = "";
                string EndSign = "";
                string DbTag = DJTools.GetParaTagByDbDialect(DbAdapter.dbDialect);
                string _dbTag = "";
                string FieldName = "";
                string unit = "";
                string unit1 = "";
                Match match = null;
                int n = 0;
                while (rg.IsMatch(sql1) && paraMaxQuantity > n)
                {
                    match = rg.Match(sql1);
                    if (func(match))
                    {
                        FieldName = match.Groups["FieldName"].Value;
                        action(FieldName, DbTag);

                        _dbTag = match.Groups["DbTag"].Value;
                        if (!DbTag.Equals(_dbTag))
                        {
                            LeftSign = match.Groups["LeftSign"].Value;
                            EndSign = match.Groups["EndSign"].Value;

                            unit = match.Groups[0].Value;
                            unit1 = LeftSign + DbTag + FieldName + EndSign;
                            sql = sql.Replace(unit, unit1);
                        }
                    }
                    sql1 = sql1.Replace(match.Groups[0].Value, "");
                    sql1 = EndSign + sql1;
                    n++;
                }
            }
        }

        public static void GetSqlParameter(ref string sql, Action<string, string> action)
        {
            GetSqlParameter(ref sql, m =>
            {
                string LeftSign = m.Groups["LeftSign"].Value;
                return isEnabledField(LeftSign);
            },
            (field, db_tag) =>
            {
                action(field, db_tag);
            });
        }

        public string GetParametersBySqlParameter(string sqlVarName, MethodInformation method, DataOptType dataOptType, ref string sql, ref string dbParaListVarName)
        {
            string code = "";
            string sql1 = sql;
            string executeDbHelper = "{ExecuteDbHelper#}";
            string leftSpace = "";
            Regex rg = rgParaField;

            if (string.IsNullOrEmpty(dbParaListVarName) || dbParaListVarName.ToLower().Equals("null"))
            {
                dbParaListVarName = "dbParaList";
                method.append(ref code, LeftSpaceLevel.one, "DbList<System.Data.Common.DbParameter> {0} = new DbList<System.Data.Common.DbParameter>();", dbParaListVarName);
            }

            string dbHelperVarName = "dbHelper";
            string resultVarName = "successVal";
            string funcResultVarName = method.methodComponent.ResultVariantName;
            if (baseTypeWithResult(method))
            {
                method.append(ref code, LeftSpaceLevel.one, "int {0} = 0;", resultVarName);
            }
            else
            {
                method.append(ref code, LeftSpaceLevel.one, "{0} {1} = default({0});", method.methodComponent.ResultType.FullName, resultVarName);
            }
            method.append(ref code, LeftSpaceLevel.one, "string err = \"\";");
            method.append(ref code, LeftSpaceLevel.one, "IDbHelper {0} = ImplementAdapter.DbHelper;", dbHelperVarName);

            if (rg.IsMatch(sql) && 0 < method.paraList.Count)
            {
                string DbTag = DJTools.GetParaTagByDbDialect(DbAdapter.dbDialect);
                string autoCallName = method.AutoCallVarName;

                EList<CKeyValue> sqlParaList1 = new EList<CKeyValue>();
                string sqlParasVarName = "sqlParaList";
                method.append(ref code, LeftSpaceLevel.one, "EList<CKeyValue> {0} = new EList<CKeyValue>();", sqlParasVarName);
                
                GetSqlParameter(ref sql, (field, db_tag) =>
                {
                    method.append(ref code, LeftSpaceLevel.one, "{0}.Add(new CKeyValue(){ Key = \"{1}\", Value = \"{2}\", other = \"{3}\" });", sqlParasVarName, field.ToLower(), field, db_tag);
                    sqlParaList1.Add(new CKeyValue() { Key = field.ToLower(), Value = field, other = db_tag });
                });

                Regex rg1 = new Regex(@"\`[0-9]+\[");
                CKeyValue kv = null;
                PList<Para> paraList = method.paraList;
                string paraClassName = ""; //DJTools.GetParamertClassNameByDbTag(DbTag);

                foreach (Para para in paraList)
                {
                    if (para.ParaType.BaseType == typeof(System.MulticastDelegate) && rg1.IsMatch(para.ParaType.ToString())) continue;
                    if (DJTools.IsBaseType(para.ParaType))
                    {
                        //方法参数为基本类型 string, int, bool 等
                        kv = sqlParaList1[para.ParaName.ToLower()];
                        if (null == kv) continue;
                        if (para.ParaType.IsEnum)
                        {
                            method.append(ref code, LeftSpaceLevel.one, "{0}.Add(\"{2}\", DJTools.ConvertTo({3}, typeof(int));", dbParaListVarName, paraClassName, kv.Value.ToString(), para.ParaName);
                        }
                        else
                        {
                            method.append(ref code, LeftSpaceLevel.one, "{0}.Add(\"{2}\", {3});", dbParaListVarName, paraClassName, kv.Value.ToString(), para.ParaName);
                        }
                    }
                    else
                    {
                        LeftSpaceLevel leftSpaceLevel = LeftSpaceLevel.one;
                        if (null != para.ParaType.GetInterface("IEnumerable"))
                        {
                            //集合复杂类型
                            //ICollection collection = (ICollection)para.ParaValue;
                            method.append(ref code, LeftSpaceLevel.one, "");
                            method.append(ref code, LeftSpaceLevel.one, "System.Collections.ICollection collection = (System.Collections.ICollection){0};", para.ParaName);

                            method.append(ref code, LeftSpaceLevel.one, "");

                            if (null != para.ParaType.GetInterface("IDictionary"))
                            {
                                method.append(ref code, leftSpaceLevel, "//键值对情况");
                                method.append(ref code, leftSpaceLevel, "object vObj = null;");
                                method.append(ref code, leftSpaceLevel, "string key = \"\";");
                                method.append(ref code, leftSpaceLevel, "foreach (var item in collection)");
                                method.append(ref code, leftSpaceLevel, "{");
                                method.append(ref code, leftSpaceLevel + 1, "key = item.GetType().GetProperty(\"Key\").GetValue(item, null).ToString();");
                                method.append(ref code, leftSpaceLevel + 1, "vObj = item.GetType().GetProperty(\"Value\").GetValue(item, null);");
                                method.append(ref code, leftSpaceLevel + 1, "if (DJTools.IsBaseType(vObj.GetType())) break;");
                                method.append(ref code, leftSpaceLevel + 1, "if (null != vObj.GetType().GetInterface(\"IEnumerable\")) break;");
                                method.append(ref code, leftSpaceLevel + 1, "//值必须是简单类型:int, string, bool, float, double等");
                                method.append(ref code, leftSpaceLevel + 1, "");
                                //Type dataType, string dbTag, object data, string fieldName, List<DbParameter> dbParas
                                method.append(ref code, leftSpaceLevel + 1, "{0}.GetDbParaByBaseType(typeof(string),\"{1}\",vObj,key,{2});", autoCallName, DbTag, dbParaListVarName);
                                method.append(ref code, leftSpaceLevel + 1, "");

                                method.append(ref code, leftSpaceLevel, "}"); //foreach (var item in collection)
                            }
                            else if (null != para.ParaType.GetInterface("IList") || para.ParaType.IsArray)
                            {
                                leftSpace = method.StartSpace + method.getSpace((int)leftSpaceLevel);
                                method.append(ref code, leftSpaceLevel, "//List集合情况 或 数组情况");
                                method.append(ref code, leftSpaceLevel, "foreach (var item in collection)");
                                method.append(ref code, leftSpaceLevel, "{");
                                method.append(ref code, leftSpaceLevel + 1, "if (DJTools.IsBaseType(item.GetType())) break;");
                                method.append(ref code, leftSpaceLevel + 1, "if (null != item.GetType().GetInterface(\"IEnumerable\")) break;");
                                method.append(ref code, leftSpaceLevel + 1, "//集合元素必须是单体复杂对象(数据实体)");
                                //object entity, List<DbParameter> dbParas, EList<CKeyValue> paraNameList
                                method.append(ref code, leftSpaceLevel + 1, "{0}.GetDbParaListByEntity(thisMethodInfo, item, \"{1}\",{2},{3});", autoCallName, para.ParaName, dbParaListVarName, sqlParasVarName);
                                code += "\r\n" + executeDbHelper;
                                method.append(ref code, leftSpaceLevel + 1, "");
                                method.append(ref code, leftSpaceLevel, "}"); //foreach (var item in collection)
                            }
                        }
                        else if (typeof(DataTable) == para.ParaType)
                        {
                            leftSpace = method.StartSpace + method.getSpace((int)leftSpaceLevel);
                            method.append(ref code, LeftSpaceLevel.one, "");
                            method.append(ref code, LeftSpaceLevel.one, "{0} = null == {0} ? new System.Data.DataTable() : {0};", para.ParaName);
                            method.append(ref code, LeftSpaceLevel.one, "System.Data.DataTable dtable = (System.Data.DataTable){0};", para.ParaName);
                            method.append(ref code, LeftSpaceLevel.one, "EList<CKeyValue> tableColumns = new EList<CKeyValue>();");
                            method.append(ref code, LeftSpaceLevel.one, "");

                            method.append(ref code, leftSpaceLevel, "foreach(System.Data.DataColumn c in dtable.Columns)");
                            method.append(ref code, leftSpaceLevel, "{");
                            method.append(ref code, leftSpaceLevel + 1, "tableColumns.Add(new CKeyValue(){ Key = c.ColumnName.ToLower(), Value = c.ColumnName, other = c.DataType });");
                            method.append(ref code, leftSpaceLevel, "}");
                            method.append(ref code, leftSpaceLevel, "");

                            method.append(ref code, leftSpaceLevel, "foreach(System.Data.DataRow dr in dtable.Rows)");
                            method.append(ref code, leftSpaceLevel, "{");
                            //GetDbParaByDataRow(DataRow row, List<DbParameter> dbParas, EList<CKeyValue> sqlParaNameList, EList<CKeyValue> tableColumns)
                            method.append(ref code, leftSpaceLevel + 1, "{0}.GetDbParaByDataRow(dr,{1},{2},tableColumns);", autoCallName, dbParaListVarName, sqlParasVarName);
                            code += "\r\n" + executeDbHelper;
                            method.append(ref code, leftSpaceLevel + 1, "");
                            method.append(ref code, leftSpaceLevel, "}");//foreach(var dr in dtable.Rows)
                        }
                        else
                        {
                            //单体复杂类型(数据实体) 或 基本数据类型的[object]对象
                            method.append(ref code, LeftSpaceLevel.one, "");
                            method.append(ref code, LeftSpaceLevel.one, "//单体复杂类型(数据实体) ");
                            method.append(ref code, LeftSpaceLevel.one, "{0}.GetDbParaListByEntity(thisMethodInfo, {1}, \"{1}\",{2},{3});", autoCallName, para.ParaName, dbParaListVarName, sqlParasVarName);
                            method.append(ref code, LeftSpaceLevel.one, "");
                        }

                    }
                }

            }

            if (string.IsNullOrEmpty(leftSpace))
            {
                method.append(ref code, LeftSpaceLevel.one, "");
                ExecuteSqlCode(method, LeftSpaceLevel.one, dataOptType, sqlVarName, dbParaListVarName, dbHelperVarName, resultVarName, ref code);
            }
            else
            {
                string code1 = "";
                ExecuteSqlCode(method, LeftSpaceLevel.two, dataOptType, sqlVarName, dbParaListVarName, dbHelperVarName, resultVarName, ref code1);
                code = code.Replace(executeDbHelper, code1);
            }

            if (!string.IsNullOrEmpty(funcResultVarName))
            {
                method.append(ref code, LeftSpaceLevel.one, "");
                if (null == method.methodComponent.ActionType)
                {
                    ReturnResult(method, LeftSpaceLevel.one, dataOptType, funcResultVarName, resultVarName, ref code);
                }
                else
                {
                    method.append(ref code, LeftSpaceLevel.one, "{0} = {1};", funcResultVarName, resultVarName);
                }
            }

            method.append(ref code, LeftSpaceLevel.one, "if (ImplementAdapter.IsDbUsed && (null != {0}))", dbHelperVarName);
            method.append(ref code, LeftSpaceLevel.one, "{");
            method.append(ref code, LeftSpaceLevel.two, "if (null != ({0} as System.IDisposable)) ((System.IDisposable){0}).Dispose();", dbHelperVarName);
            method.append(ref code, LeftSpaceLevel.one, "}");

            return code;
        }

        void ReturnResult(MethodInformation method, LeftSpaceLevel leftSpaceLevel, DataOptType dataOptType, string funcResultVarName, string resultVarName, ref string code)
        {
            Type resultType = method.methodInfo.ReturnType;
            if (DataOptType.insert == dataOptType || DataOptType.update == dataOptType || DataOptType.delete == dataOptType)
            {
                if (typeof(bool) == resultType || typeof(Boolean) == resultType)
                {
                    method.append(ref code, leftSpaceLevel, "{0} = 0 < {1};", funcResultVarName, resultVarName);
                }
                else
                {
                    method.append(ref code, leftSpaceLevel, "{0} = {1};", funcResultVarName, resultVarName);
                }
            }
            else
            {
                method.append(ref code, leftSpaceLevel, "{0} = {1};", funcResultVarName, resultVarName);
            }
        }

        /// <summary>
        /// 解析 insert / update 与接口方法参数名称对应的sql语句, 注：该参数必须是一个数据实体List或是单个数据实体, 
        /// 例: insert into UserInfo values({UserInfos}), update UserInfo set {UserInfos} where id=@id
        /// {UserInfos} -- UserInfos 对应方法参数名称,例: int insert(List<UserInfo> UserInfos)
        /// </summary>
        /// <param name="method"></param>
        /// <param name="dataOptType"></param>
        /// <param name="sqlVarName"></param>
        /// <param name="sql"></param>
        public void AnalyzeSql(MethodInformation method, DataOptType dataOptType, string sqlVarName, ref string sql)
        {
            if (string.IsNullOrEmpty(sql)) return;
            if (dataOptType == DataOptType.select || dataOptType == DataOptType.delete) return;

            AuthenticateSql(ref sql);

            string sql1 = sql.ToLower();
            string fn = "";
            PList<Para> paras = method.paraList;
            Para para = null;
            foreach (Para item in paras)
            {
                if (DJTools.IsBaseType(item.ParaType)) continue;
                fn = "{" + item.ParaName.ToLower() + "}";
                if (-1 != sql1.IndexOf(fn))
                {
                    fn = "{" + item.ParaName + "}";
                    para = item;
                    break;
                }
            }

            if (null == para) return;

            string dbTag = DJTools.GetParaTagByDbDialect(DbAdapter.dbDialect);
            string insertFields = "";
            string insertParas = "";
            string updateSets = "";
            string procParas = "";

            if (dataOptType == DataOptType.insert)
            {
                Regex regex = new Regex(@"[0-9a-z_]+\s*\((?<insertFields>[0-9a-z_\[\]\,\s]+)\)", RegexOptions.IgnoreCase);
                if (regex.IsMatch(sql1))
                {
                    insertFields = regex.Match(sql1).Groups["insertFields"].Value;
                    string[] arr = null;
                    string fn1 = "";
                    if (-1 != insertFields.IndexOf(","))
                    {
                        arr = insertFields.Split(',');
                    }
                    else
                    {
                        arr = new string[] { insertFields };
                    }

                    foreach (var item in arr)
                    {
                        fn1 = item.Trim();
                        fn1 = fn1.Replace("[", "");
                        fn1 = fn1.Replace("]", "");
                        insertParas += "," + dbTag + fn1;
                    }

                    if (!string.IsNullOrEmpty(insertParas))
                    {
                        insertParas = insertParas.Substring(1);
                        sql = sql.Replace(fn, insertParas);
                        return;
                    }
                }
            }

            Type dataType = GetDataTypeByPara(para, fn);

            if (null != dataType)
            {
                if (DJTools.IsBaseType(dataType)) return;

                GetSegmentFromSql(method, dataType, ref insertFields, ref insertParas, ref updateSets, ref procParas);

                UseSegmentToReplaceTagInSql(para, dataOptType, insertFields, insertParas, updateSets, procParas, ref sql);
            }
        }

        void AuthenticateSql(ref string sql)
        {
            sql = sql.Replace("''", "|!|");

            string s = "";
            string s1 = "";

            Dictionary<string, string> dic = new Dictionary<string, string>();
            Regex rg1 = new Regex(@"\'((?!\').)+\'", RegexOptions.IgnoreCase);
            MatchCollection mc = null;
            if (rg1.IsMatch(sql))
            {

                mc = rg1.Matches(sql);
                int n = 0;
                foreach (Match m in mc)
                {
                    s = "_str_val_" + n;
                    s1 = m.Groups[0].Value;
                    sql = sql.Replace(s1, s);
                    dic.Add(s, s1);
                    n++;
                }
            }

            sql = sql.Replace("[", "");
            sql = sql.Replace("]", "");

            //去除 dbo.
            Regex rg2 = new Regex(@"\s+((from)|(into)|(set)|(insert)|(update)|(delete))(?<MyExpression>\s+[a-z0-9_]+\.)[a-z0-9_]+[\s\,]", RegexOptions.IgnoreCase);
            if (rg2.IsMatch(sql))
            {
                mc = rg2.Matches(sql);
                foreach (Match m in mc)
                {
                    s = m.Groups["MyExpression"].Value;
                    sql = sql.Replace(s, " ");
                }
            }

            foreach (KeyValuePair<string, string> item in dic)
            {
                s = item.Key;
                s1 = item.Value;
                sql = sql.Replace(s, s1);
            }

            sql = sql.Replace("|!|", "''");

            //Regex rg = new Regex(@"\s+((from)|(into)|(set)|(insert)|(update)|(delete))\s+(?<MyExpression>[a-z0-9_\[\]]+\.[a-z0-9_\[\]]+)\s+", RegexOptions.IgnoreCase);
            //if (rg.IsMatch(sql))
            //{
            //    string err = "表达式 " + rg.Match(sql).Groups["MyExpression"].Value + " 是不允许的格式";
            //    throw new Exception(err);
            //}
        }

        void UseSegmentToReplaceTagInSql(Para para, DataOptType dataOptType, string insertFields, string insertParas, string updateSets, string procParas, ref string sql)
        {
            if (!string.IsNullOrEmpty(insertFields))
            {
                string FieldTag = "";
                string fnStr = "";
                string fn = "";
                string Values = "";
                Match mc = null;
                Regex rg = null;
                if (dataOptType == DataOptType.insert)
                {
                    //insert into TableName ({ParameterNameOfMethod}) values 
                    //或 insert into TableName {ParameterNameOfMethod} values
                    //或 insert into TableName ({ParameterNameOfMethod})
                    //或 insert into TableName {ParameterNameOfMethod}
                    rg = new Regex(@"insert\s+(into\s+)?[a-z0-9_\-]+\s*(?<FieldTag>((\(\{" + para.ParaName + @"\}\))|(\{" + para.ParaName + @"\})))(\s+(?<Values>values[^a-z0-9_]))?", RegexOptions.IgnoreCase);
                    if (rg.IsMatch(sql))
                    {
                        mc = rg.Match(sql);
                        fn = mc.Groups[0].Value;
                        FieldTag = mc.Groups["FieldTag"].Value;
                        Values = mc.Groups["Values"].Value;
                        fnStr = fn.Replace(FieldTag, "(" + insertFields + ")");
                        if (string.IsNullOrEmpty(Values))
                        {
                            fnStr += " values ";
                        }
                    }

                    if (string.IsNullOrEmpty(fnStr))
                    {
                        //insert into TableName values ({ParameterNameOfMethod})
                        //insert into TableName values {ParameterNameOfMethod}
                        rg = new Regex(@"insert\s+(into\s+)?(?<FieldTag>[a-z0-9_\-]+)(?<Values>\s+values)((\s*\(\s*\{" + para.ParaName + @"\}\s*\))|(\s*\{" + para.ParaName + @"\}))", RegexOptions.IgnoreCase);
                        if (rg.IsMatch(sql))
                        {
                            mc = rg.Match(sql);
                            fn = mc.Groups[0].Value;
                            FieldTag = mc.Groups["FieldTag"].Value;
                            Values = mc.Groups["Values"].Value;
                            fnStr = fn.Replace(" " + FieldTag + Values, " " + FieldTag + "(" + insertFields + ") values");
                        }
                    }

                    if (string.IsNullOrEmpty(fnStr)) return;
                    sql = sql.Replace(fn, fnStr);
                    fn = "";
                    fnStr = "";
                    // values ({ParameterNameOfMethod})
                    rg = new Regex(@"\s+values\s*(?<FieldTag>((\(\{" + para.ParaName + @"\}\))|(\{" + para.ParaName + @"\})))?(?<TagSign>\s*[^\s])?", RegexOptions.IgnoreCase);
                    if (rg.IsMatch(sql))
                    {
                        mc = rg.Match(sql);
                        fn = mc.Groups[0].Value;
                        FieldTag = mc.Groups["FieldTag"].Value;
                        string TagSign = mc.Groups["TagSign"].Value;

                        if (!string.IsNullOrEmpty(TagSign))
                        {
                            fn = fn.Substring(0, fn.Length - TagSign.Length);
                        }

                        if (!string.IsNullOrEmpty(FieldTag))
                        {
                            fnStr = fn.Replace(FieldTag, "(" + insertParas + ")");
                        }
                        else
                        {
                            string end = string.IsNullOrEmpty(TagSign) ? "" : " ";
                            fnStr = fn + "(" + insertParas + ")" + end;
                        }
                    }

                    if (string.IsNullOrEmpty(fnStr)) return;
                    sql = sql.Replace(fn, fnStr);
                }
                else if (dataOptType == DataOptType.update)
                {
                    //update
                    rg = new Regex(@"update\s+[a-z0-9_\-]+\s+set\s*(?<FieldTag>\{" + para.ParaName + @"\})", RegexOptions.IgnoreCase);
                    if (rg.IsMatch(sql))
                    {
                        mc = rg.Match(sql);
                        FieldTag = mc.Groups["FieldTag"].Value;
                    }

                    if (string.IsNullOrEmpty(FieldTag)) return;
                    sql = sql.Replace(FieldTag, updateSets);
                }
                else if (dataOptType == DataOptType.procedure)
                {
                    //procedure
                    rg = new Regex(@"(?<FieldTag>\{" + para.ParaName + @"\})", RegexOptions.IgnoreCase);
                    if (rg.IsMatch(sql))
                    {
                        mc = rg.Match(sql);
                        FieldTag = mc.Groups["FieldTag"].Value;
                    }

                    if (string.IsNullOrEmpty(FieldTag)) return;
                    sql = sql.Replace(FieldTag, procParas);
                }
            }
        }

        Type GetDataTypeByPara(Para para, string fieldName)
        {
            Type dataType = null;
            if (null != para.ParaType.GetInterface("IEnumerable"))
            {
                //集合类型
                if (null != para.ParaType.GetInterface("IDictionary"))
                {
                    throw new Exception("非法使用 <Dictionary> 类型参数来做为 " + fieldName + " 模糊构建操作");
                    //未调用方法之前，是无法得知方法参数 Dictionary 键值对的
                }
                else if (null != para.ParaType.GetInterface("IList") || para.ParaType.IsArray)
                {
                    //List 和 数组 类型     
                    if (null != para.ParaType.GetInterface("IList"))
                    {
                        //List
                        Type[] types = para.ParaType.GetGenericArguments();
                        if (0 < types.Length) dataType = types[0];
                    }
                    else
                    {
                        //数组
                        Type[] types = para.ParaType.Assembly.GetExportedTypes();
                        if (0 < types.Length) dataType = types[0];
                    }
                }
                else if (typeof(DataEntity<DataElement>) == para.ParaType)
                {
                    dataType = para.ParaType;
                }
            }
            else if (para.ParaType == typeof(DataTable))
            {
                throw new Exception("非法使用 <DataTable> 类型参数来做为 " + fieldName + " 模糊构建操作");
                //未调用方法之前，是无法得知方法参数 DataTable 表结构的
            }
            else
            {
                //单体类型
                dataType = para.ParaType;
            }
            return dataType;
        }

        public void GetSegmentFromSql(MethodInformation method, Type dataType, ref string insertFields, ref string insertParas, ref string updateSets, ref string procParas)
        {
            string dbTag = DJTools.GetParaTagByDbDialect(DbAdapter.dbDialect);
            string insertFields1 = "";
            string insertParas1 = "";
            string updateSets1 = "";
            string procParas1 = "";

            bool mbool = false;
            string[] _fields = method.fields;
            FieldsType fieldsType = method.fieldsType;
            _fields = null == _fields ? new string[] { } : _fields;

            Attribute attr = null;
            Dictionary<string, string> fDic = new Dictionary<string, string>();
            foreach (string field in _fields)
            {
                if (fDic.ContainsKey(field)) continue;
                fDic.Add(field.ToLower(), field);
            }

            Action<string, string, string, PropertyInfo> action = (fn1, fn2, fn_lower, pi) =>
            {
                attr = pi.GetCustomAttribute(typeof(Attrs.Constraint), true);
                if (null != attr) return;

                mbool = true;
                if (fDic.ContainsKey(fn_lower)) mbool = false;
                
                if (FieldsType.Contain == fieldsType && 0 < _fields.Length)
                {
                    mbool = !mbool;
                }

                if (mbool)
                {
                    IgnoreField.IgnoreType ignoreType = IgnoreField.IgnoreType.none;
                    if (null != pi)
                    {
                        Attribute att = pi.GetCustomAttribute(typeof(IgnoreField));
                        if (null != att) ignoreType = ((IgnoreField)att).ignoreType;
                    }

                    fn2 = string.IsNullOrEmpty(fn2) ? fn1 : fn2;
                    if (IgnoreField.IgnoreType.Insert != (ignoreType & IgnoreField.IgnoreType.Insert))
                    {
                        insertFields1 += "," + fn2;
                        insertParas1 += "," + dbTag + fn1;
                    }

                    if (IgnoreField.IgnoreType.Update != (ignoreType & IgnoreField.IgnoreType.Update))
                    {
                        updateSets1 += "," + fn2 + "=" + dbTag + fn1;
                    }

                    if (IgnoreField.IgnoreType.Procedure != (ignoreType & IgnoreField.IgnoreType.Procedure))
                    {
                        procParas1 += "," + procParaSign + fn2 + "=" + dbTag + fn1;
                    }
                }

            };

            string fn = "";
            string fn_2 = "";
            if (typeof(DataEntity<DataElement>) == dataType || typeof(List<DataEntity<DataElement>>) == dataType)
            {
                object paraVal = null;
                if (typeof(List<DataEntity<DataElement>>) == dataType)
                {
                    List<DataEntity<DataElement>> dataElements1 = (List<DataEntity<DataElement>>)method.paraList[0].ParaValue;
                    if (0 < dataElements1.Count)
                    {
                        paraVal = dataElements1[0];
                    }
                }
                else
                {
                    if (null != method.paraList[0].ParaValue)
                    {
                        paraVal = (DataEntity<DataElement>)method.paraList[0].ParaValue;
                    }
                }

                if (null == paraVal) paraVal = new DataEntity<DataElement>();
                DataEntity<DataElement> dataElements = (DataEntity<DataElement>)paraVal;
                foreach (DataElement item in dataElements)
                {
                    if (!string.IsNullOrEmpty(item.fieldNameOfSourceTable)) continue;
                    fn = item.name.ToLower();
                    action(item.name, fn_2, fn, null);
                }
            }
            else if (null != dataType.GetInterface("IDictionary"))
            {
                //
            }
            else
            {
                Type t = dataType;
                if (typeof(Collections.IEnumerable) == dataType.GetInterface("IEnumerable"))
                {
                    Type[] types = dataType.GetGenericArguments();
                    if (null != dataType.GetInterface("IList"))
                    {
                        t = types[0];
                    }
                    else if (dataType.IsArray)
                    {
                        t = dataType.GetElementType();
                    }
                }

                ForeachExtends fe = new ForeachExtends();
                fe.ForeachProperty(t, (item, pt, fieldName) =>
                {
                    fn_2 = FieldMapping.GetFieldMapping(item);
                    fn = item.Name.ToLower();
                    action(item.Name, fn_2, fn, item);
                });
            }

            if (!string.IsNullOrEmpty(insertFields1))
            {
                insertFields1 = insertFields1.Substring(1);
                insertParas1 = insertParas1.Substring(1);
            }

            if (!string.IsNullOrEmpty(updateSets1))
            {
                updateSets1 = updateSets1.Substring(1);
            }

            if (!string.IsNullOrEmpty(procParas1))
            {
                procParas1 = procParas1.Substring(1);
            }

            insertFields = insertFields1;
            insertParas = insertParas1;
            updateSets = updateSets1;
            procParas = procParas1;
        }

        void ExecuteSqlCode(MethodInformation method, LeftSpaceLevel leftSpaceLevel, DataOptType dataOptType, string sqlVarName, string paraListVarName, string dbHelperVarName, string returnVarName, ref string code)
        {
            if (DataOptType.select == dataOptType) return;
            //method.append(ref code, leftSpaceLevel, "");
            string methodName = "";
            if (DataOptType.none == method.sqlExecType)
            {
                switch (dataOptType)
                {
                    case DataOptType.insert:
                        methodName = "insert";
                        break;
                    case DataOptType.update:
                        methodName = "update";
                        break;
                    case DataOptType.delete:
                        methodName = "delete";
                        break;
                }
            }
            else
            {
                methodName = Enum.GetName(typeof(DataOptType), method.sqlExecType);
            }

            string autCall = method.AutoCallVarName;
            string enabledBuffer = method.methodComponent.EnabledBuffer.ToString().ToLower();
            enabledBuffer = method.methodComponent.IsAsync ? "true" : enabledBuffer;
            method.append(ref code, leftSpaceLevel, "if(null != {0})", dbHelperVarName);
            method.append(ref code, leftSpaceLevel, "{");
            if (DataOptType.select == method.sqlExecType || (DataOptType.procedure == dataOptType && DJTools.IsBaseType(method.methodComponent.ResultType)))
            {
                string dbTag = DJTools.GetParaTagByDbDialect(DbAdapter.dbDialect);
                method.append(ref code, leftSpaceLevel + 1, "{0} = {0}.Replace(\"{1}\", \"{2}\");", sqlVarName, procParaSign, dbTag);
                method.append(ref code, leftSpaceLevel + 1, "{4}.query({0}, {1}, {2}, {3}, dataTable =>", autCall, sqlVarName, paraListVarName, enabledBuffer, dbHelperVarName);
                method.append(ref code, leftSpaceLevel + 1, "{");
                #region ***** action
                method.append(ref code, leftSpaceLevel + 2, "dataTable = null == dataTable ? new System.Data.DataTable() : dataTable;");
                method.append(ref code, leftSpaceLevel + 2, "if(0 < dataTable.Rows.Count)");
                method.append(ref code, leftSpaceLevel + 2, "{");
                method.append(ref code, leftSpaceLevel + 3, "string valStr = System.DBNull.Value == dataTable.Rows[0][0] ? \"\" : dataTable.Rows[0][0].ToString();");
                if (baseTypeWithResult(method))
                {
                    method.append(ref code, leftSpaceLevel + 3, "int tempVal = 0;");
                    method.append(ref code, leftSpaceLevel + 3, "int.TryParse(valStr, out tempVal);");
                    method.append(ref code, leftSpaceLevel + 3, "{0} = tempVal;", returnVarName);
                }
                else if (typeof(string) == method.methodComponent.ResultType)
                {
                    method.append(ref code, leftSpaceLevel + 3, "{0} = valStr;", returnVarName);
                }
                else
                {
                    method.append(ref code, leftSpaceLevel + 3, "{0}.TryParse(valStr, out {1});", method.methodComponent.ResultType.FullName, returnVarName);
                }

                method.append(ref code, leftSpaceLevel + 2, "}");
                method.append(ref code, leftSpaceLevel + 2, "else");
                method.append(ref code, leftSpaceLevel + 2, "{");
                method.append(ref code, leftSpaceLevel + 3, "{0} = 0;", returnVarName);
                method.append(ref code, leftSpaceLevel + 2, "}");
                if (null != method.methodComponent.ActionType)
                {
                    method.append(ref code, leftSpaceLevel + 2, "{0}({1});", method.methodComponent.ActionParaName, returnVarName);
                }
                #endregion                
                method.append(ref code, leftSpaceLevel + 1, "}, ref err);");
            }
            else
            {
                method.append(ref code, leftSpaceLevel + 1, "{0} += {6}.{1}({2}, {3}, {4},{5}, resultNum =>", returnVarName, methodName, autCall, sqlVarName, paraListVarName, enabledBuffer, dbHelperVarName);
                method.append(ref code, leftSpaceLevel + 1, "{");
                #region ***** action
                method.append(ref code, leftSpaceLevel + 2, "{0} += resultNum;", returnVarName);
                if (null != method.methodComponent.ActionType)
                {
                    method.append(ref code, leftSpaceLevel + 2, "{0}(resultNum);", method.methodComponent.ActionParaName, returnVarName);
                }
                #endregion                
                method.append(ref code, leftSpaceLevel + 1, "}, ref err);");
            }
            method.append(ref code, leftSpaceLevel + 1, "");
            method.append(ref code, leftSpaceLevel + 1, "if(!string.IsNullOrEmpty(err))");
            method.append(ref code, leftSpaceLevel + 1, "{");
            method.append(ref code, leftSpaceLevel + 2, "throw new Exception(err);");
            method.append(ref code, leftSpaceLevel + 1, "}");
            method.append(ref code, leftSpaceLevel, "}");
        }

    }
}
