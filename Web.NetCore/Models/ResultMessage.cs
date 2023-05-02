using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.NetCore.Models
{
    [Serializable]
    public class ResultMessage
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
        public string Token { get; set; }
        public bool Success { get; set; }
        public int RecordCount { get; set; }
        public DateTime CurrentTime { get; set; } = DateTime.Now;

        public static Task<ResultMessage> Result(HttpContext context, object data)
        {
            return Result(context, data, null, null, null, 0);
        }

        public static Task<ResultMessage> Result(HttpContext context, object data, int recordCount)
        {
            return Result(context, data, null, null, null, recordCount);
        }

        public static Task<ResultMessage> Result(HttpContext context, object data, string msg)
        {
            return Result(context, data, msg, null, null, 0);
        }

        public static Task<ResultMessage> Result(HttpContext context, object data, string msg, string err, string token)
        {
            return Result(context, data, msg, err, token, 0);
        }

        public static Task<ResultMessage> Result(HttpContext context, object data, string msg, string err, string token, int recordCount)
        {
            ResultMessage resultMessage = new ResultMessage();
            resultMessage.Error = err;
            resultMessage.Message = msg;
            resultMessage.Data = data;
            resultMessage.Token = token;
            resultMessage.Success = string.IsNullOrEmpty(err);
            resultMessage.Status = context.Response.StatusCode;
            resultMessage.RecordCount = recordCount;
            return Task.FromResult(resultMessage);
        }
    }
}
