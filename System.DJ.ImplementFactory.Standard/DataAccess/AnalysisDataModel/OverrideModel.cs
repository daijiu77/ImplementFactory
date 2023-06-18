using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.DJ.ImplementFactory.Pipelines;
using System.IO;
using System.Net;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess.AnalysisDataModel
{
    public class OverrideModel
    {
        public const string CopyParentModel = "CopyParentModel";

        private const string WhereIgnoreFieldLazy = "WhereIgnoreFieldLazy";

        private Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
        private enum PropType
        {
            none,
            isArray,
            isList,
            isClass
        }

        class ModelPropertyInfo
        {
            private Dictionary<string, PropertyInfo> dicProp = new Dictionary<string, PropertyInfo>();
            public PropertyInfo this[string field]
            {
                get
                {
                    if (string.IsNullOrEmpty(field)) return null;
                    string fn = field.Trim().ToLower();
                    if (dicProp.ContainsKey(fn)) return dicProp[fn];
                    return null;
                }
            }

            public void Add(string field, PropertyInfo pi)
            {
                string fn = field.Trim().ToLower();
                if (dicProp.ContainsKey(fn)) return;
                dicProp.Add(fn, pi);
            }

            public bool ContainsKey(string field)
            {
                string fn = field.Trim().ToLower();
                return dicProp.ContainsKey(fn);
            }
        }

        private Dictionary<Type, ModelPropertyInfo> dicMProp = new Dictionary<Type, ModelPropertyInfo>();
        private Type GetPropertyType(Type modelType, string fieldName)
        {
            Type pt = null;
            fieldName = fieldName.ToLower().Trim();
            ModelPropertyInfo modelPropertyInfo = null;
            if (dicMProp.ContainsKey(modelType))
            {
                modelPropertyInfo = dicMProp[modelType];
            }
            else
            {
                modelPropertyInfo = new ModelPropertyInfo();
                dicMProp.Add(modelType, modelPropertyInfo);
                Attribute att = null;
                FieldMapping fm = null;
                string field = "";
                modelType.ForeachProperty((pi, type, fn) =>
                {
                    if (!modelPropertyInfo.ContainsKey(fn)) modelPropertyInfo.Add(fn, pi);
                    att = pi.GetCustomAttribute(typeof(FieldMapping));
                    if (null != att) fm = (FieldMapping)att;
                    if (null == fm) return true;
                    field = fm.FieldName;
                    if (!modelPropertyInfo.ContainsKey(field)) modelPropertyInfo.Add(field, pi);
                    return true;
                });
            }
            PropertyInfo pi1 = modelPropertyInfo[fieldName];
            if (null != pi1) pt = pi1.PropertyType;
            return pt;
        }

        private bool IsInheritAbsDataModel(Type modelType)
        {
            if (null == modelType) return false;
            return typeof(AbsDataModel).IsAssignableFrom(modelType);
        }

        public object CreateDataModel(Type dataModelType)
        {
            return CreateDataModel(dataModelType, null);
        }

        public object CreateDataModel(Type dataModelType, DbSqlBody dbSqlBody)
        {
            object dtModel = null;
            if (null == dataModelType) return null;
            if (!typeof(AbsDataModel).IsAssignableFrom(dataModelType)) return null;
            string newNamespace = dataModelType.Namespace + ".ModelMirror";
            string newClassName = dataModelType.Name + "Copy";
            string typeName = newNamespace + "." + newClassName;

            Dictionary<string, List<string>> ignoreFieldsDic = null;
            Dictionary<string, OrderbyList<OrderbyItem>> lazyOrderbyDic = null;
            OrderbyList<OrderbyItem> orderbyItemList = null;
            List<string> whereIgnoreList = null;
            if (null != dbSqlBody)
            {
                if (0 < dbSqlBody.LazyIgonreDictionary.Count) ignoreFieldsDic = dbSqlBody.LazyIgonreDictionary;
                if (0 < dbSqlBody.WhereIgnoreList.Count) whereIgnoreList = dbSqlBody.WhereIgnoreList;

                if (0 < dbSqlBody.LazyOrderbyDictionary.Count) lazyOrderbyDic = dbSqlBody.LazyOrderbyDictionary;
                if (0 < dbSqlBody.OrderbyItemList.Count) orderbyItemList = dbSqlBody.OrderbyItemList;
            }

            Type type1 = DJTools.GetDynamicType(typeName);
            if ((null != type1) && (null == dbSqlBody))
            {
                dtModel = Activator.CreateInstance(type1);
                return dtModel;
            }

            if (dic.ContainsKey(dataModelType))
            {
                type1 = dic[dataModelType];
                dtModel = Activator.CreateInstance(type1);
                return dtModel;
            }

            Attribute att = null;
            Constraint constraint = null;
            string code = "";
            string codeBody = "";
            string pro = "";
            string s = "";
            string GetBody = "";
            string tag = "";
            string whereIgnoreStr = "";
            string orderbyStr = "";
            string model_type = dataModelType.Name.ToLower();
            string model_fullType = dataModelType.TypeToString(true).ToLower();

            string fnLower = "";
            string type_name = "";
            string field_name = "";
            int dotIndex = 0;
            bool mbool = false;

            const string getFlag = "{#GetBody}";
            Type pt = null;
            Type[] types = null;
            PropType propType = PropType.none;
            int level = 0;

            if (null != whereIgnoreList)
            {
                whereIgnoreStr = string.Join("\",\"", whereIgnoreList);
                whereIgnoreStr = "\"" + whereIgnoreStr + "\"";
            }

            if (null != orderbyItemList)
            {
                foreach (OrderbyItem item in orderbyItemList)
                {
                    orderbyStr += ",OrderbyItem.Me.Set(\"{0}\", OrderByRule.{1})".ExtFormat(item.FieldName, (OrderByRule.Asc == item.Rule ? "Asc" : "Desc"));
                }
                orderbyStr = orderbyStr.Substring(1);
            }

            EList<CKeyValue> uskv = new EList<CKeyValue>();
            uskv.Add(new CKeyValue() { Key = "System" });
            uskv.Add(new CKeyValue() { Key = "System.Diagnostics" });
            uskv.Add(new CKeyValue() { Key = "System.Reflection" });
            uskv.Add(new CKeyValue() { Key = "System.Collections.Generic" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Pipelines" });
            uskv.Add(new CKeyValue() { Key = "System.DJ.ImplementFactory.Commons.Attrs" });
            uskv.Add(new CKeyValue() { Key = typeof(DJTools).Namespace });
            uskv.Add(new CKeyValue() { Key = typeof(OrderbyItem).Namespace });

            dataModelType.ForeachProperty((pi, type, fn) =>
            {
                fnLower = fn.ToLower();
                pro = "";
                level = 2;

                level++;
                if (pi.CanRead)
                {
                    DJTools.append(ref pro, level, "get");
                    DJTools.append(ref pro, level, "{");
                    DJTools.append(ref pro, 0, getFlag);
                    DJTools.append(ref pro, level + 1, "return base.{0};", fn);
                    DJTools.append(ref pro, level, "}");
                }

                if (pi.CanWrite)
                {
                    DJTools.append(ref pro, level, "set");
                    DJTools.append(ref pro, level, "{");
                    DJTools.append(ref pro, level + 1, "base.{0} = value;", fn);
                    DJTools.append(ref pro, level, "}");
                }
                level--;

                s = "";
                if (pi.CanRead)
                {
                    constraint = null;
                    GetBody = "";
                    att = pi.GetCustomAttribute(typeof(Constraint));
                    if (null != att)
                    {
                        constraint = (Constraint)att;
                    }

                    if (null != constraint)
                    {
                        propType = PropType.none;
                        typeName = "";
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && (typeof(string) != type))
                        {
                            if (type.IsArray)
                            {
                                propType = PropType.isArray;
                                typeName = type.TypeToString(true);
                                typeName = typeName.Replace("[]", "");
                                uskv.Add(new CKeyValue() { Key = type.Namespace });
                            }
                            else
                            {
                                types = type.GetGenericArguments();
                                if (0 < types.Length)
                                {
                                    propType = PropType.isList;
                                    typeName = types[0].TypeToString(true);
                                    uskv.Add(new CKeyValue() { Key = types[0].Namespace });
                                }
                            }
                        }
                        else if (!type.IsBaseType())
                        {
                            propType = PropType.isClass;
                            typeName = type.TypeToString(true);
                            uskv.Add(new CKeyValue() { Key = type.Namespace });
                        }

                        pt = DJTools.GetTypeByFullName(typeName);
                        if (!IsInheritAbsDataModel(pt)) typeName = "";

                        if (!string.IsNullOrEmpty(typeName))
                        {
                            level += 2;
                            uskv.Add(new CKeyValue() { Key = typeof(DbVisitor).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(IDbSqlScheme).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(SqlFromUnit).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(ConditionItem).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(ConditionRelation).Namespace });
                            DJTools.append(ref GetBody, level, "if (null != base.{0})", fn);
                            DJTools.append(ref GetBody, level, "{");
                            if (PropType.isArray == propType)
                            {
                                DJTools.append(ref GetBody, level + 1, "if (0 < base.{0}.Length) return base.{0};", fn);
                            }
                            else if (PropType.isList == propType)
                            {
                                DJTools.append(ref GetBody, level + 1, "if (0 < base.{0}.Count) return base.{0};", fn);
                            }
                            else
                            {
                                DJTools.append(ref GetBody, level + 1, "return base.{0};", fn);
                            }
                            DJTools.append(ref GetBody, level, "}");
                            DJTools.append(ref GetBody, level, "DbVisitor db = new DbVisitor();");
                            DJTools.append(ref GetBody, level, "IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.New.From<{0}>());", typeName);
                            pt = GetPropertyType(dataModelType, constraint.ForeignKey);
                            s = DJTools.ExtFormat("ConditionItem.Me.And(\"{0}\", ConditionRelation.Equals, this.{1}, typeof({2}))", constraint.RefrenceKey, constraint.ForeignKey, pt.TypeToString(true));
                            if (null != constraint.Foreign_refrenceKeys)
                            {
                                int x = 0;
                                foreach (string k in constraint.Foreign_refrenceKeys)
                                {
                                    if (0 == x)
                                    {
                                        pt = GetPropertyType(dataModelType, k);
                                        if (null == pt) break;
                                        s += ", ConditionItem.Me.And(\"{0}\", ConditionRelation.Equals, this.{1}, typeof({2}))";
                                        s = s.Replace("{1}", k);
                                        s = s.Replace("{2}", pt.TypeToString(true));
                                        x = 1;
                                    }
                                    else
                                    {
                                        s = s.Replace("{0}", k);
                                        x = 0;
                                    }
                                }
                            }
                            DJTools.append(ref GetBody, level, "scheme.dbSqlBody.Where({0});", s);

                            if (null != whereIgnoreList)
                            {
                                if (typeName.ToLower().Equals(model_fullType))
                                {
                                    DJTools.append(ref GetBody, level, "scheme.dbSqlBody.WhereIgnore({0});", whereIgnoreStr);
                                }
                            }

                            if (null != ignoreFieldsDic)
                            {
                                string fns = "";
                                string ignoreStr = "";
                                foreach (var item in ignoreFieldsDic)
                                {
                                    if (null == item.Value) continue;
                                    if (0 == item.Value.Count) continue;
                                    type_name = "";
                                    mbool = true;
                                    field_name = item.Key.ToLower();
                                    dotIndex = field_name.IndexOf(".");
                                    if (0 < dotIndex)
                                    {
                                        type_name = field_name.Substring(0, dotIndex);
                                        field_name = field_name.Substring(dotIndex + 1);
                                        mbool = type_name.Equals(model_type);
                                    }
                                    fns = string.Join("\", \"", item.Value.ToArray());
                                    if (!string.IsNullOrEmpty(fns)) fns = "\"" + fns + "\"";
                                    if (field_name.Equals(fnLower) && mbool)
                                    {
                                        DJTools.append(ref GetBody, level, "scheme.dbSqlBody.WhereIgnore({0});", fns);
                                    }
                                    ignoreStr += ".WhereIgnoreLazy(\"{0}\", {1})".ExtFormat(item.Key.ToLower(), fns);
                                }
                                DJTools.append(ref GetBody, level, "scheme.dbSqlBody{0};", ignoreStr);
                            }

                            if (null != orderbyItemList)
                            {
                                if (typeName.ToLower().Equals(model_fullType))
                                {
                                    DJTools.append(ref GetBody, level, "scheme.dbSqlBody.Orderby({0});", orderbyStr);
                                }
                            }

                            if (null != lazyOrderbyDic)
                            {
                                string lazyOrderbyStr = "";
                                string los = "";
                                foreach (var item in lazyOrderbyDic)
                                {
                                    if (null == item.Value) continue;
                                    if (0 == item.Value.Count) continue;
                                    type_name = "";
                                    mbool = true;
                                    field_name = item.Key.ToLower();
                                    dotIndex = field_name.IndexOf(".");
                                    if (0 < dotIndex)
                                    {
                                        type_name = field_name.Substring(0, dotIndex);
                                        field_name = field_name.Substring(dotIndex + 1);
                                        mbool = type_name.Equals(model_type);
                                    }

                                    los = "";
                                    foreach (var item1 in item.Value)
                                    {
                                        los += ",OrderbyItem.Me.Set(\"{0}\", OrderByRule.{1})".ExtFormat(item1.FieldName, (OrderByRule.Asc == item1.Rule ? "Asc" : "Desc"));
                                    }
                                    if (string.IsNullOrEmpty(los)) continue;
                                    los = los.Substring(1);

                                    if (field_name.Equals(fnLower) && mbool)
                                    {
                                        DJTools.append(ref GetBody, level, "scheme.dbSqlBody.Orderby({0});", los);
                                    }
                                    lazyOrderbyStr += ".OrderbyLazy(\"{0}\", {1})".ExtFormat(item.Key.ToLower(), los);
                                }
                                DJTools.append(ref GetBody, level, "scheme.dbSqlBody{0};", lazyOrderbyStr);
                            }

                            if (PropType.isArray == propType)
                            {
                                DJTools.append(ref GetBody, level, "IList<{0}> results = scheme.ToList<{0}>();", typeName);
                                DJTools.append(ref GetBody, level, "base.{0} = ((List<{1}>)results).ToArray();", fn, typeName);
                            }
                            else if (PropType.isList == propType)
                            {
                                DJTools.append(ref GetBody, level, "base.{0} = ({1})scheme.ToList<{2}>();", fn, type.TypeToString(true), typeName);
                            }
                            else
                            {
                                DJTools.append(ref GetBody, level, "base.{0} = scheme.DefaultFirst<{1}>();", fn, typeName);
                            }
                            level -= 2;
                        }
                    }
                    pro = pro.Replace(getFlag, GetBody);
                    tag = pi.GetGetMethod().IsVirtual ? "override" : "new";
                    s = "";
                    DJTools.append(ref s, level, "");
                    DJTools.append(ref s, level, "public {0} {1} {2}", tag, type.TypeToString(true), fn);
                    DJTools.append(ref s, level, "{");
                    DJTools.append(ref s, 0, pro);
                    DJTools.append(ref s, level, "}");
                }

                if (!string.IsNullOrEmpty(s))
                {
                    DJTools.append(ref codeBody, 0, s);
                }
            });

            if (!string.IsNullOrEmpty(codeBody))
            {
                level = 0;
                foreach (CKeyValue item in uskv)
                {
                    DJTools.append(ref code, level, "using {0};", item.Key);
                }
                DJTools.append(ref code, level, "");

                DJTools.append(ref code, level, "namespace {0}", newNamespace);
                DJTools.append(ref code, level, "{");
                level++;
                string dmType = dataModelType.TypeToString(true);
                DJTools.append(ref code, level, "public class {0} : {1}, {2}", newClassName, dmType, typeof(IEntityCopy).TypeToString(true));
                DJTools.append(ref code, level, "{");
                DJTools.append(ref code, level + 1, "public Type {0} { get { return typeof({1}); } }", CopyParentModel, dmType);
                DJTools.append(ref code, level + 1, "public int {0} { get; set; }", MultiTablesExec.RecordQuantityFN);
                DJTools.append(ref code, level + 1, "");
                DJTools.append(ref code, 0, codeBody);
                DJTools.append(ref code, level, "}");
                level--;
                DJTools.append(ref code, level, "}");

                string err = "";
                typeName = newNamespace + "." + newClassName;
                try
                {
                    string dllPath = Path.Combine(DJTools.RootPath, TempImplCode.dirName);
                    dllPath = Path.Combine(dllPath, TempImplCode.libName);
                    DJTools.InitDirectory(dllPath, true);
                    dllPath = Path.Combine(dllPath, newClassName + ".dll");
                    ImplementAdapter.codeCompiler.SavePathOfDll = dllPath;
                    Assembly assembly = ImplementAdapter.codeCompiler.TranslateCode(null, null, code, ref err);
                    if (!string.IsNullOrEmpty(err)) error = err;
                    if (null != assembly && string.IsNullOrEmpty(err))
                    {
                        Type t = assembly.GetType(typeName);
                        dic.Add(dataModelType, t);
                        DJTools.AddDynamicType(t);
                        dtModel = Activator.CreateInstance(t);
                    }
                }
                catch (Exception ex)
                {

                    //throw;
                }

                if (ImplementAdapter.dbInfo1.IsShowCode)
                {
                    string txt = code;
                    if (!string.IsNullOrEmpty(err))
                    {
                        txt += "\r\n\r\n/**\r\n" + err + "\r\n**/";
                    }
                    TempImplCode.PrintCode(txt, newClassName);
                }
            }
            return dtModel;
        }

        public string error { get; set; }
    }
}
