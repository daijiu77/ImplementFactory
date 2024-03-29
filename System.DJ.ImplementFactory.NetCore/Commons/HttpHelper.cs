﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.Commons
{
    public class HttpHelper : IHttpHelper
    {
        void IHttpHelper.SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, MethodTypes methodTypes, int timeoutOfSecond, Action<object, string> action)
        {
            byte[] dts = null;
            string mvType = "application/octet-stream";
            if (null != data)
            {
                if (isJson)
                {
                    mvType = "application/json";
                    string json = "";
                    if (typeof(IDictionary).IsAssignableFrom(data.GetType()))
                    {
                        IDictionary dic = (IDictionary)data;
                        ICollection keys = dic.Keys;
                        string vs = "";
                        Regex reg = new Regex(@"(^true$)|(^false$)|(^null$)|(^[0-9]$)|(^\-[0-9]$)|(^[1-9][0-9]*[0-9]$)|(^\-[1-9][0-9]*[0-9]$)|(^[0-9]\.[0-9]*[0-9]$)|(^\-[0-9]\.[0-9]*[0-9]$)|(^[1-9][0-9]+\.[0-9]*[0-9]$)|(^\-[1-9][0-9]+\.[0-9]*[0-9]$)", RegexOptions.IgnoreCase);
                        foreach (var item in keys)
                        {
                            if (null == item) continue;
                            if (!dic.Contains(item)) continue;
                            if (null == dic[item])
                            {
                                vs = "null";
                            }
                            else
                            {
                                vs = dic[item].ToString();
                            }

                            if (reg.IsMatch(vs))
                            {
                                vs = vs.ToLower();
                            }
                            else
                            {
                                vs = "\"" + vs + "\"";
                            }
                            json += ", \"{0}\": {1}".ExtFormat(item.ToString(), vs);
                        }

                        if (!string.IsNullOrEmpty(json))
                        {
                            json = json.Substring(1).Trim();
                            json = "{" + json + "}";
                        }
                    }
                    else if (data.GetType().IsClass && (typeof(string) != data.GetType()))
                    {
                        json = JsonConvert.SerializeObject(data);
                    }
                    else
                    {
                        json = data.ToString();
                    }
                    dts = Encoding.Default.GetBytes(json);
                }
                else if (null != data as byte[])
                {
                    dts = (byte[])data;
                }
                else
                {
                    dts = Encoding.UTF8.GetBytes(data.ToString());
                }
            }
            else
            {
                dts = Encoding.UTF8.GetBytes("{\"content\": null}");
            }
            HttpContent httpContent = new ByteArrayContent(dts);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(mvType);

            string resultData = "";
            string err = "";

            int timeout = ImplementAdapter.dbInfo1.HttpTimeout_Second;
            if (0 >= timeout) timeout = 30;
            if (0 < timeoutOfSecond) timeout = timeoutOfSecond;
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            if (null != heads)
            {
                foreach (KeyValuePair<string, string> item in heads)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            HttpResponseMessage httpResponseMessage = null;
            try
            {
                Task<HttpResponseMessage> postasync = null;
                if (MethodTypes.Post == methodTypes)
                {
                    postasync = httpClient.PostAsync(uri, httpContent);
                }
                else
                {
                    postasync = httpClient.GetAsync(uri);
                }
                postasync.Wait();
                httpResponseMessage = postasync.Result;
            }
            catch (TimeoutException ex)
            {
                err = "TimeoutException";
            }
            catch (Exception ex)
            {
                err = ex.ToString();
                //throw ex;
            }

            if (null != httpResponseMessage)
            {
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var rsa = httpResponseMessage.Content.ReadAsStringAsync();
                    rsa.Wait();
                    resultData = rsa.Result;
                }
                else
                {
                    try
                    {
                        httpResponseMessage.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        DJTools.append(ref err, ex.Message);
                        //throw;
                    }
                }
            }

            try
            {
                if (null != httpResponseMessage) httpResponseMessage.Dispose();
            }
            catch (Exception)
            {

                //throw;
            }

            try
            {
                if (null != httpClient) httpClient.Dispose();
            }
            catch (Exception)
            {

                //throw;
            }

            try
            {
                if (null != httpContent) httpContent.Dispose();
            }
            catch (Exception)
            {

                //throw;
            }

            resultData = null == resultData ? "" : resultData;
            action(resultData, err);
        }

        void IHttpHelper.SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, Action<object, string> action)
        {
            ((IHttpHelper)this).SendData(uri, heads, data, isJson, MethodTypes.Post, -1, action);
        }

        void IHttpHelper.SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, MethodTypes methodTypes, Action<object, string> action)
        {
            ((IHttpHelper)this).SendData(uri, heads, data, isJson, methodTypes, -1, action);
        }
    }
}
