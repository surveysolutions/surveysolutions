using System;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Domain;

namespace Ncqrs.Tests.Eventing.ServiceModel.Bus
{
    [TestFixture]
    public class InProcessEventBusSpecs
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public class ADomainEvent
        {

        }

        public class AEvent
        {
        }

        [Test]
        public void When_a_catch_all_handler_is_register_it_should_be_called_for_all_events()
        {
            var catchAllEventHandler = new Mock<IEventHandler<object>>();
			catchAllEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(catchAllEventHandler.Object);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());

	        catchAllEventHandler.Verify(h => h.Handle(It.IsAny<IPublishedEvent<object>>()), 
				Times.Exactly(2));
        }
        
        private static IPublishableEvent CreateAEvent()
        {
            return new UncommittedEvent(Guid.NewGuid(), Guid.NewGuid(), 0, 0, DateTime.UtcNow, new AEvent(), new Version(1, 0));
        }

        private static IPublishableEvent CreateADomainEvent()
        {
            return new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, DateTime.UtcNow, new ADomainEvent(), new Version(1, 0));
        }

        [Test]
        public void When_multiple_messages_are_published_at_once_they_all_should_be_published()
        {

            var catchAllEventHandler = new Mock<IEventHandler<object>>();
			catchAllEventHandler.Setup(h => h.Handle(It.IsAny<IPublishedEvent<object>>()));

            var bus = new InProcessEventBus();
            
			bus.RegisterHandler(catchAllEventHandler.Object);

            var events = new[]
                             {
                                 CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            catchAllEventHandler.Verify(h => h.Handle(It.IsAny<IPublishedEvent<object>>()), 
			Times.Exactly(events.Length));
        }

        [Test]
        public void Registering_handler_via_generic_overload_should_also_add_the_handler()
        {
            var aDomainEventHandler = new Mock<IEventHandler<ADomainEvent>>();
			aDomainEventHandler.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

            var bus = new InProcessEventBus();

            bus.RegisterHandler(aDomainEventHandler.Object);

            var events = new IPublishableEvent[]
                             {
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            aDomainEventHandler.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()), 
			Times.Exactly(6));
        }


        [Test]
        public void When_multiple_messages_are_published_and_a_specific_handler_is_register_oply_the_matching_events_should_be_received_at_the_handler()
        {
            var aDomainEventHandler = new Mock<IEventHandler<ADomainEvent>>();
			aDomainEventHandler.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(aDomainEventHandler.Object);

            var events = new IPublishableEvent[]
                             {
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            aDomainEventHandler.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()),
				Times.Exactly(6));
        }

        [Test]
        public void When_a_handler_is_registered_for_a_specific_type_it_should_not_receive_other_events()
        {
            var aDomainEventEventHandler = new Mock<IEventHandler<ADomainEvent>>();
			aDomainEventEventHandler.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(aDomainEventEventHandler.Object);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            aDomainEventEventHandler.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()),
				Times.Exactly(2));
        }

        [Test]
        public void When_a_multiple_catch_all_handler_are_registered_for_they_should_all_been_called()
        {
            var catchAllEventHandler1 = new Mock<IEventHandler<object>>();
			catchAllEventHandler1.Setup(h => h.Handle(It.IsAny<IPublishedEvent<object>>()));

            var catchAllEventHandler2 = new Mock<IEventHandler<object>>();
			catchAllEventHandler2.Setup(h => h.Handle(It.IsAny<IPublishedEvent<object>>()));

            var catchAllEventHandler3 = new Mock<IEventHandler<object>>();
			catchAllEventHandler3.Setup(h => h.Handle(It.IsAny<IPublishedEvent<object>>()));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(catchAllEventHandler1.Object);
            bus.RegisterHandler(catchAllEventHandler2.Object);
            bus.RegisterHandler(catchAllEventHandler3.Object);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

			catchAllEventHandler1.Verify(h => h.Handle(It.IsAny<IPublishedEvent<object>>()),
				Times.Exactly(7));
			catchAllEventHandler2.Verify(h => h.Handle(It.IsAny<IPublishedEvent<object>>()),
				Times.Exactly(7));
			catchAllEventHandler3.Verify(h => h.Handle(It.IsAny<IPublishedEvent<object>>()),
				Times.Exactly(7));
        }

        [Test]
        public void When_a_multiple_specific_handlers_are_register_they_all_should_be_called_when_the_specific_event_is_published()
        {
			var specificEventHandler1 = new Mock<IEventHandler<ADomainEvent>>();
			specificEventHandler1.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

			var specificEventHandler2 = new Mock<IEventHandler<ADomainEvent>>();
			specificEventHandler2.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

			var specificEventHandler3 = new Mock<IEventHandler<ADomainEvent>>();
			specificEventHandler3.Setup(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(specificEventHandler1.Object);
            bus.RegisterHandler(specificEventHandler2.Object);
            bus.RegisterHandler(specificEventHandler3.Object);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

			specificEventHandler1.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()),
				Times.Exactly(2));
			specificEventHandler2.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()),
				Times.Exactly(2));
			specificEventHandler3.Verify(h => h.Handle(It.IsAny<IPublishedEvent<ADomainEvent>>()),
				Times.Exactly(2));
        }
    }
}
