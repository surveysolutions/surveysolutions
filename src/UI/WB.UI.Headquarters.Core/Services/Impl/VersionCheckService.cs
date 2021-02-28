using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Models.VersionCheck;

namespace WB.UI.Headquarters.Services.Impl
{
    public class VersionCheckService : IVersionCheckService
    {
        private static VersionCheckingInfo AvailableVersion = null;
        private static DateTime? LastLoadedAt = null;
        private static readonly int SecondsForCacheIsValid = 60 * 60;

        private static DateTime? ErrorOccuredAt = null;
        private static readonly int DelayOnErrorInSeconds = 3 * 60;

        private readonly IPlainKeyValueStorage<VersionCheckingInfo> appSettingsStorage;

        private readonly IProductVersion productVersion;
        private readonly IOptions<VersionCheckConfig> versionCheckConfig;
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly IRestServiceSettings serviceSettings;

        public VersionCheckService(IPlainKeyValueStorage<VersionCheckingInfo> appSettingsStorage,
            IProductVersion productVersion,
            IOptions<VersionCheckConfig> versionCheckConfig,
            IInScopeExecutor inScopeExecutor,
            IRestServiceSettings serviceSettings)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.productVersion = productVersion;
            this.versionCheckConfig = versionCheckConfig;
            this.inScopeExecutor = inScopeExecutor;
            this.serviceSettings = serviceSettings;
        }

        public bool DoesNewVersionExist()
        {
            if (!versionCheckConfig.Value.NewVersionCheckEnabled)
            {
                return false;
            }

            this.SetVersion();

            return AvailableVersion?.Build > productVersion.GetBuildNumber();
        }

        public string GetNewVersionString()
        {
            return AvailableVersion?.VersionString;
        }

        private void SetVersion()
        {
            if (versionCheckConfig.Value.NewVersionCheckEnabled && !string.IsNullOrEmpty(versionCheckConfig.Value.NewVersionCheckUrl))
            {
                if (AvailableVersion == null)
                {
                    AvailableVersion = this.appSettingsStorage.GetById(VersionCheckingInfo.VersionCheckingInfoKey);
                }

                var isCacheExpired = (DateTime.Now - LastLoadedAt)?.Seconds > SecondsForCacheIsValid;

                if (LastLoadedAt == null || isCacheExpired)
                {
                    var isErrorDelayExpired = ErrorOccuredAt != null && (DateTime.Now - ErrorOccuredAt)?.Seconds > DelayOnErrorInSeconds;

                    if ((ErrorOccuredAt == null) || isErrorDelayExpired)
                    {
                        Task.Run(this.UpdateVersionAsync);
                    }
                }
            }
        }

        private async Task UpdateVersionAsync()
        {
            try
            {
                var versionInfo =
                    await DoRequestJsonAsync<VersionCheckingInfo>(versionCheckConfig.Value.NewVersionCheckUrl);

                AvailableVersion = versionInfo;
                LastLoadedAt = DateTime.Now;
                ErrorOccuredAt = null;
                inScopeExecutor.Execute((serviceLocatorLocal) =>
                {
                    serviceLocatorLocal.GetInstance<IPlainKeyValueStorage<VersionCheckingInfo>>()
                        .Store(versionInfo, VersionCheckingInfo.VersionCheckingInfoKey);
                });
            }
            catch (Exception)
            {
                ErrorOccuredAt = DateTime.Now;
            }
        }

        private async Task<T> DoRequestJsonAsync<T>(string uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", serviceSettings.UserAgent);

                var jsonData = await httpClient.GetStringAsync(uri);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
    }
}
