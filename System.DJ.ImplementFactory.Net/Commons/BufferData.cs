using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.DJ.ImplementFactory.Commons
{
    public class BufferData
    {
        static BufferData _bufferData = null;

        static BufferData()
        {
            _bufferData = new BufferData();
        }

        public static BufferData Instance { get { return _bufferData; } }
    }
}
