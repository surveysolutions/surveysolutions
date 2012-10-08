using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Android.Content;
using Android.Database.Sqlite;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
	public class SQLiteEventStore : IEventStore
	{
		private readonly Context _context;
		private readonly SQLiteContext _sqLiteContext;
		private readonly DataBaseHelper _databaseHelper;
		private readonly IPropertyBagConverter _propertyBagConverter;

		public SQLiteEventStore(Context context)
		{
			_context = context;

			_databaseHelper = new DataBaseHelper(context);

			_sqLiteContext = new SQLiteContext(_databaseHelper);

			_propertyBagConverter = new PropertyBagConverter();
		}

		public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
		{
			var cursor = _databaseHelper
				.ReadableDatabase
				.RawQuery(Query.SelectAllEventsFromQuery(id, minVersion, maxVersion), null);

			var events = new List<CommittedEvent>();

			var eventIdIndex = cursor.GetColumnIndex("EventId");
			var sequenceIndex = cursor.GetColumnIndex("Sequence");
			var timestampIndex = cursor.GetColumnIndex("TimeStamp");
			var dataIndex = cursor.GetColumnIndex("Data");

			while (cursor.MoveToNext())
			{
				var eventId = Guid.Parse(cursor.GetString(eventIdIndex));
				var sequenceId = cursor.GetLong(sequenceIndex);
				var eventTimeStamp = DateTime.FromBinary(cursor.GetLong(timestampIndex));
				var data = GetObject(cursor.GetBlob(dataIndex));

				var @event = new CommittedEvent(Guid.Empty,
				                                eventId,
				                                id,
				                                sequenceId,
				                                eventTimeStamp,
				                                data,
				                                new Version(1, 0));

				events.Add(@event);
			}
			return new CommittedEventStream(id, events);
		}

		public void Store(UncommittedEventStream events)
		{
			_sqLiteContext.ExecuteWithTransaction(() => SaveEvents(events));
		}

		private void SaveEvents(UncommittedEventStream events)
		{
			foreach (var @event in events)
			{
				var data = GetBinaryData(@event.Payload);

				var values = new ContentValues();
				values.Put("EventSourceId", @event.EventSourceId.ToString());
				values.Put("EventId", @event.EventIdentifier.ToString());
				values.Put("Sequence", @event.EventSequence);
				values.Put("Timestamp", @event.EventTimeStamp.Ticks);
				values.Put("Data", data);
				values.Put("Name", @event.Payload.GetType().FullName);

				_databaseHelper.WritableDatabase.Insert("Events", null, values);
			}
		}

		private byte[] GetBinaryData(object payload)
		{
			var bag = _propertyBagConverter.Convert(payload);
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, bag);
				stream.Position = 0;
				return stream.ToArray();
			}
		}

		private object GetObject(byte[] data)
		{
			using (var stream = new MemoryStream(data))
			{
				var formatter = new BinaryFormatter();
				var bag = (PropertyBag)formatter.Deserialize(stream);
				return _propertyBagConverter.Convert(bag);
			}
		}
	}
}
