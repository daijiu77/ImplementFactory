using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DCache.Attrs;
using System.Threading.Tasks;

namespace Test.Framework.InterfaceTest
{
    public delegate void getData(object data);
    public interface ICalculate
    {
        event getData GetData;

        string PropertyTest { get; set; }

        [DataCache]
        int Sum(int a, int b);

        [DataCache]
        Task<int> TaskSum(int a, int b);

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
