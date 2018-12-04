using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class ExportSettings : IExportSettings
    {
        private static ExportEncryptionSettings settingCache = null;

        private readonly IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly InterviewDataExportSettings exportSettings;

        public ExportSettings(IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            InterviewDataExportSettings exportSettings)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.exportServiceSettings = exportServiceSettings;
            this.exportSettings = exportSettings;
        }

        public bool EncryptionEnforced()
        {
            if (settingCache == null)
                settingCache = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);

            return settingCache != null && settingCache.IsEnabled;
        }

        public string GetPassword()
        {
            if (settingCache == null)
                settingCache = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);

            return settingCache != null ? settingCache.Value : string.Empty;
        }

        public void SetEncryptionEnforcement(bool enabled)
        {
            var setting = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);
            var password = setting != null ? setting.Value : this.GeneratePassword();

            var newSetting = new ExportEncryptionSettings(enabled, password);
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncriptionSettingId);

            settingCache = newSetting;
        }

        public void RegeneratePassword()
        {
            var setting = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);
            if (setting != null && setting.IsEnabled)
            {
                var newSetting = new ExportEncryptionSettings(setting.IsEnabled, GeneratePassword());
                this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncriptionSettingId);

                settingCache = newSetting;
            }
        }

        public string ExportServiceBaseUrl
        {
            get { return exportSettings.ExportServiceUrl; }
        }

        public string ApiKey =>
            this.exportServiceSettings.GetById(ExportEncryptionSettings.ExportServiceStorageKey).Key;

        private string GeneratePassword()
        {
            return System.Web.Security.Membership.GeneratePassword(12, 4);
        }
    }
}
