using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class when_two_threads_storing_events : with_postgres_db
    {
        [Test]
        public void third_thread_should_not_acquire_events_with_higher_globalsequence_then_first_thread()
        {
            // open first wait for reset event
            // open second - wait for complete for 5 seconds
            // open third - query events
            // reset event
            // query events once more

            var tick1 = new ManualResetEvent(false);
            var tick2 = new ManualResetEvent(false);
            var tick3 = new ManualResetEvent(false);
            var tick4 = new ManualResetEvent(false);
            var tick5 = new ManualResetEvent(false);
            var tickCCompleted = new ManualResetEvent(false);

            TimeSpan TimeOut = TimeSpan.FromSeconds(30);

            long expectedGlobalSequence = 0;

            var threadA = new Thread(() =>
            {
                var db = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                db.Open();

                var tr = db.BeginTransaction();
                var store = CreateNewEventStore(db);

                tick1.WaitOne(TimeOut);

                var id = Id.gA;
                var sequence = 0;

                store.Store(new UncommittedEventStream("threadA", Enumerable.Range(0, 3).Select(i =>
                    new UncommittedEvent(Guid.NewGuid(), id, ++sequence, 0,
                        DateTime.UtcNow,
                        new AccountRegistered
                        {
                            ApplicationName = "App",
                            ConfirmationToken = Interlocked.Increment(ref expectedGlobalSequence).ToString(),
                            Email = "test@test.com"
                        })).ToArray()));

                // allow thB to start
                tick2.Set();

                // wait for thB complete(or not)
                tick4.WaitOne(TimeOut);

                tr.Commit();
                tick5.Set(); // notify that thA completed
            });

            var threadB = new Thread(() =>
            {
                var db = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                db.Open();

                var tr = db.BeginTransaction();
                var store = CreateNewEventStore(db);
                var id = Id.gB;
                var sequence = 0;
                tick2.WaitOne(TimeOut); // wait for thA to be ready to commit and acquire sequences

                store.Store(new UncommittedEventStream("threadB", Enumerable.Range(0, 3).Select(i =>
                    new UncommittedEvent(Guid.NewGuid(), id, ++sequence, 0,
                        DateTime.UtcNow,
                        new AccountRegistered
                        {
                            ApplicationName = "App",
                            ConfirmationToken = Interlocked.Increment(ref expectedGlobalSequence).ToString(),
                            Email = "test@test.com"
                        })).ToArray()));

                tr.Commit();
                tick3.Set(); // notify that thB is completed
            });


            List<RawEvent> eventList = null;
            var threadC = new Thread(() =>
            {
                var db = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
                db.Open();

                using (db.BeginTransaction())
                {
                   var store = CreateNewEventStore(db);
                   tick3.WaitOne(TimeOut);
                   eventList = store.GetRawEventsFeed(0, 100).ToList();
                }

                tickCCompleted.Set();
            });

            threadA.Start();
            threadB.Start();
            threadC.Start();

            var db3 = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            db3.Open();
            var eventStore = CreateNewEventStore(db3);

            // before first tick. Assert that there is no events in database
            using (db3.BeginTransaction())
            {
                Assert.That(eventStore.GetRawEventsFeed(0, 100).ToList().Count, Is.EqualTo(0));
            }

            // thA store events set tick 2, wait tick 4, stop before commiting, then thB store events, set tick 3
            tick1.Set();

            // wait for thB. If thread is blocked, wait for 5 seconds
            var tick3Result = tick3.WaitOne(TimeSpan.FromSeconds(5));

            // Wait for thread C to start waiting for events table
            var waitOne = tickCCompleted.WaitOne(TimeSpan.FromSeconds(5));
            if(waitOne)
            {
                Assert.That(eventList, Is.Null, "Reading thread should wait for all transactions to be completed");
            }
            
            // notify thA to continue as we done all asserts
            tick4.Set();

            tickCCompleted.WaitOne(TimeOut);
            Assert.That(eventList, Is.Not.Null, "Reading thread should wait for all transactions to be completed");
            
            // wait for thA to commit
            tick5.WaitOne(TimeOut); 

            // if thB were blocked then it should be unblocked as thA is commit, wait for thB to commit
            if (!tick3Result) tick3.WaitOne(TimeOut);

            List<RawEvent> events;
            using (db3.BeginTransaction())
            {
                events = eventStore.GetRawEventsFeed(0, 100).ToList();
            }

            Assert.That(events.Count, Is.EqualTo(6));

            Assert.That(events[0].GlobalSequence, Is.EqualTo(1));
            Assert.That(events[1].GlobalSequence, Is.EqualTo(2));
            Assert.That(events[2].GlobalSequence, Is.EqualTo(3));
            Assert.That(events[3].GlobalSequence, Is.EqualTo(4));
            Assert.That(events[4].GlobalSequence, Is.EqualTo(5));
            Assert.That(events[5].GlobalSequence, Is.EqualTo(6));
        }

        private PostgresEventStore CreateNewEventStore(NpgsqlConnection connection)
        {
            var eventTypeResolver = new EventTypeResolver();
            eventTypeResolver.RegisterEventDataType(typeof(AccountRegistered));
            eventTypeResolver.RegisterEventDataType(typeof(AccountConfirmed));
            eventTypeResolver.RegisterEventDataType(typeof(AccountLocked));

            var sessionProvider = new Mock<IUnitOfWork>();

            sessionProvider.Setup(x => x.Session)
                .Returns(Mock.Of<ISession>(i => i.Connection == connection));

            return new PostgresEventStore(eventTypeResolver, sessionProvider.Object, Mock.Of<ILogger<PostgresEventStore>>());
        }
    }
}
