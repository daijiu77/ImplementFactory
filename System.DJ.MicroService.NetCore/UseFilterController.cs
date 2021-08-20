using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines;
using System.DJ.MicroService.Pipelines;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public static class UseFilterController
    {
        private static SvrMng svrMng = null;
        static UseFilterController()
        {
            svrMng = new SvrMng();
        }

        private class SvrMng: ImplementAdapter
        {
            [AutoCall]
            private IManageSvrInfo manageSvrInfo;

            public IManageSvrInfo GetMSI()
            {
                return manageSvrInfo;
            }

            public IInstanceCodeCompiler GetCodeCompiler()
            {
                return ImplementAdapter.codeCompiler;
            }
        }

        private static Func<HttpContext, Func<Task>, Task> func = (context, next) =>
        {
            context.Filter();
            return next();
        };

        private static Func<RequestDelegate, RequestDelegate> middleware = (request) =>
        {
            RequestDelegate requestDelegate = (context) =>
            {
                context.Filter();
                return request(context);
            };
            return requestDelegate;
        };

        public static Func<RequestDelegate, RequestDelegate> Filter()
        {
            return middleware;
        }

        public static Func<HttpContext, Func<Task>, Task> ExtFilter()
        {
            return func;
        }

        private static void Filter(this HttpContext context)
        {
            IHeaderDictionary head = context.Request.Headers;
            string token = head["token"];
            string name = head["name"];

            string contentType = context.Request.ContentType;
            if (null == contentType) contentType = "";

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(name))
            {
                Illegal(context);
            }
            else if(-1 != contentType.ToLower().IndexOf("json"))
            {
                string txt = GetContent(context);

                if (!ValidateData(txt))
                {
                    Illegal(context);
                    return;
                }

                JToken jt = JToken.Parse(txt);
                JToken json = jt[""];
            }

            string type = head["type"];
            if (string.IsNullOrEmpty(type)) type = "";
            object vObj = new { token = token };
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
            string txt = "";
            StreamReader reader = new StreamReader(context.Request.Body);
            Task<string> ts = reader.ReadToEndAsync();
            ts.Wait();
            txt = ts.Result;
            reader.Close();
            reader.Dispose();

            byte[] dt = Encoding.Default.GetBytes(txt);
            context.Request.Body = new MemoryStream(dt);
            if (null == txt) txt = "";
            return txt.Trim();
        }

    }
}
