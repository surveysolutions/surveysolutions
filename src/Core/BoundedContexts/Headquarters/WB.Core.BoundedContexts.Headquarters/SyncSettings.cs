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

        public string Origin { get; private set; }
    }
}
