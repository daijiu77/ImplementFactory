using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using Web.NetCore.Models;

namespace Web.NetCore.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private SvrAPISchema svrAPISchema = new SvrAPISchema();

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

        [HttpPost, UIData, MSClientRegisterAction, Route("RegisterIP")]
        public object RegisterIP(string contractKey)
        {
            return contractKey;
        }

        [HttpPost, UIData, MSConfiguratorAction, Route("SetEnabledRegister")]
        public object SetEnabledRegister(DateTime startTime, DateTime endTime, string contractKey)
        {
            return new { startTime, endTime, contractKey };
        }

        [HttpPost, UIData, Route("Test")]
        public object Test()
        {
            return new { Message = "Hello World!" };
        }

        [HttpPost, UIData, Route("ReceiveManage")]
        public object ReceiveManage(object data)
        {
            string ip = Startup.serviceIPCollector.GetIP(this);            
            svrAPISchema.Save(ip, data);
            return new { Message = "Successfully", CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        }

        [HttpPost, UIData, Route("GetApiMethods")]
        public object GetApiNames()
        {
            return svrAPISchema.GetServiceNames();
        }

        [HttpPost, UIData, Route("GetApiByServiceName")]
        public object GetApiByServiceName(string serviceName)
        {
            return svrAPISchema.GetServiceAPIByServiceName(serviceName);
        }
    }
}
