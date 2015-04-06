using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class handlers_does_not_catch_other_event_types : EventBusTestsContext
    {
        Establish context = () =>
        {
            @event = 10;

            var eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            sub1Mock = Mock.Of<IEventBusEventHandler<int>>();
            eventRegistry.Subscribe(sub1Mock);

            sub2Mock = Mock.Of<IEventBusEventHandler<long>>();
            eventRegistry.Subscribe(sub2Mock);
        };

        Because of = () =>
            eventBus.Publish(@event);

        It should_sub1Mock_doesnot_call_Handle = () =>
            Mock.Get(sub1Mock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_sub2Mock_call_Handle_once = () =>
            Mock.Get(sub2Mock).Verify(s => s.Handle(@event), Times.Once());


        private static ILiteEventBus eventBus;
        private static long @event;
        private static IEventBusEventHandler<int> sub1Mock;
        private static IEventBusEventHandler<long> sub2Mock;
    }
}