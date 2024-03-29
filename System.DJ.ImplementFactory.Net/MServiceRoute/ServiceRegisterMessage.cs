﻿using System.Diagnostics;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class ServiceRegisterMessage
    {
        /// <summary>
        /// It is called illegal.
        /// </summary>
        /// <returns></returns>
        private static bool isIllegalCall()
        {
            Type srcType = DJTools.GetSrcType<ServiceRegisterMessage>();
            if (null == srcType) return true;
            if (srcType != typeof(MService)) return true;
            return false;
        }

        public void RegisterSuccess(string routeName, string url, MethodTypes methodTypes, string contractValue, string message)
        {
            if (isIllegalCall()) return;
            MSDataVisitor.RegisterSuccess(routeName, url, methodTypes, contractValue, message);
            Success(routeName, url, methodTypes, contractValue, message);
        }

        public void RegisterFail(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            if (isIllegalCall()) return;
            Fail(routeName, url, methodTypes, contractValue, message, err);
        }

        public void TestVisit(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            if (isIllegalCall()) return;
            bool success = string.IsNullOrEmpty(err);
            MSDataVisitor.TestVisit(routeName, url, methodTypes, contractValue, message, err, success);
            Test(routeName, url, methodTypes, contractValue, message, err);
        }

        public virtual void Success(string routeName, string url, MethodTypes methodTypes, string contractValue, string message)
        {
            //
        }

        public virtual void Fail(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            //
        }

        public virtual void Test(string routeName, string url, MethodTypes methodTypes, string contractValue, string message, string err)
        {
            //
        }
    }
}
