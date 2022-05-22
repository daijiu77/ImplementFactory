using System.DJ.ImplementFactory.Pipelines;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.Attrs.Generic
{
    public class AnalyzeSqlForSelectOnClass: AnalyzeSqlOnClass
    {
        public AnalyzeSqlForSelectOnClass(Type methodParameterType) : base(DataOptType.select, methodParameterType) { }
    }
}
