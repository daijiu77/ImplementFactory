using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DJ.ImplementFactory.MServiceRoute.Controllers
{
    [Route("MSCommunication")]
    [ApiController]
    public class MSCommunicationController : ControllerBase
    {
        [HttpPost, Route("Receiver")]
        public ActionResult Receiver(object data)
        {
            string err = "Data is null.";
            if (null == data) return ResultData(err, false);
            string txt = data.ToString();
            if (string.IsNullOrEmpty(txt)) return ResultData(err, false);

            DataSyncMessage syncMessage = ToDataSyncMessage(txt);
            if (null == syncMessage) return ResultData(err, false);
            if (syncMessage.ResourceKey.Equals(MicroServiceRoute.Key)) return ResultData("Data sync end.", false);

            if (!syncMessage.ServiceFlagDic.ContainsKey(MicroServiceRoute.Key))
            {
                syncMessage.ServiceFlagDic.Add(MicroServiceRoute.Key, MicroServiceRoute.ID);
            }

            DataSyncExchange.AddExchnage(syncMessage);
            DataSyncExchange.DataSyncToLocal(syncMessage.DataSyncOption);
            byte[] dt = Encoding.UTF8.GetBytes(txt);
            int size = dt.Length;
            return ResultData("Data size: {0} B.".ExtFormat(size.ToString()), true);
        }

        private DataSyncMessage ToDataSyncMessage(string txt)
        {
            DataSyncMessage syncMessage = null;
            JToken jtk = JToken.Parse(txt);
            JToken ResourceKey = jtk[MSConst.ResourceKey];
            JToken DataSyncsName = jtk[MSConst.DataSyncsName];
            JToken ServiceFlagDic = jtk[MSConst.ServiceFlagDic];
            JToken DataSyncOption = jtk[MSConst.DataSyncOption];
            if ((null == ResourceKey)
                || (null == DataSyncsName)
                || (null == ServiceFlagDic)
                || (null == DataSyncOption)) return syncMessage;

            string dataSyncsName = DataSyncsName.ToString();
            syncMessage = new DataSyncMessage();
            syncMessage.SetDataSyncsName(dataSyncsName)
                .SetResourceKey(ResourceKey.ToString());

            Guid id = Guid.Empty;
            JObject jObj = ServiceFlagDic.ToObject<JObject>();
            IEnumerable<JProperty> jProperties = jObj.Properties();
            foreach (JProperty prop in jProperties)
            {
                id = Guid.Empty;
                Guid.TryParse(prop.Value.ToString(), out id);
                syncMessage.ServiceFlagDic[prop.Name] = id;
            }

            syncMessage.DataSyncOption = new DataSyncItem();
            syncMessage.DataSyncOption.SetDataSyncsName(dataSyncsName);

            string dtType = DataSyncOption[MSConst.DataType].ToString();
            int num = 0;
            int.TryParse(dtType, out num);
            syncMessage.DataSyncOption.DataType = (DataTypes)num;
            syncMessage.DataSyncOption.Data = DataSyncOption[MSConst.Data];
            return syncMessage;
        }

        private ActionResult ResultData(string msg, bool success)
        {
            object data = new { Message = msg, Success = success };
            string resultStr = JsonConvert.SerializeObject(data);
            Response.ContentType = "application/json";
            return Content(resultStr);
        }

        [MSUnlimited]
        [HttpPost, Route("SysTest")]
        public ActionResult SysTest()
        {
            string ip = AbsActionFilterAttribute.GetIP(this.HttpContext);
            MSIPInfo data = new MSIPInfo()
            {
                Message = "Successfully",
                Code = HttpContext.Response.StatusCode,
                IP = ip
            };
            string json = JsonConvert.SerializeObject(data);
            Response.ContentType = "application/json";
            return Content(json);
        }

        [HttpPost, Route("GetCurrentSvrIP")]
        public ActionResult GetCurrentSvrIP(string url, string contractKey)
        {
            Response.ContentType = "application/json";

            string json = "";
            string msUrl = url;
            Regex rg = new Regex(@"^(?<HttpHeader>(http)|(https))\:\/\/(?<HttpBody>[^\/]+)(\/(?<AreaName>[^\/]+))?", RegexOptions.IgnoreCase);
            if (!rg.IsMatch(msUrl)) return Content(json);
            Match m = rg.Match(msUrl);
            string HttpHeader = m.Groups["HttpHeader"].Value;
            string HttpBody = m.Groups["HttpBody"].Value;
            msUrl = "{0}://{1}/{2}/{3}".ExtFormat(HttpHeader, HttpBody, MSConst.MSCommunication, MSConst.SysTest);

            Dictionary<string, string> headDic = new Dictionary<string, string>();
            headDic.Add(MSConst.contractKey, contractKey);

            string ip = "";
            IHttpHelper httpHelper = new HttpHelper();
            httpHelper.SendData(msUrl, headDic, null, true, (resultData, err) =>
            {
                if (!string.IsNullOrEmpty(err)) return;
                if (null == resultData) return;
                string dataStr = ExtMethod.GetCollectionData(resultData.ToString());
                if (string.IsNullOrEmpty(dataStr)) return;
                MSIPInfo mSIPInfo = ExtMethod.JsonToEntity<MSIPInfo>(dataStr);
                if (null == mSIPInfo) return;                
                ip = mSIPInfo.IP;
            });
            MSIPInfo data = new MSIPInfo()
            {
                IP = ip
            };
            json = JsonConvert.SerializeObject(data);
            return Content(json);
        }

        [HttpPost, Route("GetUrlInfoByServiceName")]
        public ActionResult GetUrlInfoByServiceName(string serviceName)
        {
            string json = "";
            SvrAPISchema svrAPISchema = new SvrAPISchema();
            SvrAPI svrApi = svrAPISchema.GetServiceAPIByServiceName(serviceName);
            if (null != svrApi)
            {
                if (0 < svrApi.Items.Count)
                {
                    int index = svrApi.index;
                    SvrAPIOption option = svrApi.Items[index];
                    index++;
                    svrApi.index = index % svrApi.Items.Count;
                    string ip = AbsActionFilterAttribute.GetIP(this.HttpContext);
                    string httpProtocal = "http";
                    if (0 < option.SvrUris.Count)
                    {
                        if (!string.IsNullOrEmpty(option.SvrUris[0].Uri))
                        {
                            Regex rg = new Regex(@"^(?<HttpPro>((http)|(https)))\:", RegexOptions.IgnoreCase);
                            if (rg.IsMatch(option.SvrUris[0].Uri))
                            {
                                httpProtocal = rg.Match(option.SvrUris[0].Uri).Groups["HttpPro"].Value;
                            }
                        }
                    }
                    string url = "{0}://{1}:{2}/{3}/{4}";
                    url = url.ExtFormat(
                        httpProtocal,
                        option.IP,
                        option.Port,
                        MSConst.MSCommunication,
                        MSConst.GetCurrentSvrIP);

                    string paraUrl = "{0}://{1}:{2}".ExtFormat(httpProtocal, ip, option.Port);
                    string paraContractKey = option.ContractKey;
                    object data = new { url = paraUrl, contractKey = paraContractKey };

                    Dictionary<string, string> heads = new Dictionary<string, string>();
                    heads.Add(MSConst.contractKey, option.ContractKey);

                    IHttpHelper httpHelper = new HttpHelper();
                    MSIPInfo mSIPInfo = null;
                    httpHelper.SendData(url, heads, data, true, (resultData, err) =>
                    {
                        if (!string.IsNullOrEmpty(err)) return;
                        if (null == resultData) return;
                        mSIPInfo = ExtMethod.JsonToEntity<MSIPInfo>(resultData.ToString());
                    });

                    if (null != mSIPInfo)
                    {
                        SvrAPIOption option1 = new SvrAPIOption();
                        option1.IP = mSIPInfo.IP;
                        option1.Port = option.Port;
                        option1.ContractKey = option.ContractKey;
                        json = JsonConvert.SerializeObject(option1);
                    }
                }
            }
            return Content(json);
        }
    }
}
/*
DataSyncMessage syncMessage = new DataSyncMessage();
syncMessage.Key = "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416";
syncMessage.SetDataSyncsName("TokenSync");

syncMessage.ServiceFlagDic.Add(Guid.NewGuid().ToString(), Guid.NewGuid());
syncMessage.ServiceFlagDic.Add(Guid.NewGuid().ToString(), Guid.NewGuid());

syncMessage.dataSyncItem = new DataSyncItem()
{
    DataType = DataTypes.Add,
    Data = new { Message = "Test-123", Success = true },
};

转换为 Json 字符串数据：
string result = JsonConvert.SerializeObject(syncMessage);

result 的值：
{"Key": "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416", "DataSyncsName": "TokenSync", "ServiceFlagDic":{"e59a600d-19b3-48be-9b99-169d681aaafb":"88ce3e04-396a-4634-933c-1a46ab2fb416","bfec59b8-d613-4891-90be-38e55eed52cc":"0e4a533f-7c53-419f-a54c-e7e4d7f822bb"},"DataSyncOption":{"DataType":1,"Data":{"Message":"Test","Success":true}}}

格式化为：
{
    "Key": "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416",
    "DataSyncsName": "TokenSync",
	"ServiceFlagDic": {
		"e59a600d-19b3-48be-9b99-169d681aaafb": "88ce3e04-396a-4634-933c-1a46ab2fb416",
		"bfec59b8-d613-4891-90be-38e55eed52cc": "0e4a533f-7c53-419f-a54c-e7e4d7f822bb"
	},
	"DataSyncOption": {
		"DataType": 1,
		"Data": {
			"Message": "Test-123",
			"Success": true
		}
	}
}
 */
