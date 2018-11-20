using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_10_events_on_bus_with_functional_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();
            functionalStyleEventHandlerMock = ncqrsStyleDenormalizerMock.As<IFunctionalEventHandler>();

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance(Moq.It.IsAny<Type>())).Returns(functionalStyleEventHandlerMock.Object);

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(serviceLocator:serviceLocator.Object);
            ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);

            eventSourceId = Guid.NewGuid();
            eventsToPublish = CreatePublishableEvents(10, eventSourceId).ToArray();
            BecauseOf();
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Publish(eventsToPublish);

        [NUnit.Framework.Test]
        public void should_functional_denormalizer_method_handle_be_called_once_with_whole_published_stream() =>
            functionalStyleEventHandlerMock.Verify(
                handler => handler.Handle(Moq.It.Is<IEnumerable<IPublishableEvent>>(events => events.SequenceEqual(eventsToPublish)), eventSourceId),
                Times.Once());

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IFunctionalEventHandler> functionalStyleEventHandlerMock;
        private static IPublishableEvent[] eventsToPublish;
        private static Guid eventSourceId;
    }
}
