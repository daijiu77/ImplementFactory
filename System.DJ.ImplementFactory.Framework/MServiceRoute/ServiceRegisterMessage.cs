using System.Diagnostics;
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
