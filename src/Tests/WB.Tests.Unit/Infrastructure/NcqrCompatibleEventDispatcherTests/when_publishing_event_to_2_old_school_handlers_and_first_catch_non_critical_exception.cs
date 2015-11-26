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
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_to_2_old_school_handlers_and_first_catch_non_critical_exception : NcqrCompatibleEventDispatcherTestContext
    {
        private class FirstEventHandler : IEventHandler, IEventHandler<ILiteEvent>
        {
            public void Handle(IPublishedEvent<ILiteEvent> evnt)
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

            eventDispatcher = Create.NcqrCompatibleEventDispatcher(logger: loggerMock.Object,
                eventBusSettings: new EventBusSettings()
                {
                    EventHandlerTypesWithIgnoredExceptions = new[]
                    {
                        typeof (FirstEventHandler)
                    },
                    DisabledEventHandlerTypes = new Type[0]
                });
            eventDispatcher.OnCatchingNonCriticalEventHandlerException +=
                (e) => { handledNonCriticalEventHandlerException = e; };

            var firstEventHandler = new FirstEventHandler();

            var uniqueEventHandlerMock = new Mock<IEnumerable<bool>>();
            var eventHandlerMock = uniqueEventHandlerMock.As<IEventHandler>();
            secondOldSchoolEventHandlerMock = eventHandlerMock.As<IEventHandler<ILiteEvent>>();
            secondOldSchoolEventHandlerMock
                .Setup(_ => _.Handle(Moq.It.IsAny<IPublishedEvent<ILiteEvent>>()))
                .Throws<Exception>();;

            eventDispatcher.Register(firstEventHandler);
            eventDispatcher.Register(eventHandlerMock.Object);
        };

        Because of = () =>
            aggregateException = Catch.Only<AggregateException>(() =>
                eventDispatcher.Publish(publishableEvent.ToEnumerable().ToArray()));

        It should_throw_AggregateException = () =>
            aggregateException.ShouldNotBeNull();

        It should_put_1_exception_to_AggregateException = () =>
            aggregateException.InnerExceptions.Count.ShouldEqual(1);

        It should_call_2_event_handlers = () =>
            secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
                Moq.It.IsAny<IPublishedEvent<ILiteEvent>>()),
                Times.Once);

        It should_log_catched_exception = () =>
            loggerMock.Verify(x => x.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Times.Once);

        It should_be_handled_event_handler_exception = () =>
            handledNonCriticalEventHandlerException.ShouldNotBeNull();

        It should_not_event_handler_exception_be_critical = () =>
            handledNonCriticalEventHandlerException.IsCritical.ShouldBeFalse();

        private static NcqrCompatibleEventDispatcher eventDispatcher;
        private static IPublishableEvent publishableEvent;
        private static AggregateException aggregateException;
        private static Mock<IEventHandler<ILiteEvent>> secondOldSchoolEventHandlerMock;
        private static EventHandlerException handledNonCriticalEventHandlerException;
        private static readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
    }
}