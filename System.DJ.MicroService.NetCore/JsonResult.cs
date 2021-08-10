using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace System.DJ.MicroService.NetCore
{
    public class JsonResult: IActionResult
    {
        object _data = null;

        public JsonResult(object json)
        {
            _data = json;
        }

        Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            response.ContentType = $"{context.HttpContext.Request.ContentType}; charset=utf-8";

            string json = string.Empty;
            if (this._data != null)
            {
                json = JsonConvert.SerializeObject(_data);
            }

            return Task.FromResult(response.WriteAsync(json));
        }
    }
}
