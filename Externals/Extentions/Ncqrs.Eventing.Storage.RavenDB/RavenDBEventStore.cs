// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RavenDBEventStore.cs" company="">
//   
// </copyright>
// <summary>
//   The raven db event store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ncqrs.Restoring.EventStapshoot.EventStores;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Ncqrs.Restoring.EventStapshoot;

    using Raven.Client;
    using Raven.Client.Document;

    using ConcurrencyException = Ncqrs.Eventing.Storage.ConcurrencyException;

    /// <summary>
    /// The raven DB event store.
    /// </summary>
    public class RavenDBEventStore : IStreamableEventStore, ISnapshootEventStore
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
        /// Initializes a new instance of the <see cref="RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="ravenUrl">
        /// The raven url.
        /// </param>
        /// <param name="pageSize"></param>
        public RavenDBEventStore(string ravenUrl, int pageSize)
        {
            this.DocumentStore = new DocumentStore { Url = ravenUrl, Conventions = CreateConventions() }.Initialize();
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="externalDocumentStore">
        /// The external document store.
        /// </param>
        /// <param name="pageSize"></param>
        public RavenDBEventStore(DocumentStore externalDocumentStore, int pageSize)
        {
            externalDocumentStore.Conventions = CreateConventions();
            this.DocumentStore = externalDocumentStore;
            this.DocumentStore.JsonRequestFactory.ConfigureRequest += (sender, e) =>
                {
                    e.Request.Timeout = 10 * 60 * 1000; /*ms*/
                };
            this.pageSize = pageSize;
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
            return from chunk in this.GetStreamByChunk() from item in chunk select ToCommittedEvent(item);
        }

        public CommittedEvent GetLastEvent(Guid aggregateRootId)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                var eventLast = session
                    .Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                    .Where(e => e.EventSourceId == aggregateRootId)
                    .OrderByDescending(y => y.EventSequence).FirstOrDefault();

                if (eventLast != null)
                    return ToCommittedEvent(eventLast);
                return null;
            }
        }

        public bool IsEventPresent(Guid aggregateRootId, Guid eventIdentifier)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                return session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout))).Any(e => e.EventSourceId == aggregateRootId && e.EventIdentifier == eventIdentifier);
            }
        }

        /// <summary>
        /// The get stream by chunk.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private IEnumerable<IEnumerable<StoredEvent>> GetStreamByChunk()
        {
            var page = 0;
            while (true)
            {
                List<StoredEvent> chunk;
                using (IDocumentSession session = this.DocumentStore.OpenSession())
                {
                    chunk = session
                        .Query<StoredEvent>()
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                        .OrderBy(y => y.EventSequence)
                        .Skip(page * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                if (chunk.Count == 0)
                {
                    yield break;
                }

                page++;
                yield return chunk;
            }
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
        public virtual CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            var storedEvents =
                this.AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion);
            return new CommittedEventStream(id, storedEvents.Select(ToCommittedEvent));

            // }
        }

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

        public CommittedEvent GetLatestSnapshoot(Guid aggreagateRootId)
        {
            using (IDocumentSession session = this.DocumentStore.OpenSession())
            {
                var snapshoot =
                    session.Query<StoredEvent>()
                            .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                           .Where(e => e.IsSnapshot && e.EventSourceId == aggreagateRootId)
                           .OrderByDescending(y => y.EventSequence)
                           .FirstOrDefault();
                if (snapshoot!=null)
                {
                    return ToCommittedEvent(snapshoot);
                }
            }
            return null;
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
                        .Query<StoredEvent>()
                        .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromSeconds(timeout)))
                        .Where(query).OrderBy(x => x.EventSequence)
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

            return result.OrderBy(x => x.EventSequence);
        }

        /// <summary>
        /// The create conventions.
        /// </summary>
        /// <returns>
        /// The <see cref="DocumentConvention"/>.
        /// </returns>
        private static DocumentConvention CreateConventions()
        {
            return new DocumentConvention
                {
                    JsonContractResolver = new PropertiesOnlyContractResolver(), 
                    FindTypeTagName = x => "Events"
                    // NewDocumentETagGenerator = GenerateETag
                };
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
                    IsSnapshot = uncommittedEvent.Payload is SnapshootLoaded
                };
        }

        #endregion
    }
}