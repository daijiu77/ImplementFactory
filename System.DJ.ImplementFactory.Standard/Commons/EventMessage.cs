using System;
using System.Collections.Generic;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public delegate void TypeReceiver(Type type);
    public class EventMessage
    {
        public static event TypeReceiver OnTypeReceiver;

        public static void ExcEvent(Type type)
        {
            if (null == OnTypeReceiver) return;
            OnTypeReceiver(type);
        }
    }
}
