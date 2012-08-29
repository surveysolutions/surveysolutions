using Synchronization.Core.Interface;

namespace Browsing.Supervisor.ClientSettings
{
    public class ClientSettingsProvider : ISettingsProvider
    {
        #region Properties

        private ISettings clientSettings;

        public ISettings Settings
        {
            get {
                return this.clientSettings ??
                       (this.clientSettings =
                        new ClientSettings(Properties.Settings.Default.ClientId, Properties.Settings.Default.ParentId));
            }
        }

        #endregion

        #region Constructor

        public ClientSettingsProvider()
        {
        }

        public ClientSettingsProvider(ISettings clientSettings)
        {
            this.clientSettings = clientSettings;
        }

        #endregion
    }
}
