namespace WB.Core.BoundedContexts.Headquarters
{
    public class SyncSettings
    {
        public SyncSettings()
        {
        }

        public SyncSettings(string origin)
        {
            this.Origin = origin;
        }

        public SyncSettings(string origin, bool useBackgroundJobForProcessingPackages)
        {
            this.Origin = origin;
            this.UseBackgroundJobForProcessingPackages = useBackgroundJobForProcessingPackages;
        }

        public string Origin { get; private set; }

        public bool UseBackgroundJobForProcessingPackages { get; set; }
    }
}
