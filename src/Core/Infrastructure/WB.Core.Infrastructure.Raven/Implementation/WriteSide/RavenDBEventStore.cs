using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Abstractions.Data;
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

        private readonly bool useStreamingForAllEvents;

        /// <summary>
        /// PageSize for loading by chunk
        /// </summary>
        private readonly int pageSize = 1024;

        private readonly int timeout = 180;

        public RavenDBEventStore(string ravenUrl, int pageSize, bool useStreamingForAllEvents = true, FailoverBehavior failoverBehavior = FailoverBehavior.FailImmediately, string activeBundles = null)
        {
            this.DocumentStore = new DocumentStore
            {
                Url = ravenUrl,
                Conventions = CreateStoreConventions(CollectionName, failoverBehavior)
            }.Initialize();
           
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
            {
                e.Request.Timeout = 30*60*1000; /*ms*/
            };
            this.pageSize = pageSize;
            this.useStreamingForAllEvents = useStreamingForAllEvents;
            
            this.DocumentStore.ActivateBundles(activeBundles);
            IndexCreation.CreateIndexes(typeof (UniqueEventsIndex).Assembly, DocumentStore);
            IndexCreation.CreateIndexes(typeof (EventsByTimeStampAndSequenceIndex).Assembly, DocumentStore);
        }

        public RavenDBEventStore(DocumentStore externalDocumentStore, int pageSize, FailoverBehavior failoverBehavior = FailoverBehavior.FailImmediately, bool useStreamingForAllEvents = true)
        {
            externalDocumentStore.Conventions = CreateStoreConventions(CollectionName, failoverBehavior);
            this.DocumentStore = externalDocumentStore;

            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
            {
                e.Request.Timeout = 30*60*1000; /*ms*/
            };
            this.pageSize = pageSize;
            this.useStreamingForAllEvents = useStreamingForAllEvents;
            IndexCreation.CreateIndexes(typeof (UniqueEventsIndex).Assembly, DocumentStore);

            IndexCreation.CreateIndexes(typeof (EventsByTimeStampAndSequenceIndex).Assembly, DocumentStore);
        }

        public static CommittedEvent ToCommittedEvent(StoredEvent x)
        {
            return new CommittedEvent(
                x.CommitId, x.Origin, x.EventIdentifier, x.EventSourceId, x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
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
                        .AsProjection<UniqueEventsResults>().OrderBy(x => x.EventTimeStamp)
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

            return retval /*.OrderBy(e=>e.EventTimeStamp)*/;
            //  return from chunk in this.GetStreamByChunk() from item in chunk select ToCommittedEvent(item);

        }

        public virtual CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var commmitedEvents = new List<CommittedEvent>();

            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                var query = session.Query<StoredEvent, EventsByTimeStampAndSequenceIndex>()
                    .Where(x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion)
                    .OrderBy(y => y.EventSequence);

                var enumerator = session.Advanced.Stream(query);

                while (enumerator.MoveNext())
                {
                    commmitedEvents.Add(ToCommittedEvent(enumerator.Current.Document));
                }
            }
            var lastEventSequenceReadWithStreaming = commmitedEvents.Any()
                ? commmitedEvents.Last().EventSequence + 1
                : minVersion;

            commmitedEvents.AddRange(
                this.AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= lastEventSequenceReadWithStreaming && x.EventSequence <= maxVersion)
                    .Select(ToCommittedEvent));

            return new CommittedEventStream(id, commmitedEvents);
        }

        public long GetLastEventSequence(Guid id)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                var lastEvent = session.Query<StoredEvent, EventsByTimeStampAndSequenceIndex>()
                    .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                    .Where(x => x.EventSourceId == id).OrderByDescending(x => x.EventSequence).Take(1).ToList()[0];

                if (lastEvent == null)
                    return 0;
                return lastEvent.EventSequence;
            }
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
            catch (global::Raven.Abstractions.Exceptions.ConcurrencyException cex)
            {
                Guid sourceId = Guid.Empty;
                long version = 0;
                if (eventStream.HasSingleSource)
                {
                    sourceId = eventStream.SourceId;
                    version = eventStream.Sources.Single().CurrentVersion;
                }

                throw new ConcurrencyException(sourceId, version, cex);
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
                        .Skip(page*pageSize)
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
                Origin = uncommittedEvent.Origin,
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

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize, int skipEvents)
        {
            skipEvents = Math.Max(0, skipEvents);

            return this.useStreamingForAllEvents
                ? this.GetAllEventsWithStream(skipEvents)
                : this.GetAllEventsWithPaging(bulkSize, skipEvents);
        }

        private IEnumerable<CommittedEvent[]> GetAllEventsWithPaging(int bulkSize, int skipEvents)
        {
            int returnedEventCount = skipEvents;

            while (true)
            {
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    StoredEvent[] storedEventsBulk =
                        QueryAllEvents(session)
                        .OrderBy(y => y.EventTimeStamp)
                        .ThenBy(y => y.EventSequence)
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

        private IEnumerable<CommittedEvent[]> GetAllEventsWithStream(int skipEvents)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                var query = session.Query<StoredEvent, EventsByTimeStampAndSequenceIndex>()
                    .OrderBy(y => y.EventTimeStamp)
                    .ThenBy(y => y.EventSequence)
                    .Skip(skipEvents);

                var enumerator = session.Advanced.Stream(query);

                while (enumerator.MoveNext())
                {
                    yield return new CommittedEvent[] { ToCommittedEvent(enumerator.Current.Document) };
                }
            }
        }
    }
}