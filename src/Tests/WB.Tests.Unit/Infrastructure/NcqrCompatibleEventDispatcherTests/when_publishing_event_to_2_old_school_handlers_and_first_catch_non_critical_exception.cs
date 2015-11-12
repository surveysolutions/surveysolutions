using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_2_old_school_handlers_and_first_catch_non_critical_exception : NcqrCompatibleEventDispatcherTestContext
    {
        private class FirstEventHandler : IEventHandler, IEventHandler<object>
        {
            public void Handle(IPublishedEvent<object> evnt)
            {
                throw new NotImplementedException();
            }

            public string Name => "First Event Handler";
            public object[] Readers { get; }
            public object[] Writers { get; }
        }

        Establish context = () =>
        {
            publishableEvent = Create.PublishableEvent();

            eventDispatcher = Create.NcqrCompatibleEventDispatcher(new EventBusSettings()
            {
                CatchExceptionsByEventHandlerTypes = new[]
                {
                    typeof (FirstEventHandler)
                },
                IgnoredEventHandlerTypes = new Type[0]
            }, loggerMock.Object);

            var firstEventHandler = new FirstEventHandler();

            var uniqueEventHandlerMock = new Mock<IEnumerable<bool>>();
            var eventHandlerMock = uniqueEventHandlerMock.As<IEventHandler>();
            secondOldSchoolEventHandlerMock = eventHandlerMock.As<IEventHandler<object>>();
            secondOldSchoolEventHandlerMock
                .Setup(_ => _.Handle(Moq.It.IsAny<IPublishedEvent<object>>()))
                .Throws<Exception>();;

            eventDispatcher.Register(firstEventHandler);
            eventDispatcher.Register(eventHandlerMock.Object);
        };

        Because of = () =>
            aggregateException = Catch.Only<AggregateException>(() =>
                eventDispatcher.Publish(publishableEvent, onCatchingNonCriticalEventHandlerExceptionActionMock.Object));

        It should_throw_AggregateException = () =>
            aggregateException.ShouldNotBeNull();

        It should_put_1_exception_to_AggregateException = () =>
            aggregateException.InnerExceptions.Count.ShouldEqual(1);

        It should_call_2_event_handlers = () =>
            secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
                Moq.It.IsAny<IPublishedEvent<object>>()),
                Times.Once);

        It should_log_catched_exception = () =>
            loggerMock.Verify(x => x.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Times.Once);

        It should_invoked_action_onCatchingNonCriticalEventHandlerException = () =>
            onCatchingNonCriticalEventHandlerExceptionActionMock.Verify(x => x(Moq.It.IsAny<EventHandlerException>()), Times.Once);

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static Mock<IEventHandler<object>> secondOldSchoolEventHandlerMock;
        private static readonly Mock<Action<EventHandlerException>> onCatchingNonCriticalEventHandlerExceptionActionMock = new Mock<Action<EventHandlerException>>();
        private static readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
    }
}