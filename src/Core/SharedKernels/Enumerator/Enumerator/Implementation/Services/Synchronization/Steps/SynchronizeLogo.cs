using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class SynchronizeLogo : SynchronizationStep
    {
        private readonly ICompanyLogoSynchronizer logoSynchronizer;

        public SynchronizeLogo(ICompanyLogoSynchronizer logoSynchronizer, 
            ISynchronizationService synchronizationService,
            ILogger logger,
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.logoSynchronizer = logoSynchronizer;
        }

        public override Task ExecuteAsync()
        {
            return this.logoSynchronizer.DownloadCompanyLogo(Context.Progress, Context.CancellationToken);
        }
    }
}
