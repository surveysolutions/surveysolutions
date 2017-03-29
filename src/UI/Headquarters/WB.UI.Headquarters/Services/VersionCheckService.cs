﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.VersionCheck;

namespace WB.UI.Headquarters.Services
{
    public class VersionCheckService : IVersionCheckService
    {
        private static VersionCheckingInfo AvailableVersion = null;
        private static DateTime? LastLoadedAt = null;
        private static int SecondsForCacheIsValid = 60 * 60;

        private static DateTime? ErrorOccuredAt = null;
        private static int DelayOnErrorInSeconds = 3 * 60;

        private IPlainKeyValueStorage<VersionCheckingInfo> versionCheckInfoStorage;
        private IPlainTransactionManager plainTransactionManager;
        private IProductVersion productVersion;

        public VersionCheckService(IPlainKeyValueStorage<VersionCheckingInfo> versionCheckInfoStorage,
            IPlainTransactionManager plainTransactionManager,
            IProductVersion productVersion)
        {
            this.plainTransactionManager = plainTransactionManager;
            this.versionCheckInfoStorage = versionCheckInfoStorage;
            this.productVersion = productVersion;
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
                    AvailableVersion = this.versionCheckInfoStorage.GetById(VersionCheckingInfo.StorageKey);
                }

                var isCacheExpired = (DateTime.Now - LastLoadedAt)?.Seconds > SecondsForCacheIsValid;

                if(LastLoadedAt == null || isCacheExpired)
                {
                    var isErrorDelayExpired = ErrorOccuredAt != null && (DateTime.Now - ErrorOccuredAt)?.Seconds > DelayOnErrorInSeconds;

                    if ((ErrorOccuredAt == null) || isErrorDelayExpired)
                    {
                        Task.Run(async () =>
                        {
                            await this.UpdateVersion();
                        });
                    }
                }
            }
        }

        private async Task UpdateVersion()
        {
            try
            {
                var versionInfo = await DoRequestJsonAsync<VersionCheckingInfo>(ApplicationSettings.NewVersionCheckUrl);

                AvailableVersion = versionInfo;
                LastLoadedAt = DateTime.Now;
                ErrorOccuredAt = null;

                try
                {
                    this.plainTransactionManager.BeginTransaction();
                    this.versionCheckInfoStorage.Store(versionInfo, VersionCheckingInfo.StorageKey);
                    this.plainTransactionManager.CommitTransaction();
                }
                catch
                {
                    this.plainTransactionManager.RollbackTransaction();
                }

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