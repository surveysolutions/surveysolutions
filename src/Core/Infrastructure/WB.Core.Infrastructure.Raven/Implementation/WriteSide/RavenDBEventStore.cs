using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide.Indexes;
using StoredEvent = Ncqrs.Eventing.Storage.RavenDB.StoredEvent;

namespace WB.Core.Infrastructure.Raven.Implementation.WriteSide
{
    internal class RavenDBEventStore : RavenWriteSideStore, IStreamableEventStore
    {
        private const string CollectionName = "Events";

        protected readonly IDocumentStore DocumentStore;

        private bool useAsyncSave = false; // research: in the embedded mode true is not valid.

        /// <summary>
        /// PageSize for loading by chunk
        /// </summary>
        private readonly int pageSize = 1024;

        private readonly int timeout = 120;

        public RavenDBEventStore(string ravenUrl, int pageSize)
        {
            this.DocumentStore = new DocumentStore
                {
                    Url = ravenUrl, 
                    Conventions = CreateStoreConventions(CollectionName)
                }.Initialize();

            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, DocumentStore);
        }

        public RavenDBEventStore(DocumentStore externalDocumentStore, int pageSize)
        {
            externalDocumentStore.Conventions = CreateStoreConventions(CollectionName);
            this.DocumentStore = externalDocumentStore;
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
            IndexCreation.CreateIndexes(typeof(UniqueEventsIndex).Assembly, DocumentStore);
        }

        public static CommittedEvent ToCommittedEvent(StoredEvent x)
        {
            return new CommittedEvent(
                x.CommitId, x.EventIdentifier, x.EventSourceId, x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
        }

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
            IEnumerable<StoredEvent> storedEvents =
                this.AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion);
            return new CommittedEventStream(id, storedEvents.Select(ToCommittedEvent));
        }

        public void Store(UncommittedEventStream eventStream)
        {
            try
            {
                if (this.useAsyncSave)
                {
                    using (IAsyncDocumentSession session = this.DocumentStore.OpenAsyncSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;
                        foreach (UncommittedEvent uncommittedEvent in eventStream)
                        {
                            session.StoreAsync(ToStoredEvent(eventStream.CommitId, uncommittedEvent));
                        }

                        session.SaveChangesAsync();
                    }
                }
                else
                {
                    using (IDocumentSession session = this.DocumentStore.OpenSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;
                        foreach (UncommittedEvent uncommittedEvent in eventStream)
                        {
                            session.Store(ToStoredEvent(eventStream.CommitId, uncommittedEvent));
                        }

                        session.SaveChanges();
                    }
                }
            }
            catch (global::Raven.Abstractions.Exceptions.ConcurrencyException)
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

        private static IQueryable<StoredEvent> QueryAllEvents(IDocumentSession session)
        {
            return session
                .Query<StoredEvent>()
                .Customize(x => x.WaitForNonStaleResultsAsOfNow());
        }

        public int CountOfAllEvents()
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                return
                    QueryAllEvents(session)
                    .Count();
            }
        }

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize)
        {
            int returnedEventCount = 0;

            while (true)
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    StoredEvent[] storedEventsBulk =
                        QueryAllEvents(session)
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
    }
}