using System.Web.Mvc;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// A method for setting the registration permission of client 'IP' in the SvrIPAddr.xml file, which must contain three parameters: 'startTime' and 'endTime' of date type, and 'contractKey' of string type
    /// </summary>
    public class MSConfiguratorAction : AbsSysAttributer
    {
        private string _startTime = "start";
        private string _endTime = "end";
        private string _contractKey = "key";

        /// <summary>
        /// A method for setting the registration permission of client 'IP' in the SvrIPAddr.xml file, which must contain three parameters: 'startTime' and 'endTime' of date type, and 'contractKey' of string type
        /// </summary>
        public MSConfiguratorAction() { }

        /// <summary>
        /// A method for setting the registration permission of client 'IP' in the SvrIPAddr.xml file
        /// </summary>
        /// <param name="startTimeMapping">The parameter name of the StartTime mapping</param>
        /// <param name="endTimeMapping">The parameter name of the EndTime mapping</param>
        /// <param name="contractKeyMapping">The parameter name of the ContractKey mapping</param>
        public MSConfiguratorAction(string startTimeMapping, string endTimeMapping, string contractKeyMapping)
        {
            _startTime = startTimeMapping;
            _endTime = endTimeMapping;
            _contractKey = contractKeyMapping;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            IDictionary<string, object> dicPara = context.ActionParameters;
            List<string> list = new List<string>() { _startTime, _endTime, _contractKey };
            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromBody(context.HttpContext, list, true);
            }

            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromForm(context.HttpContext, list, true);
            }

            if (0 == dicPara.Count)
            {
                dicPara = GetKVListFromHeader(context.HttpContext, list, true);
            }
            string start = "";
            string end = "";
            string key = "";
            string fKey = "";
            foreach (var item in dicPara)
            {
                fKey = item.Key.ToLower();
                if (-1 != fKey.IndexOf("start"))
                {
                    if (null != item.Value) start = item.Value.ToString();
                }
                else if (-1 != fKey.IndexOf("end"))
                {
                    if (null != item.Value) end = item.Value.ToString();
                }
                else if (-1 != fKey.IndexOf("key"))
                {
                    if (null != item.Value) key = item.Value.ToString();
                }
            }

            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end) || string.IsNullOrEmpty(key))
            {
                string displayName = context.ActionDescriptor.ActionName;
                Regex rg = new Regex(@"controller\.(?<MethodName>[a-z0-9_]+)\s*\(", RegexOptions.IgnoreCase);
                string MethodName = "";
                if (rg.IsMatch(displayName))
                {
                    MethodName = rg.Match(displayName).Groups["MethodName"].Value;
                }
                else
                {
                    MethodName = displayName;
                }
                MethodName = "(" + MethodName + ") ";
                string err = "The method {0}parameter must contain and set a valid start time(startTime) and end time(endTime) and contract key({1}).".ExtFormat(MethodName, MSConst.contractKey);
                throw new Exception(err);
            }

            DateTime startTime = DateTime.Now;
            DateTime.TryParse(start, out startTime);

            DateTime endTime = DateTime.Now;
            DateTime.TryParse(end, out endTime);

            _mSService.SetEnabledTime(startTime, endTime, key);
        }
    }
}
