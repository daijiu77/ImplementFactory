using System.Diagnostics;
using System.DJ.ImplementFactory.Pipelines;
using System.Reflection;

namespace System.DJ.ImplementFactory.MServiceRoute
{
    public class ServiceRegisterMessage
    {
        private static Type GetSrcType()
        {
            StackTrace trace = new StackTrace();
            StackFrame stackFrame = null;
            MethodBase mb = null;
            Type meType = typeof(ServiceRegisterMessage);
            Type pt = null;
            Type srcType = null;
            const int maxNum = 10;
            int num = 0;
            while (num <= maxNum)
            {
                stackFrame = trace.GetFrame(num);
                if (null == stackFrame) break;
                mb = stackFrame.GetMethod();
                if (null == mb) break;
                pt = mb.DeclaringType;
                if (null == pt) break;
                if (pt != meType)
                {
                    srcType = pt;
                    break;
                }
                num++;
            }

            return srcType;
        }

        /// <summary>
        /// It is called illegal.
        /// </summary>
        /// <returns></returns>
        private static bool isIllegalCall()
        {
            Type srcType = GetSrcType();
            if (null == srcType) return true;
            if (srcType != typeof(MService)) return true;
            return false;
        }

        public void RegisterSuccess(string routeName, string url, MethodTypes methodTypes, string contractValue)
        {
            if (isIllegalCall()) return;
            MSDataVisitor.RegisterSuccess(routeName, url, methodTypes, contractValue);
            Success(routeName, url, methodTypes, contractValue);
        }

        public void RegisterFail(string routeName, string url, MethodTypes methodTypes, string contractValue, string err)
        {
            if (isIllegalCall()) return;
            Fail(routeName, url, methodTypes, contractValue, err);
        }

        public void TestVisit(string routeName, string url, MethodTypes methodTypes, string contractValue, string err)
        {
            if (isIllegalCall()) return;
            MSDataVisitor.TestVisit(routeName, url, methodTypes, contractValue, err);
            Test(routeName, url, methodTypes, contractValue, err);
        }

        public virtual void Success(string routeName, string url, MethodTypes methodTypes, string contractValue)
        {
            //
        }

        public virtual void Fail(string routeName, string url, MethodTypes methodTypes, string contractValue, string err)
        {
            //
        }

        public virtual void Test(string routeName, string url, MethodTypes methodTypes, string contractValue, string err)
        {
            //
        }
    }
}
