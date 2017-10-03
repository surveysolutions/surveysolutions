using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using Npgsql;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    public class when_appending_events_with_wrong_expected_version : with_postgres_db
    {
        private Establish context = () =>
        {
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

            var sessionProvider = new Mock<ISessionProvider>();
            npgsqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            npgsqlConnection.Open();
            sessionProvider.Setup(x => x.GetSession())
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
        };

        Because of = () => exception = Catch.Only<InvalidOperationException>(() => eventStore.Store(appendStream));

        It should_throw_invalid_operation_exception = () => exception.Message.ShouldContain("Unexpected stream version");

        Cleanup c = () => npgsqlConnection?.Dispose();

        static PostgresEventStore eventStore;
        static UncommittedEventStream appendStream;
        static Exception exception;
        static NpgsqlConnection npgsqlConnection;
    }
}