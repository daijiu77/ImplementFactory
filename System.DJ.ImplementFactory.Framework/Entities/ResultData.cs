using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Entities
{
    public class ResultData
    {
        public Guid id { get { return Guid.NewGuid(); } }
        public bool isSuccess { get; set; }
        public object data { get; set; }
        public string message { get; set; }
        public bool isJsonData { get; set; }
        public DateTime dateTime { get { return Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); } }
    }
}
