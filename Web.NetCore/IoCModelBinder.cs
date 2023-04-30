using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Web.NetCore
{
    //confirmBinder 代理方法是用来对外实现控制器里的接口方法过滤的，如果返回值为 false 表示无需执行数据模型重新绑定
    public delegate bool confirmBinder(string controller, string action, object entity, string propertyName, object propertyValue);

    public class IoCModelBinder : IModelBinder
    {
        private static confirmBinder ConfirmBinder = null;

        public static void SetConfirmBinder(confirmBinder ConfirmBinder)
        {
            IoCModelBinder.ConfirmBinder = ConfirmBinder;
        }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.ActionContext.HttpContext.Request;

            JObject jObject = null;
            string dataFromSource = "";
            if (null != bindingContext.BindingSource) dataFromSource = bindingContext.BindingSource.DisplayName;
            Type type = bindingContext.ModelType;
            if (typeof(byte[]) == type || typeof(Stream).IsAssignableFrom(type) || typeof(Stream) == type)
            {
                return Task.CompletedTask;
            }

            string json = "";
            string txt = "";
            string k = "";
            object fv = null;
            if (null == dataFromSource) dataFromSource = "";
            dataFromSource = dataFromSource.ToLower();
            if (string.IsNullOrEmpty(dataFromSource) || dataFromSource.Equals("body") || dataFromSource.Equals("query"))
            {
                using (var reader = new StreamReader(request.Body))
                {
                    Task<string> ts = reader.ReadToEndAsync();
                    ts.Wait();
                    txt = ts.Result;
                    if (null == txt) txt = "";
                    if (!string.IsNullOrEmpty(txt))
                    {
                        byte[] dt = Encoding.Default.GetBytes(txt);
                        request.Body = new MemoryStream(dt);
                    }
                }
            }

            if (3 < txt.Trim().Length)
            {
                json = txt.Trim();
                if (json.Substring(0, 1).Equals("{") && json.Substring(json.Length - 1).Equals("}"))
                {
                    jObject = JObject.Parse(json);
                }
            }


            if ((string.IsNullOrEmpty(dataFromSource) || dataFromSource.Equals("form")) && false == request.Method.ToLower().Equals("get"))
            {
                try
                {
                    if (0 < request.Form.Count)
                    {
                        if (null == jObject) jObject = new JObject();
                        foreach (var item in request.Form)
                        {
                            k = item.Key.Replace("-", "_");
                            if (null != jObject.Property(k)) continue;
                            fv = item.Value;
                            if (null == fv) fv = "";
                            jObject.Add(k, fv.ToString());
                        }
                    }
                }
                catch (Exception)
                {

                    //throw;
                }
            }

            if ((string.IsNullOrEmpty(dataFromSource) || dataFromSource.Equals("query")) && null != request.Query)
            {
                try
                {
                    if (0 < request.Query.Count)
                    {
                        if (null == jObject) jObject = new JObject();
                        foreach (var item in request.Query)
                        {
                            k = item.Key.Replace("-", "_");
                            if (null != jObject.Property(k)) continue;
                            fv = item.Value;
                            if (null == fv) fv = "";
                            jObject.Add(k, fv.ToString());
                        }
                    }
                }
                catch (Exception)
                {

                    //throw;
                }

            }

            if ((string.IsNullOrEmpty(dataFromSource) || dataFromSource.Equals("header")) && null != request.Headers)
            {
                try
                {
                    if (0 < request.Headers.Count)
                    {
                        if (null == jObject) jObject = new JObject();
                        foreach (var item in request.Headers)
                        {
                            k = item.Key.Replace("-", "_");
                            if (null != jObject.Property(k)) continue;
                            fv = item.Value;
                            if (null == fv) fv = "";
                            jObject.Add(k, fv.ToString());
                        }
                    }
                }
                catch (Exception)
                {

                    //throw;
                }

            }

            if (3 == request.RouteValues.Keys.Count)
            {
                if (null == jObject) jObject = new JObject();
                k = request.RouteValues.Keys.ToList()[2];
                string v = request.RouteValues.Values.ToList<object>()[2].ToString();
                if (null != jObject.Property(k)) jObject.Remove(k);
                jObject.Add(k, v);
            }

            object para = bindingContext.Model;
            if (null != jObject)
            {
                Stream stream = null;
                //如果参数为 object 或 dynamic 类型,侧传入所有数据
                if (type == typeof(object))
                {
                    txt = JsonConvert.SerializeObject(jObject);
                    para = DataTranslation.JsonToObject(txt);
                }
                else if (typeof(byte[]) == type || typeof(Stream).IsAssignableFrom(type) || typeof(Stream) == type)
                {
                    para = null;
                    stream = null;
                    if (0 < request.Form.Files.Count)
                    {
                        if (null != request.Form.Files[bindingContext.FieldName]) stream = request.Form.Files[bindingContext.FieldName].OpenReadStream();
                        if (null == stream) stream = request.Form.Files[0].OpenReadStream();
                    }

                    if (null != stream)
                    {
                        if (typeof(Stream).IsAssignableFrom(type) || typeof(Stream) == type)
                        {
                            para = stream;
                        }
                        else
                        {
                            long len = stream.Length;
                            byte[] bts = new byte[len];
                            stream.Read(bts, 0, (int)len);
                            stream.Close();
                            stream.Dispose();
                            para = bts;
                        }
                    }
                }
                else if (DJTools.IsBaseType(type))
                {
                    IEnumerable<JProperty> jps = jObject.Properties();
                    string fn = bindingContext.ModelName.ToLower();
                    if (string.IsNullOrEmpty(fn)) fn = bindingContext.FieldName.ToLower();
                    foreach (var item in jps)
                    {
                        if (item.Name.ToLower().Equals(fn))
                        {
                            para = DJTools.ConvertTo(item.Value.ToString(), type);
                            break;
                        }
                    }
                }
                else if (type.IsClass)
                {
                    try
                    {
                        para = Activator.CreateInstance(type);
                    }
                    catch (Exception) { }

                    if (null != para)
                    {
                        IEnumerable<JProperty> jps = jObject.Properties();
                        string v = "";
                        RouteValueDictionary rvDic = bindingContext.ActionContext.RouteData.Values;
                        PropertyInfo pi = null;
                        string controller = "";
                        string action = "";
                        if (rvDic.ContainsKey("controller")) controller = rvDic["controller"].ToString();
                        if (rvDic.ContainsKey("action")) action = rvDic["action"].ToString();
                        foreach (var item in jps)
                        {
                            v = item.Value.ToString();
                            if (null != ConfirmBinder)
                            {
                                if (!ConfirmBinder(controller, action, para, item.Name, v)) continue;
                            }
                            pi = para.GetPropertyInfo(item.Name);
                            if (null == pi) continue;
                            if (typeof(byte[]) == pi.PropertyType || typeof(Stream).IsAssignableFrom(pi.PropertyType) || typeof(Stream) == pi.PropertyType)
                            {
                                stream = null;
                                if (0 < request.Form.Files.Count)
                                {
                                    if (null != request.Form.Files[pi.Name]) stream = request.Form.Files[pi.Name].OpenReadStream();
                                    if (null == stream) stream = request.Form.Files[0].OpenReadStream();
                                }

                                if (null != stream)
                                {
                                    if (typeof(Stream).IsAssignableFrom(type) || typeof(Stream) == type)
                                    {
                                        para.SetPropertyValue(item.Name, stream);
                                    }
                                    else
                                    {
                                        long len = stream.Length;
                                        byte[] bts = new byte[len];
                                        stream.Read(bts, 0, (int)len);
                                        stream.Close();
                                        stream.Dispose();
                                        para.SetPropertyValue(item.Name, bts);
                                    }
                                }
                                continue;
                            }
                            para.SetPropertyValue(item.Name, v);  //给实体属性赋值
                        }
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(para);
            }
            else if (!string.IsNullOrEmpty(txt))
            {
                if (typeof(object) == type)
                {
                    bindingContext.Result = ModelBindingResult.Success(txt);
                }
                else if (type.IsBaseType())
                {
                    bool mbool = false;
                    object vObj = txt.ConvertTo(type, ref mbool);
                    if(mbool)
                    {
                        bindingContext.Result = ModelBindingResult.Success(vObj);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
