using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class AbstractFunctionalEventHandlerTestContext
    {
        protected static Guid? eventSourceId;

        protected static TestableFunctionalEventHandler CreateAbstractFunctionalEventHandler(
            IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideRepositoryWriter = null)
        {
            return new TestableFunctionalEventHandler(readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>());
        }

        protected static IReadSideRepositoryEntity CreateReadSideRepositoryEntity()
        {
            return Mock.Of<IReadSideRepositoryEntity>();
        }

        protected static IPublishableEvent CreatePublishableEvent(IEvent payload =null)
        {
            return Create.Fake.PublishableEvent(eventSourceId, payload: payload ?? new TestableFunctionalEvent()); 
        }
    }
}
