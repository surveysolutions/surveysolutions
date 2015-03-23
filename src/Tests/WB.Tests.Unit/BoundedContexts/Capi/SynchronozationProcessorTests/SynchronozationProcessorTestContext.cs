using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Implementation.Synchronization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.BoundedContexts.Capi.SynchronozationProcessorTests
{
    public class SynchronozationProcessorTestContext
    {
        protected SynchronozationProcessor CreateSynchronozationProcessor(
            IDeviceChangingVerifier deviceChangingVerifier = null,
            ISyncAuthenticator authentificator = null,
            ICapiDataSynchronizationService dataProcessor = null,
            ICapiCleanUpService cleanUpExecutor = null,
            IInterviewSynchronizationFileStorage fileSyncRepository = null,
            ISyncPackageIdsStorage packageIdStorage = null,
            ILogger logger = null,
            ISynchronizationService synchronizationService = null,
            IInterviewerSettings interviewerSettings = null)
        {
            return new SynchronozationProcessor(
                deviceChangingVerifier ?? Mock.Of<IDeviceChangingVerifier>(),
                authentificator ?? Mock.Of<ISyncAuthenticator>(),
                dataProcessor ?? Mock.Of<ICapiDataSynchronizationService>(),
                cleanUpExecutor ?? Mock.Of<ICapiCleanUpService>(),
                fileSyncRepository ?? Mock.Of<IInterviewSynchronizationFileStorage>(),
                packageIdStorage ?? Mock.Of<ISyncPackageIdsStorage>(),
                logger ?? Mock.Of<ILogger>(),
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                interviewerSettings ?? Mock.Of<IInterviewerSettings>());
        }
    }
}
