using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_registering_event_handler_from_ignore_list : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();

            ncqrCompatibleEventDispatcher =
                CreateNcqrCompatibleEventDispatcher(new EventBusSettings() { DisabledEventHandlerTypes  =  new[] { ncqrsStyleDenormalizerMock.Object.GetType() }});
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);

        [NUnit.Framework.Test] public void should_lits_of_register_event_handlers_be_empty () =>
            ncqrCompatibleEventDispatcher.GetAllRegistredEventHandlers().Should().BeEmpty();

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IEventHandler> ncqrsStyleDenormalizerMock;
    }
}
