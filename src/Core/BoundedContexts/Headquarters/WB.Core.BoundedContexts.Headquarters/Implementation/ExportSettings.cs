using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Security;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class ExportSettings : IExportSettings
    {
        private ExportEncryptionSettings settingCache = null;

        private readonly IPlainKeyValueStorage<ExportEncryptionSettings> settingsStorage;

        public ExportSettings(
            IPlainKeyValueStorage<ExportEncryptionSettings> settingsStorage)
        {
            this.settingsStorage = settingsStorage;
        }

        public bool EncryptionEnforced()
        {
            if (this.settingCache == null)
                this.settingCache = this.settingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);

            return this.settingCache != null && this.settingCache.IsEnabled;
        }

        public string GetPassword()
        {
            if (this.settingCache == null)
                this.settingCache = this.settingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);

            return this.settingCache != null ? this.settingCache.Value : string.Empty;
        }

        public void SetEncryptionEnforcement(bool enabled)
        {
            var setting = this.settingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);
            var password = setting != null ? setting.Value : this.GeneratePassword();

            var newSetting = new ExportEncryptionSettings(enabled, password);
            this.settingsStorage.Store(newSetting, ExportEncryptionSettings.EncriptionSettingId);

            this.settingCache = newSetting;
        }

        public void RegeneratePassword()
        {
            var setting = this.settingsStorage.GetById(ExportEncryptionSettings.EncriptionSettingId);
            if (setting != null && setting.IsEnabled)
            {
                var newSetting = new ExportEncryptionSettings(setting.IsEnabled, GeneratePassword());
                this.settingsStorage.Store(newSetting, ExportEncryptionSettings.EncriptionSettingId);

                this.settingCache = newSetting;
            }
        }

        private string GeneratePassword()
        {
            return System.Web.Security.Membership.GeneratePassword(12, 4);
        }
    }
}
