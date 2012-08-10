using Client.Properties;
using Synchronization.Core.ClientSettings;

namespace Client
{
    public class ClientSettingsProvider : IClientSettingsProvider
    {
        private Synchronization.Core.ClientSettings.ClientSettings clientSettings;

        public ClientSettingsProvider()
        {
        }

        public ClientSettingsProvider(Synchronization.Core.ClientSettings.ClientSettings clientSettings)
        {
            this.clientSettings = clientSettings;
        }

        public Synchronization.Core.ClientSettings.ClientSettings ClientSettings
        {
            get
            {
                if (clientSettings == null)
                {
                    clientSettings =
                        new Synchronization.Core.ClientSettings.ClientSettings(Settings.Default.ClientId, Settings.Default.ParentId);
                }
                return clientSettings;
            }
        }
    }
}
