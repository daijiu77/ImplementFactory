using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace System.DJ.MicroService
{
    public class HttpHelper
    {
        public void SendData(string uri, string json, Action<string, string> action)
        {
            byte[] dts = Encoding.Default.GetBytes(json);
            HttpContent httpContent = new ByteArrayContent(dts);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string resultData = "";
            string err = "";

            HttpClient httpClient = new HttpClient();
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

            action(resultData, err);
        }
    }
}
