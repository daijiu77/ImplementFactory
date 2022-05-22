using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Text;

namespace Test.Standard
{
    public class MyAutoCall: AutoCall
    {
        /// <summary>
        /// 执行方法之前被触发
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implement"></param>
        /// <param name="methodName"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public override bool ExecuteBeforeFilter(Type interfaceType, object implement, string methodName, PList<Para> paras)
        {
            return base.ExecuteBeforeFilter(interfaceType, implement, methodName, paras);
        }

        /// <summary>
        /// 执行方法之后被触发
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implement"></param>
        /// <param name="methodName"></param>
        /// <param name="paras"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool ExecuteAfterFilter(Type interfaceType, object implement, string methodName, PList<Para> paras, object result)
        {
            return base.ExecuteAfterFilter(interfaceType, implement, methodName, paras, result);
        }

        /// <summary>
        /// 拦截异常
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="implement"></param>
        /// <param name="methodName"></param>
        /// <param name="paras"></param>
        /// <param name="ex"></param>
        public override void ExecuteException(Type interfaceType, object implement, string methodName, PList<Para> paras, Exception ex)
        {
            base.ExecuteException(interfaceType, implement, methodName, paras, ex);
        }
    }
}
