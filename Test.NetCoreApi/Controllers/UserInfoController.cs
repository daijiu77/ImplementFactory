using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.DJ.MicroService;
using System.Linq;
using System.Threading.Tasks;

namespace Test.NetCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : AbsController
    {
        [HttpPost, Route("GetUserInfo")]        
        public object GetUserInfo(object data)
        {
            return new { success = true, message = "", data = data };
        }
    }
}
