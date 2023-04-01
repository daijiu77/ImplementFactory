using System.Web.Mvc;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSFilter : AbsActionFilterAttribute
    {

        public MSFilter()
        {
            //
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Type type = context.ActionDescriptor.GetType();
            string ip = GetIP(context);
            if (!_kvDic.ContainsKey(ip))
            {
                throw new Exception("Illegal access");
            }
            base.OnActionExecuting(context);
        }

    }
}
