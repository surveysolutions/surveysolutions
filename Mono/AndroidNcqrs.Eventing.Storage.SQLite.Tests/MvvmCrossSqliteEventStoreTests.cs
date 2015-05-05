using System;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using FluentAssertions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage.SQLite.Tests.Fakes;
using NUnit.Framework;

namespace AndroidNcqrs.Eventing.Storage.SQLite.Tests
{
    [TestFixture]
    public class MvvmCrossSqliteEventStoreTests
	{
        [SetUp]
        public void Setup()
        {
            this.Teardown();
    
            var sqlFactoryMock = new SqlFactoryMock(DBPath);

            Mvx.RegisterSingleton(sqlFactoryMock);
            this._store = new MvvmCrossSqliteEventStore(TestDataBaseName);

        }

        public string DBPath
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(path, TestDataBaseName);
            }
        }

        private const string TestDataBaseName = "test_db";

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(this.DBPath))
                
                File.Delete(this.DBPath);
       //     SqliteTestsContext.Context.DeleteDatabase(TestDataBaseName);
        }

        private MvvmCrossSqliteEventStore _store;

		[Test]
		public void Save_SmokeTest()
		{
			var id = Guid.NewGuid();

			var stream = this.GetUncommiteEventStream(id);

			this._store.Store(stream);
		}

		private UncommittedEventStream GetUncommiteEventStream(Guid id)
		{
			var stream = new UncommittedEventStream(Guid.NewGuid(), null);
			var sequenceCounter = 0;
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerCreatedEvent("Foo", 35)));
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow,
									 new CustomerNameChanged("Name" + sequenceCounter)));
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow,
									 new CustomerNameChanged("Name" + sequenceCounter)));

			return stream;
		}

		[Test]
		public void Retrieving_all_events_should_return_the_same_as_added()
		{
			var id = Guid.NewGuid();

			var stream = this.GetUncommiteEventStream(id);

			this._store.Store(stream);

			var result = this._store.ReadFrom(id, int.MinValue, int.MaxValue);

			result.Count().Should().Be(stream.Count());

			result.First().EventIdentifier.Should().Be(stream.First().EventIdentifier);

			var streamList = stream.ToList();
			var resultList = result.ToList();

			for (int i = 0; i < resultList.Count; i++)
			{
				Assert.IsTrue(AreEqual(streamList[i], resultList[i]));
			}
		}

		private static bool AreEqual(UncommittedEvent uncommitted, CommittedEvent committed)
		{
			return uncommitted.EventIdentifier == committed.EventIdentifier
			       && uncommitted.EventSourceId == committed.EventSourceId
			       && uncommitted.Payload.Equals(committed.Payload)
			       && uncommitted.EventTimeStamp == committed.EventTimeStamp
			       && uncommitted.EventSequence == committed.EventSequence;
		}

		[Test]
		public void Retrieved_event_should_having_identical_timestamp_as_persisted()
		{
			var id = Guid.NewGuid();
			var utcNow = DateTime.UtcNow; //.Date.AddHours(9).AddTicks(-1);

			var stream = new UncommittedEventStream(Guid.NewGuid(), null);
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, 1, 0, utcNow, new CustomerCreatedEvent("Foo", 35)));

			this._store.Store(stream);

			var commitedStream = this._store.ReadFrom(id, int.MinValue, int.MaxValue);

			commitedStream.Count().Should().BeGreaterThan(0);

			foreach (var @event in commitedStream)
			{
				@event.EventTimeStamp.Should().Be(utcNow);
			}
		}

		[Test]
		public void events_should_be_retrieved_correctly_by_version()
		{
			var id = Guid.NewGuid();

		    var stream = this.GetUncommiteEventStream(id);

			this._store.Store(stream);

			var commitedEvents = this._store.ReadFrom(id, 2, 3);

			commitedEvents.Count().Should().Be(1);
		}

		[Test]
		public void store_should_return_all_events_from_all_sources()
		{
			var firstId = Guid.NewGuid();
			var firstStream = this.GetUncommiteEventStream(firstId);
			this._store.Store(firstStream);

			var secondId = Guid.NewGuid();
			var secondStream = this.GetUncommiteEventStream(secondId);
			this._store.Store(secondStream);

			var thirdId = Guid.NewGuid();
			var thirdStream = this.GetUncommiteEventStream(thirdId);
			this._store.Store(thirdStream);
		}

		[Test]
		public void store_should_retriev_events_by_source_and_version()
		{
			var firstId = Guid.NewGuid();
			var firstStream = this.GetUncommiteEventStream(firstId);
			this._store.Store(firstStream);

			var secondId = Guid.NewGuid();
			var secondStream = this.GetUncommiteEventStream(secondId);
			this._store.Store(secondStream);

			var thirdId = Guid.NewGuid();
			var thirdStream = this.GetUncommiteEventStream(thirdId);
			this._store.Store(thirdStream);

			var events = this._store.ReadFrom(secondId, minVersion: 0, maxVersion: 1);

			Assert.That(events.Count(), Is.EqualTo(2));

			events.First().EventSourceId.Should().Be(secondId);
		}
	}
}