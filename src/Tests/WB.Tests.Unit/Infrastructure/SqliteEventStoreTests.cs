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
                new List<UncommittedEvent> { Create.UncommittedEvent(
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
                uncommittedEvents.Add(Create.UncommittedEvent(eventSourceId,
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

        [TestCase(1)]
        [TestCase(3)]
        public void should_not_allow_to_insert_event_stream_with_wrong_version(int nextVersion)
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated(), sequence: 1) }));


            var duplicateSequeceStream = new UncommittedEventStream(null,
                new List<UncommittedEvent> { Create.UncommittedEvent(
                    eventSourceId, Create.Event.StaticTextUpdated(), sequence: nextVersion) });

            var exception = Assert.Throws<InvalidOperationException>(() => sqliteEventStorage.Store(duplicateSequeceStream));
            Assert.That(exception.Message, Does.Contain("Wrong version number"));
        }

        [Test]
        public void should_be_able_to_read_starting_from_version()
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var uncommittedEvents = new List<UncommittedEvent>();

            for (int i = 1; i <= 20; i++)
            {
                uncommittedEvents.Add(Create.UncommittedEvent(eventSourceId,
                    Create.Event.StaticTextUpdated(text: "text " + i),
                    sequence: i));
            }

            sqliteEventStorage.Store(new UncommittedEventStream(null,
                uncommittedEvents));

            var committedEvents = sqliteEventStorage.Read(eventSourceId, 16);

            Assert.That(committedEvents.Count(), Is.EqualTo(5));
            Assert.That((committedEvents.First().Payload as StaticTextUpdated).Text, Is.EqualTo("text 16"));
        }

        [TearDown]
        public void TearDown()
        {
            SqliteEventStorage.JsonSerializerSettings = this.origSerializerSettings;
        }
    }
}