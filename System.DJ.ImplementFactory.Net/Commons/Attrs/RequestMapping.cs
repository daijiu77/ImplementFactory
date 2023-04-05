using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Pipelines;
using System.Text;

namespace System.DJ.ImplementFactory.Commons.Attrs
{
    public class RequestMapping : Attribute
    {
        private string _name;
        private MethodTypes _methodTypes;

        public RequestMapping(string name, MethodTypes methodTypes)
        {
            _name = name;
            _methodTypes = methodTypes;
        }

        public RequestMapping(string name)
        {
            _name = name;
            _methodTypes = MethodTypes.Post;
        }

        public string Name { get { return _name; } }
        public MethodTypes MethodType { get { return _methodTypes; } }
    }
}
