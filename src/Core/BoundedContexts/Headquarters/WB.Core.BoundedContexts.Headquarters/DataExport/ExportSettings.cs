#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PasswordGenerator;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class ExportSettings : IExportSettings
    {
        private readonly IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage;
        private readonly IPlainKeyValueStorage<ExportRetentionSettings> exportRetentionSettingsStorage;
        private readonly IMemoryCache settingCache;
        private readonly string exportencryptionsettings = ExportEncryptionSettings.EncryptionSettingId;

        public ExportSettings(
            IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage,
            IPlainKeyValueStorage<ExportRetentionSettings> exportRetentionSettingsStorage,
            IMemoryCache memoryCache)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.settingCache = memoryCache;
            this.exportRetentionSettingsStorage = exportRetentionSettingsStorage;
        }

        public bool EncryptionEnforced()
        {
            ExportEncryptionSettings? setting = GetSetting();
            
            return setting?.IsEnabled == true;
        }

        private ExportEncryptionSettings? GetSetting()
        {
            return this.settingCache.GetOrCreate(exportencryptionsettings, cache =>
            {
                cache.SlidingExpiration = TimeSpan.FromMinutes(5);
                return this.appSettingsStorage.GetById(ExportEncryptionSettings.EncryptionSettingId);
            });
        }

        public string GetPassword()
        {
            var setting = GetSetting();
            
            return setting != null ? setting.Value : string.Empty;
        }

        public void SetEncryptionEnforcement(bool enabled)
        {
            var setting = GetSetting();
            var password = setting != null ? setting.Value : this.GeneratePassword();

            var newSetting = new ExportEncryptionSettings(enabled, password);
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncryptionSettingId);

            settingCache.Remove(exportencryptionsettings);
        }

        public void RegeneratePassword()
        {
            var setting = GetSetting();
            if (setting == null || !setting.IsEnabled) return;

            var newSetting = new ExportEncryptionSettings(setting.IsEnabled, GeneratePassword());
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncryptionSettingId);

            settingCache.Remove(exportencryptionsettings);
        }

        private string GeneratePassword()
        {
            var pwd = new Password(true, true, true, true, 12);
            var result = pwd.Next();
            return result;
        }

        public ExportRetentionSettings? GetExportRetentionSettings()
        {
            
            var setting = this.settingCache.GetOrCreate(ExportRetentionSettings.ExportRetentionSettingsKey, cache =>
            {
                cache.SlidingExpiration = TimeSpan.FromMinutes(5);
                
                return this.exportRetentionSettingsStorage.GetById(ExportRetentionSettings.ExportRetentionSettingsKey);
            });
            
            return setting;
        }
        
        public void SetExportRetentionSettings(bool enabled, int? daysToKeep, int? countToKeep)
        {
            var setting = this.exportRetentionSettingsStorage.GetById(ExportRetentionSettings.ExportRetentionSettingsKey);
            if (setting == null)
            {
                setting = new ExportRetentionSettings(enabled, daysToKeep, countToKeep);
            }
            else
            {
                setting.Enabled = enabled;
                setting.DaysToKeep = daysToKeep;
                setting.CountToKeep = countToKeep;
            }

            this.exportRetentionSettingsStorage.Store(setting, ExportRetentionSettings.ExportRetentionSettingsKey);
            
            settingCache.Remove(ExportRetentionSettings.ExportRetentionSettingsKey);
        }
    }
}
