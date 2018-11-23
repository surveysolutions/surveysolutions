namespace support
{
    public interface IConfigurationManagerSettings
    {
        void SetPhysicalPathToWebsite(string path);
        bool IsInitialized { get; }
    }
}
