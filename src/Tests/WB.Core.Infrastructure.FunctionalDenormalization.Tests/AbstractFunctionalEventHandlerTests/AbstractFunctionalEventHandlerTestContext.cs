using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    [Subject(typeof(AbstractFunctionalEventHandler<>))]
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
