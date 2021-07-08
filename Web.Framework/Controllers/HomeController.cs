using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Framework.Models;

namespace Web.Framework.Controllers
{
    public class HomeController : Controller
    {
        [AutoCall]
        private Calculate calculate;

        public HomeController()
        {
            string s = System.Web.HttpContext.Current.Request.MapPath("~/");
            ImplementAdapter.Register(this);
        }

        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}