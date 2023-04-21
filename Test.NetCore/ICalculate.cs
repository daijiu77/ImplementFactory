using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DCache.Attrs;
using System.Threading.Tasks;

namespace Test.NetCore.InterfaceTest
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
        /// 当参数类型为 AutoCall，并且该参数值为 null 时，组件自动注入 AutoCall 实例对象
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="autoCall"></param>
        /// <returns></returns>
        int Division(int a, int b, AutoCall autoCall = null);
    }
}
