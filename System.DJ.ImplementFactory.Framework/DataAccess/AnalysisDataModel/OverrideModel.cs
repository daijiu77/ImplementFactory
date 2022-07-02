using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess.FromUnit;
using System.DJ.ImplementFactory.DataAccess.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.DataAccess.AnalysisDataModel
{
    public class OverrideModel
    {
        private Dictionary<Type, Type> dic = new Dictionary<Type, Type>();
        private enum PropType
        {
            none,
            isArray,
            isList,
            isClass
        }

        private object NewDataModel(Type dataModelType)
        {
            object dtModel = null;
            if (null == dataModelType) return null;
            if (!typeof(AbsDataModel).IsAssignableFrom(dataModelType)) return null;
            string newNamespace = dataModelType.Namespace + "." + dataModelType.Name + "_override";
            string newClassName = dataModelType.Name + "Override";
            string typeName = newNamespace + "." + newClassName;
            Type type1 = null;
            if (dic.ContainsKey(dataModelType))
            {
                type1 = dic[dataModelType];
                dtModel = Activator.CreateInstance(type1);
                return dtModel;
            }
            object[] atts = null;
            Constraint constraint = null;
            string code = "";
            string codeBody = "";
            string pro = "";
            string s = "";
            string GetBody = "";
            const string getFlag = "{#GetBody}";
            Type[] types = null;
            PropType propType = PropType.none;
            int level = 0;

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

            object dataModel = Activator.CreateInstance(dataModelType);
            dataModel.ForeachProperty((pi, type, fn, fv) =>
            {
                pro = "";
                level = 2;
                if (pi.CanRead)
                {
                    if (pi.GetGetMethod().IsVirtual)
                    {
                        level++;
                        DJTools.append(ref pro, level, "get");
                        DJTools.append(ref pro, level, "{");
                        DJTools.append(ref pro, 0, getFlag);
                        DJTools.append(ref pro, level + 1, "return base.{0};", fn);
                        DJTools.append(ref pro, level, "}");
                        level--;
                    }
                }

                if (pi.CanWrite)
                {
                    if (pi.GetSetMethod().IsVirtual)
                    {
                        level++;
                        DJTools.append(ref pro, level, "set");
                        DJTools.append(ref pro, level, "{");
                        DJTools.append(ref pro, level + 1, "base.{0} = value;", fn);
                        DJTools.append(ref pro, level, "}");
                        level--;
                    }
                }

                s = "";
                if (!string.IsNullOrEmpty(pro))
                {
                    constraint = null;
                    GetBody = "";
                    atts = pi.GetCustomAttributes(typeof(Constraint), false);
                    if (0 < atts.Length)
                    {
                        constraint = (Constraint)atts[0];
                    }

                    if (null != constraint)
                    {
                        propType = PropType.none;
                        typeName = "";
                        if (typeof(System.Collections.ICollection).IsAssignableFrom(type) && (typeof(string) != type))
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
                        }

                        if (!string.IsNullOrEmpty(typeName))
                        {
                            level += 2;
                            uskv.Add(new CKeyValue() { Key = typeof(DbVisitor).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(IDbSqlScheme).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(SqlFromUnit).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(ConditionItem).Namespace });
                            uskv.Add(new CKeyValue() { Key = typeof(ConditionRelation).Namespace });
                            DJTools.append(ref GetBody, level, "DbVisitor db = new DbVisitor();");
                            DJTools.append(ref GetBody, level, "Type type = DJTools.GetTypeByFullName(\"{0}\");", typeName);
                            DJTools.append(ref GetBody, level, "AbsDataModel dataModel = (AbsDataModel)Activator.CreateInstance(type);");
                            DJTools.append(ref GetBody, level, "IDbSqlScheme scheme = db.CreateSqlFrom(SqlFromUnit.New.From(dataModel));");
                            DJTools.append(ref GetBody, level, "scheme.dbSqlBody.Where(ConditionItem.Me.And(\"{0}\", ConditionRelation.Equals, this.{1}));", constraint.RefrenceKey, constraint.ForeignKey);
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
                                DJTools.append(ref GetBody, level, "base.{0} = scheme.DefaultFrist<{1}>();", fn, typeName);
                            }
                            level -= 2;
                        }
                    }
                    pro = pro.Replace(getFlag, GetBody);
                    DJTools.append(ref s, level, "");
                    DJTools.append(ref s, level, "public override {0} {1}", type.TypeToString(true), fn);
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
                DJTools.append(ref code, level, "public class {0} : {1}", newClassName, dataModelType.TypeToString(true));
                DJTools.append(ref code, level, "{");
                DJTools.append(ref code, 0, codeBody);
                DJTools.append(ref code, level, "}");
                level--;
                DJTools.append(ref code, level, "}");

                string err = "";
                typeName = newNamespace + "." + newClassName;
                try
                {
                    Assembly assembly = ImplementAdapter.codeCompiler.TranslateCode(null, null, code, ref err);
                    Type t = assembly.GetType(typeName);
                    dic.Add(dataModelType, t);
                    dtModel = Activator.CreateInstance(t);
                }
                catch (Exception ex)
                {

                    //throw;
                }
            }
            return dtModel;
        }

        public object CreateDataModel(Type dataModelType)
        {
            object dm = NewDataModel(dataModelType);
            return dm;
        }
    }
}
