using Microsoft.AspNetCore.Mvc;
using System;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;

namespace Test.NetCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        /// <summary>
        /// 对服务设置注册的有效时间和约束key
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="contractKey"></param>
        /// <returns></returns>
        [HttpPost, MSConfiguratorAction, Route("SetMSConfig")]
        public object SetMSConfig(DateTime startTime, DateTime endTime, string contractKey)
        {
            return new { startTime = startTime, endTime = endTime, contractKey = contractKey };
        }

        /// <summary>
        /// 向服务注册访问端 ip 地址
        /// </summary>
        /// <param name="contractKey"></param>
        /// <returns></returns>
        [HttpPost, MSClientRegisterAction, Route("RegisterIP")]
        public object RegisterIP(string contractKey)
        {
            return new { contractKey = contractKey };
        }

        /// <summary>
        /// 设置服务路由访问的地址及注册相关信息，作用于 MicroServiceRoute.xml 
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="url"></param>
        /// <param name="addr"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        [MSAddServiceRouteItemAction(serviceRouteNameMapping:"routeName", uriMapping: "url", registerAddrMapping: "addr", registerActionTypeMapping: "actionType")]
        [HttpGet, Route("SetMSRouteItem")]
        public object SetMSRouteItem(string routeName, string url, string addr, int actionType)
        {
            return new { routeName = routeName, url = url, addr = addr, actionType = actionType };
        }

        /// <summary>
        /// 删除服务路由地址项，作用于 MicroServiceRoute.xml 
        /// </summary>
        /// <param name="routeName"></param>
        /// <returns></returns>
        [MSRemoveServiceRouteItemAction(serviceRouteNameMapping: "routeName")]
        [HttpGet, Route("DelMSRouteItem")]
        public object DelMSRouteItem(string routeName)
        {
            return new { routeName = routeName };
        }
    }
}
