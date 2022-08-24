using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
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

        public string ToJson(Func<Type, string, bool> property)
        {
            string json = "";
            object dataModel = this;
            Type tp = dataModel.GetType();
            object fv = null;
            tp.ForeachProperty((pi, type, fn) =>
            {
                if (!property(type, fn))
                {
                    json += ", " + fn + ": null";
                    return;
                }
                fv = pi.GetValue(dataModel);
                if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime)
                 || type == typeof(Guid?) || type == typeof(DateTime?))
                {
                    fv = null == fv ? "null" : "\"" + fv + "\"";
                }
                else if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    if (null == fv)
                    {
                        fv = "null";
                    }
                    else
                    {
                        IEnumerable arr = fv as IEnumerable;
                        AbsDataModel absDataModel = null;
                        string s = "";
                        foreach (object item in arr)
                        {
                            absDataModel = item as AbsDataModel;
                            if (null == absDataModel) break;
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
                    if (null == fv)
                    {
                        fv = "null";
                    }
                    else
                    {
                        AbsDataModel absDataModel = (AbsDataModel)fv;
                        fv = absDataModel.ToJson(property);
                    }
                }
                else if (type.IsClass)
                {
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
                    fv = null == fv ? "null" : fv;
                }
                json += ", " + fn + ": " + fv.ToString();
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
    }
}
