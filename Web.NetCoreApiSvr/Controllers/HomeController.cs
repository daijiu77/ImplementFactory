using Microsoft.AspNetCore.Mvc;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using Test.NetCore.MSVisitor;

namespace Web.NetCoreApiSvr.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [AutoCall]
        private readonly IHomeController _homeController;

        public HomeController()
        {
            ImplementAdapter.Register(this);
        }

        [HttpGet, Route("Test")]
        public object Test()
        {
            object v = _homeController.Test();
            return new { message = "Hello world!" };
        }
    }
}
