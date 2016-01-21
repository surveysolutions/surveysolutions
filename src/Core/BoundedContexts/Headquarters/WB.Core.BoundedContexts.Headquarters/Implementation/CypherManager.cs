using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Security;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class CypherManager : ICypherManager
    {
        private Setting settingCache = null;

        private readonly IPlainKeyValueStorage<Setting> settingsStorage;

        public CypherManager(IPlainKeyValueStorage<Setting> settingsStorage)
        {
            this.settingsStorage = settingsStorage;
        }

        public bool EncryptionEnforced()
        {
            if (this.settingCache == null)
                this.settingCache = this.settingsStorage.GetById(Setting.EncriptionSettingId);
            
            return this.settingCache != null && this.settingCache.IsEnabled;
        }

        public string GetPassword()
        {
            if (this.settingCache == null)
                this.settingCache = this.settingsStorage.GetById(Setting.EncriptionSettingId);

            return this.settingCache != null ? this.settingCache.Value : string.Empty;
        }

        public void SetEncryptionEnforcement(bool enabled)
        {
            var setting = this.settingsStorage.GetById(Setting.EncriptionSettingId);
            var password = setting != null ? setting.Value : this.GeneratePassword();

            var newSetting = new Setting(enabled, password);
            this.settingsStorage.Store(newSetting, Setting.EncriptionSettingId);

            this.settingCache = newSetting;
        }

        public void RegeneratePassword()
        {
            var setting = this.settingsStorage.GetById(Setting.EncriptionSettingId);
            if (setting != null && setting.IsEnabled)
            {
                var newSetting = new Setting(setting.IsEnabled, GeneratePassword());
                this.settingsStorage.Store(newSetting, Setting.EncriptionSettingId);

                this.settingCache = newSetting;
            }
        }

        private string GeneratePassword()
        {
            return System.Web.Security.Membership.GeneratePassword(12, 4);
        }
    }
}
