using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class RightJoin : SqlFromUnit
    {
        private RightJoin() { }
        public static RightJoin Me { get { return new RightJoin(); } }
        public static RightJoin Instance { get { return new RightJoin(); } }
    }
}
