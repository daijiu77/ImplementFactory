using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.Framework
{
    public delegate void getData(object data);
    public interface ICalculate
    {
        event getData GetData;

        string PropertyTest { get; set; }

        [DataCache]
        int Sum(int a, int b);

        /// <summary>
        /// ����������Ϊ AutoCall�����Ҹò���ֵΪ null ʱ������Զ�ע�� AutoCall ʵ������
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="autoCall"></param>
        /// <returns></returns>
        int Division(int a, int b, AutoCall autoCall = null);
    }
}
