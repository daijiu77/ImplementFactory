using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Reflection;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{
    public abstract class AbsDataModel
    {
        public T GetValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return default(T);
            string field = propertyName.Trim().ToLower();
            object fv = default(T);
            object dataModel = this;
            Type tp = dataModel.GetType();
            tp.ForeachProperty((pi, type, fn) =>
            {
                if (!fn.ToLower().Equals(field)) return true;
                fv = pi.GetValue(dataModel);
                return false;
            });
            return (T)fv;
        }

        public bool IsLegalType(Type type)
        {
            object dataModel = this;
            Type pt = dataModel.GetType();
            if (pt == type) return false;
            bool mbool = true;
            AbsDataModel dm = this;
            if (null != dm.parentModel)
            {
                mbool = dm.parentModel.IsLegalType(type);
            }
            return mbool;
        }

        public string ToJson(Func<Type, string, bool> property)
        {
            string json = "";
            object dataModel = this;
            Type tp = dataModel.GetType();
            object fv = null;
            tp.ForeachProperty((pi, type, fn) =>
            {
                if (!((AbsDataModel)dataModel).IsLegalType(type))
                {
                    json += ", \"" + fn + "\": null";
                    return;
                }

                if (!property(type, fn))
                {
                    json += ", \"" + fn + "\": null";
                    return;
                }

                if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime)
                 || type == typeof(Guid?) || type == typeof(DateTime?))
                {
                    fv = pi.GetValue(dataModel);
                    fv = null == fv ? "null" : "\"" + fv + "\"";
                }
                else if (type.IsArray || typeof(IList).IsAssignableFrom(type))
                {
                    fv = pi.GetValue(dataModel);
                    if (null == fv)
                    {
                        fv = "null";
                    }
                    else
                    {
                        bool isBaseVal = false;
                        IEnumerable arr = fv as IEnumerable;
                        AbsDataModel absDataModel = null;
                        string s = "";
                        Type t = null;

                        foreach (object item in arr)
                        {
                            if (null == item)
                            {
                                s += ", null";
                                continue;
                            }
                            absDataModel = item as AbsDataModel;
                            if (null == absDataModel && string.IsNullOrEmpty(s))
                            {
                                isBaseVal = DJTools.IsBaseType(item.GetType());
                            }

                            if (isBaseVal)
                            {
                                t = item.GetType();
                                if ((typeof(string) == t) || (typeof(Guid) == t) || (typeof(DateTime) == t)
                                 || (typeof(Guid?) == t) || (typeof(DateTime?) == t))
                                {
                                    s += ", \"" + item + "\"";
                                }
                                else
                                {
                                    s += ", " + item;
                                }
                                continue;
                            }
                            absDataModel.parentModel = this;
                            s += ", " + absDataModel.ToJson(property);
                        }

                        if (!string.IsNullOrEmpty(s))
                        {
                            s = s.Substring(2);
                            s = "[" + s + "]";
                        }
                        else
                        {
                            s = "null";
                        }
                        fv = s;
                    }
                }
                else if (typeof(AbsDataModel).IsAssignableFrom(type))
                {
                    fv = pi.GetValue(dataModel);
                    if (null == fv)
                    {
                        fv = "null";
                    }
                    else
                    {
                        AbsDataModel absDataModel = (AbsDataModel)fv;
                        absDataModel.parentModel = this;
                        fv = absDataModel.ToJson(property);
                    }
                }
                else if (type.IsClass)
                {
                    fv = pi.GetValue(dataModel);
                    if (null == fv)
                    {
                        fv = "null";
                    }
                    else
                    {
                        fv = JsonConvert.SerializeObject(fv);
                    }
                }
                else
                {
                    fv = pi.GetValue(dataModel);
                    fv = null == fv ? "null" : fv;
                }
                json += ", \"" + fn + "\": " + fv.ToString();
            });
            if (!string.IsNullOrEmpty(json))
            {
                json = json.Substring(2);
                json = "{" + json + "}";
            }
            return json;
        }

        public string ToJson()
        {
            return ToJson((tp, fn) =>
            {
                return true;
            });
        }

        [IgnoreForeachProp]
        public AbsDataModel parentModel { get; set; }
    }
}
