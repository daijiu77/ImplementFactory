using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.Reflection;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class ServiceIPCollector
    {
        private CallMethodInfo[] _callMethodInfos = null;
        private static Dictionary<object, string> controllerDic = new Dictionary<object, string>();
        private static object _serviceIPCollectorLock = new object();

        public ServiceIPCollector()
        {
            if (null == ImplementAdapter.mSFilterMessage) return;
            AbsMSFilterMessage mSFilter = ImplementAdapter.mSFilterMessage as AbsMSFilterMessage;
            if (null == mSFilter) return;
            mSFilter.SetIPReceiver(ReceiveIP);
        }

        private void ReceiveIP(string ip, object controller, MethodInfo actionMethod, bool isFinished)
        {
            lock (_serviceIPCollectorLock)
            {
                if (null == _callMethodInfos) return;
                if (0 == _callMethodInfos.Length) return;
                bool mbool = false;
                Type controllerType = controller.GetType();
                string methodName = actionMethod.Name.ToLower();
                foreach (CallMethodInfo mMethodInfo in _callMethodInfos)
                {
                    if (mMethodInfo.ControllerType.Equals(controllerType))
                    {
                        if (!string.IsNullOrEmpty(mMethodInfo.MethodName))
                        {
                            mbool = mMethodInfo.MethodName.ToLower().Equals(methodName);
                        }
                        else if (null != mMethodInfo.MethodNames)
                        {
                            if (0 < mMethodInfo.MethodNames.Length)
                            {
                                foreach (string mName in mMethodInfo.MethodNames)
                                {
                                    if (mName.Trim().ToLower().Equals(methodName))
                                    {
                                        mbool = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                mbool = true;
                            }
                        }
                        else
                        {
                            mbool = true;
                        }
                    }
                    if (mbool) break;
                }

                if (!mbool) return;
                if (false == isFinished)
                {
                    controllerDic[controller] = ip;
                }
                else
                {
                    controllerDic.Remove(controller);
                }
            }
        }

        /// <summary>
        /// Set by the accessed IP address, the invoked controller, or you can set the called method name
        /// </summary>
        /// <param name="callMethodInfos">Sets the type of controller being called, or the name of the method being called</param>
        public void Monitor(params CallMethodInfo[] callMethodInfos)
        {
            _callMethodInfos = callMethodInfos;
        }

        /// <summary>
        /// Obtain the corresponding client IP address based on the called controller object
        /// </summary>
        /// <param name="controller">The controller object that is called</param>
        /// <returns>Returns the client IP address</returns>
        public string GetIP(object controller)
        {
            lock (_serviceIPCollectorLock)
            {
                string ip = "";
                controllerDic.TryGetValue(controller, out ip);
                return ip;
            }
        }
    }
}
