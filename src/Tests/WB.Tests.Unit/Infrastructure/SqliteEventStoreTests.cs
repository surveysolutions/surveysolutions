using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using NUnit.Framework;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Tests.Unit.Infrastructure
{
    public class SqliteEventStoreTests
    {
        private Func<JsonSerializerSettings> origSerializerSettings;
        private SqliteEventStorage sqliteEventStorage;

        [SetUp]
        public void Setup()
        {
            this.origSerializerSettings = SqliteEventStorage.JsonSerializerSettings;

            SqliteEventStorage.JsonSerializerSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = new SqliteEventStorage.CapiAndMainCoreToInterviewerAndSharedKernelsBinder()
            };

            sqliteEventStorage = new SqliteEventStorage(new SQLitePlatformWin32(),
              Mock.Of<ILogger>(),
              Mock.Of<ITraceListener>(),
              new SqliteSettings
              {
                  PathToDatabaseDirectory = ":memory:"
              },
              Mock.Of<IEnumeratorSettings>(x => x.EventChunkSize == 100));
        }

        [Test]
        public void Should_be_able_to_store_events()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated()) }));


            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0);
            Assert.That(committedEvents.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_to_read_events_page_by_page()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var uncommittedEvents = new List<UncommittedEvent>();

            for (int i = 1; i <= 301; i++)
            {
                uncommittedEvents.Add(Create.Other.UncommittedEvent(eventSourceId,
                    Create.Event.StaticTextUpdated(text: "text " + i),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                uncommittedEvents));

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0).ToList();
            Assert.That(committedEvents.Count, Is.EqualTo(301));
            
            var payload = committedEvents.Last().Payload as StaticTextUpdated;
            Assert.That(payload, Is.Not.Null, "Should keep event type after save/read");
            Assert.That(payload.Text, Is.EqualTo("text 301"));
            
        }

        [Test]
        public void Should_be_able_to_read_events_if_events_in_the_middle_are_missing()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var missingEventIds = new List<Guid>();
            var uncommittedEvents = new List<UncommittedEvent>();

            for (int i = 1; i <= 1000; i++)
            {
                var uncommittedEvent = Create.Other.UncommittedEvent(eventSourceId, sequence: i,
                    payload: Create.Event.StaticTextUpdated(text: $"text {i}"));

                if (101 <= i && i <= 900)
                {
                    missingEventIds.Add(uncommittedEvent.EventIdentifier);
                }

                uncommittedEvents.Add(uncommittedEvent);
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null, uncommittedEvents));

            // manually removing events from the middle of store event stream
            foreach (var missingEventId in missingEventIds)
            {
                this.sqliteEventStorage.connection.Delete<EventView>(missingEventId);
            }

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0).ToList();
            Assert.That(committedEvents.Count, Is.EqualTo(200));

            var payload = committedEvents.Last().Payload as StaticTextUpdated;
            Assert.That(payload, Is.Not.Null, "Should keep event type after save/read");
            Assert.That(payload.Text, Is.EqualTo("text 1000"));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void should_not_allow_to_insert_event_stream_with_wrong_version(int nextVersion)
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated(), sequence: 1) }));


            var duplicateSequeceStream = new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated(), sequence: nextVersion) });

            var exception = Assert.Throws<InvalidOperationException>(() => sqliteEventStorage.Store(duplicateSequeceStream));
            Assert.That(exception.Message, Does.Contain("Expected event stream with version"));
        }

        [Test]
        public void should_not_recreate_already_existing_stream()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, 
                    Create.Event.StaticTextUpdated(), 
                    sequence: 1,
                    initialVersion:0) }));


            var duplicateSequeceStream = new UncommittedEventStream(null,
                new List<UncommittedEvent> {
                    Create.Other.UncommittedEvent(
                        eventSourceId, 
                        Create.Event.StaticTextUpdated(), 
                        sequence: 1,
                        initialVersion: 0)
                });

            var exception = Assert.Throws<InvalidOperationException>(() => sqliteEventStorage.Store(duplicateSequeceStream));
            Assert.That(exception.Message, Does.Contain("Expected to store new event stream, but it already exists"));
        }

        [Test]
        public void should_be_able_to_read_starting_from_version()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var uncommittedEvents = new List<UncommittedEvent>();

            for (int i = 1; i <= 20; i++)
            {
                uncommittedEvents.Add(Create.Other.UncommittedEvent(eventSourceId,
                    Create.Event.StaticTextUpdated(text: "text " + i),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                uncommittedEvents));

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 16);

            Assert.That(committedEvents.Count(), Is.EqualTo(5));
            Assert.That((committedEvents.First().Payload as StaticTextUpdated).Text, Is.EqualTo("text 16"));
        }

        [Test]
        public void should_be_able_to_remove_stream_by_id()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated()) }));

            this.sqliteEventStorage.RemoveEventSourceById(eventSourceId);
            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0);
            Assert.That(committedEvents.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Should_be_able_to_read_last_event_sequence()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var uncommittedEvents = new List<UncommittedEvent>();

            for (int i = 1; i <= 301; i++)
            {
                uncommittedEvents.Add(Create.Other.UncommittedEvent(eventSourceId,
                    Create.Event.StaticTextUpdated(text: "text " + i),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null, uncommittedEvents));

            var lastEventSequence = sqliteEventStorage.GetLastEventSequence(eventSourceId);
            Assert.That(lastEventSequence, Is.EqualTo(301));
        }

        [Test]
        public void Should_be_able_to_read_last_event_sequence_when_aggregate_is_not_exists()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var lastEventSequence = sqliteEventStorage.GetLastEventSequence(eventSourceId);
            Assert.That(lastEventSequence, Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            SqliteEventStorage.JsonSerializerSettings = this.origSerializerSettings;
        }
    }
}