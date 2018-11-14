using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    public class when_stroring_event : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            int sequenceCounter = 1;
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

            events = new UncommittedEventStream(Guid.NewGuid(), null);

            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountRegistered { ApplicationName = "App",
                    ConfirmationToken = "token",
                    Email = "test@test.com" }));

            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountConfirmed()));
            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountLocked()));

            var sessionProvider = new Mock<IUnitOfWork>();
            npgsqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            npgsqlConnection.Open();
            sessionProvider.Setup(x => x.Session)
                .Returns(Mock.Of<ISession>(i => i.Connection == npgsqlConnection));

            eventStore = new PostgresEventStore(
                new PostgreConnectionSettings
                {
                    ConnectionString = connectionStringBuilder.ConnectionString,
                    SchemaName = schemaName
                }, 
                eventTypeResolver,
                sessionProvider.Object);

            BecauseOf();
        }

        public void BecauseOf() => eventStore.Store(events);

        [NUnit.Framework.Test] public void should_read_stored_events () 
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            eventStream.Count().Should().Be(3);

            var firstEvent = eventStream.First();
            firstEvent.Payload.Should().BeOfType<AccountRegistered>();
            var accountRegistered = (AccountRegistered)firstEvent.Payload;

            accountRegistered.Email.Should().Be("test@test.com");
        }

        [NUnit.Framework.Test] public void should_persist_items_with_global_sequence_set () 
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            eventStream.Select(x => x.GlobalSequence).Should().NotContain(0);
        }
        
        [NUnit.Framework.Test] public void should_count_stored_events () => eventStore.CountOfAllEvents(); // it should not fail

        [OneTimeTearDown]
        public void TearDown()
        {
            npgsqlConnection.Close();
        }

        static PostgresEventStore eventStore;
        static UncommittedEventStream events;
        static Guid eventSourceId;
        static NpgsqlConnection npgsqlConnection;
    }
}
