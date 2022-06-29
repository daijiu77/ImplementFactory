using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class InnerJoin : SqlFromUnit
    {
        private InnerJoin() { }

        public static InnerJoin Me { get { return new InnerJoin(); } }
        public static InnerJoin Instance { get { return new InnerJoin(); } }
    }
}
