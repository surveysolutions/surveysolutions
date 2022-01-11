using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    public class when_stroring_event : with_postgres_db
    {
        private DateTimeOffset textAnswerTime =
            new DateTimeOffset(2020, 9, 10, 18, 0, 0, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(10));

        [NUnit.Framework.OneTimeSetUp]
        public void OneTimeSetUp()
        {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            int sequenceCounter = 1;
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));
            eventTypeResolver.RegisterEventDataType(typeof(TextQuestionAnswered));

            events = new UncommittedEventStream(Guid.NewGuid(), null);

            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountRegistered
                {
                    ApplicationName = "App",
                    ConfirmationToken = "token",
                    Email = "test@test.com",

                }));

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

            events.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new TextQuestionAnswered(Id.g1, Id.g2, RosterVector.Empty, textAnswerTime, "test")));

            var sessionProvider = new Mock<IUnitOfWork>();
            npgsqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            npgsqlConnection.Open();
            sessionProvider.Setup(x => x.Session)
                .Returns(Mock.Of<ISession>(i => i.Connection == npgsqlConnection));

            eventStore = new PostgresEventStore(
                eventTypeResolver,
                sessionProvider.Object,
                Mock.Of<ILogger<PostgresEventStore>>());

            BecauseOf();
        }

        public void BecauseOf()
        {
            using var transaction = npgsqlConnection.BeginTransaction();
            eventStore.Store(events);
            transaction.Commit();
        }

        [NUnit.Framework.Test]
        public void should_read_stored_events()
        {
            using (npgsqlConnection.BeginTransaction())
            {
                var eventStream = eventStore.Read(eventSourceId, 0);
                eventStream.Count().Should().Be(4);

                var firstEvent = eventStream.First();
                firstEvent.Payload.Should().BeOfType<AccountRegistered>();
                var accountRegistered = (AccountRegistered)firstEvent.Payload;

                accountRegistered.Email.Should().Be("test@test.com");
            }
        }

        [NUnit.Framework.Test]
        public void should_persist_items_with_global_sequence_set()
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            eventStream.Select(x => x.GlobalSequence).Should().NotContain(0);
        }

        [Test]
        public void should_preserve_DateTimeOffset_timezone_for_originDate()
        {
            var eventStream = eventStore.Read(eventSourceId, 0);
            var answer = eventStream.Select(a => a.Payload as TextQuestionAnswered).Single(a => a != null);

            Assert.That(answer.OriginDate, Is.EqualTo(textAnswerTime));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            npgsqlConnection.Close();
        }

        static PostgresEventStore eventStore;
        static UncommittedEventStream events;
        static Guid eventSourceId;
        static NpgsqlConnection npgsqlConnection;
    }
}
