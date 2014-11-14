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
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    [Subject(typeof(AbstractFunctionalEventHandler<>))]
    internal class AbstractFunctionalEventHandlerTestContext
    {
        protected static TestableFunctionalEventHandler<T> CreateAbstractFunctionalEventHandler<T>(
            IReadSideRepositoryWriter<T> readSideRepositoryWriter = null) where T : class, IReadSideRepositoryEntity
        {
            return new TestableFunctionalEventHandler<T>(readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<T>>());
        }

        protected static IReadSideRepositoryEntity CreateReadSideRepositoryEntity()
        {
            return Mock.Of<IReadSideRepositoryEntity>();
        }

        protected static IPublishableEvent CreatePublishableEvent()
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == new object());
        }

        protected static IEnumerable<IPublishableEvent> CreatePublishableEvents(int countOfEvents)
        {
            for (int i = 0; i < countOfEvents; i++)
            {
                yield return CreatePublishableEvent();
            }
        }
    }
}
