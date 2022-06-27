using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess
{


    public class DbBody : DbVisitor
    {
        private List<ConditionItem> conditionItems = new List<ConditionItem>();
        private Dictionary<string, object> dicPara = new Dictionary<string, object>();

        public DbBody() { }

        public DbBody Where(params ConditionItem[] conditionItems)
        {
            if (null != conditionItems)
            {
                foreach (var item in conditionItems)
                {
                    this.conditionItems.Add(item);
                }
            }
            return this;
        }

        public DbBody PageSize(int pageSize)
        {
            this.pageSize = pageSize;
            return this;
        }

        public DbBody Skip(int pageNumber)
        {
            this.pageNumber = pageNumber;
            return this;
        }

        public DbBody Top(int top)
        {
            this.top = top;
            return this;
        }

        private List<OrderbyItem> orderbyItems = new List<OrderbyItem>();
        public DbBody Orderby(params OrderbyItem[] orderbyItems)
        {
            if (null != orderbyItems)
            {
                foreach (var item in orderbyItems)
                {
                    this.orderbyItems.Add(item);
                }
            }
            return this;
        }

        private List<string> groupFields = new List<string>();
        public List<string> group
        {
            get { return groupFields; }
        }

        private List<object> selectFields = new List<object>();
        public List<object> Select
        {
            get { return selectFields; }
        }

        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public int top { get; set; }

        public void SetParameter(string parameterName, object parameterValue)
        {
            if (dicPara.ContainsKey(parameterName)) dicPara.Remove(parameterName);
            dicPara.Add(parameterName, parameterValue);
        }

        public string GetSql()
        {
            return "";
        }
    }
}
