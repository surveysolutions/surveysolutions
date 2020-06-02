using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Ncqrs.Eventing;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_publishing_event_by_prototype_event_source : NcqrCompatibleEventDispatcherTestContext
    {
        [NUnit.Framework.Test]
        public void should_not_call_registered_event_handler()
        {
            Guid ignoredEventSource = Guid.NewGuid();

            var secondEventHandlerMock = new Mock<TestDenormalzier>();
            var denormalizerRegistry = Create.Service.DenormalizerRegistryNative();
            denormalizerRegistry.Register<TestDenormalzier>();
            var serviceLocator = Mock.Of<IServiceLocator>(x =>
                x.GetInstance(typeof(TestDenormalzier)) == secondEventHandlerMock.Object);

            var prototype = Create.Service.MockOfAggregatePrototypeService(PrototypeType.Temporary);

            var ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(null, 
                serviceLocator, denormalizerRegistry, prototype);

            // Act
            ncqrCompatibleEventDispatcher.Publish(new[] { Create.Fake.PublishableEvent(eventSourceId: ignoredEventSource) });

            // Assert
            secondEventHandlerMock.Verify(x => x.Handle(It.IsAny<IPublishedEvent<InterviewCreated>>()), Times.Never);
        }

        [ReceivesIgnoredEvents]
        public class CustomHandler : IEventHandler, IEventHandler<InterviewCreated>
        {
            public bool HandleCalled = false;
            public void Handle(IPublishedEvent<InterviewCreated> _)
            {
                HandleCalled = true;
            }

            public string Name => "CustomHandler";
            public object[] Readers { get; }
            public object[] Writers { get; }
        }

        [NUnit.Framework.Test]
        public void should_call_registered_event_handler_with_receive_ignored_attribute_applied()
        {
            Guid ignoredEventSource = Guid.NewGuid();

            var customHandler = new CustomHandler();

            var denormalizerRegistry = Create.Service.DenormalizerRegistryNative();
            denormalizerRegistry.Register<CustomHandler>();

            var serviceLocator = Mock.Of<IServiceLocator>(x =>
                x.GetInstance(typeof(CustomHandler)) == customHandler);

            var prototype = Create.Service.MockOfAggregatePrototypeService(PrototypeType.Temporary);
            var ncqrCompatibleEventDispatcher = CreateNcqrCompatibleEventDispatcher(
                serviceLocator: serviceLocator, 
                denormalizerRegistry: denormalizerRegistry, 
                prototypeService: prototype);

            // Act
            ncqrCompatibleEventDispatcher.Publish(new[] { Create.PublishedEvent.InterviewCreated(interviewId: ignoredEventSource) });

            // Assert
            customHandler.HandleCalled.Should().BeTrue();
        }
    }
}
