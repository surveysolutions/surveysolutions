using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services.Synchronization
{
    public class CompanyLogoSynchronizer
    {
        private readonly IPlainStorage<CompanyLogo> logoStorage;
        private readonly ISynchronizationService synchronizationService;

        public CompanyLogoSynchronizer(IPlainStorage<CompanyLogo> logoStorage, 
            ISynchronizationService synchronizationService)
        {
            this.logoStorage = logoStorage;
            this.synchronizationService = synchronizationService;
        }

        public virtual async Task DownloadCompanyLogo(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_DownloadingLogo
            });

            var logoStorageId = CompanyLogo.StorageKey;
            var existingLogo = this.logoStorage.GetById(logoStorageId);
            CompanyLogoInfo remoteLogoInfo = await this.synchronizationService.GetCompanyLogo(existingLogo?.ETag, cancellationToken);

            if (!remoteLogoInfo.HasCustomLogo)
            {
                this.logoStorage.Remove(logoStorageId);
            }
            else
            {
                if (remoteLogoInfo.LogoNeedsToBeUpdated)
                {
                    this.logoStorage.Store(new CompanyLogo
                    {
                        Id = logoStorageId,
                        ETag = remoteLogoInfo.Etag,
                        File = remoteLogoInfo.Logo
                    });
                }
            }
        }
    }
}