using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Linq;
using System.Threading.Tasks;
using Web.NetCore.Models;

namespace Web.NetCore.Controllers
{
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
    }
}
