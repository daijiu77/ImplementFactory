using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory;
using System.Reflection;

namespace System.DJ.ImplementFactory.MServiceRoute.ServiceManager
{
    public class ServiceIPCollector
    {
        private CallMethodInfo[] _callMethodInfos = null;
        private Dictionary<object, string> controllerDic = new Dictionary<object, string>();

        public ServiceIPCollector()
        {
            if (null == ImplementAdapter.mSFilterMessage) return;
            AbsMSFilterMessage mSFilter = ImplementAdapter.mSFilterMessage as AbsMSFilterMessage;
            if (null == mSFilter) return;
            mSFilter.SetIPReceiver(ReceiveIP);
        }

        private void ReceiveIP(string ip, object controller, MethodInfo actionMethod, bool isFinished)
        {
            lock (this)
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

        public void Monitor(params CallMethodInfo[] callMethodInfos)
        {
            _callMethodInfos = callMethodInfos;
        }

        public string GetIP(object controller)
        {
            lock (this)
            {
                string ip = "";
                controllerDic.TryGetValue(controller, out ip);
                return ip;
            }
        }
    }
}
