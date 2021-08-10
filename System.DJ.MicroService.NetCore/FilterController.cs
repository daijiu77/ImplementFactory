using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public class FilterController: ActionFilterAttribute
    {
        private string svrConfigFile = "svr_config.dt";
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IQueryCollection qc = context.HttpContext.Request.Query;
            //context.Result = new JsonResult(new { data="this is a test."});
            //base.OnActionExecuting(context);
        }

    }
}
