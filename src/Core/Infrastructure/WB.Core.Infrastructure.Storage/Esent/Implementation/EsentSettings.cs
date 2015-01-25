namespace WB.Core.Infrastructure.Storage.Esent.Implementation
{
    internal class EsentSettings
    {
        public EsentSettings(string folder)
        {
            this.Folder = folder;
        }

        public string Folder { get; private set; }
    }
}