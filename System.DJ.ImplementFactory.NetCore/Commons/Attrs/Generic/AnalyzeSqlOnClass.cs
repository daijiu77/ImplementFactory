using System.DJ.ImplementFactory.Pipelines;

/// <summary>
/// Author: 代久 - Allan
/// QQ: 564343162
/// Email: 564343162@qq.com
/// CreateDate: 2020-03-05
/// </summary>
namespace System.DJ.ImplementFactory.Commons.Attrs.Generic
{
    public class AnalyzeSqlOnClass : Attribute
    {
        DataOptType dataOptType = DataOptType.none;
        Type methodParameterType = null;

        public AnalyzeSqlOnClass(DataOptType dataOptType, Type methodParameterType)
        {
            this.dataOptType = dataOptType;
            this.methodParameterType = methodParameterType;
        }

        public DataOptType GetDataOptType() { return dataOptType; }

        public Type GetMethodParameterType() { return methodParameterType; }

        public IAnalyzeSql analyzeSql { get; set; }
    }
}
