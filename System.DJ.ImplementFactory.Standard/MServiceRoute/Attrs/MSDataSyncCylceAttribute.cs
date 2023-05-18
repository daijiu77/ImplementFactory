namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public enum SyncCylces
    {
        OnlyOne, 
        OnlyTwo, 
        OnlyThree, 
        OnlyFour, 
        OnlyFive, 
        OnlySix,
        OnlySeven,
        OnlyEight,
        OnlyNine,
        Always
    }

    public class MSDataSyncCylceAttribute : Attribute
    {
        private SyncCylces syncCylces = SyncCylces.OnlyOne;

        public MSDataSyncCylceAttribute() { }

        public MSDataSyncCylceAttribute(SyncCylces syncCylces)
        {
            this.syncCylces = syncCylces;
        }

        public SyncCylces Cylce { get { return syncCylces; } }
    }
}
