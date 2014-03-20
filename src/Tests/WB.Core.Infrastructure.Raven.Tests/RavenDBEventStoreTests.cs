﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using Ncqrs.Eventing.Sourcing;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Raven.Tests
{
    [TestFixture]
    public class RavenDBEventStoreTests : RavenDBTestBase
    {
        [SetUp]
        public void Setup()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Serializable]
        public class CustomerCreatedEvent
        {
            public CustomerCreatedEvent(string name, int age)
            {
                Name = name;
                Age = age;
            }

            public string Name { get; set; }

            public int Age
            {
                get;
                set;
            }

            public override bool Equals(object obj)
            {
                var other = obj as CustomerCreatedEvent;
                if (other == null)
                {
                    return false;
                }
                bool result = Name.Equals(other.Name)
                       && Age.Equals(other.Age);
                return result;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Serializable]
        public class CustomerNameChanged
        {
            public string NewName { get; private set; }

            public CustomerNameChanged(string newName)
            {
                NewName = newName;
            }

            public override bool Equals(object obj)
            {
                var other = obj as CustomerNameChanged;
                if (other == null)
                {
                    return false;
                }
                bool result = NewName.Equals(other.NewName);
                return result;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        

        [Test]
        public void Event_saving_smoke_test()
        {
            var targetStore = new RavenDBEventStore(_documentStore, 100);
            var id = Guid.NewGuid();

            int sequenceCounter = 0;

            var events = new UncommittedEventStream(Guid.NewGuid());

            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerCreatedEvent("Foo",
                                                                                                                                   35),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));
                             

            targetStore.Store(events);
        }

        [Test]
        public void Saving_event_source_while_there_is_a_newer_event_source_should_throw_concurency_exception()
        {
            var targetStore = new RavenDBEventStore(_documentStore, 100);
            var id = Guid.NewGuid();

            int sequenceCounter = 0;

            var events = new UncommittedEventStream(Guid.NewGuid());

            events.Append(
                new UncommittedEvent(
                    Guid.NewGuid(),
                    id,
                    sequenceCounter++,
                    0,
                    DateTime.UtcNow,
                    new CustomerCreatedEvent("Foo", 35),
                    new Version(1, 0)));
            events.Append(
                new UncommittedEvent(
                    Guid.NewGuid(),
                    id,
                    sequenceCounter++,
                    0,
                    DateTime.UtcNow,
                    new CustomerNameChanged("Name" + sequenceCounter),
                    new Version(1, 0)));
            
            targetStore.Store(events);

            Action act = () => targetStore.Store(events);
            act.ShouldThrow<ConcurrencyException>();
        }

        [Test]
        public void Retrieving_all_events_should_return_the_same_as_added()
        {
            var targetStore = new RavenDBEventStore(_documentStore, 100, true, false);
            var id = Guid.NewGuid();

            int sequenceCounter = 0;

            var events = new UncommittedEventStream(Guid.NewGuid());

            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerCreatedEvent("Foo",
                                                                                                                                   35),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerNameChanged(
                                                                                                              "Name" + sequenceCounter),
                                               new Version(1, 0)));

            targetStore.Store(events);

            var result = targetStore.ReadFrom(id, long.MinValue, long.MaxValue);
            result.Count().Should().Be(events.Count());
            result.First().EventIdentifier.Should().Be(events.First().EventIdentifier);

            var streamList = events.ToList();
            var resultList = result.ToList();

            for (int i = 0; i < resultList.Count; i++)
            {
                Assert.IsTrue(AreEqual(streamList[i], resultList[i]));
            }
            
        }

        private static bool AreEqual(UncommittedEvent uncommitted, CommittedEvent committed)
        {
            return uncommitted.EventIdentifier == committed.EventIdentifier
                   && uncommitted.EventSourceId == committed.EventSourceId
                   && uncommitted.Payload.Equals(committed.Payload)
                   && uncommitted.EventSequence == committed.EventSequence;
        }
    }
}
