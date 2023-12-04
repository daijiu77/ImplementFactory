using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons.Attrs;
using System.DJ.ImplementFactory.DCache.Attrs;
using System.Text;
using System.Threading.Tasks;
using Test.Framework.DataInterface;
using Test.Framework.Entities;
using Test.Framework.InterfaceTest;
using Test.Framework.MSVisitor;

namespace Test.Framework
{
    public class CalculateImpl : ICalculate
    {
        [MyAutoCall]
        private MSVisitor.IUserInfo apiUserInfo;

        [MyAutoCall]
        DataInterface.IUserInfo userInfo;

        private MyCache _myCache = null;
        private IDepart _depart = null;

        public CalculateImpl(MyCache myCache, IDepart depart)
        {
            _depart = depart;
            _myCache = myCache;
        }

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

        [DataCache(true)]
        Plan IBaseTest<Plan>.getDataObj(ref int a, out int b)
        {
            a = 1;
            b = 2;
            return new Plan()
            {
                PName = "abc",
                Id = Guid.NewGuid(),
                num = 5,
            };
        }

        [DataCache(true)]
        int ICalculate.Sum(int a, int b, ref int c, out int d)
        {
            int num = a + b;
            c = num;
            d = a * b;
            return a - b;
        }
    }
}
