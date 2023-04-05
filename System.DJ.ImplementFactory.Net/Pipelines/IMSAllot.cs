using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSAllot
    {
        /// <summary>
        /// 根据服务名称动态设置目标服务集群的访问地址
        /// </summary>
        /// <param name="routeName">目标服务集群名称</param>
        /// <returns></returns>
        string[] UrlCollection(string routeName);

        /// <summary>
        /// 设置 HttpClient 访问操作的 Headers
        /// </summary>
        /// <param name="routeName">目标服务名称</param>
        /// <param name="controllerName">控制器名称</param>
        /// <param name="actionName">路由 action 方法名称</param>
        /// <returns>返回 headers 参数集合</returns>
        Dictionary<string, string> HttpHeaders(string routeName, string controllerName, string actionName);


        /// <summary>
        /// 设置 HttpClient 访问操作的 Parameters
        /// </summary>
        /// <param name="routeName">目标服务名称</param>
        /// <param name="controllerName">控制器名称</param>
        /// <param name="actionName">路由 action 方法名称</param>
        /// <returns>返回 parameters 参数集合</returns>
        Dictionary<string, string> HttpParameters(string routeName, string controllerName, string actionName);
    }
}
