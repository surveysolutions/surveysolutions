using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class class_must_not_have_handle_events_after_unsubscribe : EventBusTestsContext
    {
        Establish context = () =>
        {
            eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            subMock = CreateClassWithEventHandlers();
            eventRegistry.Subscribe(subMock);
            eventRegistry.Unsubscribe(subMock);
        };

        Because of = () =>
            eventBus.Publish(10);

        It should_doesnot_call_Handle_for_int_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_doesnot_call_Handle_for_long_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<long>()), Times.Never);

        It should_doesnot_call_Handle_for_string_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<string>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static IEventRegistry eventRegistry;
        private static DumyEventHandlers subMock;
    }
}