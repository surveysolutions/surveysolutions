namespace WB.Core.Infrastructure.ReadSide
{
    public class ReadSideSettings
    {
        public ReadSideSettings(int readSideVersion)
        {
            this.ReadSideVersion = readSideVersion;
        }

        public int ReadSideVersion { get; private set; }
    }
}