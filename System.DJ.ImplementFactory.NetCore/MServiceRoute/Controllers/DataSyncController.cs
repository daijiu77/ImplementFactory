using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.DJ.ImplementFactory.Entities;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
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
            if (syncMessage.Key.Equals(MicroServiceRoute.Key)) return ResultData("Data sync end.", false);

            if (!syncMessage.ServiceFlagDic.ContainsKey(MicroServiceRoute.Key))
            {
                syncMessage.ServiceFlagDic.Add(MicroServiceRoute.Key, MicroServiceRoute.ID);
            }
            MService.DataSyncToLocal(syncMessage.DataSyncOption);
            byte[] dt = Encoding.UTF8.GetBytes(txt);
            int size = dt.Length;
            return ResultData("Data size: {0} B.".ExtFormat(size.ToString()), true);
        }

        private DataSyncMessage ToDataSyncMessage(string txt)
        {
            DataSyncMessage syncMessage = null;
            JToken jtk = JToken.Parse(txt);
            JToken Key = jtk[MSConst.Key];
            JToken ServiceFlagDic = jtk[MSConst.ServiceFlagDic];
            JToken DataSyncOption = jtk[MSConst.DataSyncOption];
            if ((null == Key) || (null == ServiceFlagDic) || (null == DataSyncOption)) return syncMessage;

            syncMessage = new DataSyncMessage();
            syncMessage.Key = Key.ToString();

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
    }
}
/*
DataSyncMessage syncMessage = new DataSyncMessage();
syncMessage.Key = "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416";

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
{"Key": "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416", "ServiceFlagDic":{"e59a600d-19b3-48be-9b99-169d681aaafb":"88ce3e04-396a-4634-933c-1a46ab2fb416","bfec59b8-d613-4891-90be-38e55eed52cc":"0e4a533f-7c53-419f-a54c-e7e4d7f822bb"},"DataSyncOption":{"DataType":1,"Data":{"Message":"Test","Success":true}}}

格式化为：
{
    "Key": "MemberService@88ce3e04-396a-4634-933c-1a46ab2fb416",
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
