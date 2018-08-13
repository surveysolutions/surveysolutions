using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public interface ICompanyLogoSynchronizer
    {
        Task DownloadCompanyLogo(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken);
    }

    public class CompanyLogoSynchronizer : ICompanyLogoSynchronizer
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

            if (remoteLogoInfo == null || !remoteLogoInfo.HasCustomLogo)
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
