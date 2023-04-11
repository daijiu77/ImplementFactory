namespace System.DJ.ImplementFactory.NetCore.Commons.Attrs
{
    /// <summary>
    /// Identifies data model properties that do not participate in the specified data operations
    /// </summary>
    public class IgnoreField : Attribute
    {
        public enum IgnoreType
        {
            none = 0,
            /// <summary>
            /// Does not participate in data insertion operations
            /// </summary>
            Insert = 1,
            /// <summary>
            /// Does not participate in data update operations
            /// </summary>
            Update = 2,
            /// <summary>
            /// Does not participate in joining stored procedure parameters
            /// </summary>
            Procedure = 4,
            /// <summary>
            /// Does not participate in all data operations
            /// </summary>
            All = (1 | 2 | 4)
        }

        /// <summary>
        /// Set up data operations that do not participate
        /// </summary>
        public IgnoreType ignoreType { get; set; } = IgnoreType.none;

        /// <summary>
        /// Identifies data model properties that do not participate in the specified data operations
        /// </summary>
        /// <param name="ignoreType">Set up data operations that do not participate</param>
        public IgnoreField(IgnoreType ignoreType)
        {
            this.ignoreType = ignoreType;
        }
    }
}
