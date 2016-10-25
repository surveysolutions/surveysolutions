using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;

namespace WB.Tests.Unit.Infrastructure.ReadSideServiceTests
{
    [Subject(typeof(ReadSideService))]
    internal class ReadSideServiceTestContext
    {
        protected static ReadSideService CreateReadSideService(IStreamableEventStore streamableEventStore = null,
            IEventDispatcher eventDispatcher = null,
            IPostgresReadSideBootstraper postgresReadSideBootstraper = null,
            ITransactionManagerProviderManager transactionManagerProviderManager = null)
        {
            ReadSideService.InstanceCount = 0;

            return new ReadSideService(
                streamableEventStore ?? Mock.Of<IStreamableEventStore>(),
                eventDispatcher ?? Mock.Of<IEventDispatcher>(), Mock.Of<ILogger>(),
                postgresReadSideBootstraper ?? Mock.Of<IPostgresReadSideBootstraper>(),
                transactionManagerProviderManager ?? Mock.Of<ITransactionManagerProviderManager>(x => x.GetTransactionManager() == Mock.Of<ITransactionManager>()),
                Create.Entity.ReadSideSettings(),
                Mock.Of<IReadSideKeyValueStorage<ReadSideVersion>>());
        }

        protected static void WaitRebuildReadsideFinish(ReadSideService readSideService)
        {
            Thread.Sleep(1500);

            while (readSideService.AreViewsBeingRebuiltNow())
            {
                Thread.Sleep(100);
            }
        }
    }
}
