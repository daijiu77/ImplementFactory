using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DataAccess;

namespace System.DJ.ImplementFactory.Entities
{
    public class DataCacheTable : AbsDataModel
    {
        [FieldMapping("Id", typeof(Guid), 1, "", true, true)]
        public Guid Id { get; set; } = Guid.Empty;

        [FieldMapping("MethodPath", typeof(string), 1000)]
        public string MethodPath { get; set; }

        [FieldMapping("Key", typeof(string), 500)]
        public string Key { get; set; }

        public int CycleTimeSecond { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        [FieldMapping("DataType", typeof(string), 500)]
        public string DataType { get; set; }

        [FieldMapping("Data", typeof(byte[]), 4000)]
        public byte[] Data { get; set; }

    }
}
