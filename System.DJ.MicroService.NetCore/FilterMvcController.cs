using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public class FilterMvcController : ActionFilterAttribute
    {
        private string svrConfigFile = "svr_config.dt";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IHeaderDictionary head = context.HttpContext.Request.Headers;
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

            //IQueryCollection query = context.HttpContext.Request.Query;
            //IFormCollection form = context.HttpContext.Request.Form;
            //context.Result = new JsonResult(new { data="this is a test."});
            //base.OnActionExecuting(context);
        }

        private bool ValidateData(string txt)
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

        private string GetContent(ActionExecutingContext context)
        {
            string contentType = context.HttpContext.Request.ContentType;
            string txt = "";
            if (!string.IsNullOrEmpty(contentType))
            {
                StreamReader reader = new StreamReader(context.HttpContext.Request.Body);
                Task<string> ts = reader.ReadToEndAsync();
                ts.Wait();
                txt = ts.Result;
                reader.Close();
                reader.Dispose();
            }
            if (null == txt) txt = "";
            return txt.Trim();
        }

        private void Illegal(ActionExecutingContext context)
        {
            object obj = new { message = "Illegal access" };
            string json = JsonConvert.SerializeObject(obj);
            byte[] dt = Encoding.Default.GetBytes(json);
            context.HttpContext.Response.Body.WriteAsync(dt, 0, dt.Length);
        }

    }
}
