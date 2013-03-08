// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SQLiteEventStore.cs" company="">
//   
// </copyright>
// <summary>
//   The sq lite event store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Data;
using System.Linq;
using Mono.Data.Sqlite;

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

   /*     /// <summary>
        /// The _sq lite context.
        /// </summary>
        private readonly SQLiteContext _sqLiteContext;*/

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
            //   this._sqLiteContext = new SQLiteContext(this._databaseHelper);

            // _propertyBagConverter = new PropertyBagConverter();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clear db.
        /// </summary>
        public void ClearDB()
        {
            this._databaseHelper.ExecuteCommand(Query.ClearTable());
          /*  this._sqLiteContext.ExecuteWithTransaction(
                () => this._databaseHelper.WritableDatabase.Delete("Events", null, null));*/
        }
        protected  IEnumerable<CommittedEvent> GetEvents(string query)
        {
            var cursor = this._databaseHelper.QueryData(query);

            var events = new List<CommittedEvent>();
            try
            {
                foreach (object[] objectse in cursor)
                {
                    Guid commitId = Guid.Parse(objectse[0].ToString());
                    Guid eventSourceId = Guid.Parse(objectse[1].ToString());
                    Guid eventId = Guid.Parse(objectse[2].ToString());
                    //TimeStamp, Data, Sequence
                    long timestamp = Convert.ToInt64(objectse[3]);
                    DateTime eventTimeStamp = DateTime.FromBinary(timestamp);
                    object data = this.GetObject(objectse[4].ToString());
                    long sequenceId = Convert.ToInt64(objectse[5]);
                    var @event = new CommittedEvent(
                        commitId, eventId, eventSourceId, sequenceId, eventTimeStamp, data, new Version(1,1,1,1));

                    events.Add(@event);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return @events;
        }

        /// <summary>
        /// The get all events.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<CommittedEvent> GetAllEvents()
        {
            return GetEvents(Query.SelectAllEventsQuery());
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
            var events = GetEvents(Query.SelectAllEventsByGuidQuery(id, minVersion, maxVersion));
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
            this._databaseHelper.ExecuteCommandsInTrnsactionScope(SaveEvents(events));
       //     this._sqLiteContext.ExecuteWithTransaction(() => this.SaveEvents(events));
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
        private IList<CommandWithParams> SaveEvents(UncommittedEventStream events)
        {
            var result = new List<CommandWithParams>();
            foreach (UncommittedEvent @event in events)
            {
                string data = this.GetJsonData(@event.Payload);

               /* var values = new ContentValues();
                values.Put("CommitId", @event.CommitId.ToString());
                values.Put("EventSourceId", @event.EventSourceId.ToString());
                values.Put("EventId", @event.EventIdentifier.ToString());
                values.Put("Sequence", @event.EventSequence);
                values.Put("Timestamp", @event.EventTimeStamp.Ticks);
                values.Put("Data", data);
                values.Put("Name", @event.Payload.GetType().FullName);*/
                var command = new CommandWithParams(Query.InsertNewEventQuery());
                //[CommitId], [EventSourceId], [EventId], [Name], [Data], [Sequence], [TimeStamp]
                command.Paramaters.Add(new SqliteParameter("@commitId", @event.CommitId.ToString()));
                command.Paramaters.Add(new SqliteParameter("@eventSourceId",@event.EventSourceId.ToString()));
                command.Paramaters.Add(new SqliteParameter("@eventId", @event.EventIdentifier.ToString()));
                command.Paramaters.Add(new SqliteParameter("@name", @event.Payload.GetType().FullName));
                command.Paramaters.Add(new SqliteParameter("@data", data));
                command.Paramaters.Add(new SqliteParameter("@timestamp", @event.EventTimeStamp.Ticks));
                command.Paramaters.Add(new SqliteParameter("@sequence", @event.EventSequence));
                result.Add(command);
               
                //      this._databaseHelper.WritableDatabase.Insert("Events", null, values);
            }
            return result;
        }

        #endregion

    }

    public class CommandWithParams
    {
        public CommandWithParams(string command)
        {
            Command = command;
            this.Paramaters = new List<SqliteParameter>();
        }

        public string Command { get; private set; }
        public IList<SqliteParameter> Paramaters { get; private set; }
    }
}