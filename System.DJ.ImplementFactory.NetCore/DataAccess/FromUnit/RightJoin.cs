using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class RightJoin : SqlFromUnit
    {
        private RightJoin() { }
        public static new RightJoin New { get { return new RightJoin(); } }
        public static new RightJoin Me { get { return new RightJoin(); } }
        public static new RightJoin Instance { get { return new RightJoin(); } }
    }
}
