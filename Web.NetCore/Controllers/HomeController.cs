using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.Linq;
using System.Threading.Tasks;
using Web.NetCore.Models;

namespace Web.NetCore.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        [AutoCall]
        private Calculate calculate;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            ImplementAdapter.Register(this);
        }

        public IActionResult Index()
        {
            string s = calculate.GetStr();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost, MSClientRegisterAction, Route("RegisterIP")]
        public IActionResult RegisterIP(string contractKey)
        {
            return new JsonResult(new { contractKey });
        }

        [HttpPost, MSConfiguratorAction, Route("SetEnabledRegister")]
        public IActionResult SetEnabledRegister(DateTime startTime, DateTime endTime, string contractKey)
        {
            return new JsonResult(new { startTime, endTime, contractKey });
        }

        [HttpPost, Route("ReceiveManage")]
        public IActionResult ReceiveManage(object data)
        {
            return new JsonResult(new { Message = "Successfully" });
        }
    }
}
