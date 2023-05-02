using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public abstract class AbsMSAllot : IMSAllot
    {
        Dictionary<string, string> IMSAllot.HttpHeaders(string routeName, string controllerName, string actionName)
        {
            return HttpHeaders(routeName, controllerName, actionName);
        }

        Dictionary<string, string> IMSAllot.HttpParameters(string routeName, string controllerName, string actionName)
        {
            return HttpParameters(routeName, controllerName, actionName);
        }

        string[] IMSAllot.UrlCollection(string routeName)
        {
            return UrlCollection(routeName);
        }

        void IMSAllot.HttpVisitingException(string routeName, string url, string exceptionMessage)
        {
            HttpVisitingException(routeName, url, exceptionMessage);
        }

        /// <summary>
        /// Sets the Headers for the HttpClient access operation
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="actionName">The name of the routing action method</param>
        /// <returns>Returns the 'headers' parameter collection</returns>
        public virtual Dictionary<string, string> HttpHeaders(string routeName, string controllerName, string actionName)
        {
            return null;
        }

        /// <summary>
        /// Sets the Parameters for the HttpClient access operation
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="actionName">The name of the routing action method</param>
        /// <returns>Returns the 'parameters' parameter collection</returns>
        public virtual Dictionary<string, string> HttpParameters(string routeName, string controllerName, string actionName)
        {
            return null;
        }

        /// <summary>
        /// Dynamically set the access address of the target service cluster based on the service name
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <returns></returns>
        public virtual string[] UrlCollection(string routeName)
        {
            return null;
        }

        /// <summary>
        /// Receive information about exceptions that occurred during access
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="url">Http address</param>
        /// <param name="exceptionMessage">Exception message</param>
        public virtual void HttpVisitingException(string routeName, string url, string exceptionMessage)
        {
            //
        }
    }
}
