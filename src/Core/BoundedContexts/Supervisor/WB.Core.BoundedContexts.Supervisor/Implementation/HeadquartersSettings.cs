namespace WB.Core.BoundedContexts.Supervisor.Implementation
{
    internal class HeadquartersSettings
    {
        public string Url { get; private set; }

        public HeadquartersSettings(string url)
        {
            this.Url = url;
        }
    }
}