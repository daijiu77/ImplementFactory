using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class InnerJoin : SqlFromUnit
    {
        private InnerJoin() { }

        public static new InnerJoin New { get { return new InnerJoin(); } }

        public static new InnerJoin Me { get { return new InnerJoin(); } }
        public static new InnerJoin Instance { get { return new InnerJoin(); } }
    }
}
