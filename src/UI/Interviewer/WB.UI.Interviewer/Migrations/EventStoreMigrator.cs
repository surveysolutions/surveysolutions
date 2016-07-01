using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ncqrs.Eventing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite.Net;
using SQLite.Net.Interop;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Migrations
{
    [Obsolete("v 5.10.10")]
    public class EventStoreMigrator
    {
        private ISQLitePlatform sqLitePlatform;
        private IEnumeratorSettings enumeratorSettings;
        private IAsyncPlainStorage<InterviewView> interviewViewRepository;

        private SQLiteConnectionWithLock connection = null;

        static readonly Encoding TextEncoding = Encoding.UTF8;
        
        private string renamedEventStoreDBPath;
        private string eventStoreDBPath;

        public EventStoreMigrator(ISQLitePlatform sqLitePlatform,
            SqliteSettings settings,
            IEnumeratorSettings enumeratorSettings,
            IAsyncPlainStorage<InterviewView> interviewViewRepository)
        {
            this.enumeratorSettings = enumeratorSettings;
            this.sqLitePlatform = sqLitePlatform;
            this.interviewViewRepository = interviewViewRepository;

            this.eventStoreDBPath = Path.Combine(settings.PathToDatabaseDirectory, "events-data.sqlite3");
            this.renamedEventStoreDBPath = Path.Combine(settings.PathToDatabaseDirectory, "events-data-old.sqlite3");
        }

        [Obsolete("v 5.10.10")]
        public bool IsMigrationShouldBeDone()
        {
            return File.Exists(eventStoreDBPath);
        }

        [Obsolete("v 5.10.10")]
        public void Migrate(IInterviewerEventStorage storage)
        {
            if(storage == null) 
                throw new Exception("storage is incorrect");

            if (!File.Exists(eventStoreDBPath))
                return;

            if (connection == null)
                this.connection = this.GetConnection();

            var bulkSize = Math.Min(this.enumeratorSettings.EventChunkSize, 250);
            var localInterviews = this.interviewViewRepository.LoadAll().ToList();

            foreach (var interview in localInterviews)
            {
                List<UncommittedEvent> bulk;
                int lastReadEventSequence = storage.GetMaxEventSequenceById(interview.InterviewId);
                do
                {
                    var startSequenceInTheBulk = lastReadEventSequence;
                    var endSequenceInTheBulk = startSequenceInTheBulk + bulkSize;

                    using (connection.Lock())
                        bulk = connection
                            .Table<EventView>()
                            .Where(eventView
                                => eventView.EventSourceId == interview.InterviewId
                                && eventView.EventSequence > startSequenceInTheBulk
                                && eventView.EventSequence < endSequenceInTheBulk)
                            .OrderBy(x => x.EventSequence)
                            .Select(y => ToUnCommitedEvent(y, startSequenceInTheBulk))
                            .ToList();

                    if (bulk.Count <= 0)
                        continue;
                    
                    lastReadEventSequence = bulk.Max(x => x.EventSequence);
                    var eventStream = new UncommittedEventStream(string.Empty, bulk);
                    storage.Store(eventStream);
                    
                } while (bulk.Count > 0);
            }

            connection.Dispose();
            connection = null;

            if (File.Exists(eventStoreDBPath))
                File.Move(eventStoreDBPath, renamedEventStoreDBPath);
        }

        private SQLiteConnectionWithLock GetConnection()
        {
            return new SQLiteConnectionWithLock(this.sqLitePlatform,
                new SQLiteConnectionString(eventStoreDBPath, true,
                    new BlobSerializerDelegate(
                        (obj) => TextEncoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None)),
                        (data, type) =>
                            JsonConvert.DeserializeObject(TextEncoding.GetString(data, 0, data.Length), type),
                        (type) => true),
                    openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex));
        }

        private static UncommittedEvent ToUnCommitedEvent(EventView storedEvent, int initialVersion)
        {
            return new UncommittedEvent(
                eventIdentifier: storedEvent.EventId,
                eventSourceId: storedEvent.EventSourceId,
                eventSequence: storedEvent.EventSequence,
                eventTimeStamp: storedEvent.DateTimeUtc,
                initialVersionOfEventSource: initialVersion,
                payload: JsonConvert.DeserializeObject<WB.Core.Infrastructure.EventBus.IEvent>(storedEvent.JsonEvent, JsonSerializerSettings()));
        }


        internal static Func<JsonSerializerSettings> JsonSerializerSettings = () => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Binder = new CapiAndMainCoreToInterviewerAndSharedKernelsBinder()
        };

        [Obsolete("Resolves old namespaces. Cuold be dropped after incompatibility shift with the next version.")]
        internal class CapiAndMainCoreToInterviewerAndSharedKernelsBinder : DefaultSerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                var oldCapiAssemblyName = "WB.UI.Capi";
                var newCapiAssemblyName = "WB.Core.BoundedContexts.Interviewer";
                var newQuestionsAssemblyName = "WB.Core.SharedKernels.Questionnaire";
                var oldMainCoreAssemblyName = "Main.Core";

                if (String.Equals(assemblyName, oldCapiAssemblyName, StringComparison.Ordinal))
                {
                    assemblyName = newCapiAssemblyName;
                }
                else if (String.Equals(assemblyName, oldMainCoreAssemblyName, StringComparison.Ordinal))
                {
                    if (oldMainCoreTypeMap.ContainsKey(typeName))
                        assemblyName = oldMainCoreTypeMap[typeName];
                    else
                        assemblyName = newQuestionsAssemblyName;
                }

                return base.BindToType(assemblyName, typeName);
            }

            private readonly Dictionary<string, string> oldMainCoreTypeMap = new Dictionary<string, string>()
            {
                {"Main.Core.Events.AggregateRootEvent", "WB.Core.Infrastructure"},
                {"Main.Core.Events.User.NewUserCreated", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserChanged", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserLocked", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserLockedBySupervisor", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserUnlocked", "WB.Core.SharedKernels.DataCollection"},
                {"Main.Core.Events.User.UserUnlockedBySupervisor", "WB.Core.SharedKernels.DataCollection"},
            };
        }
    }
}