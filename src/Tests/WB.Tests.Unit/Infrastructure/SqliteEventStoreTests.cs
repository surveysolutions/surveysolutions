using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    public class SqliteEventStoreTests
    {
        private SqliteMultiFilesEventStorage sqliteEventStorage;

        [SetUp]
        public void Setup()
        {
            var mockOfEncryptionService = new Mock<IEncryptionService>();
            mockOfEncryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns<string>(x => x);
            mockOfEncryptionService.Setup(x => x.Decrypt(It.IsAny<string>())).Returns<string>(x => x);

            sqliteEventStorage = new SqliteMultiFilesEventStorage(Mock.Of<ILogger>(),
              new SqliteSettings
              {
                  PathToDatabaseDirectory = "",
                  PathToInterviewsDirectory = "",
                  InMemoryStorage = true
              },
              Mock.Of<IEnumeratorSettings>(x => x.EventChunkSize == 100),
              Mock.Of<IFileSystemAccessor>(x=>x.IsFileExists(Moq.It.IsAny<string>()) == true),
              Mock.Of<IEventTypeResolver>(x => x.ResolveType("TextQuestionAnswered") == typeof(TextQuestionAnswered)), 
              mockOfEncryptionService.Object);
        }

        [Test]
        public void should_check_that_HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes()
        {
            var eventSourceId = Id.gA;

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> {
                    Create.Other.UncommittedEvent(eventSourceId, Create.Event.TextQuestionAnswered(), sequence: 1),
                    Create.Other.UncommittedEvent(eventSourceId, Create.Event.TextQuestionAnswered(), sequence: 2)
                }));

            var hasChanged = sqliteEventStorage.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(1, eventSourceId, typeof(TextQuestionAnswered).Name);
            Assert.That(hasChanged, Is.True);
        }

        [Test]
        public void Should_be_able_to_store_events()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.TextQuestionAnswered()) }));

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
                    Create.Event.TextQuestionAnswered(answer: "text 301"),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                uncommittedEvents));

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0).ToList();
            Assert.That(committedEvents.Count, Is.EqualTo(301));
            
            var payload = committedEvents.Last().Payload as TextQuestionAnswered;
            Assert.That(payload, Is.Not.Null, "Should keep event type after save/read");
            Assert.That(payload.Answer, Is.EqualTo("text 301"));
            
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
                    payload: Create.Event.TextQuestionAnswered(answer: $"text {i}"));

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
                this.sqliteEventStorage.connectionByEventSource.Values.Single().Delete<EventView>(missingEventId);
            }

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0).ToList();
            Assert.That(committedEvents.Count, Is.EqualTo(200));

            var payload = committedEvents.Last().Payload as TextQuestionAnswered;
            Assert.That(payload, Is.Not.Null, "Should keep event type after save/read");
            Assert.That(payload.Answer, Is.EqualTo("text 1000"));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void should_not_allow_to_insert_event_stream_with_wrong_version(int nextVersion)
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.TextQuestionAnswered(), sequence: 1) }));


            var duplicateSequeceStream = new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.TextQuestionAnswered(), sequence: nextVersion) });

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
                    Create.Event.TextQuestionAnswered(), 
                    sequence: 1,
                    initialVersion:0) }));


            var duplicateSequeceStream = new UncommittedEventStream(null,
                new List<UncommittedEvent> {
                    Create.Other.UncommittedEvent(
                        eventSourceId, 
                        Create.Event.TextQuestionAnswered(), 
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
                    Create.Event.TextQuestionAnswered(answer: "text " + i),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                uncommittedEvents));

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 16);

            Assert.That(committedEvents.Count(), Is.EqualTo(5));
            Assert.That((committedEvents.First().Payload as TextQuestionAnswered).Answer, Is.EqualTo("text 16"));
        }

        [Test]
        public void should_be_able_to_remove_stream_by_id()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.Other.UncommittedEvent(
                    eventSourceId, Create.Event.TextQuestionAnswered()) }));

            this.sqliteEventStorage.RemoveEventSourceById(eventSourceId);
            var committedEvents = sqliteEventStorage.Read(eventSourceId, 0);
            Assert.That(committedEvents.Count(), Is.EqualTo(0));
        }
    }
}
