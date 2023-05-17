namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public enum SyncCylces
    {
        OnlyOne, 
        OnlyTwo, 
        OnlyThree, 
        OnlyFour, 
        OnlyFive, 
        Always
    }

    public class DataSyncCylceAttribute : Attribute
    {
        private SyncCylces syncCylces = SyncCylces.OnlyOne;

        public DataSyncCylceAttribute() { }

        public DataSyncCylceAttribute(SyncCylces syncCylces)
        {
            this.syncCylces = syncCylces;
        }

        public SyncCylces Cylce { get { return syncCylces; } }
    }
}
