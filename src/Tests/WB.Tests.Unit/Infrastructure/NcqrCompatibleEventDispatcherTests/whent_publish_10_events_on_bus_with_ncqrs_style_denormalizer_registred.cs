using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_10_events_on_bus_with_ncqrs_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            var ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();
            ncqrsStyleEventHandlerMock = ncqrsStyleDenormalizerMock.As<IEventHandler<IEvent>>();

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher();
            ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);
        };

        Because of = () => ncqrCompatibleEventDispatcher.Publish(CreatePublishableEvents(10));

        It should_regitred_denormalizer_method_handle_be_called_10_times = () =>
            ncqrsStyleEventHandlerMock.Verify(x => x.Handle(Moq.It.IsAny<IPublishedEvent<IEvent>>()), Times.Exactly(10));

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IEventHandler<IEvent>> ncqrsStyleEventHandlerMock;
    }
}
