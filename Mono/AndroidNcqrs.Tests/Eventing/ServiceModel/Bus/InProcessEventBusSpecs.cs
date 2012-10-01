using System;
using System.Reflection;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using AndroidMocks;
using Ncqrs.Domain;

namespace Ncqrs.Tests.Eventing.ServiceModel.Bus
{
    [TestFixture]
    public class InProcessEventBusSpecs
    {
        public class ADomainEvent
        {

        }

        public class AEvent
        {
        }

        [Test]
        public void When_a_catch_all_handler_is_register_it_should_be_called_for_all_events()
        {
            var catchAllEventHandler = new DynamicMock<IEventHandler<object>>();
			catchAllEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(catchAllEventHandler.Instance);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());

            catchAllEventHandler.AssertWasCalled(h => h.Handle(null), times: 2);
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

            var catchAllEventHandler = new DynamicMock<IEventHandler<object>>();
			catchAllEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            
			bus.RegisterHandler(catchAllEventHandler.Instance);

            var events = new[]
                             {
                                 CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            catchAllEventHandler.AssertWasCalled(h => h.Handle(null), times: events.Length);
        }

        [Test]
        public void Registering_handler_via_generic_overload_should_also_add_the_handler()
        {
            var aDomainEventHandler = new DynamicMock<IEventHandler<ADomainEvent>>();
			aDomainEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();

            bus.RegisterHandler(aDomainEventHandler.Instance);

            var events = new IPublishableEvent[]
                             {
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            aDomainEventHandler.AssertWasCalled(h => h.Handle(null), times: 6);
        }


        [Test]
        public void When_multiple_messages_are_published_and_a_specific_handler_is_register_oply_the_matching_events_should_be_received_at_the_handler()
        {
            var aDomainEventHandler = new DynamicMock<IEventHandler<ADomainEvent>>();
			aDomainEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(aDomainEventHandler.Instance);

            var events = new IPublishableEvent[]
                             {
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent(),
                                 CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(), CreateAEvent()
                             };

            bus.Publish(events);

            aDomainEventHandler.AssertWasCalled(h => h.Handle(null), times: 6);
        }

        [Test]
        public void When_a_handler_is_registered_for_a_specific_type_it_should_not_receive_other_events()
        {
            var aDomainEventEventHandler = new DynamicMock<IEventHandler<ADomainEvent>>();
			aDomainEventEventHandler.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(aDomainEventEventHandler.Instance);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            aDomainEventEventHandler.AssertWasCalled(h => h.Handle(null), times: 2);
        }

        [Test]
        public void When_a_multiple_catch_all_handler_are_registered_for_they_should_all_been_called()
        {
            var catchAllEventHandler1 = new DynamicMock<IEventHandler<object>>();
			catchAllEventHandler1.Expect(h => h.Handle(null));

            var catchAllEventHandler2 = new DynamicMock<IEventHandler<object>>();
			catchAllEventHandler2.Expect(h => h.Handle(null));

            var catchAllEventHandler3 = new DynamicMock<IEventHandler<object>>();
			catchAllEventHandler3.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(catchAllEventHandler1.Instance);
            bus.RegisterHandler(catchAllEventHandler2.Instance);
            bus.RegisterHandler(catchAllEventHandler3.Instance);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            catchAllEventHandler1.AssertWasCalled(h => h.Handle(null), times : 7);
            catchAllEventHandler2.AssertWasCalled(h => h.Handle(null), times : 7);
            catchAllEventHandler3.AssertWasCalled(h => h.Handle(null), times : 7);
        }

        [Test]
        public void When_a_multiple_specific_handlers_are_register_they_all_should_be_called_when_the_specific_event_is_published()
        {
			var specificEventHandler1 = new DynamicMock<IEventHandler<ADomainEvent>>();
			specificEventHandler1.Expect(h => h.Handle(null));

			var specificEventHandler2 = new DynamicMock<IEventHandler<ADomainEvent>>();
			specificEventHandler2.Expect(h => h.Handle(null));

			var specificEventHandler3 = new DynamicMock<IEventHandler<ADomainEvent>>();
			specificEventHandler3.Expect(h => h.Handle(null));

            var bus = new InProcessEventBus();
            bus.RegisterHandler(specificEventHandler1.Instance);
            bus.RegisterHandler(specificEventHandler2.Instance);
            bus.RegisterHandler(specificEventHandler3.Instance);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            specificEventHandler1.AssertWasCalled(h => h.Handle(null), times : 2);
            specificEventHandler2.AssertWasCalled(h => h.Handle(null), times : 2);
            specificEventHandler3.AssertWasCalled(h => h.Handle(null), times : 2);
        }
    }
}
