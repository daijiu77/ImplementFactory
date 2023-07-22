using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using System.Text;

namespace System.DJ.ImplementFactory.MServiceRoute.Controllers
{
    [Route("DataSync")]
    [ApiController]
    public class DataSyncController : ControllerBase
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
            object data = new { Message = "Successfully", Code = this.HttpContext.Response.StatusCode, IP = ip };
            string json = JsonConvert.SerializeObject(data);
            Response.ContentType = "application/json";
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
                json = JsonConvert.SerializeObject(svrApi);
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
