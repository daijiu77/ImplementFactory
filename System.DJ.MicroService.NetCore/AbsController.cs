using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.Text;

namespace System.DJ.MicroService
{
    public abstract class AbsController: ControllerBase
    {
        public AbsController()
        {
            ImplementAdapter.Register(this);
        }

        public override RedirectToRouteResult RedirectToRoute(string routeName, object routeValues, string fragment)
        {
            return base.RedirectToRoute(routeName, routeValues, fragment);
        }

        public override RedirectToActionResult RedirectToAction(string actionName, string controllerName, object routeValues, string fragment)
        {
            return base.RedirectToAction(actionName, controllerName, routeValues, fragment);
        }
    }
}
