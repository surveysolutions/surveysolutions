using System;
using System.Collections.Generic;
using System.Linq;
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
    public class when_stroring_event : with_postgres_db
    {
        Establish context = () =>
        {
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
        };

        Because of = () => eventStore.Store(events);

        It should_read_stored_events = () =>
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            eventStream.Count().ShouldEqual(3);

            var firstEvent = eventStream.First();
            firstEvent.Payload.ShouldBeOfExactType<AccountRegistered>();
            var accountRegistered = (AccountRegistered)firstEvent.Payload;

            accountRegistered.Email.ShouldEqual("test@test.com");
        };

        It should_persist_items_with_global_sequence_set = () =>
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            eventStream.Select(x => x.GlobalSequence).ShouldNotContain(0);
        };
        
        It should_count_stored_events = () => eventStore.CountOfAllEvents(); // it should not fail

        It should_be_able_to_read_all_events = () =>
        {
            var committedEvents = eventStore.GetAllEvents().ToList();
            committedEvents.Count.ShouldEqual(3);
            committedEvents[0].Payload.ShouldBeOfExactType(typeof(AccountRegistered));
            committedEvents[1].Payload.ShouldBeOfExactType(typeof(AccountConfirmed));
            committedEvents[2].Payload.ShouldBeOfExactType(typeof(AccountLocked));
        };

        It should_be_able_to_count_events_after_position = () => eventStore.GetEventsCountAfterPosition(new EventPosition(0, 0, eventSourceId, 1)).ShouldEqual(2);

        It should_be_able_to_get_events_after_position = () =>
        {
            List<CommittedEvent> eventsAfterPosition = eventStore.GetEventsAfterPosition(new EventPosition(0, 0, eventSourceId, 1)).ToList()[0].ToList();
            eventsAfterPosition[0].Payload.ShouldBeOfExactType(typeof(AccountConfirmed));
            eventsAfterPosition[1].Payload.ShouldBeOfExactType(typeof(AccountLocked));
        };

        Cleanup c = () => npgsqlConnection?.Dispose();

        static PostgresEventStore eventStore;
        static UncommittedEventStream events;
        static Guid eventSourceId;
        static NpgsqlConnection npgsqlConnection;
    }
}