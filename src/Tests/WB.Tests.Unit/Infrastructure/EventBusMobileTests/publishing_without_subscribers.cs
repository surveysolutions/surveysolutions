using Machine.Specifications;
using WB.Core.Infrastructure.EventBus;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class publishing_without_subscribers : EventBusTestsContext
    {
        Establish context = () =>
        {
            value = 10;

            eventBus = CreateEventBus();
        };

        Because of = () =>
            eventBus.Publish(value);

        It should_nothing_happen_including_exceptions = () =>
            value = 10;


        private static ILiteEventBus eventBus;
        private static int value;
    }
}