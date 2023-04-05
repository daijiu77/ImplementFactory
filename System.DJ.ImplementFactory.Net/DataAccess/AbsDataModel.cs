using System.Collections;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Commons.Attrs;

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
            if (this.GetType() == type) return false;
            if (null == parentModel) return true;
            bool mbool = parentModel.IsLegalType(type);            
            return mbool;
        }

        public bool IsLegalValue(object val)
        {
            if (val == this) return false;
            if (null == parentModel) return true;
            bool mbool = parentModel.IsLegalValue(val);
            return mbool;
        }

        private string toJsonValue(object v, AbsDataModel parent, bool limitType, Func<Type,string,bool> func)
        {
            if (null == v) return "null";
            Type t = v.GetType();
            string vs = "";
            if (t.IsBaseType())
            {
                if (typeof(DateTime) == t || typeof(Guid) == t || typeof(DateTime?) == t || typeof(Guid?) == t || typeof(string) == t)
                {
                    vs = "\"" + v.ToString() + "\"";
                }
                else
                {
                    vs = v.ToString();
                    if (typeof(bool) == t) vs = vs.ToLower();
                }
            }
            else if (typeof(ICollection).IsAssignableFrom(t))
            {
                ICollection collection = (ICollection)v;
                string ele = "";
                foreach (var item in collection)
                {
                    ele += ", " + toJsonValue(item, parent, limitType, func);
                }
                if (!string.IsNullOrEmpty(ele)) ele = ele.Substring(2);
                vs = "[" + ele + "]";
            }
            else
            {
                AbsDataModel abs = (AbsDataModel)v;
                abs.parentModel = parent;
                bool enabled = false;
                object vObj = null;
                v.GetType().ForeachProperty((pi, pt, fn) =>
                {
                    enabled = true;
                    if (null != func)
                    {
                        enabled = func(pt, fn);
                    }

                    if (enabled)
                    {
                        if (limitType)
                        {
                            enabled = IsLegalType(pt);
                        }
                        else
                        {
                            vObj = pi.GetValue(v, null);
                            enabled = abs.IsLegalValue(vObj);
                        }
                    }                    

                    if (enabled)
                    {
                        if (limitType) vObj = pi.GetValue(v, null);
                        vs += ", \"" + fn + "\": " + toJsonValue(vObj, abs, limitType, func);
                    }
                    else
                    {
                        vs += ", \"" + fn + "\": null";
                    }
                });
                if (!string.IsNullOrEmpty(vs)) vs = vs.Substring(2);
                vs = "{" + vs + "}";
            }
            return vs;
        }

        public string ToJson(bool limitType, Func<Type, string, bool> func)
        {
            return toJsonValue(this, null, limitType, func);
        }

        public string ToJson(Func<Type, string, bool> func)
        {
            return toJsonValue(this, null, true, func);
        }

        public string ToJson(bool limitType)
        {
            return toJsonValue(this, null, limitType, (tp, fn) =>
            {
                return true;
            });
        }

        public string ToJson()
        {
            return toJsonValue(this, null, true, (tp, fn) =>
            {
                return true;
            });
        }

        [IgnoreForeachProp]
        public AbsDataModel parentModel { get; set; }
    }
}
