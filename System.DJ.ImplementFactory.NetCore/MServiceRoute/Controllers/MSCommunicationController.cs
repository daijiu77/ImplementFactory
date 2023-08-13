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
using System.Threading;
using System.Threading.Tasks;

namespace System.DJ.ImplementFactory.MServiceRoute.Controllers
{
    [ApiController]
    [Route("MSCommunication")]
    public class MSCommunicationController : ControllerBase
    {
        private static Dictionary<string, TempContractKey> idDic = new Dictionary<string, TempContractKey>();
        private static List<string> idList = new List<string>();

        static MSCommunicationController()
        {
            Task.Run(() =>
            {
                const int sleepNum = 2000;
                int size = 0;
                int num = 0;
                TempContractKey tempContractKey = null;
                DateTime dt = DateTime.Now;
                string key = "";
                while (true)
                {
                    size = idDic.Count;
                    num = 0;
                    while (num < size)
                    {
                        key = GetKey(num);
                        tempContractKey = TCK(key, false);
                        dt = DateTime.Now;
                        if (tempContractKey.endTime <= dt)
                        {
                            TCK(key, true);
                            num = 0;
                            size = idDic.Count;
                        }
                        else
                        {
                            num++;
                        }
                    }
                    Thread.Sleep(sleepNum);
                }
            });
        }

        private static string GetKey(int index)
        {
            lock (idDic)
            {
                if (index >= idList.Count) return "";
                return idList[index];
            }
        }

        private static TempContractKey TCK(string contractKey, bool delete)
        {
            lock (idDic)
            {
                TempContractKey tempContractKey = null;
                string keyLower = contractKey.ToLower();
                if (delete)
                {
                    idDic.TryGetValue(keyLower, out tempContractKey);
                    idDic.Remove(keyLower);
                    idList.Remove(keyLower);
                }
                else if (idDic.ContainsKey(keyLower))
                {
                    tempContractKey = idDic[keyLower];
                }
                else
                {
                    DateTime dt = DateTime.Now.AddMinutes(2);
                    idDic.Add(keyLower, new TempContractKey()
                    {
                        contractKey = contractKey,
                        endTime = dt,
                    });
                    idList.Add(keyLower);
                }
                return tempContractKey;
            }
        }

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

        /// <summary>
        /// Get target service url information.
        /// </summary>
        /// <param name="serviceName">Target service name</param>
        /// <param name="callerPort">A port of service of call method 'GetUrlInfoByServiceName'</param>
        /// <returns></returns>
        [HttpPost, Route("GetUrlInfoByServiceName")]
        public ActionResult GetUrlInfoByServiceName(string serviceName, string callerPort)
        {
            Response.ContentType = "application/json";
            SvrAPISchema svrAPISchema = new SvrAPISchema();
            SvrAPI svrApi = svrAPISchema.GetServiceAPIByServiceName(serviceName);
            if (null == svrApi) return GetSvrAPIOptionResult();
            if (0 == svrApi.Items.Count) return GetSvrAPIOptionResult();

            SvrAPIOption option = svrApi.GetSvrAPIOption();
            string callerIP = AbsActionFilterAttribute.GetIP(this.HttpContext);

            string url_src = "{0}://{1}:{2}/{3}/{4}";
            string url = "";
            IHttpHelper httpHelper = new HttpHelper();
            if (1 < svrApi.Items.Count)
            {
                const int maxNum = 100;
                int num = 0;
                int ncount = svrApi.Items.Count;
                string msg = "";
                while (maxNum > num)
                {
                    num++;
                    url = url_src.ExtFormat(option.HttpType,
                    option.IP,
                    option.Port,
                    MSConst.MSCommunication,
                    MSConst.SysTest);
                    httpHelper.SendData(url, null, null, true, MethodTypes.Post, 5, (res, err) =>
                    {
                        msg = err;
                    });
                    if (num == ncount) break;
                    if (string.IsNullOrEmpty(msg)) break;
                    option = svrApi.GetSvrAPIOption();
                }

                if(!string.IsNullOrEmpty(msg)) return GetSvrAPIOptionResult();
            }

            if (option.IP.Equals(MSConst.Localhost) && (false == callerIP.Equals(MSConst.Localhost)))
            {
                url = url_src.ExtFormat(
                option.HttpType,
                callerIP,
                callerPort,
                MSConst.MSCommunication,
                MSConst.SysTest);
                string ip = "";
                httpHelper.SendData(url, null, null, true, (res, err) =>
                {
                    if (!string.IsNullOrEmpty(err)) return;
                    if (null == res) return;
                    MSIPInfo mSIPInfo1 = res.ToString().JsonToEntity<MSIPInfo>();
                    if (null == mSIPInfo1) return;
                    ip = mSIPInfo1.IP;
                });
                return GetSvrAPIOptionResult(option.HttpType, ip, option.Port, option.ContractKey);
            }
            else
            {
                return GetSvrAPIOptionResult(option.HttpType, option.IP, option.Port, option.ContractKey);
            }
        }

        [HttpPost, Route("AuthenticateKey")]
        public ActionResult AuthenticateKey(string key)
        {
            TempContractKey temp = TCK(key, true);
            string result = "false";
            if (null != temp)
            {
                result = "true";
            }
            return Content(result);
        }

        private ActionResult GetSvrAPIOptionResult(string httpType, string ip, string port, string contractKey)
        {
            string json = GetSvrAPIOption(httpType, ip, port, contractKey);
            return Content(json);
        }

        private ActionResult GetSvrAPIOptionResult()
        {
            string json = GetSvrAPIOption(null, null, null, null);
            return Content(json);
        }

        private string GetSvrAPIOption(string httpType, string ip, string port, string contractKey)
        {
            string key = contractKey;
            if (!string.IsNullOrEmpty(key))
            {
                string kv=Guid.NewGuid().ToString();
                TCK(kv, false);
                key += MSConst.keySplit + kv;
            }
            SvrAPIOption option1 = new SvrAPIOption();
            option1.HttpType = httpType;
            option1.IP = ip;
            option1.Port = port;
            option1.ContractKey = key;
            string json = JsonConvert.SerializeObject(option1);
            return json;
        }

        class TempContractKey
        {
            public string contractKey { get; set; }
            public DateTime endTime { get; set; }
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
