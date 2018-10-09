using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Ncqrs.Eventing;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_by_ignored_event_source : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.Test]
        public void should_not_call_registered_event_handler()
        {
            Guid ignoredEventSource = Guid.NewGuid();

            var secondEventHandlerMock = new Mock<IEventHandler>();
            var secondOldSchoolEventHandlerMock = secondEventHandlerMock.As<IEventHandler<IEvent>>();
            var ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(new EventBusSettings()
            {
                IgnoredAggregateRoots = new HashSet<string>(new[] { ignoredEventSource.FormatGuid() })
            });

            ncqrCompatibleEventDispatcher.Register(secondEventHandlerMock.Object);

            // Act
            ncqrCompatibleEventDispatcher.Publish(new[] { Create.Fake.PublishableEvent(eventSourceId: ignoredEventSource) });

            // Assert
            secondOldSchoolEventHandlerMock.Verify(x => x.Handle(
                    Moq.It.IsAny<IPublishedEvent<IEvent>>()),
                Times.Never);
        }

        [ReceivesIgnoredEvents]
        public class CustomHandler : IEventHandler, IEventHandler<IEvent>
        {
            public bool HandleCalled = false;
            public void Handle(IPublishedEvent<IEvent> _)
            {
                HandleCalled = true;
            }

            public string Name => "CustomHandler";
            public object[] Readers => Array.Empty<object>();
            public object[] Writers => Array.Empty<object>();
        }

        [NUnit.Framework.Test]
        public void should_call_registered_event_handler_with_receive_ignored_attribute_applied()
        {
            Guid ignoredEventSource = Guid.NewGuid();

            var ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(new EventBusSettings()
            {
                IgnoredAggregateRoots = new HashSet<string>(new[] { ignoredEventSource.FormatGuid() })
            });

            var customHandler = new CustomHandler();
            ncqrCompatibleEventDispatcher.Register(customHandler);

            // Act
            ncqrCompatibleEventDispatcher.Publish(new[] { Create.Fake.PublishableEvent(eventSourceId: ignoredEventSource) });

            // Assert
            customHandler.HandleCalled.Should().BeTrue();
        }
    }
}
