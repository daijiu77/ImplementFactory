using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public static class UseFilterController
    {
        public static void Filter(this HttpContext context)
        {
            IHeaderDictionary head = context.Request.Headers;
            string token = head["token"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                Illegal(context);
            }
            else
            {
                string txt = GetContent(context);

                if (!ValidateData(txt))
                {
                    Illegal(context);
                    return;
                }

                JToken jt = JToken.Parse(txt);
                var type = jt["type"];

                object vObj = new { token = token };
            }
        }

        private static bool ValidateData(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return false;
            }

            if (false == txt.Substring(0, 1).Equals("{") || false == txt.Substring(txt.Length - 1).Equals("}"))
            {
                return false;
            }
            return true;
        }

        private static void Illegal(HttpContext context)
        {
            object obj = new { message = "Illegal access" };
            string json = JsonConvert.SerializeObject(obj);
            byte[] dt = Encoding.Default.GetBytes(json);
            context.Response.Body.WriteAsync(dt, 0, dt.Length);
        }

        private static string GetContent(HttpContext context)
        {
            string contentType = context.Request.ContentType;
            if (null == contentType) contentType = "";
            string txt = "";
            if (-1 != contentType.ToLower().IndexOf("json"))
            {
                StreamReader reader = new StreamReader(context.Request.Body);
                Task<string> ts = reader.ReadToEndAsync();
                ts.Wait();
                txt = ts.Result;
                reader.Close();
                reader.Dispose();

                byte[] dt = Encoding.Default.GetBytes(txt);
                context.Request.Body = new MemoryStream(dt);
            }
            if (null == txt) txt = "";
            return txt.Trim();
        }

    }
}
