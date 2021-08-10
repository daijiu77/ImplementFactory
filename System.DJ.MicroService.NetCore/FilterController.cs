using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public class FilterController: ActionFilterAttribute
    {
        private string svrConfigFile = "svr_config.dt";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IQueryCollection query = context.HttpContext.Request.Query;
            Stream stream = context.HttpContext.Request.Body;
            //IFormCollection form = context.HttpContext.Request.Form;
            IHeaderDictionary head = context.HttpContext.Request.Headers;
            //context.Result = new JsonResult(new { data="this is a test."});
            //base.OnActionExecuting(context);
        }

    }
}
