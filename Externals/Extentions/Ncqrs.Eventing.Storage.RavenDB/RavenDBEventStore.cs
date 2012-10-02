// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RavenDBEventStore.cs" company="">
//   
// </copyright>
// <summary>
//   The raven db event store.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ncqrs.Eventing.Storage.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Raven.Abstractions.Exceptions;
    using Raven.Client;
    using Raven.Client.Document;

    using ConcurrencyException = Ncqrs.Eventing.Storage.ConcurrencyException;

    /// <summary>
    /// The raven db event store.
    /// </summary>
    public class RavenDBEventStore : IEventStore
    {
        #region Constants and Fields

        /// <summary>
        /// The _document store.
        /// </summary>
        protected readonly IDocumentStore _documentStore;

        /// <summary>
        /// The use async save.
        /// </summary>
        private bool useAsyncSave; // research. in the embedded mode true is not valid  

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="ravenUrl">
        /// The raven url.
        /// </param>
        public RavenDBEventStore(string ravenUrl)
        {
            this._documentStore = new DocumentStore { Url = ravenUrl, Conventions = CreateConventions() }.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenDBEventStore"/> class.
        /// </summary>
        /// <param name="externalDocumentStore">
        /// The external document store.
        /// </param>
        public RavenDBEventStore(DocumentStore externalDocumentStore)
        {
            externalDocumentStore.Conventions = CreateConventions();
            this._documentStore = externalDocumentStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to comitted event.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// </returns>
        public static CommittedEvent ToComittedEvent(StoredEvent x)
        {
            return new CommittedEvent(
                x.CommitId, x.EventIdentifier, x.EventSourceId, x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
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
        /// </returns>
        public virtual CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            /*  using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventSourceId == id)
                    .Where(x => x.EventSequence >= minVersion)
                    .Where(x => x.EventSequence <= maxVersion)
                    .ToList().OrderBy(x => x.EventSequence);*/
            IOrderedEnumerable<StoredEvent> storedEvents =
                this.AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion).
                    OrderBy(x => x.EventSequence);
            return new CommittedEventStream(id, storedEvents.Select(ToComittedEvent));

            // }
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="eventStream">
        /// The event stream.
        /// </param>
        /// <exception cref="ConcurrencyException">
        /// </exception>
        public void Store(UncommittedEventStream eventStream)
        {
            try
            {
                if (this.useAsyncSave)
                {
                    using (IAsyncDocumentSession session = this._documentStore.OpenAsyncSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;
                        foreach (UncommittedEvent uncommittedEvent in eventStream)
                        {
                            session.Store(ToStoredEvent(eventStream.CommitId, uncommittedEvent));
                        }

                        session.SaveChangesAsync();
                    }
                }
                else
                {
                    using (IDocumentSession session = this._documentStore.OpenSession())
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
            catch (ConcurrencyException)
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

        #endregion

        #region Methods

        /// <summary>
        /// Reads all committed events from storage. 
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <returns>
        /// </returns>
        protected virtual IEnumerable<StoredEvent> AccumulateEvents(Expression<Func<StoredEvent, bool>> query)
        {
            IQueryable<StoredEvent> result = Enumerable.Empty<StoredEvent>().AsQueryable();
            int maxPageSize = 1024;
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = this._documentStore.OpenSession())
                {
                    IQueryable<StoredEvent> chunk =
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).Skip(page * maxPageSize)
                            .Take(maxPageSize).Where(query);
                    if (!chunk.Any())
                    {
                        break;
                    }

                    result = result.Concat(chunk);
                    page++;
                }
            }

            return result.OrderBy(x => x.EventSequence);

            /*  List<StoredEvent> retval = new List<StoredEvent>();
            int maxPageSize = 1024;
            int page = 0;
            while (true)
            {


                using (var session = _documentStore.OpenSession())
                {
                    Raven.Client.Linq.RavenQueryStatistics stats;
                    var chunk =
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).Statistics(out stats).Skip(page * maxPageSize).
                        Take(maxPageSize).Where(query);
                    if (!chunk.Any())
                        break;

                    retval.AddRange(chunk);
                    page++;
                }
            }

            return retval.OrderBy(x=>x.EventSequence);*/
        }

        /// <summary>
        /// The create conventions.
        /// </summary>
        /// <returns>
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
        /// The generate e tag.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// </returns>
        private static Guid? GenerateETag(object entity)
        {
            var sourcedEvent = entity as StoredEvent;
            if (sourcedEvent != null)
            {
                return Guid.NewGuid();
            }

            return null;
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
                };
        }

        #endregion
    }
}