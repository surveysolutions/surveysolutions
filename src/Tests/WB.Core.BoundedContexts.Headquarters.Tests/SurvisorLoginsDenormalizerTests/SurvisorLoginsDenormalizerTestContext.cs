using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurvisorLoginsDenormalizerTests
{
    internal class SurvisorLoginsDenormalizerTestContext
    {
        protected static SupervisorLoginsDenormalizer CreateSurveyDetailsViewDenormalizer(
           IReadSideRepositoryWriter<SupervisorLoginView> repositoryWriter = null)
        {
            return new SupervisorLoginsDenormalizer(repositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<SupervisorLoginView>>());
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(Guid? eventSourceId = null, T @event = null) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("1234567890abcdef0101010102020304"))
                && publishedEvent.EventSequence == 1);
        }
    }
}
