using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using System.Linq;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Identifies a method as an API interface information receiving method
    /// </summary>
    public class MSApiReceiverAction : AbsActionFilterAttribute
    {
        private string dataMapping = "";

        /// <summary>
        /// Identifies a method as an API interface information receiving method
        /// </summary>
        public MSApiReceiverAction() { }

        /// <summary>
        /// Identifies a method as an API interface information receiving method
        /// </summary>
        /// <param name="dataMapping">The parameter name of the Data mapping</param>
        public MSApiReceiverAction(string dataMapping)
        {
            this.dataMapping = dataMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            object data = null;
            IDictionary<string, object> paraDic = context.ActionArguments;
            if (0 < paraDic.Count)
            {
                if (string.IsNullOrEmpty(dataMapping))
                {
                    string[] keys = paraDic.Keys.ToArray();
                    data = paraDic[keys[0]];
                }
                else
                {
                    data = paraDic[dataMapping];
                }
            }

            if(null != data)
            {
                string ip = GetIP(context.HttpContext);
                SvrAPISchema svrAPISchema = new SvrAPISchema();
                svrAPISchema.Save(ip, data);
            }
            base.OnActionExecuting(context);
        }

    }
}
