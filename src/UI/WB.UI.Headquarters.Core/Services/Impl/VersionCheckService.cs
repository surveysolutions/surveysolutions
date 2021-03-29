using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.UI.Headquarters.Configs;
using WB.UI.Headquarters.Models.VersionCheck;

namespace WB.UI.Headquarters.Services.Impl
{
    public class VersionCheckService : IVersionCheckService
    {
        private static DateTime? errorOccurredAt = null;
        private static readonly int DelayOnErrorInSeconds = 3 * 60;

        private readonly IProductVersion productVersion;
        private readonly IOptions<VersionCheckConfig> versionCheckConfig;
        private readonly IRestServiceSettings serviceSettings;

        private static bool isChecking = false;
        private readonly IMemoryCache memoryCache;
        private static readonly string CachePrefix = $"AvailableVersion:";


        public VersionCheckService(
            IProductVersion productVersion,
            IOptions<VersionCheckConfig> versionCheckConfig,
            IRestServiceSettings serviceSettings,
            IMemoryCache memoryCache)
        {
            this.productVersion = productVersion;
            this.versionCheckConfig = versionCheckConfig;
            this.serviceSettings = serviceSettings;
            this.memoryCache = memoryCache;
        }

        public bool DoesNewVersionExist()
        {
            if (!versionCheckConfig.Value.NewVersionCheckEnabled || string.IsNullOrEmpty(versionCheckConfig.Value.NewVersionCheckUrl)) 
                return false;

            if (!memoryCache.TryGetValue(CachePrefix, out VersionCheckingInfo version))
            {
                var isErrorDelayExpired = errorOccurredAt != null && (DateTime.Now - errorOccurredAt)?.Seconds > DelayOnErrorInSeconds;

                if ((errorOccurredAt == null) || isErrorDelayExpired)
                {
                    if (!isChecking)
                    {
                        isChecking = true;
                        Task.Run(this.UpdateVersionAsync);
                    }
                }
            }

            return version?.Build > productVersion.GetBuildNumber();
        }

        public string GetNewVersionString()
        {
            return memoryCache.TryGetValue(CachePrefix, out VersionCheckingInfo version) ? version?.VersionString : String.Empty;
        }

        private async Task UpdateVersionAsync()
        {
            try
            {
                var versionInfo = await DoRequestJsonAsync<VersionCheckingInfo>(versionCheckConfig.Value.NewVersionCheckUrl);
                memoryCache.Set(CachePrefix, versionInfo, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
                errorOccurredAt = null;
            }
            catch (Exception)
            {
                errorOccurredAt = DateTime.Now;
            }
            finally
            {
                isChecking = false;
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
