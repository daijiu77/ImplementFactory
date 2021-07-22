using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;

namespace Test.Framework
{
    public interface ICalculate
    {
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
