using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.DataAccess;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace System.DJ.ImplementFactory.Commons
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

        static object createArray(Type type, int length)
        {
            object arr = null;
            if (false == type.IsArray) return arr;

            try
            {
                arr = type.InvokeMember("Set", BindingFlags.CreateInstance, null, arr, new object[] { length });
            }
            catch { }

            return arr;
        }

        static void arrayAdd(object arrObj, object arrElement, int eleIndex)
        {
            if (null == arrObj) return;

            Type type = arrObj.GetType();
            if (false == type.IsArray) return;

            Array array = (Array)arrObj;

            try
            {
                array.SetValue(arrElement, eleIndex);
            }
            catch { }
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

        class JsonData
        {
            public JToken data { get; set; }
            public JToken list { get; set; }
            public JToken arr { get; set; }
            public JToken dts { get; set; }
        }

        /// <summary>
        /// 是合法的 JToken 类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool IsLegalType(JProperty item)
        {
            if (item.Value.Type == JTokenType.None
                    || item.Value.Type == JTokenType.Constructor
                    || item.Value.Type == JTokenType.Property
                    || item.Value.Type == JTokenType.Comment
                    || item.Value.Type == JTokenType.Null
                    || item.Value.Type == JTokenType.Undefined
                    || item.Value.Type == JTokenType.Raw) return false;
            return true;
        }

        public static IEnumerable JsonToList<T>(this string json)
        {
            IEnumerable list = new List<T>();
            if (string.IsNullOrEmpty(json)) return list;
            string s = json.Trim();
            if (false == s.Substring(0, 1).Equals("{") || false == s.Substring(s.Length - 1).Equals("}")) return list;

            JToken arr = null;
            JsonData jsonData = new JsonData();
            JObject jo = JObject.Parse(json);
            IEnumerable<JProperty> ps = jo.Properties();
            foreach (JProperty item in ps)
            {
                if (item.Value.Type == JTokenType.Array)
                {
                    arr = item.Value;
                    if (-1 != item.Name.ToLower().IndexOf("data"))
                    {
                        jsonData.data = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("datas"))
                    {
                        jsonData.data = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("result"))
                    {
                        jsonData.data = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("results"))
                    {
                        jsonData.data = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("list"))
                    {
                        jsonData.list = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("lists"))
                    {
                        jsonData.list = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("arr"))
                    {
                        jsonData.arr = item.Value;
                    }
                    else if (-1 != item.Name.ToLower().IndexOf("dts"))
                    {
                        jsonData.dts = item.Value;
                    }
                    else
                    {
                        jsonData.dts = item.Value;
                    }
                }
            }

            if (null != jsonData.data)
            {
                arr = jsonData.data;
            }
            else if (null != jsonData.list)
            {
                arr = jsonData.list;
            }
            else if (null != jsonData.arr)
            {
                arr = jsonData.arr;
            }
            else if (null != jsonData.dts)
            {
                arr = jsonData.dts;
            }

            if (null == arr) return list;

            bool isArray = typeof(T).IsArray;
            if (isArray)
            {
                JArray ja = arr.ToObject<JArray>();
                int ncount = ja.Count;
                list = (IEnumerable)createArray(typeof(T), ncount);
            }

            object v = null;
            object vObj = null;
            PropertyInfo ppi = null;
            int index = 0;
            jo = null;
            bool isBaseType = DJTools.IsBaseType(typeof(T));
            Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
            foreach (JToken item in arr)
            {
                if (isBaseType)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        jo = item.ToObject<JObject>();
                        ps = jo.Properties();
                        foreach (JProperty p in ps)
                        {
                            v = p.Value.ToString();
                            break;
                        }
                    }
                    else
                    {
                        v = item.ToString();
                    }

                    if (null == v) v = default(T);
                    vObj = DJTools.ConvertTo(v.ToString(), typeof(T));
                    if (isArray)
                    {
                        arrayAdd(list, vObj, index);
                        index++;
                    }
                    else
                    {
                        ((IList<T>)list).Add((T)vObj);
                    }
                }
                else if (item.Type == JTokenType.Object)
                {
                    try
                    {
                        if (isArray)
                        {
                            Type t = typeof(T).GetElementType();
                            vObj = Activator.CreateInstance(t);
                        }
                        else
                        {
                            vObj = Activator.CreateInstance(typeof(T));
                        }

                        if (0 == dic.Count)
                        {
                            PropertyInfo[] pi = vObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                            foreach (PropertyInfo p in pi)
                            {
                                dic.Add(p.Name.ToLower(), p);
                            }
                        }

                        jo = item.ToObject<JObject>();
                        ps = jo.Properties();
                        foreach (JProperty p in ps)
                        {
                            if (false == IsLegalType(p)) continue;
                            if (!dic.ContainsKey(p.Name.ToLower())) continue;
                            ppi = dic[p.Name.ToLower()];
                            v = ExecGenericMethod(_defaultValue, ppi.PropertyType, null);
                            if (p.Value.Type == JTokenType.Object)
                            {
                                if (ppi.PropertyType.IsClass
                                    && (false == ppi.PropertyType.IsInterface)
                                    && (false == ppi.PropertyType.IsAbstract))
                                {
                                    v = ExecGenericMethod(_JsonToEntity, ppi.PropertyType, p.Value.ToString());
                                }
                            }
                            else if (p.Value.Type == JTokenType.Array)
                            {
                                Type eleTp = null;
                                if (ppi.PropertyType.IsArray)
                                {
                                    eleTp = ppi.PropertyType.GetTypeForArrayElement();
                                }
                                else if (ppi.PropertyType.IsList())
                                {
                                    eleTp = ppi.PropertyType.GetGenericArguments()[0];
                                }

                                if (null != eleTp)
                                {
                                    JObject jobj = new JObject();
                                    jobj.Add("data", p.Value);
                                    string txt = JsonConvert.SerializeObject(jobj);
                                    v = ExecGenericMethod(_JsonToList, eleTp, txt);
                                }
                            }
                            else
                            {
                                v = DJTools.ConvertTo(p.Value.ToString(), ppi.PropertyType);
                            }

                            if (null != v) vObj.GetType().GetProperty(ppi.Name).SetValue(vObj, v);
                        }

                        if (isArray)
                        {
                            arrayAdd(list, vObj, index);
                            index++;
                        }
                        else
                        {
                            ((IList<T>)list).Add((T)vObj);
                        }
                    }
                    catch (Exception)
                    {
                        break;
                        //throw;
                    }
                }
            }

            return list;
        }

        public static T JsonToEntity<T>(this string json)
        {
            T vObj = default(T);
            if (string.IsNullOrEmpty(json)) return vObj;
            string s = json.Trim();
            if (false == s.Substring(0, 1).Equals("{") || false == s.Substring(s.Length - 1).Equals("}")) return vObj;

            vObj = (T)Activator.CreateInstance(typeof(T));
            Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] piArr = vObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in piArr)
            {
                dic.Add(p.Name.ToLower(), p);
            }

            object v = null;
            PropertyInfo pi = null;
            JObject jo = JObject.Parse(json);
            IEnumerable<JProperty> ps = jo.Properties();
            foreach (JProperty item in ps)
            {
                if (false == IsLegalType(item)) continue;
                if (!dic.ContainsKey(item.Name.ToLower())) continue;
                pi = dic[item.Name.ToLower()];
                v = ExecGenericMethod(_defaultValue, pi.PropertyType, null);
                if (item.Value.Type == JTokenType.Array)
                {
                    Type tp = null;
                    if (pi.PropertyType.IsList())
                    {
                        tp = pi.PropertyType.GetGenericArguments()[0];
                    }
                    else if (pi.PropertyType.IsArray)
                    {
                        tp = pi.PropertyType.GetElementType();
                    }

                    if (null != tp)
                    {
                        JObject jobj = new JObject();
                        jobj.Add("data", item.Value);
                        string txt = JsonConvert.SerializeObject(jobj);
                        v = ExecGenericMethod(_JsonToList, tp, txt);
                    }
                }
                else if (item.Value.Type == JTokenType.Object)
                {
                    v = ExecGenericMethod(_JsonToEntity, pi.PropertyType, item.Value.ToString());
                }
                else if (item.Value.Type != JTokenType.Null)
                {
                    v = DJTools.ConvertTo(item.Value.ToString(), pi.PropertyType);
                }

                if (null == v) continue;
                vObj.SetPropertyValue(pi.Name, v);
            }

            return vObj;
        }

        private static object ExecGenericMethod(string methodName, Type tp, string json)
        {
            object v = null;
            ExtJsonToObject ext = new ExtJsonToObject();
            MethodInfo mi = ext.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (null != mi)
            {
                mi = mi.MakeGenericMethod(tp);
                try
                {
                    if (null != json)
                    {
                        v = mi.Invoke(ext, new object[] { json });
                    }
                    else
                    {
                        v = mi.Invoke(ext, null);
                    }
                }
                catch (Exception)
                {

                    //throw;
                }
            }
            return v;
        }

        private const string _JsonToEntity = "json_entity";
        private const string _JsonToList = "json_list";
        private const string _defaultValue = "default_value";

        private class ExtJsonToObject
        {
            public T json_entity<T>(string json)
            {
                return json.JsonToEntity<T>();
            }

            public IEnumerable json_list<T>(string json)
            {
                return json.JsonToList<T>();
            }

            public object default_value<T>()
            {
                return default(T);
            }
        }

        public static int Count(this IEnumerable srcList)
        {
            int count = 0;
            if (null == srcList) return count;
            foreach (var src in srcList) count++;
            return count;
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, funcAssign, (tg, src, fn, fv) =>
            {
                return mapping.Call_back1<T, TT>(tg, src, fn, fv, funcVal, null, null);
            });
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, isTrySetVal, funcAssign, (tg, src, fn, fv) =>
            {
                return mapping.Call_back1<T, TT>(tg, src, fn, fv, funcVal, null, null);
            });
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <returns>Returns an assigned data entity</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj)
        {
            return srcObj.ToObjectFrom<T, TT>(false, null, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T>(this object srcObj)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, null, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T>(this object srcObj, Func<PropertyInfo, string, bool> funcAssign)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, funcAssign, null);
        }

        public static T ToObjectFrom<T>(this object srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, isTrySetVal, funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity</returns>
        public static T ToObjectFrom<T>(this object srcObj, Func<T, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, null, (tg, src, fn, fv) =>
            {
                return mapping.Call_back4<T>(tg, src, fn, fv, funcVal, null);
            });
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity</returns>
        public static T ToObjectFrom<T>(this object srcObj, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, funcAssign, (tg, src, fn, fv) =>
            {
                return mapping.Call_back4<T>(tg, src, fn, fv, null, funcVal);
            });
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <returns>Returns an assigned data entity</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal)
        {
            return srcObj.ToObjectFrom<T, TT>(isTrySetVal, null, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, Func<PropertyInfo, string, bool> funcAssign)
        {
            return srcObj.ToObjectFrom<T, TT>(funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            return srcObj.ToObjectFrom<T, TT>(isTrySetVal, funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, Func<T, TT, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, null, (tg, src, fn, fv) =>
            {
                return mapping.Call_back1<T, TT>(tg, src, fn, fv, funcVal, null, null);
            });
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <typeparam name="TT"></typeparam>
        /// <param name="srcObj"></param>
        /// <param name="funcVal"></param>
        /// <returns></returns>
        public static T ToObjectWithChildModel<T, TT>(this TT srcObj, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom<T>(srcObj, false, null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<T, TT, string, object, object> funcVal)
        {
            return srcObj.ToObjectFrom<T, TT>(isTrySetVal, null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj)
        {
            T t = srcObj.ToObjectFrom<T, TT>(null, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T>(this object srcObj)
        {
            T t = srcObj.ToObjectFrom<T>(null, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T>(this object srcObj, Func<PropertyInfo, string, bool> funcAssign)
        {
            T t = srcObj.ToObjectFrom<T>(funcAssign, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T>(this object srcObj, Func<object, object, string, object, object> funcVal)
        {
            T t = srcObj.ToObjectFrom<T>(null, funcVal);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal)
        {
            T t = srcObj.ToObjectFrom<T, TT>(isTrySetVal, null, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, Func<T, TT, string, object, object> funcVal)
        {
            T t = srcObj.ToObjectFrom<T, TT>(null, funcVal);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<T, TT, string, object, object> funcVal)
        {
            T t = srcObj.ToObjectFrom<T, TT>(isTrySetVal, null, funcVal);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <typeparam name="TT">The source data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, Func<PropertyInfo, string, bool> funcAssign)
        {
            T t = srcObj.ToObjectFrom<T, TT>(funcAssign, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <typeparam name="TT">The source data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            T t = srcObj.ToObjectFrom<T, TT>(isTrySetVal, funcAssign, null);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <typeparam name="TT">The source data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T, TT>(this TT srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            T t = srcObj.ToObjectFrom<T, TT>(isTrySetVal, funcAssign, funcVal);
            return Task.FromResult(t);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <typeparam name="TT">The source data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectWithChildModel<T, TT>(this TT srcObj, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            object tg = mapping.ToObjectFrom<T>(srcObj, isTrySetVal, funcAssign, funcVal);
            return Task.FromResult((T)tg);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToListFrom<T, TT>(srcList, false, funcAssign, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToListFrom<T, TT>(srcList, isTrySetVal, funcAssign, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, Func<T, TT, string, object, object> funcVal)
        {
            return srcList.ToListFrom<T, TT>(null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListWithChildModel<T, TT>(this IEnumerable srcList, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping dataModelMapping = new DataModelMapping();
            return dataModelMapping.ToListWhithChildModel<T, TT>(srcList, false, null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListWithChildModel<T, TT>(this IEnumerable srcList, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping dataModelMapping = new DataModelMapping();
            return dataModelMapping.ToListWhithChildModel<T, TT>(srcList, false, funcAssign, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListWithChildModel<T, TT>(this IEnumerable srcList, bool isTrySetVal, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping dataModelMapping = new DataModelMapping();
            return dataModelMapping.ToListWhithChildModel<T, TT>(srcList, isTrySetVal, null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListWithChildModel<T, TT>(this IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            DataModelMapping dataModelMapping = new DataModelMapping();
            return dataModelMapping.ToListWhithChildModel<T, TT>(srcList, isTrySetVal, funcAssign, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, bool isTrySetVal, Func<T, TT, string, object, object> funcVal)
        {
            return srcList.ToListFrom<T, TT>(isTrySetVal, null, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, Func<PropertyInfo, string, bool> funcAssign)
        {
            return srcList.ToListFrom<T, TT>(funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            return srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList)
        {
            return srcList.ToListFrom<T, TT>(null, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, bool isTrySetVal)
        {
            return srcList.ToListFrom<T, TT>(isTrySetVal, null, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(isTrySetVal, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, Func<T, TT, string, object, object> funcVal)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(null, funcVal);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<T, TT, string, object, object> funcVal)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(isTrySetVal, null, funcVal);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, Func<PropertyInfo, string, bool> funcAssign)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(funcAssign, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, funcVal);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListWithChildModel<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            IList<T> list = srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, (tg, src, fn, fv) =>
            {
                if (null != funcVal) return funcVal(tg, src, fn, fv);
                return fv;
            });
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(isTrySetVal, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, Func<T, TT, string, object, object> funcVal)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(null, funcVal);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<T, TT, string, object, object> funcVal)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(isTrySetVal, null, funcVal);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, Func<PropertyInfo, string, bool> funcAssign)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(funcAssign, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, null);
            return Task.FromResult(list);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="isTrySetVal">Try to execute set-method to set value of property.</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<T, TT, string, object, object> funcVal)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, funcVal);
            return Task.FromResult(list);
        }

        public static Task<List<T>> ToTaskListWithChildModel<T, TT>(this IList<TT> srcList, bool isTrySetVal, Func<PropertyInfo, string, bool> funcAssign, Func<object, object, string, object, object> funcVal)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(isTrySetVal, funcAssign, (tg, src, fn, fv) =>
            {
                if (null != funcVal) return funcVal(tg, src, fn, fv);
                return fv;
            });
            return Task.FromResult(list);
        }

        public static void ForeachChildNode(this XmlElement parentNode, Func<XmlElement, bool> funcChildren)
        {
            if (null == parentNode) return;
            if (XmlNodeType.Element != parentNode.NodeType) return;
            foreach (XmlNode item in parentNode.ChildNodes)
            {
                if (XmlNodeType.Element != item.NodeType) continue;
                if (!funcChildren((XmlElement)item)) break;
            }
        }

        public static void ForeachChildNode(this XmlElement parentNode, Action<XmlElement> actionChildren)
        {
            if (null == parentNode) return;
            if (!parentNode.HasChildNodes) return;
            parentNode.ForeachChildNode(item =>
            {
                actionChildren(item);
                return true;
            });
        }

        public static void ForeachChildNode(this XmlNode parentNode, Func<XmlElement, bool> funcChildren)
        {
            if (null == parentNode) return;
            if (null == (parentNode as XmlElement)) return;
            ((XmlElement)parentNode).ForeachChildNode(item =>
            {
                return funcChildren(item);
            });
        }

        public static void ForeachChildNode(this XmlNode parentNode, Action<XmlElement> actionChildren)
        {
            if (null == parentNode) return;
            if (!parentNode.HasChildNodes) return;
            parentNode.ForeachChildNode(item =>
            {
                actionChildren(item);
                return true;
            });
        }

        public static int AddData<T>(this IList<T> list, T data)
        {
            int num = 0;
            if (null == list) return num;
            if (null == (list as DOList<T>))
            {
                list.Add(data);
                return num;
            }
            num = ((DOList<T>)list).Add(data);
            return num;
        }

        public static int InsertData<T>(this IList<T> list, T data)
        {
            return AddData<T>(list, (T)data);
        }

        public static int RemoveData<T>(this IList<T> list, T data)
        {
            int num = 0;
            if (null == list) return num;
            if (0 == list.Count) return num;
            if (null == (list as DOList<T>))
            {
                list.Remove(data);
                return num;
            }
            num = ((DOList<T>)list).Remove(data);
            return num;
        }

        public static int RemoveData<T>(this IList<T> list, Func<T, bool> func)
        {
            int num = 0;
            if (null == list) return num;
            if (0 == list.Count) return num;
            foreach (var item in list)
            {
                if (func(item))
                {
                    num = list.RemoveData<T>(item);
                }
            }
            return num;
        }

        public static int DeleteData<T>(this IList<T> list, T data)
        {
            return RemoveData<T>(list, data);
        }

        public static int DeleteData<T>(this IList<T> list, Func<T, bool> func)
        {
            int num = 0;
            if (null == list) return num;
            if (0 == list.Count) return num;
            foreach (var item in list)
            {
                if (func(item))
                {
                    num = list.RemoveData<T>(item);
                }
            }
            return num;
        }

        public static int RemoveDataAt<T>(this IList<T> list, int index)
        {
            int num = 0;
            if (null == list) return num;
            if (0 == list.Count) return num;
            if (null == (list as DOList<T>))
            {
                list.RemoveAt(index);
                return num;
            }
            num = ((DOList<T>)list).RemoveAt(index);
            return num;
        }

        public static int RemoveAllData<T>(this IList<T> list)
        {
            int num = 0;
            if (null == list) return num;
            if (0 == list.Count) return num;
            if (null == (list as DOList<T>))
            {
                list.Clear();
                return num;
            }
            num = ((DOList<T>)list).Clear();
            return num;
        }

        public static int ClearData<T>(this IList<T> list)
        {
            return RemoveAllData<T>(list);
        }
    }
}
