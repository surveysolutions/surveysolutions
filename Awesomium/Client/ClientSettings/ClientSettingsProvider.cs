using Client.Properties;

namespace Client.ClientSettings
{
    public class ClientSettingsProvider : IClientSettingsProvider
    {
        private ClientSettings clientSettings;

        public ClientSettingsProvider()
        {
        }

        public ClientSettingsProvider(ClientSettings clientSettings)
        {
            this.clientSettings = clientSettings;
        }

        public ClientSettings ClientSettings
        {
            get
            {
                if (clientSettings == null)
                {
                    clientSettings =
                        new ClientSettings(Settings.Default.ClientId, Settings.Default.ParentId);
                }
                return clientSettings;
            }
        }
    }
}
