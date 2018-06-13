using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_0_events_on_bus_with_functional_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();
            functionalStyleEventHandlerMock = ncqrsStyleDenormalizerMock.As<IFunctionalEventHandler>();

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher();
            ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);

            eventSourceId = Guid.NewGuid();
            eventsToPublish = new IPublishableEvent[0];
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Publish(eventsToPublish);

        [NUnit.Framework.Test] public void should_functional_denormalizer_method_handle_be_called_once_with_whole_published_stream () =>
            functionalStyleEventHandlerMock.Verify(x => x.Handle(eventsToPublish, eventSourceId), Times.Never());

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IFunctionalEventHandler> functionalStyleEventHandlerMock;
        private static IEnumerable<IPublishableEvent> eventsToPublish;
        private static Guid eventSourceId;
    }
}
