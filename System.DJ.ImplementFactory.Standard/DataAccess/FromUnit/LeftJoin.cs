namespace System.DJ.ImplementFactory.DataAccess.FromUnit
{
    public class LeftJoin : SqlFromUnit
    {
        private LeftJoin() { }

        public static LeftJoin Me
        {
            get { return new LeftJoin(); }
        }

        public static LeftJoin Instance
        {
            get { return new LeftJoin(); }
        }

    }
}
