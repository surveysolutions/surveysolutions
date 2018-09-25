using System;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.UI.Interviewer.Activities;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    internal class TabletDiagnosticService : EnumeratorTabletDiagnosticService
    {
        public TabletDiagnosticService(IFileSystemAccessor fileSystemAccessor, IPermissions permissions,
            ISynchronizationService synchronizationService, IDeviceSettings deviceSettings,
            IArchivePatcherService archivePatcherService, ILogger logger, IViewModelNavigationService navigationService) : base(fileSystemAccessor, permissions,
            synchronizationService, deviceSettings, archivePatcherService, logger, navigationService)
        {
        }

        protected override Type SplashActivityType => typeof(SplashActivity);
    }
}
