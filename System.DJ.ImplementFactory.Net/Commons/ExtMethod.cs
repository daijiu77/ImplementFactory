using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
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
                    else if (-1 != item.Name.ToLower().IndexOf("list"))
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
                            if (!dic.ContainsKey(p.Name.ToLower())) continue;
                            ppi = dic[p.Name.ToLower()];
                            v = DJTools.ConvertTo(p.Value.ToString(), ppi.PropertyType);
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
                if (item.Value.Type == JTokenType.None
                    || item.Value.Type == JTokenType.Object
                    || item.Value.Type == JTokenType.Array
                    || item.Value.Type == JTokenType.Constructor
                    || item.Value.Type == JTokenType.Property
                    || item.Value.Type == JTokenType.Comment
                    || item.Value.Type == JTokenType.Null
                    || item.Value.Type == JTokenType.Undefined
                    || item.Value.Type == JTokenType.Raw) continue;
                if (!dic.ContainsKey(item.Name.ToLower())) continue;
                pi = dic[item.Name.ToLower()];
                v = DJTools.ConvertTo(item.Value.ToString(), pi.PropertyType);
                if (null == v) continue;
                vObj.SetPropertyValue(pi.Name, v);
            }

            return vObj;
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
        public static T ToObjectFrom<T>(this object srcObj, Func<PropertyInfo, string, bool> funcAssign, Func<T, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToObjectFrom(srcObj, funcAssign, funcVal);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <returns>Returns an assigned data entity</returns>
        public static T ToObjectFrom<T>(this object srcObj)
        {
            return srcObj.ToObjectFrom<T>(null, null);
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
            return srcObj.ToObjectFrom<T>(funcAssign, null);
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The target data type</typeparam>
        /// <param name="srcObj">Data source entity</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns the target object after the assignment</returns>
        public static T ToObjectFrom<T>(this object srcObj, Func<T, object, string, object, object> funcVal)
        {
            return srcObj.ToObjectFrom<T>(null, funcVal);
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
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned data entity of type task.</returns>
        public static Task<T> ToTaskObjectFrom<T>(this object srcObj, Func<T, object, string, object, object> funcVal)
        {
            T t = srcObj.ToObjectFrom<T>(null, funcVal);
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
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcAssign">When false is returned, no value is assigned to the current property</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, Func<PropertyInfo, string, bool> funcAssign, Func<T, object, string, object, object> funcVal)
        {
            DataModelMapping mapping = new DataModelMapping();
            return mapping.ToListFrom<T, TT>(srcList, funcAssign, funcVal);            
        }

        /// <summary>
        /// Object property-relationship mapping assignments
        /// </summary>
        /// <typeparam name="T">The element type of the target data collection</typeparam>
        /// <typeparam name="TT">The element type of the data source collection</typeparam>
        /// <param name="srcList">Data source collection object</param>
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object</returns>
        public static IList<T> ToListFrom<T, TT>(this IEnumerable srcList, Func<T, object, string, object, object> funcVal)
        {
            return srcList.ToListFrom<T, TT>(null, funcVal);
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
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned IList element collection object of type task</returns>
        public static Task<IList<T>> ToTaskIListFrom<T, TT>(this IList<TT> srcList, Func<T, object, string, object, object> funcVal)
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
        /// <param name="funcVal">Returns a value and assigns a value to the current property</param>
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList, Func<T, object, string, object, object> funcVal)
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
        /// <returns>Returns an assigned List element collection object of type task</returns>
        public static Task<List<T>> ToTaskListFrom<T, TT>(this IList<TT> srcList)
        {
            List<T> list = (List<T>)srcList.ToListFrom<T, TT>(null);
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
    }
}
