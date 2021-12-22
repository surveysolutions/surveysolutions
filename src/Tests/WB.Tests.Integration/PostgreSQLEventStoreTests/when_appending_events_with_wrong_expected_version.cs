using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.PostgreSQLEventStoreTests
{
    public class when_appending_events_with_wrong_expected_version : with_postgres_db
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var sessionProvider = new Mock<IUnitOfWork>();
            dbConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            dbConnection.Open();
            sessionProvider.Setup(x => x.Session)
                .Returns(Mock.Of<ISession>(i => i.Connection == dbConnection));

            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

            eventStore = new PostgresEventStore(
                eventTypeResolver,
                sessionProvider.Object,
                Mock.Of<ILogger<PostgresEventStore>>());
        }

        [SetUp]
        public void PerTestSetup()
        {
            transaction = dbConnection?.BeginTransaction(IsolationLevel.ReadUncommitted);
        }

        [TearDown]
        public void PerTestTearDown()
        {
            transaction?.Rollback();
        }

        [Test]
        public void should_throw_invalid_operation_exception()
        {
            Guid eventSourceId = Id.gA;

            int sequenceCounter = 1;
          
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

            
            eventStore.Store(initialStoredStream);

            var appendStream = new UncommittedEventStream(null);

            appendStream.Append(new UncommittedEvent(Guid.NewGuid(),
                eventSourceId,
                5,
                4,
                DateTime.UtcNow,
                new AccountLocked()));

            var exception = Assert.Throws<InvalidOperationException>(() => eventStore.Store(appendStream));
            exception.Message.Should().Contain("Unexpected stream version");
        }

        [Test]
        public void should_be_able_to_page_all_events()
        {
            Guid eventSourceId = Id.gB;

            int sequenceCounter = 1;
          
            var initialStoredStream = new UncommittedEventStream(Guid.NewGuid(), null);
            initialStoredStream.Append(new UncommittedEvent(Id.g1,
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

            initialStoredStream.Append(new UncommittedEvent(Id.g2,
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountConfirmed()));

            
            initialStoredStream.Append(new UncommittedEvent(Id.g3,
                eventSourceId,
                sequenceCounter++,
                0,
                DateTime.UtcNow,
                new AccountLocked()));

            eventStore.Store(initialStoredStream);

            // Act 
            var page1 = eventStore.GetRawEventsFeed(0, 2).ToList();

            // Assert
            Assert.That(page1, Has.Count.EqualTo(2));
            Assert.That(page1[0].Id, Is.EqualTo(Id.g1));
            Assert.That(page1[1].Id, Is.EqualTo(Id.g2));


            // Act
            var page2 = eventStore.GetRawEventsFeed(page1[1].GlobalSequence, 2).ToList();

            // Assert
            Assert.That(page2, Has.Count.EqualTo(1));
            Assert.That(page2[0].Id, Is.EqualTo(Id.g3));
        }

        public void TearDown()
        {
            dbConnection.Close();
        }


        static PostgresEventStore eventStore;
        static NpgsqlConnection dbConnection;
        private NpgsqlTransaction transaction;
    }
}
