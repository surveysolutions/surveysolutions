namespace Synchronization.Core.ClientSettings
{
    public interface IClientSettingsProvider
    {
        Synchronization.Core.ClientSettings.ClientSettings ClientSettings { get; }
    }

}
