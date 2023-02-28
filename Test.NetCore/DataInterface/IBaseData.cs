using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.Pipelines.Pojo;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.NetCore.DataInterface
{
    public interface IBaseData<T>
    {
        [AutoInsert("insert into {T}{userInfo}",
            fields: new string[] { "cdatetime" },
            fieldsType: FieldsType.Exclude,
            EnabledBuffer: true)]
        int insert(T userInfo);
    }
}
