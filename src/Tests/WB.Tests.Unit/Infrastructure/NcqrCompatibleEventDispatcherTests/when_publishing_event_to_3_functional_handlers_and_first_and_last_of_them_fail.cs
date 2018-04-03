using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_3_functional_handlers_and_first_and_last_of_them_fail : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            publishableEvent = Create.Fake.PublishableEvent();

            var secondEventHandlerMock = new Mock<IEventHandler>();
            secondFunctionalEventHandlerMock = secondEventHandlerMock.As<IFunctionalEventHandler>();

            eventDispatcher = Create.Service.NcqrCompatibleEventDispatcher();
            eventDispatcher.Register(Setup.FailingFunctionalEventHandlerHavingUniqueType<int>());
            eventDispatcher.Register(secondEventHandlerMock.Object);
            eventDispatcher.Register(Setup.FailingFunctionalEventHandlerHavingUniqueType<bool>());
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
            secondFunctionalEventHandlerMock.Verify(
                handler => handler.Handle(Moq.It.Is<IEnumerable<IPublishableEvent>>(events => events.Single() == publishableEvent), Moq.It.IsAny<Guid>()),
                Times.Once());

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static Mock<IFunctionalEventHandler> secondFunctionalEventHandlerMock;
    }
}
