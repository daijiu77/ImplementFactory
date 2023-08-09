using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public abstract class AbsMSAllot : IMSAllot
    {
        Dictionary<string, string> IMSAllot.HttpHeaders(string routeName, string controllerName, string actionName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return HttpHeaders(routeName, controllerName, actionName, arg, extMSDataVisitor);
        }

        Dictionary<string, string> IMSAllot.HttpParameters(string routeName, string controllerName, string actionName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return HttpParameters(routeName, controllerName, actionName, arg, extMSDataVisitor);
        }

        string[] IMSAllot.UrlCollection(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return UrlCollection(routeName, arg, extMSDataVisitor);
        }

        void IMSAllot.HttpVisitingException(string routeName, string url, string exceptionMessage, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            HttpVisitingException(routeName, url, exceptionMessage, arg, extMSDataVisitor);
        }

        string IMSAllot.GetContractKey(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return GetContractKey(routeName, arg, extMSDataVisitor);
        }

        MethodTypes IMSAllot.GetMethodTypes(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return GetMethodTypes(routeName, arg, extMSDataVisitor);
        }

        object IMSAllot.GetSendData(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return GetSendData(routeName, arg, extMSDataVisitor);
        }

        void IMSAllot.HttpResponse(string routeName, string controllerName, string actionName, string url, object resultData)
        {
            HttpResponse(routeName, controllerName, actionName, url, resultData);
        }

        /// <summary>
        /// Sets the Headers for the HttpClient access operation
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="actionName">The name of the routing action method</param>
        /// <returns>Returns the 'headers' parameter collection</returns>
        public virtual Dictionary<string, string> HttpHeaders(string routeName, string controllerName, string actionName, object arg, IExtMSDataVisitor extMSDataVisitor)
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
        public virtual Dictionary<string, string> HttpParameters(string routeName, string controllerName, string actionName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return null;
        }

        /// <summary>
        /// Dynamically set the access address of the target service cluster based on the service name
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <returns></returns>
        public virtual string[] UrlCollection(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return null;
        }

        /// <summary>
        /// Receive information about exceptions that occurred during access
        /// </summary>
        /// <param name="routeName">Target service name</param>
        /// <param name="url">Http address</param>
        /// <param name="exceptionMessage">Exception message</param>
        public virtual void HttpVisitingException(string routeName, string url, string exceptionMessage, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            //
        }

        public virtual string GetContractKey(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return null;
        }

        public virtual MethodTypes GetMethodTypes(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return MethodTypes.None;
        }

        public virtual object GetSendData(string routeName, object arg, IExtMSDataVisitor extMSDataVisitor)
        {
            return null;
        }

        public virtual void HttpResponse(string routeName, string controllerName, string actionName, string url, object resultData)
        {
            //
        }
    }
}
