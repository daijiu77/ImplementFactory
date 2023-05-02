using System.Collections.Generic;

namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IMSAllot
    {
        /// <summary>
        /// Dynamically set the access address of the target service cluster based on the service name
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <returns></returns>
        string[] UrlCollection(string routeName);

        /// <summary>
        /// Sets the Headers for the HttpClient access operation
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="actionName">The name of the routing action method</param>
        /// <returns>Returns the 'headers' parameter collection</returns>
        Dictionary<string, string> HttpHeaders(string routeName, string controllerName, string actionName);


        /// <summary>
        /// Sets the Parameters for the HttpClient access operation
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="actionName">The name of the routing action method</param>
        /// <returns>Returns the 'parameters' parameter collection</returns>
        Dictionary<string, string> HttpParameters(string routeName, string controllerName, string actionName);

        /// <summary>
        /// Receive information about exceptions that occurred during access
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="url">Http address</param>
        /// <param name="exceptionMessage">Exception message</param>
        void HttpVisitingException(string routeName, string url, string exceptionMessage);
    }
}
