using System;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Supervisor.Activities;

namespace WB.UI.Supervisor.Services.Implementation
{
    internal class TabletDiagnosticService : EnumeratorTabletDiagnosticService
    {
        public TabletDiagnosticService(
            IFileSystemAccessor fileSystemAccessor,
            IPermissionsService permissions,
            ISynchronizationService synchronizationService,
            IDeviceSettings deviceSettings,
            IArchivePatcherService archivePatcherService,
            ILogger logger,
            IViewModelNavigationService navigationService,
            IPathUtils pathUtils
            )
            : base(
                fileSystemAccessor,
                permissions,
                synchronizationService,
                deviceSettings,
                archivePatcherService,
                logger,
                navigationService,
                pathUtils)
        {
        }

        protected override Type SplashActivityType => typeof(SplashActivity);
    }
}
