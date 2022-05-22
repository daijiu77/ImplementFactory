using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public class HttpHelper : IHttpHelper
    {
        void IHttpHelper.SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, Action<object, string> action)
        {
            byte[] dts = null;
            string mvType = "application/octet-stream";
            if (null != data)
            {
                if (isJson)
                {
                    mvType = "application/json";
                    string json = JsonConvert.SerializeObject(data);
                    dts = Encoding.Default.GetBytes(json);
                }
                else if (null != data as byte[])
                {
                    dts = (byte[])data;
                }
            }
            else
            {
                dts = Encoding.Default.GetBytes("{\"content\": null}");
            }
            HttpContent httpContent = new ByteArrayContent(dts);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(mvType);

            string resultData = "";
            string err = "";

            HttpClient httpClient = new HttpClient();
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
                var postasync = httpClient.PostAsync(uri, httpContent);
                postasync.Wait();
                httpResponseMessage = postasync.Result;
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
                    object ex = httpResponseMessage.Content.ReadAsStringAsync().Exception;
                    if (null != ex)
                    {
                        err = ex.ToString();
                        //throw new Exception(err);
                    }
                }
            }            

            resultData = null == resultData ? "" : resultData;
            action(resultData, err);
        }
    }
}
