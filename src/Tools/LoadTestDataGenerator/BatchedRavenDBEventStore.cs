using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace LoadTestDataGenerator
{
    using System.Net;
    using Ncqrs.Eventing.Storage.RavenDB.RavenIndexes;
    using Ncqrs.Eventing.Storage.RavenDB;

    using Raven.Client.Indexes;
     using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Raven.Client;
    using Raven.Client.Document;

    using ConcurrencyException = Ncqrs.Eventing.Storage.ConcurrencyException;

    public class BatchedRavenDBEventStore: IStreamableEventStore
    {
        #region Fields

        /// <summary>
        /// The _document store.
        /// </summary>
        protected readonly IDocumentStore DocumentStore;

        /// <summary>
        /// The use async save.
        /// </summary>
        private bool useAsyncSave = false; // research: in the embedded mode true is not valid.

        /// <summary>
        /// PageSize for loading by chunk
        /// </summary>
        private readonly int pageSize = 1024;

        /// <summary>
        /// PageSize for loading by chunk
        /// </summary>
        private readonly int timeout = 120;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Ncqrs.Eventing.Storage.RavenDB.RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="ravenUrl">
        /// The raven url.
        /// </param>
        /// <param name="pageSize"></param>
        public BatchedRavenDBEventStore(string ravenUrl, int pageSize)
        {
            this.DocumentStore = new DocumentStore { Url = ravenUrl, Conventions = CreateConventions()}.Initialize();
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, DocumentStore);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ncqrs.Eventing.Storage.RavenDB.RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="externalDocumentStore">
        /// The external document store.
        /// </param>
        /// <param name="pageSize"></param>
        public BatchedRavenDBEventStore(DocumentStore externalDocumentStore, int pageSize)
        {
            externalDocumentStore.Conventions = CreateConventions();
            this.DocumentStore = externalDocumentStore;
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, DocumentStore);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to committed event.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// The <see cref="CommittedEvent"/>.
        /// </returns>
        public static CommittedEvent ToCommittedEvent(StoredEvent x)
        {
            return new CommittedEvent(
                x.CommitId, x.EventIdentifier, x.EventSourceId, x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
        }

        /// <summary>
        /// The get event stream.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IEnumerable<CommittedEvent> GetEventStream()
        {
            var retval = new List<CommittedEvent>();
            
            List<UniqueEventsResults> aggregateRoots = Enumerable.Empty<UniqueEventsResults>().ToList();
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    List<UniqueEventsResults> chunk = session
                        .Query<StoredEvent, UniqueEventsIndex>()
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                        .AsProjection<UniqueEventsResults>().OrderBy(x=>x.EventTimeStamp)
                        .Skip(page*pageSize)
                        .Take(pageSize)
                        .ToList();

                    if (chunk.Count == 0)
                    {
                        break;
                    }

                    aggregateRoots.AddRange(chunk);
                    page++;
                }
            }

            foreach (UniqueEventsResults uniqueEventsResultse in aggregateRoots)
            {
                retval.AddRange(
                    ReadFrom(uniqueEventsResultse.EventSourceId, uniqueEventsResultse.LastSnapshot, long.MaxValue)
                        .ToList());
            }

            return retval/*.OrderBy(e=>e.EventTimeStamp)*/;
            //  return from chunk in this.GetStreamByChunk() from item in chunk select ToCommittedEvent(item);

        }


        public virtual CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var storedEvents =
                this.AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion);
            return new CommittedEventStream(id, storedEvents.Select(ToCommittedEvent));

            // }
        }

        private static readonly List<StoredEvent> Cache = new List<StoredEvent>(128);

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="eventStream">
        /// The event stream.
        /// </param>
        /// <exception cref="Storage.ConcurrencyException">
        /// </exception>
        public void Store(UncommittedEventStream eventStream)
        {
            try
            {
                Cache.AddRange(eventStream.Select(uncommittedEvent => ToStoredEvent(eventStream.CommitId, uncommittedEvent)));

                if (Cache.Count >= 64)
                {
                    this.StoreCache();

                    Cache.Clear();
                }
            }
            catch (Raven.Abstractions.Exceptions.ConcurrencyException)
            {
                Guid sourceId = Guid.Empty;
                long version = 0;
                if (eventStream.HasSingleSource)
                {
                    sourceId = eventStream.SourceId;
                    version = eventStream.Sources.Single().CurrentVersion;
                }

                throw new ConcurrencyException(sourceId, version);
            }
        }

        public void StoreCache()
        {
            if (this.useAsyncSave)
            {
                using (IAsyncDocumentSession session = this.DocumentStore.OpenAsyncSession())
                {
                    //session.Advanced.UseOptimisticConcurrency = true;
                    foreach (StoredEvent storedEvent in Cache)
                    {
                        session.StoreAsync(storedEvent);
                    }

                    session.SaveChangesAsync();
                }
            }
            else
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    //session.Advanced.UseOptimisticConcurrency = true;
                    foreach (StoredEvent storedEvent in Cache)
                    {
                        session.Store(storedEvent);
                    }

                    session.SaveChanges();
                }
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Reads all committed events from storage. 
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        protected virtual IEnumerable<StoredEvent> AccumulateEvents(Expression<Func<StoredEvent, bool>> query)
        {
            IQueryable<StoredEvent> result = Enumerable.Empty<StoredEvent>().AsQueryable();
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    List<StoredEvent> chunk = session
                        .Query<StoredEvent, Event_ByEventSource>()
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                        .Where(query).OrderBy(e => e.EventSequence)
                        .Skip(page * pageSize)
                        .Take(pageSize)
                        .ToList();

                    if (chunk.Count == 0)
                    {
                        break;
                    }

                    result = result.Concat(chunk);
                    page++;
                }
            }

            return result;
        }

        /// <summary>
        /// The create conventions.
        /// </summary>
        /// <returns>
        /// The <see cref="DocumentConvention"/>.
        /// </returns>
        public static DocumentConvention CreateConventions()
        {
           var docStore = new DocumentConvention
               {
                   JsonContractResolver = new PropertiesOnlyContractResolver(),
                   FindTypeTagName = x => "Events"
                   /*, CustomizeJsonSerializer = serializer => serializer.Binder = new TypeNameSerializationBinder("{0}");*/
               };
            return docStore;
        }


        /// <summary>
        /// The to stored event.
        /// </summary>
        /// <param name="commitId">
        /// The commit id.
        /// </param>
        /// <param name="uncommittedEvent">
        /// The uncommitted event.
        /// </param>
        /// <returns>
        /// The <see cref="StoredEvent"/>.
        /// </returns>
        private static StoredEvent ToStoredEvent(Guid commitId, UncommittedEvent uncommittedEvent)
        {
            return new StoredEvent
                {
                    Id = uncommittedEvent.EventSourceId + "/" + uncommittedEvent.EventSequence,
                    EventIdentifier = uncommittedEvent.EventIdentifier,
                    EventTimeStamp = uncommittedEvent.EventTimeStamp,
                    Version = uncommittedEvent.EventVersion,
                    CommitId = commitId,
                    Data = uncommittedEvent.Payload,
                    EventSequence = uncommittedEvent.EventSequence,
                    EventSourceId = uncommittedEvent.EventSourceId,
                    EventType = uncommittedEvent.Payload.GetType().Name
                };
        }

        #endregion
        private int CountOfAllEvents(bool includeShapshots)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                return
                    QueryAllEvents(session, includeShapshots)
                    .Count();
            }
        }

        private IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize, bool includeShapshots)
        {
            int returnedEventCount = 0;

            while (true)
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    StoredEvent[] storedEventsBulk =
                        QueryAllEvents(session, includeShapshots)
                        .OrderBy(y => y.EventSequence)
                        .Skip(returnedEventCount)
                        .Take(bulkSize)
                        .ToArray();

                    bool allEventsWereAlreadyReturned = storedEventsBulk.Length == 0;
                    if (allEventsWereAlreadyReturned)
                        yield break;

                    yield return Array.ConvertAll(storedEventsBulk, ToCommittedEvent);

                    returnedEventCount += bulkSize;
                }
            }
        }

        private static IQueryable<StoredEvent> QueryAllEvents(IDocumentSession session, bool includeShapshots)
        {
            return includeShapshots
                       ? session
                             .Query<StoredEvent>()
                             .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                       : session
                             .Query<StoredEvent>()
                             .Customize(x => x.WaitForNonStaleResultsAsOfNow());
        }
        public int CountOfAllEventsWithoutSnapshots()
        {
            return this.CountOfAllEvents(includeShapshots: false);
        }

        public int CountOfAllEventsIncludingSnapshots()
        {
            return this.CountOfAllEvents(includeShapshots: true);
        }

        public IEnumerable<CommittedEvent[]> GetAllEventsWithoutSnapshots(int bulkSize)
        {
            return this.GetAllEvents(bulkSize, includeShapshots: false);
        }

        [Obsolete("because there are no snapshots in event stream now")]
        public IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize)
        {
            return this.GetAllEvents(bulkSize, includeShapshots: true);
        }
    }
}
