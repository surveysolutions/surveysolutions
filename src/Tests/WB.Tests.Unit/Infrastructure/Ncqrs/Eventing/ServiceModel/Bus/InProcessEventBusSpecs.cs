using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using Rhino.Mocks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using MockRepository = Rhino.Mocks.MockRepository;

namespace WB.Tests.Unit.Infrastructure.Ncqrs.Eventing.ServiceModel.Bus
{
    [TestFixture]
    internal class InProcessEventBusSpecs
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        public class ADomainEvent : IEvent
        {
        }

        public class AEvent : IEvent
        {
        }

        private static IPublishableEvent CreateAEvent()
        {
            return Create.Fake.PublishableEvent(payload: new AEvent());
        }

        private static IPublishableEvent CreateADomainEvent()
        {
            return Create.Fake.PublishableEvent(payload: new ADomainEvent());
        }

        [Test]
        public void Registering_handler_via_generic_overload_should_also_add_the_handler()
        {
            var aDomainEventHandler = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();
            var bus = GetEventBus();

            bus.RegisterHandler(aDomainEventHandler);

            var events = new[]
            {
                CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(),
                CreateAEvent(),
                CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(),
                CreateAEvent()
            };

            bus.Publish(events);

            aDomainEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(6));
        }
        private static InProcessEventBus GetEventBus()
        {
            return new InProcessEventBus(Mock.Of<IEventStore>(), new EventBusSettings(), Mock.Of<ILogger>());
        }

        [Test]
        public void When_a_catch_all_handler_is_register_it_should_be_called_for_all_events()
        {
            var catchAllEventHandler = MockRepository.GenerateMock<IEventHandler<IEvent>>();

            var bus = GetEventBus();
            bus.RegisterHandler(catchAllEventHandler);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());

            catchAllEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Twice());
        }

        [Test]
        public void When_a_handler_is_registered_for_a_specific_type_it_should_not_receive_other_events()
        {
            var aDomainEventEventHandler = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();

            var bus = GetEventBus();
            bus.RegisterHandler(aDomainEventEventHandler);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            aDomainEventEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Twice());
        }

        [Test]
        public void When_a_multiple_catch_all_handler_are_registered_for_they_should_all_been_called()
        {
            var catchAllEventHandler1 = MockRepository.GenerateMock<IEventHandler<IEvent>>();
            var catchAllEventHandler2 = MockRepository.GenerateMock<IEventHandler<IEvent>>();
            var catchAllEventHandler3 = MockRepository.GenerateMock<IEventHandler<IEvent>>();

            var bus = GetEventBus();
            bus.RegisterHandler(catchAllEventHandler1);
            bus.RegisterHandler(catchAllEventHandler2);
            bus.RegisterHandler(catchAllEventHandler3);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            catchAllEventHandler1.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(7));
            catchAllEventHandler2.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(7));
            catchAllEventHandler3.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(7));
        }

        [Test]
        public void
            When_a_multiple_specific_handlers_are_register_they_all_should_be_called_when_the_specific_event_is_published()
        {
            var specificEventHandler1 = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();
            var specificEventHandler2 = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();
            var specificEventHandler3 = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();

            var bus = GetEventBus();
            bus.RegisterHandler(specificEventHandler1);
            bus.RegisterHandler(specificEventHandler2);
            bus.RegisterHandler(specificEventHandler3);

            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateADomainEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());
            bus.Publish(CreateAEvent());

            specificEventHandler1.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(2));
            specificEventHandler2.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(2));
            specificEventHandler3.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(2));
        }


        [Test]
        public void When_multiple_messages_are_published_and_a_specific_handler_is_register_oply_the_matching_events_should_be_received_at_the_handler()
        {
            var aDomainEventHandler = MockRepository.GenerateMock<IEventHandler<ADomainEvent>>();
            var bus = GetEventBus();
            bus.RegisterHandler(aDomainEventHandler);

            var events = new []
            {
                CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(),
                CreateAEvent(),
                CreateAEvent(), CreateADomainEvent(), CreateADomainEvent(), CreateAEvent(), CreateADomainEvent(),
                CreateAEvent()
            };

            bus.Publish(events);

            aDomainEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(6));
        }

        [Test]
        public void When_multiple_messages_are_published_at_once_they_all_should_be_published()
        {
            var catchAllEventHandler = MockRepository.GenerateMock<IEventHandler<IEvent>>();
            var bus = GetEventBus();
            bus.RegisterHandler(catchAllEventHandler);

            var events = new[]
            {
                CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(),
                CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent(), CreateAEvent()
            };

            bus.Publish(events);

            catchAllEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(events.Length));
        }

        [Test]
        public void When_event_of_ignored_event_stream_is_published_it_should_not_be_published()
        {
            var eventSourceToIgnore = Guid.NewGuid();

            var eventToPublish = Create.Fake.PublishableEvent(eventSourceId: eventSourceToIgnore, payload: new AEvent());
            
            var catchAllEventHandler = MockRepository.GenerateMock<IEventHandler<IEvent>>();
            var bus = new InProcessEventBus(Mock.Of<IEventStore>(),
                new EventBusSettings()
                {
                    IgnoredAggregateRoots = new List<string>(new[] {eventSourceToIgnore.FormatGuid()})
                },
                Mock.Of<ILogger>());
            bus.RegisterHandler(catchAllEventHandler);

            var events = new[]
            {
                eventToPublish
            };

            bus.Publish(events);

            catchAllEventHandler.AssertWasCalled(h => h.Handle(null),
                options => options.IgnoreArguments().Repeat.Times(0));
        }
    }
}
