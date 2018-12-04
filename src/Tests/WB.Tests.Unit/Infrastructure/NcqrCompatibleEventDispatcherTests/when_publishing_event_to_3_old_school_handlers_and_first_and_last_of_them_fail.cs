using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_3_old_school_handlers_and_first_and_last_of_them_fail : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            publishableEvent = Create.Fake.PublishableEvent();
            var secondEventHandlerMock = new Mock<IEventHandler>();
            secondOldSchoolEventHandlerMock = secondEventHandlerMock.As<IEventHandler<IEvent>>();

            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher();
            
            eventDispatcher.Register(Setup.FailingOldSchoolEventHandlerHavingUniqueType<int>());
            eventDispatcher.Register(secondEventHandlerMock.Object);
            eventDispatcher.Register(Setup.FailingOldSchoolEventHandlerHavingUniqueType<bool>());
            BecauseOf();
        }

        public void BecauseOf() =>
            aggregateException = Assert.Throws<AggregateException>(() =>
                eventDispatcher.Publish(publishableEvent.ToEnumerable().ToArray()));

        [NUnit.Framework.Test] public void should_throw_AggregateException () =>
            aggregateException.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_put_2_exceptions_to_AggregateException () =>
            aggregateException.InnerExceptions.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_call_second_event_handler () =>
            secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
                Moq.It.IsAny<IPublishedEvent<IEvent>>()),
                Times.Once());

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static Mock<IEventHandler<IEvent>> secondOldSchoolEventHandlerMock;
    }
}
