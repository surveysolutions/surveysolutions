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
        private readonly IMemoryCache settingCache;
        private readonly IExportServiceApi exportServiceApi;

        public ExportSettings(
            IPlainKeyValueStorage<ExportEncryptionSettings> appSettingsStorage,
            IMemoryCache memoryCache,
            IExportServiceApi exportServiceApi)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.settingCache = memoryCache;
            this.exportServiceApi = exportServiceApi;
        }

        public bool EncryptionEnforced()
        {
            ExportEncryptionSettings? setting = GetSetting();
            
            return setting?.IsEnabled == true;
        }

        private ExportEncryptionSettings? GetSetting()
        {
            return this.settingCache.GetOrCreate("ExportEncryptionSettings", cache =>
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

            settingCache.Remove("ExportEncryptionSettings");
        }

        public void RegeneratePassword()
        {
            var setting = GetSetting();
            if (setting == null || !setting.IsEnabled) return;

            var newSetting = new ExportEncryptionSettings(setting.IsEnabled, GeneratePassword());
            this.appSettingsStorage.Store(newSetting, ExportEncryptionSettings.EncryptionSettingId);

            settingCache.Remove("ExportEncryptionSettings");
        }

        private string GeneratePassword()
        {
            var pwd = new Password(true, true, true, true, 12);
            var result = pwd.Next();
            return result;
        }
        
        public async Task RemoveExportCache()
        {
            await exportServiceApi.DeleteTenant();
        }
    }
}
