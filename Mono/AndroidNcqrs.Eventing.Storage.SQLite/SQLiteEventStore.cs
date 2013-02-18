using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Android.Content;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class SQLiteEventStore : IEventStore
	{
		private readonly Context _context;
		private readonly SQLiteContext _sqLiteContext;
		private readonly DataBaseHelper _databaseHelper;
		//private readonly IPropertyBagConverter _propertyBagConverter;

		public SQLiteEventStore(Context context)
		{
			_context = context;

			_databaseHelper = new DataBaseHelper(context);

			_sqLiteContext = new SQLiteContext(_databaseHelper);

			//_propertyBagConverter = new PropertyBagConverter();
		}

		#region Public methods
		public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
		{
			var cursor = _databaseHelper
				.ReadableDatabase
				.RawQuery(Query.SelectAllEventsByGuidQuery(id, minVersion, maxVersion), null);

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
				var data = GetObject(cursor.GetString(dataIndex));

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
        public void ClearDB()
        {
            _sqLiteContext.ExecuteWithTransaction(() => _databaseHelper.WritableDatabase.Delete("Events", null, null));
        }
		public IEnumerable<CommittedEvent> GetAllEvents()
		{
			var cursor = _databaseHelper
				.ReadableDatabase
				.RawQuery(Query.SelectAllEventsQuery(), null);

			var events = new List<CommittedEvent>();

			var eventIdIndex = cursor.GetColumnIndex("EventId");
			var eventSourceIndex = cursor.GetColumnIndex("EventSourceId");
			var sequenceIndex = cursor.GetColumnIndex("Sequence");
			var timestampIndex = cursor.GetColumnIndex("TimeStamp");
			var dataIndex = cursor.GetColumnIndex("Data");

			while (cursor.MoveToNext())
			{
				var eventSourceId = Guid.Parse(cursor.GetString(eventSourceIndex));
				var eventId = Guid.Parse(cursor.GetString(eventIdIndex));
				var sequenceId = cursor.GetLong(sequenceIndex);
				var eventTimeStamp = DateTime.FromBinary(cursor.GetLong(timestampIndex));
				var data = GetObject(cursor.GetString(dataIndex));

				var @event = new CommittedEvent(Guid.Empty,
												eventId,
												eventSourceId,
												sequenceId,
												eventTimeStamp,
												data,
												new Version(1, 0));

				events.Add(@event);
			}
			return @events;
		}
		#endregion

		#region Private methods
		private void SaveEvents(UncommittedEventStream events)
		{
			foreach (var @event in events)
			{
				var data = GetJsonData(@event.Payload);

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
        

	    #region DataConverter

		private string GetJsonData(object payload)
		{
			return JsonConvert.SerializeObject(payload, Formatting.None, 
				new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects});
		}

		private object GetObject(string json)
		{
			return JsonConvert.DeserializeObject(json,
				new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects});
		}

		//private byte[] GetBinaryData(object payload)
		//{
		//    var bag = _propertyBagConverter.Convert(payload);
		//    using (var stream = new MemoryStream())
		//    {
		//        var formatter = new BinaryFormatter();
		//        formatter.Serialize(stream, bag);
		//        stream.Position = 0;
		//        return stream.ToArray();
		//    }
		//}

		//private object GetObject(byte[] data)
		//{
		//    using (var stream = new MemoryStream(data))
		//    {
		//        var formatter = new BinaryFormatter();
		//        var bag = (PropertyBag)formatter.Deserialize(stream);
		//        return _propertyBagConverter.Convert(bag);
		//    }
		//}
		#endregion

		#endregion
	}
}
