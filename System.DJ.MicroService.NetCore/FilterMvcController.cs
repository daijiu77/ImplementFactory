using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public class FilterMvcController: ActionFilterAttribute
    {
        private string svrConfigFile = "svr_config.dt";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContent httpContent = null;
            string contentType = context.HttpContext.Request.ContentType;
            int len = 0;
            Stream stream = null;
            if (!string.IsNullOrEmpty(contentType))
            {
                len = (int)context.HttpContext.Request.ContentLength;
            }
            IQueryCollection query = context.HttpContext.Request.Query;
            
            //IFormCollection form = context.HttpContext.Request.Form;
            IHeaderDictionary head = context.HttpContext.Request.Headers;
            //context.Result = new JsonResult(new { data="this is a test."});
            //base.OnActionExecuting(context);
        }

    }
}
