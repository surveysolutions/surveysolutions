using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.VersionCheck;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Headquarters.Services
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
        private IServiceLocator serviceLocator;

        public VersionCheckService(IPlainKeyValueStorage<VersionCheckingInfo> appSettingsStorage,
            IProductVersion productVersion,
            IServiceLocator serviceLocator)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.productVersion = productVersion;
            this.serviceLocator = serviceLocator;
        }

        public bool DoesNewVersionExist()
        {
            if (!ApplicationSettings.NewVersionCheckEnabled)
            {
                return false;
            }

            this.SetVersion();

            return AvailableVersion?.Build > productVersion.GetBildNumber();
        }

        public string GetNewVersionString()
        {
            return AvailableVersion?.VersionString;
        }

        private void SetVersion()
        {
            if (ApplicationSettings.NewVersionCheckEnabled && !string.IsNullOrEmpty(ApplicationSettings.NewVersionCheckUrl))
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
                    await DoRequestJsonAsync<VersionCheckingInfo>(ApplicationSettings.NewVersionCheckUrl);

                AvailableVersion = versionInfo;
                LastLoadedAt = DateTime.Now;
                ErrorOccuredAt = null;
                this.serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
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

                var jsonData = await httpClient.GetStringAsync(uri);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
    }
}
