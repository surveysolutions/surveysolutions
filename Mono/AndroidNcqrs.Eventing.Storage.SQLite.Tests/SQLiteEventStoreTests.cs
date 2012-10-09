﻿using AndroidNcqrs.Eventing.Storage.SQLite;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage.SQLite.Tests
{
	using System;
	using System.IO;
	using System.Linq;
	using Fakes;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class SQLiteEventStoreTests
	{
		[SetUp]
		public void Setup()
		{
			TestsContext.Context.DeleteDatabase(DataBaseHelper.DATABASE_NAME);
			_store = new SQLiteEventStore(TestsContext.CurrentContext);
		}

		[TearDown]
		public void Teardown()
		{
			TestsContext.Context.DeleteDatabase(DataBaseHelper.DATABASE_NAME);
		}

		private SQLiteEventStore _store;

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

			var allEvents = _store.GetAllEvents();
			allEvents.Count().Should().Be(9);

			allEvents.GroupBy(e => e.EventSourceId)
				.Count().Should().Be(3);
		}
	}
}