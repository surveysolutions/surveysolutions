using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Plugins;
using Cirrious.MvvmCross.IoC;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Moq;
using Ncqrs.Eventing.Sourcing;
using SQLite;

namespace Ncqrs.Eventing.Storage.SQLite.Tests
{
	using System;
	using System.IO;
	using System.Linq;
	using Fakes;
	using FluentAssertions;
	using NUnit.Framework;

   

    [TestFixture]
    public class MvvmCrossSqliteEventStoreTests
	{


        [SetUp]
        public void Setup()
        {
            Teardown();
           
            if (MvxServiceProvider.Instance == null)
            {
                Mock<IMvxPluginManager> pluginManagerCache = new Mock<IMvxPluginManager>();
                var provider= new MvxServiceProvider(new MvxSimpleIoCServiceProvider());
                provider.RegisterServiceInstance<IMvxPluginManager>(pluginManagerCache.Object);
                
            }
            Mock<ISQLiteConnectionFactory> sqlFactoryMock = new Mock<ISQLiteConnectionFactory>();
            MvxServiceProvider.Instance.RegisterServiceInstance<ISQLiteConnectionFactory>(sqlFactoryMock.Object);
            ISQLiteConnection sqlConnection = new SQLiteConnection(DBPath);
            sqlFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(sqlConnection);
            _store = new MvvmCrossSqliteEventStore(TestDataBaseName);

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
            if (File.Exists(DBPath))
                
                File.Delete(DBPath);
       //     SqliteTestsContext.Context.DeleteDatabase(TestDataBaseName);
        }

        private MvvmCrossSqliteEventStore _store;

		[Test]
		public void Save_SmokeTest()
		{
			var id = Guid.NewGuid();

			var stream = GetUncommiteEventStream(id);

			_store.Store(stream);
		}

		private UncommittedEventStream GetUncommiteEventStream(Guid id)
		{
			var stream = new UncommittedEventStream(Guid.NewGuid());
			var sequenceCounter = 0;
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow, new CustomerCreatedEvent("Foo", 35),
									 new Version(1, 0)));
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow,
									 new CustomerNameChanged("Name" + sequenceCounter), new Version(1, 0)));
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, sequenceCounter++, 0, DateTime.UtcNow,
									 new CustomerNameChanged("Name" + sequenceCounter), new Version(1, 0)));

			return stream;
		}

		[Test]
		public void Retrieving_all_events_should_return_the_same_as_added()
		{
			var id = Guid.NewGuid();

			var stream = GetUncommiteEventStream(id);

			_store.Store(stream);

			var result = _store.ReadFrom(id, long.MinValue, long.MaxValue);

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

			var stream = new UncommittedEventStream(Guid.NewGuid());
			stream.Append(
				new UncommittedEvent(Guid.NewGuid(), id, 1, 0, utcNow, new CustomerCreatedEvent("Foo", 35),
				                     new Version(1, 0)));

			_store.Store(stream);

			var commitedStream = _store.ReadFrom(id, long.MinValue, long.MaxValue);

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
			var sequenceCounter = 0;

			var stream = GetUncommiteEventStream(id);

			_store.Store(stream);

			var commitedEvents = _store.ReadFrom(id, 2, 3);

			commitedEvents.Count().Should().Be(1);
		}

		[Test]
		public void store_should_return_all_events_from_all_sources()
		{
			var firstId = Guid.NewGuid();
			var firstStream = GetUncommiteEventStream(firstId);
			_store.Store(firstStream);

			var secondId = Guid.NewGuid();
			var secondStream = GetUncommiteEventStream(secondId);
			_store.Store(secondStream);

			var thirdId = Guid.NewGuid();
			var thirdStream = GetUncommiteEventStream(thirdId);
			_store.Store(thirdStream);

		    var allEvents = _store.GetEventStream();
			allEvents.Count().Should().Be(9);

			allEvents.GroupBy(e => e.EventSourceId)
				.Count().Should().Be(3);
		}

		[Test]
		public void store_should_retriev_events_by_source_and_version()
		{
			var firstId = Guid.NewGuid();
			var firstStream = GetUncommiteEventStream(firstId);
			_store.Store(firstStream);

			var secondId = Guid.NewGuid();
			var secondStream = GetUncommiteEventStream(secondId);
			_store.Store(secondStream);

			var thirdId = Guid.NewGuid();
			var thirdStream = GetUncommiteEventStream(thirdId);
			_store.Store(thirdStream);

			var events = _store.ReadFrom(secondId, minVersion: 0, maxVersion: 1);

			Assert.That(events.Count(), Is.EqualTo(2));

			events.First().EventSourceId.Should().Be(secondId);
		}
	}
}