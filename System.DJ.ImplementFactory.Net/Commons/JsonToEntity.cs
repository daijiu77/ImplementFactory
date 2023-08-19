using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Exts;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.Commons
{
    class JsonToEntity
    {
        private static Dictionary<string, Type> dicType = new Dictionary<string, Type>();
        private Dictionary<string, string> kvUsing = new Dictionary<string, string>();
        private Dictionary<string, string> kvClassName = new Dictionary<string, string>();
        private List<string> codes = new List<string>();
        private string namespaceText = "System.DJ.ImplementFactory.Commons.Dynamic";

        public object GetObject(string json)
        {
            kvUsing.Clear();
            codes.Clear();
            kvClassName.Clear();

            kvUsing.Add("System.Collections.Generic;", "");
            kvUsing.Add("System.Text", "");
            kvUsing.Add("System", "");

            string txt = "";
            string s = "";
            Regex rg = new Regex(@"(^using\s+(?<name_space>[a-z0-9_\.]+)\s*\;$)|(^(?<name_space>[a-z0-9_\.]+)\s*\;$)", RegexOptions.IgnoreCase);
            foreach (KeyValuePair<string, string> item in kvUsing)
            {
                s = item.Key.Trim();
                if (rg.IsMatch(s))
                {
                    s = rg.Match(s).Groups["name_space"].Value;
                }
                DJTools.append(ref txt, "using {0};", s);
            }

            string clsName = "A" + Guid.NewGuid().ToString().Replace("-", "");
            clsName = clsName.ToLower();
            JObject jobj = JObject.Parse(json);
            JToken jt = jobj.ToObject<JToken>();
            Type clsType = null;
            object vObj = null;

            MatchProperties(jt, ref clsType);
            if (null == clsType)
            {
                JsonToClassDesign(clsName, "cs", jt);
                if (0 < codes.Count)
                {
                    foreach (var item in codes)
                    {
                        DJTools.append(ref txt, "");
                        DJTools.append(ref txt, item);
                    }
                    string err = "";
                    //ImplementAdapter.codeCompiler.SavePathOfDll = "";
                    Assembly assembly = null; // ImplementAdapter.codeCompiler.TranslateCode(null, null, txt, ref err);
                    assembly = ShareCodeCompiler.TranslateCode("", null, null, txt, ref err);
                    if (null != assembly)
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (var item in types)
                        {
                            dicType.Add(item.Name.ToLower(), item);
                        }

                        if (dicType.ContainsKey(clsName))
                        {
                            clsType = dicType[clsName];
                        }
                    }
                }
            }

            if (null != clsType)
            {
                vObj = Activator.CreateInstance(clsType);
                SetObjVal(vObj, jt);
            }

            return vObj;
        }

        private void JsonToClassDesign(string key, string extendName, JToken val)
        {
            if (kvClassName.ContainsKey(key)) return;
            kvClassName.Add(key, "");
            string parentClassNameText = "";

            string classNameText = key.Substring(0, 1).ToUpper() + key.Substring(1);

            int level = 0;
            string txt = "";
            string eleType = "";
            string proName = "";
            string eleTypeName = "";

            if (!string.IsNullOrEmpty(namespaceText))
            {
                DJTools.append(ref txt, "namespace {0}", namespaceText);
                DJTools.append(ref txt, "{");
                level++;
            }

            if (!string.IsNullOrEmpty(parentClassNameText)) parentClassNameText = " : " + parentClassNameText;
            DJTools.append(ref txt, level, "public class {0}{1}", classNameText, parentClassNameText);
            DJTools.append(ref txt, level, "{");
            level++;

            JObject jobj = val.ToObject<JObject>();
            IEnumerable<JProperty> properties = jobj.Properties();
            JToken ele = null;
            JToken ele1 = null;
            Type clsType = null;
            foreach (JProperty item in properties)
            {
                ele = item.Value;
                if (JTokenType.String == ele.Type || JTokenType.Null == ele.Type || JTokenType.Undefined == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public string {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Integer == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public int {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Float == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public float {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Boolean == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public bool {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Array == ele.Type)
                {
                    eleType = GetEleTypeOfArray(item.Name, ele, ref eleTypeName, ref ele1);
                    DJTools.append(ref txt, level, "");

                    proName = item.Name;
                    if (eleTypeName.Equals("object"))
                    {
                        clsType = null;
                        MatchProperties(ele1, ref clsType);
                        if (null == clsType)
                        {
                            JsonToClassDesign(item.Name, extendName, ele1);
                        }
                        else
                        {
                            eleType = clsType.FullName;
                            proName = clsType.Name;
                        }
                    }

                    proName = proName.Substring(0, 1).ToLower() + proName.Substring(1);
                    DJTools.append(ref txt, level, "public List<{0}> {1} { get; set; }", eleType, proName);
                }
                else if (JTokenType.Date == ele.Type || JTokenType.TimeSpan == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public DateTime {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Guid == ele.Type)
                {
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public Guid {0} { get; set; }", item.Name);
                }
                else if (JTokenType.Object == ele.Type)
                {
                    clsType = null;
                    MatchProperties(ele, ref clsType);
                    if (null == clsType)
                    {
                        JsonToClassDesign(item.Name, extendName, ele);
                        eleType = item.Name.Substring(0, 1).ToUpper() + item.Name.Substring(1);
                        proName = item.Name.Substring(0, 1).ToLower() + item.Name.Substring(1);
                    }
                    else
                    {
                        eleType = clsType.FullName;
                        proName = clsType.Name;
                    }

                    proName = proName.Substring(0, 1).ToLower() + proName.Substring(1);
                    DJTools.append(ref txt, level, "");
                    DJTools.append(ref txt, level, "public {0} {1} { get; set; }", eleType, proName);
                }
            }

            if (!string.IsNullOrEmpty(namespaceText))
            {
                DJTools.append(ref txt, 1, "}");
                DJTools.append(ref txt, 0, "}");
            }
            codes.Add(txt);
        }

        private void MatchProperties(JToken val, ref Type clsType)
        {
            clsType = null;
            if (null == val) return;
            JObject jobj = val.ToObject<JObject>();
            IEnumerable<JProperty> properties = jobj.Properties();
            int size = properties.Count();
            bool mbool = false;
            PropertyInfo[] piArr = null;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var itemDic in dicType)
            {
                piArr = itemDic.Value.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                if (piArr.Length != size) continue;
                dic.Clear();
                foreach (var prop in piArr)
                {
                    dic[prop.Name.ToLower()] = prop.Name;
                }
                mbool = true;
                foreach (JProperty prop in properties)
                {
                    if (!dic.ContainsKey(prop.Name.ToLower()))
                    {
                        mbool = false;
                        break;
                    }
                }

                if (mbool)
                {
                    clsType = itemDic.Value;
                    break;
                }
            }
        }

        private void SetObjVal(object vObj, JToken val)
        {
            JObject jo = val.ToObject<JObject>();
            IEnumerable<JProperty> properties1 = jo.Properties();
            PropertyInfo pi = null;
            Type type = null;
            Type vType = null;
            object v1 = null;
            foreach (var item in properties1)
            {
                if (JTokenType.Array == item.Value.Type)
                {
                    vType = null;
                    if (dicType.ContainsKey(item.Name.ToLower())) vType = dicType[item.Name.ToLower()];
                    if (null != vType)
                    {
                        object list = ExtCollection.createListByType(vType);
                        foreach (var item1 in item.Value.Children())
                        {
                            v1 = Activator.CreateInstance(vType);
                            SetObjVal(v1, item1);
                            ExtCollection.listAdd(list, v1);
                        }
                        vObj.SetPropertyValue(item.Name, list);
                    }
                    else
                    {
                        pi = vObj.GetPropertyInfo(item.Name);
                        if (null == pi) continue;
                        type = pi.PropertyType.GetGenericArguments()[0];
                        object list = ExtCollection.createListByType(type);
                        foreach (var item1 in item.Value.Children())
                        {
                            v1 = DJTools.ConvertTo(item1.ToString(), type);
                            if (null == v1) continue;
                            ExtCollection.listAdd(list, v1);
                        }
                        vObj.SetPropertyValue(item.Name, list);
                    }
                    continue;
                }
                else if (JTokenType.Object == item.Value.Type)
                {
                    vType = null;
                    if (dicType.ContainsKey(item.Name.ToLower())) vType = dicType[item.Name.ToLower()];
                    if (null == vType)
                    {
                        pi = vObj.GetPropertyInfo(item.Name);
                        if (null == pi) continue;
                        vType = pi.PropertyType;
                    }
                    v1 = Activator.CreateInstance(vType);
                    SetObjVal(v1, item.Value);
                    vObj.SetPropertyValue(item.Name, v1);
                    continue;
                }
                else if (JTokenType.None == item.Value.Type || JTokenType.Null == item.Value.Type || JTokenType.Undefined == item.Value.Type)
                {
                    continue;
                }
                v1 = item.Value;
                if (null == v1) continue;
                v1 = v1.ToString();
                vObj.SetPropertyValue(item.Name, v1);
            }
        }

        private string GetEleTypeOfArray(string key, JToken jToken, ref string eleTypeName, ref JToken jToken1)
        {
            string tp = "string";
            string eleType = "";
            foreach (JToken ele in jToken.Children())
            {
                eleTypeName = ele.Type.ToString().ToLower();
                jToken1 = ele;
                if (JTokenType.String == ele.Type || JTokenType.Null == ele.Type || JTokenType.Undefined == ele.Type)
                {
                    tp = "string";
                }
                else if (JTokenType.Integer == ele.Type)
                {
                    tp = "int";
                }
                else if (JTokenType.Float == ele.Type)
                {
                    tp = "float";
                }
                else if (JTokenType.Boolean == ele.Type)
                {
                    tp = "bool";
                }
                else if (JTokenType.Array == ele.Type)
                {
                    eleType = GetEleTypeOfArray(key, ele, ref eleTypeName, ref jToken1);
                    tp = "List<" + eleType + ">";
                }
                else if (JTokenType.Date == ele.Type || JTokenType.TimeSpan == ele.Type)
                {
                    tp = "DateTime";
                }
                else if (JTokenType.Guid == ele.Type)
                {
                    tp = "Guid";
                }
                else if (JTokenType.Object == ele.Type)
                {
                    tp = key.Substring(0, 1).ToUpper() + key.Substring(1);
                }
                break;
            }
            return tp;
        }

    }
}
