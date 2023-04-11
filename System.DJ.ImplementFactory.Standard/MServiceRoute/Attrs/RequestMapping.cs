using System.DJ.ImplementFactory.Pipelines;

namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    /// <summary>
    /// Sets the mapping relationship between the current interface method and the remote service interface method.
    /// </summary>
    public class RequestMapping : Attribute
    {
        private string _name;
        private MethodTypes _methodTypes;

        /// <summary>
        /// Sets the mapping relationship between the current interface method and the remote service interface method.
        /// </summary>
        /// <param name="name">Remote interface method name</param>
        /// <param name="methodTypes">Remote Service Interface Method Access Mode (Get/Post)</param>
        public RequestMapping(string name, MethodTypes methodTypes)
        {
            _name = name;
            _methodTypes = methodTypes;
        }

        /// <summary>
        /// Sets the mapping relationship between the current interface method and the remote service interface method.
        /// </summary>
        /// <param name="name">Remote interface method name</param>
        public RequestMapping(string name)
        {
            _name = name;
            _methodTypes = MethodTypes.Post;
        }

        /// <summary>
        /// Remote interface method name
        /// </summary>
        public string Name { get { return _name; } }
        /// <summary>
        /// Remote Service Interface Method Access Mode (Get/Post)
        /// </summary>
        public MethodTypes MethodType { get { return _methodTypes; } }
    }
}
