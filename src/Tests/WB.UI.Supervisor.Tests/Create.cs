using Moq;
using Ncqrs.Commanding.ServiceModel;
using Quartz;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Tests
{
    internal static class Create
    {
        public static HQSyncController HQSyncController(
            ISynchronizer synchronizer = null,
            IGlobalInfoProvider globalInfoProvider = null,
            HeadquartersPushContext headquartersPushContext = null)
        {
            return new HQSyncController(
                Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                HeadquartersPullContext(),
                headquartersPushContext ?? HeadquartersPushContext(),
                Mock.Of<IScheduler>(),
                synchronizer ?? Mock.Of<ISynchronizer>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>());
        }

        public static HeadquartersPullContext HeadquartersPullContext()
        {
            return new HeadquartersPullContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>());
        }

        public static HeadquartersPushContext HeadquartersPushContext()
        {
            return new HeadquartersPushContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>());
        }
    }
}