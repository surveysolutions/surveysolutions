using Microsoft.Extensions.Options;
using PasswordGenerator;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class ExportSettings : IExportSettings
    {
        private static ExportEncryptionSettings settingCache = null;

        private readonly IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly IOptions<ExportServiceConfig> exportOptions;

        public ExportSettings(
            IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IOptions<ExportServiceConfig> exportOptions)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.exportServiceSettings = exportServiceSettings;
            this.exportOptions = exportOptions;
        }

        public bool EncryptionEnforced()
        {
            if (settingCache == null)
                settingCache = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncryptionSettingId);

            return settingCache != null && settingCache.IsEnabled;
        }

        public string GetPassword()
        {
            if (settingCache == null)
                settingCache = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncryptionSettingId);

            return settingCache != null ? settingCache.Value : string.Empty;
        }

        public void SetEncryptionEnforcement(bool enabled)
        {
            var setting = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncryptionSettingId);
            var password = setting != null ? setting.Value : this.GeneratePassword();

            var newSetting = new ExportEncryptionSettings(enabled, password);
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncryptionSettingId);

            settingCache = newSetting;
        }

        public void RegeneratePassword()
        {
            var setting = this.appSettingsStorage.GetById(ExportEncryptionSettings.EncryptionSettingId);
            if (setting == null || !setting.IsEnabled) return;

            var newSetting = new ExportEncryptionSettings(setting.IsEnabled, GeneratePassword());
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncryptionSettingId);

            settingCache = newSetting;
        }

        public string ExportServiceBaseUrl => exportOptions.Value.ExportServiceUrl;

        public string ApiKey => this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key;

        private string GeneratePassword()
        {
            var pwd = new Password(true, true, true, true, 12);
            var result = pwd.Next();
            return result;
        }
    }
}
