using System;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Text;

namespace System.DJ.MicroService
{
    public static class ExtMethod
    {
        static string getValueByDictionary(IDictionary dic)
        {
            if (null == dic) return "null";
            if (0 == dic.Count) return "null";

            string sign = ", ";
            string fv = "";
            string key = "";
            object val = null;
            Type type = null;
            Type[] types = dic.GetType().GetGenericArguments();
            Type type1 = types[1];
            foreach (var item in dic)
            {
                type = item.GetType();
                key = type.GetProperty("Key").GetValue(item, null).ToString();
                val = type.GetProperty("Value").GetValue(item, null);
                fv += sign + "\"" + key + "\": " + getValueByType(type1, val);
            }

            fv = fv.Substring(sign.Length);
            fv = "{" + fv + "}";
            return fv;
        }

        static string getValueByList(IEnumerable list)
        {
            if (null == list) return "null";
            string sign = ", ";
            string fv = "";
            Type[] types = list.GetType().GetGenericArguments();
            Type type = types[0];
            foreach (var item in list)
            {
                fv += sign + getValueByType(type, item);
            }

            fv = fv.Substring(sign.Length);
            fv = "[" + fv + "]";
            return fv;
        }

        static string getValueByArray(Array arr)
        {
            if (null == arr) return "null";
            if (0 == arr.Length) return "null";
            string sign = ", ";
            string fv = "";
            Type type = arr.GetType().GetElementType();
            foreach (var item in arr)
            {
                fv += sign + getValueByType(type, item);
            }
            fv = fv.Substring(sign.Length);
            fv = "[" + fv + "]";
            return fv;
        }

        static string getValueByBaseEntity(object baseEntity)
        {
            if (null == baseEntity) return "null";
            string fv = baseEntity.ToJsonUnit();
            return fv;
        }

        static string getValueByType(Type type, object fieldVale)
        {
            string fv = "";
            if (null == fieldVale) return "null";

            if (type == typeof(string)
            || type == typeof(DateTime)
            || type == typeof(Guid))
            {
                fv = "\"" + fieldVale.ToString() + "\"";
            }
            else if (type == typeof(bool))
            {
                fv = fieldVale.ToString().ToLower();
            }
            else if (type.IsArray)
            {
                Array arr = null == fieldVale ? null : (Array)fieldVale;
                fv = getValueByArray(arr);
            }
            else if (null != (fieldVale as IDictionary))
            {
                IDictionary dic = null == fieldVale ? null : (IDictionary)fieldVale;
                fv = getValueByDictionary(dic);
            }
            else if (null != (fieldVale as IEnumerable))
            {
                IEnumerable list = null == fieldVale ? null : (IEnumerable)fieldVale;
                fv = getValueByList(list);
            }
            else if (type.IsClass)
            {
                fv = getValueByBaseEntity(fieldVale);
            }
            else
            {
                fv = fieldVale.ToString();
            }
            return fv;
        }

        public static string ToJsonUnit(this object entity)
        {
            string sign = ", ";
            string s1 = "";
            if (typeof(DateTime) == entity.GetType())
            {
                s1 = Convert.ToDateTime(entity).ToString();
            }
            else if (typeof(Guid) == entity.GetType())
            {
                s1 = entity.ToString();
            }
            else if (DJTools.IsBaseType(entity.GetType()))
            {
                if (typeof(string) == entity.GetType())
                {
                    s1 = "\"" + entity.ToString() + "\"";
                }
                else if (typeof(bool) == entity.GetType())
                {
                    s1 = entity.ToString().ToLower();
                }
                else
                {
                    s1 = entity.ToString();
                }
            }

            if (!string.IsNullOrEmpty(s1)) return s1;

            entity.ForeachProperty((propertyInfo, propertyType, fieldName, fieldVale) =>
            {
                string fv = getValueByType(propertyInfo.PropertyType, fieldVale);
                s1 += sign + "\"" + fieldName + "\": " + fv;
            });

            if (!string.IsNullOrEmpty(s1))
            {
                s1 = s1.Substring(sign.Length);
                s1 = "{" + s1 + "}";
            }
            return s1;
        }
    }
}
