using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class class_must_donot_handle_events_after_unsubscribe : EventBusTestsContext
    {
        Establish context = () =>
        {
            value = 10;
            
            eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            subMock = CreateClassWithEventHandlers();
            eventRegistry.Subscribe(subMock);
            eventRegistry.Unsubscribe(subMock);
        };

        Because of = () =>
            eventBus.Publish(value);

        It should_doesnot_call_Handle_for_int = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_doesnot_call_Handle_for_long = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<long>()), Times.Never);

        It should_doesnot_call_Handle_for_string = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<string>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static IEventRegistry eventRegistry;
        private static int value;
        private static DumyEventHandlers subMock;
    }
}