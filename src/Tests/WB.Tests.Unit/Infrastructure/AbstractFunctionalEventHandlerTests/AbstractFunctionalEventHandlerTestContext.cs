using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class AbstractFunctionalEventHandlerTestContext
    {
        protected static TestableFunctionalEventHandler CreateAbstractFunctionalEventHandler(
            IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideRepositoryWriter = null)
        {
            return new TestableFunctionalEventHandler(readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>());
        }

        protected static IReadSideRepositoryEntity CreateReadSideRepositoryEntity()
        {
            return Mock.Of<IReadSideRepositoryEntity>();
        }

        protected static IPublishableEvent CreatePublishableEvent(object payload=null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? new object()));
        }
    }
}
