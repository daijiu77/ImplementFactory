using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Pipelines
{
    public enum MethodTypes
    {
        Get,
        Post
    }

    public interface IHttpHelper
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="uri">访问地址</param>
        /// <param name="heads">head 参数</param>
        /// <param name="data">待发送的数据</param>
        /// <param name="isJson">是否是 json 数据, 如果不是以字节数组方式发送</param>
        /// <param name="action">参数1: 返回数据, 参数2: 返回错误信息</param>
        void SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, Action<object, string> action);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="uri">访问地址</param>
        /// <param name="heads">head 参数</param>
        /// <param name="data">待发送的数据</param>
        /// <param name="isJson">是否是 json 数据, 如果不是以字节数组方式发送</param>
        /// <param name="methodTypes"></param>
        /// <param name="action">参数1: 返回数据, 参数2: 返回错误信息</param>
        void SendData(string uri, Dictionary<string, string> heads, object data, bool isJson, MethodTypes methodTypes, Action<object, string> action);
    }
}
