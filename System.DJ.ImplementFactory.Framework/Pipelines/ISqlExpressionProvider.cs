using System.Collections.Generic;
using System.Data.Common;
using System.DJ.ImplementFactory.Pipelines.Pojo;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Pipelines
{
    public interface ISqlExpressionProvider
    {
        string provideSql(DbList<DbParameter> dbParameters, DataOptType dataOptType, PList<Para> paraList, object[] methodParameters);
    }
}
