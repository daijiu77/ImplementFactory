using Microsoft.AspNetCore.Mvc;

namespace Test.NetCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : Controller
    {
        [HttpPost, Route("GetUserInfo")]        
        public object GetUserInfo(object data)
        {
            //string pwd = DJPW.GetPW(3024);
            return new { success = true, message = "", data = data };
        }
    }
}
