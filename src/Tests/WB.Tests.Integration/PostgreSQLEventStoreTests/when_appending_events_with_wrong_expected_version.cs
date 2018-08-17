using System;
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
    public class when_appending_events_with_wrong_expected_version : with_postgres_db
    {
        [Test] public void should_throw_invalid_operation_exception () {
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            int sequenceCounter = 1;
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

            var initialStoredStream = new UncommittedEventStream(Guid.NewGuid(), null);

            initialStoredStream.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountRegistered
                {
                    ApplicationName = "App",
                    ConfirmationToken = "token",
                    Email = "test@test.com"
                }));

            initialStoredStream.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountConfirmed()));

            var sessionProvider = new Mock<IUnitOfWork>();
            npgsqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            npgsqlConnection.Open();
            sessionProvider.SetupGet(x => x.Session)
                .Returns(Mock.Of<ISession>(i => i.Connection == npgsqlConnection));

            eventStore = new PostgresEventStore(
                new PostgreConnectionSettings
                {
                    ConnectionString = connectionStringBuilder.ConnectionString,
                    SchemaName = schemaName
                },
                eventTypeResolver,
                sessionProvider.Object);

            eventStore.Store(initialStoredStream);

            appendStream = new UncommittedEventStream(null);

            appendStream.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                5,
                4,
                DateTime.UtcNow,
                new AccountLocked()));

            var exception = Assert.Throws<InvalidOperationException>(() => eventStore.Store(appendStream));
            exception.Message.Should().Contain("Unexpected stream version");
        }
        
        public void TearDown()
        {
            npgsqlConnection.Close();
        }


        static PostgresEventStore eventStore;
        static UncommittedEventStream appendStream;
        static NpgsqlConnection npgsqlConnection;
    }
}
