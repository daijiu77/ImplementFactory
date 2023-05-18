namespace System.DJ.ImplementFactory.MServiceRoute.Attrs
{
    public class MSDataSyncReceiverAttribute : Attribute
    {
        private string dataSyncsName = "";
        public MSDataSyncReceiverAttribute()
        {
            //
        }

        public MSDataSyncReceiverAttribute(string dataSyncsName)
        {
            this.dataSyncsName = dataSyncsName;
        }

        public string DataSyncsName { get { return dataSyncsName; } }
    }
}
