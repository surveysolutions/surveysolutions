using Synchronization.Core.Interface;

namespace Browsing.CAPI.ClientSettings

{
    public class ClientSettingsProvider : ISettingsProvider
    {
        private ISettings clientSettings;

        public ClientSettingsProvider()
        {
        }

        public ClientSettingsProvider(ISettings clientSettings)
        {
            this.clientSettings = clientSettings;
        }

        public ISettings Settings
        {
            get
            {
                if (this.clientSettings == null)
                {
                    this.clientSettings =
                        new ClientSettings(Properties.Settings.Default.ClientId, Properties.Settings.Default.ParentId);
                }

                return this.clientSettings;
            }
        }
    }
}
