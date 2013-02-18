// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SQLiteEventStore.cs" company="">
//   
// </copyright>
// <summary>
//   The sq lite event store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    using System;
    using System.Collections.Generic;

    using Android.Content;
    using Android.Database;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage;

    using Newtonsoft.Json;

    /// <summary>
    /// The sq lite event store.
    /// </summary>
    public class SQLiteEventStore : IStreamableEventStore
    {
        #region Fields

        /// <summary>
        /// The _context.
        /// </summary>
        private readonly Context _context;

        /// <summary>
        /// The _database helper.
        /// </summary>
        private readonly DataBaseHelper _databaseHelper;

        /// <summary>
        /// The _sq lite context.
        /// </summary>
        private readonly SQLiteContext _sqLiteContext;

        #endregion

        // private readonly IPropertyBagConverter _propertyBagConverter;
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteEventStore"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public SQLiteEventStore(Context context)
        {
            this._context = context;

            this._databaseHelper = new DataBaseHelper(context);

            this._sqLiteContext = new SQLiteContext(this._databaseHelper);

            // _propertyBagConverter = new PropertyBagConverter();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clear db.
        /// </summary>
        public void ClearDB()
        {
            this._sqLiteContext.ExecuteWithTransaction(
                () => this._databaseHelper.WritableDatabase.Delete("Events", null, null));
        }

        /// <summary>
        /// The get all events.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            ICursor cursor = this._databaseHelper.ReadableDatabase.RawQuery(Query.SelectAllEventsQuery(), null);

            var events = new List<CommittedEvent>();

            int commitIdIndex = cursor.GetColumnIndex("CommitId");
            int eventIdIndex = cursor.GetColumnIndex("EventId");
            int eventSourceIndex = cursor.GetColumnIndex("EventSourceId");
            int sequenceIndex = cursor.GetColumnIndex("Sequence");
            int timestampIndex = cursor.GetColumnIndex("TimeStamp");
            int dataIndex = cursor.GetColumnIndex("Data");

            while (cursor.MoveToNext())
            {
                Guid commitId = Guid.Parse(cursor.GetString(commitIdIndex));
                Guid eventSourceId = Guid.Parse(cursor.GetString(eventSourceIndex));
                Guid eventId = Guid.Parse(cursor.GetString(eventIdIndex));
                long sequenceId = cursor.GetLong(sequenceIndex);
                DateTime eventTimeStamp = DateTime.FromBinary(cursor.GetLong(timestampIndex));
                object data = this.GetObject(cursor.GetString(dataIndex));

                var @event = new CommittedEvent(
                    commitId, eventId, eventSourceId, sequenceId, eventTimeStamp, data, new Version(1, 0));

                events.Add(@event);
            }

            return @events;
        }

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<CommittedEvent> GetEventStream()
        {
            return this.GetAllEvents();
        }

        /// <summary>
        /// The read from.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="minVersion">
        /// The min version.
        /// </param>
        /// <param name="maxVersion">
        /// The max version.
        /// </param>
        /// <returns>
        /// The <see cref="CommittedEventStream"/>.
        /// </returns>
        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            ICursor cursor =
                this._databaseHelper.ReadableDatabase.RawQuery(
                    Query.SelectAllEventsByGuidQuery(id, minVersion, maxVersion), null);

            var events = new List<CommittedEvent>();

            int commitIdIndex = cursor.GetColumnIndex("CommitId");
            int eventIdIndex = cursor.GetColumnIndex("EventId");
            int sequenceIndex = cursor.GetColumnIndex("Sequence");
            int timestampIndex = cursor.GetColumnIndex("TimeStamp");
            int dataIndex = cursor.GetColumnIndex("Data");

            while (cursor.MoveToNext())
            {
                Guid commitId = Guid.Parse(cursor.GetString(commitIdIndex));
                Guid eventId = Guid.Parse(cursor.GetString(eventIdIndex));
                long sequenceId = cursor.GetLong(sequenceIndex);
                DateTime eventTimeStamp = DateTime.FromBinary(cursor.GetLong(timestampIndex));
                object data = this.GetObject(cursor.GetString(dataIndex));

                var @event = new CommittedEvent(
                    commitId, eventId, id, sequenceId, eventTimeStamp, data, new Version(1, 0));

                events.Add(@event);
            }

            return new CommittedEventStream(id, events);
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        public void Store(UncommittedEventStream events)
        {
            this._sqLiteContext.ExecuteWithTransaction(() => this.SaveEvents(events));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get json data.
        /// </summary>
        /// <param name="payload">
        /// The payload.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetJsonData(object payload)
        {
            return JsonConvert.SerializeObject(
                payload, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }

        /// <summary>
        /// The get object.
        /// </summary>
        /// <param name="json">
        /// The json.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private object GetObject(string json)
        {
            return JsonConvert.DeserializeObject(
                json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }

        /// <summary>
        /// The save events.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        private void SaveEvents(UncommittedEventStream events)
        {
            foreach (UncommittedEvent @event in events)
            {
                string data = this.GetJsonData(@event.Payload);

                var values = new ContentValues();
                values.Put("CommitId", @event.CommitId.ToString());
                values.Put("EventSourceId", @event.EventSourceId.ToString());
                values.Put("EventId", @event.EventIdentifier.ToString());
                values.Put("Sequence", @event.EventSequence);
                values.Put("Timestamp", @event.EventTimeStamp.Ticks);
                values.Put("Data", data);
                values.Put("Name", @event.Payload.GetType().FullName);

                this._databaseHelper.WritableDatabase.Insert("Events", null, values);
            }
        }

        #endregion

        // private byte[] GetBinaryData(object payload)
        // {
        // var bag = _propertyBagConverter.Convert(payload);
        // using (var stream = new MemoryStream())
        // {
        // var formatter = new BinaryFormatter();
        // formatter.Serialize(stream, bag);
        // stream.Position = 0;
        // return stream.ToArray();
        // }
        // }

        // private object GetObject(byte[] data)
        // {
        // using (var stream = new MemoryStream(data))
        // {
        // var formatter = new BinaryFormatter();
        // var bag = (PropertyBag)formatter.Deserialize(stream);
        // return _propertyBagConverter.Convert(bag);
        // }
        // }
    }
}