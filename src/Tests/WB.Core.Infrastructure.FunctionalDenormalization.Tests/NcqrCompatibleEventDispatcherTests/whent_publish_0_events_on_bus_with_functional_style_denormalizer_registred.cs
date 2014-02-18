using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_0_events_on_bus_with_functional_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            var ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();
            functionalStyleEventHandlerMock = ncqrsStyleDenormalizerMock.As<IFunctionalEventHandler>();

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher();
            ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);

            eventSourceId = Guid.NewGuid();
            eventsToPublish = new IPublishableEvent[0];
        };

        Because of = () => ncqrCompatibleEventDispatcher.Publish(eventsToPublish);

        private It should_functional_denormalizer_method_handle_be_called_once_with_whole_published_stream = () =>
            functionalStyleEventHandlerMock.Verify(x => x.Handle(eventsToPublish, eventSourceId), Times.Never());

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IFunctionalEventHandler> functionalStyleEventHandlerMock;
        private static IEnumerable<IPublishableEvent> eventsToPublish;
        private static Guid eventSourceId;
    }
}
