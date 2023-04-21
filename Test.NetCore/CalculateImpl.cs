using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.Text;
using System.Threading.Tasks;
using Test.NetCore.DataInterface;
using Test.NetCore.InterfaceTest;

namespace Test.NetCore
{
    public class CalculateImpl : ICalculate
    {
        [MyAutoCall]
        private IApiUserInfo apiUserInfo;

        [MyAutoCall]
        IUserInfo userInfo;

        private event getData GetData;

        event getData ICalculate.GetData
        {
            add
            {
                GetData += value;
            }
            remove
            {
                GetData -= value;
            }
        }

        private string pi = "";
        string ICalculate.PropertyTest
        {
            get { return pi; }
            set { pi = value; }
        }

        int ICalculate.Division(int a, int b, AutoCall autoCall)
        {
            int c = 0;
            try
            {
                c = a / b;
            }
            catch (Exception ex)
            {
                autoCall.e(ex.ToString(), System.DJ.ImplementFactory.Commons.ErrorLevels.dangerous);
                //throw;
            }
            return c;
        }

        int ICalculate.Sum(int a, int b)
        {
            return a + b;
        }

        async Task<int> ICalculate.TaskSum(int a, int b)
        {
            return await Task.FromResult((a + b));
        }
    }
}
