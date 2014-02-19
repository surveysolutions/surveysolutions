using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.NcqrCompatibleEventDispatcherTests
{
    internal class whent_publish_10_events_on_bus_with_ncqrs_style_denormalizer_registred : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            var ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();
            ncqrsStyleEventHandlerMock = ncqrsStyleDenormalizerMock.As<IEventHandler<object>>();

            ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher();
            ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);
        };

        Because of = () => ncqrCompatibleEventDispatcher.Publish(CreatePublishableEvents(10));

        It should_regitred_denormalizer_method_handle_be_called_10_times = () =>
            ncqrsStyleEventHandlerMock.Verify(x => x.Handle(Moq.It.IsAny<IPublishedEvent<object>>()), Times.Exactly(10));

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IEventHandler<object>> ncqrsStyleEventHandlerMock;
    }
}
