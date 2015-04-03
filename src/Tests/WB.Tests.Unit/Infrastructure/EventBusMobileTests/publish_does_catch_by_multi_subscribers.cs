using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class publish_does_catch_by_multi_subscribers : EventBusTestsContext
    {
        Establish context = () =>
        {
            @event = CreatePublishableEvent();

            var eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            sub1Mock = Mock.Of<IEventBusEventHandler<IPublishableEvent>>();
            eventRegistry.Subscribe(sub1Mock);

            sub2Mock = Mock.Of<IEventBusEventHandler<IPublishableEvent>>();
            eventRegistry.Subscribe(sub2Mock);
        };

        Because of = () =>
            eventBus.Publish(@event);

        It should_sub1Mock_call_Handle_once = () =>
            Mock.Get(sub1Mock).Verify(s => s.Handle(@event), Times.Once());

        It should_sub2Mock_call_Handle_once = () =>
            Mock.Get(sub2Mock).Verify(s => s.Handle(@event), Times.Once());


        private static ILiteEventBus eventBus;
        private static IPublishableEvent @event;
        private static IEventBusEventHandler<IPublishableEvent> sub1Mock;
        private static IEventBusEventHandler<IPublishableEvent> sub2Mock;
    }
}