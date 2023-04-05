namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSStart : Attribute
    {
        public MSStart()
        {
            MService.Start();       
        }
    }
}
