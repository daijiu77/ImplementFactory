using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Web.NetCore.Models
{
    public class UIResultModel : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            bool isUIData = false;
            object controller = context.Controller;
            Type controllerType = controller.GetType();
            Attribute attr = controllerType.GetCustomAttribute(typeof(UIDataAttribute), true);
            if (null == attr) attr = controllerType.GetCustomAttribute(typeof(UIResultModel), true);
            if (null != attr) isUIData = true;

            if (!isUIData)
            {
                string actionName = "";
                ActionDescriptor actionObj = context.ActionDescriptor;
                PropertyInfo pi = actionObj.GetType().GetProperty("ActionName");
                if (null != pi)
                {
                    object piVal = pi.GetValue(actionObj);
                    if (null != piVal) actionName = piVal.ToString();
                }

                string actionName1 = "";
                string s = actionObj.DisplayName;
                Regex rg = new Regex(@"(?<ActionName>[a-z0-9_]+)\s*\(", RegexOptions.IgnoreCase);
                if (rg.IsMatch(s))
                {
                    actionName1 = rg.Match(s).Groups["ActionName"].Value;
                }

                if (!string.IsNullOrEmpty(actionName1)) actionName = actionName1;

                if (!string.IsNullOrEmpty(actionName))
                {
                    MethodInfo mi = controllerType.GetMethod(actionName);
                    if (null != mi)
                    {
                        attr = mi.GetCustomAttribute(typeof(UIDataAttribute), true);
                        if (null == attr) attr = mi.GetCustomAttribute(typeof(UIResultModel), true);
                        if (null != attr) isUIData = true;
                    }
                }
            }

            if (isUIData)
            {
                string err = "";
                FieldInfo fi = controllerType.GetField("err", BindingFlags.Instance | BindingFlags.NonPublic);
                if (null != fi)
                {
                    object fiVal = fi.GetValue(controller);
                    if (null != fiVal) err = fiVal.ToString().Trim();
                }
                object result = context.Result;
                ResultMessage msg = null;
                if (null != result)
                {
                    PropertyInfo pi = result.GetType().GetProperty("Value");
                    if (null != pi)
                    {
                        result = pi.GetValue(result, null);
                    }
                    msg = result as ResultMessage;
                }

                if (null == msg)
                {
                    msg = new ResultMessage()
                    {
                        Data = result,
                        Status = context.HttpContext.Response.StatusCode,
                        Success = string.IsNullOrEmpty(err),
                        Error = err
                    };
                }

                context.Result = new JsonResult(msg);
            }

            base.OnActionExecuted(context);
        }
    }
}
