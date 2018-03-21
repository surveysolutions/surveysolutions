using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_by_ignored_event_source : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var secondEventHandlerMock = new Mock<IEventHandler>();
            secondOldSchoolEventHandlerMock = secondEventHandlerMock.As<IEventHandler<IEvent>>();
            ncqrCompatibleEventDispatcher =
                CreateNcqrCompatibleEventDispatcher(new EventBusSettings()
                {
                    IgnoredAggregateRoots = new HashSet<string>(new[] {ignoredEvenetSource.FormatGuid()})
                });

            ncqrCompatibleEventDispatcher.Register(secondEventHandlerMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => ncqrCompatibleEventDispatcher.Publish(new[] { Create.Fake.PublishableEvent(eventSourceId:ignoredEvenetSource) });

        [NUnit.Framework.Test] public void should_not_call_registred_event_handler () =>
         secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
             Moq.It.IsAny<IPublishedEvent<IEvent>>()),
             Times.Never);

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Guid ignoredEvenetSource=Guid.NewGuid();
        private static Mock<IEventHandler<IEvent>> secondOldSchoolEventHandlerMock;
    }
}
