namespace WB.Core.BoundedContexts.Headquarters.Implementation
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