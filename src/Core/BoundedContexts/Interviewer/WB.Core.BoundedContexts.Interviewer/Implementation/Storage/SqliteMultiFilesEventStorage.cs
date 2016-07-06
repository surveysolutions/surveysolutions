using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public class SqliteMultiFilesEventStorage : SqliteEventStorage, IInterviewerEventStorage
    {
        private SQLiteConnectionWithLock eventStoreInSingleFile;
        internal readonly Dictionary<Guid, SQLiteConnectionWithLock> connectionByEventSource = new Dictionary<Guid, SQLiteConnectionWithLock>();
        private readonly SqliteSettings settings;

        private readonly ISQLitePlatform sqLitePlatform;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private ITraceListener traceListener;

        private string connectionStringToEventStoreInSingleFile;
        private static readonly Object creatorLock = new Object();
        static readonly Encoding TextEncoding = Encoding.UTF8;

        public SqliteMultiFilesEventStorage(ISQLitePlatform sqLitePlatform,
            ILogger logger,
            ITraceListener traceListener,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings,
            IFileSystemAccessor fileSystemAccessor,
            IEventTypeResolver eventTypeResolver) : base(logger, enumeratorSettings, eventTypeResolver)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
            this.sqLitePlatform = sqLitePlatform;
            this.traceListener = traceListener;

            this.InitializeEventStoreInSingleFile();
        }

        private SQLiteConnectionWithLock CreateConnection(string connectionString)
        {
            var connection = new SQLiteConnectionWithLock(this.sqLitePlatform,
                new SQLiteConnectionString(connectionString, true,
                    new BlobSerializerDelegate(
                        (obj) => TextEncoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None)),
                        (data, type) => JsonConvert.DeserializeObject(TextEncoding.GetString(data, 0, data.Length), type),
                        (type) => true),
                    openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex))
            {
                //TraceListener = traceListener
            };

            connection.CreateTable<EventView>();
            connection.CreateIndex<EventView>(entity => entity.EventId);

            return connection;
        }

        private string GetEventSourceConnectionString(Guid eventSourceId)
            => this.ToSqliteConnectionString(Path.Combine(this.settings.PathToInterviewsDirectory, $"{eventSourceId.FormatGuid()}.sqlite3"));

        private string ToSqliteConnectionString(string pathToDatabase)
            => this.settings.InMemoryStorage ? $"file:{pathToDatabase}?mode=memory" : pathToDatabase;

        private SQLiteConnectionWithLock GetOrCreateConnection(Guid eventSourceId)
        {
            SQLiteConnectionWithLock connection;

            if (!this.connectionByEventSource.TryGetValue(eventSourceId, out connection))
            {
                lock (creatorLock)
                {
                    connection = this.CreateConnection(this.GetEventSourceConnectionString(eventSourceId));
                    this.connectionByEventSource.Add(eventSourceId, connection);
                }
            }

            return connection;
        }

        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion) => this.Read(id, minVersion, null, CancellationToken.None);
        public IEnumerable<CommittedEvent> Read(Guid id, int minVersion, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            IEnumerable<CommittedEvent> events = null;

            var eventSourceFilePath = this.GetEventSourceConnectionString(id);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                events = base.Read(this.GetOrCreateConnection(id), id, minVersion, this.eventSerializer, progress, cancellationToken);
            }

            return events ?? this.ReadFromEventStoreInSingleFile(id, minVersion, progress, cancellationToken) ?? new CommittedEvent[0];
        }

        public CommittedEventStream Store(UncommittedEventStream eventStream)
            => this.StoreToEventStoreInSingleFile(eventStream) ?? base.Store(this.GetOrCreateConnection(eventStream.SourceId), eventStream, this.eventSerializer);

        public void RemoveEventSourceById(Guid interviewId)
        {
            var eventSourceFilePath = this.GetEventSourceConnectionString(interviewId);
            if (this.fileSystemAccessor.IsFileExists(eventSourceFilePath))
            {
                SQLiteConnectionWithLock connection;

                if (this.connectionByEventSource.TryGetValue(interviewId, out connection))
                {
                    connection.Dispose();
                    this.connectionByEventSource.Remove(interviewId);
                }

                this.fileSystemAccessor.DeleteFile(eventSourceFilePath);
            }
            
            this.RemoveFromEventStoreInSingleFile(interviewId);
        }

        public void Dispose()
        {
            this.DisposeEventStoreInSingleFile();

            foreach (var sqLiteConnectionWithLock in this.connectionByEventSource.Values)
            {
                sqLiteConnectionWithLock.Dispose();
            }
        }

        [Obsolete("Since v6.0")]
        private void InitializeEventStoreInSingleFile()
        {
            this.connectionStringToEventStoreInSingleFile = this.ToSqliteConnectionString(Path.Combine(this.settings.PathToDatabaseDirectory, "events-data.sqlite3"));

            if (this.fileSystemAccessor.IsFileExists(this.connectionStringToEventStoreInSingleFile))
                this.eventStoreInSingleFile = this.CreateConnection(this.connectionStringToEventStoreInSingleFile);
        }

        [Obsolete("Since v6.0")]
        private IEnumerable<CommittedEvent> ReadFromEventStoreInSingleFile(Guid id, int minVersion, 
            IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return this.eventStoreInSingleFile != null
                ? base.Read(this.eventStoreInSingleFile, id, minVersion, this.backwardCompatibleEventSerializer, progress, cancellationToken)
                : null;
        }

        [Obsolete("Since v6.0")]
        private CommittedEventStream StoreToEventStoreInSingleFile(UncommittedEventStream eventStream)
            => this.eventStoreInSingleFile != null &&
               base.GetTotalEventsByEventSourceId(this.eventStoreInSingleFile, eventStream.SourceId, 0) > 0
                ? base.Store(this.eventStoreInSingleFile, eventStream, this.backwardCompatibleEventSerializer)
                : null;

        [Obsolete("Since v6.0")]
        private void RemoveFromEventStoreInSingleFile(Guid interviewId)
        {
            if (this.eventStoreInSingleFile == null) return;

            base.RemoveEventSourceById(this.eventStoreInSingleFile, interviewId);
        }

        [Obsolete("Since v6.0")]
        private void DisposeEventStoreInSingleFile()
        {
            if (this.eventStoreInSingleFile == null) return;

            var countOfEventsInSingleFile = this.GetTotalEventsInEventStoreInSingleFile();

            this.eventStoreInSingleFile.Dispose();

            if (countOfEventsInSingleFile == 0)
                this.fileSystemAccessor.DeleteFile(this.connectionStringToEventStoreInSingleFile);
        }

        [Obsolete("Since v6.0")]
        protected int GetTotalEventsInEventStoreInSingleFile()
        {
            using (this.eventStoreInSingleFile.Lock())
            {
                return this.eventStoreInSingleFile.Table<EventView>().Count();
            }
        }
    }
}
