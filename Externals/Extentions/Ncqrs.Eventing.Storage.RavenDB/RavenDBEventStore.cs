using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenDBEventStore : IEventStore
    {
        private bool useAsyncSave = true;//research

        private readonly IDocumentStore _documentStore;

        public RavenDBEventStore(string ravenUrl)
        {
            _documentStore = new DocumentStore
            {
                Url = ravenUrl,                
                Conventions = CreateConventions()
            }.Initialize(); 
        }

        public RavenDBEventStore(DocumentStore externalDocumentStore)
        {
            externalDocumentStore.Conventions = CreateConventions();
            _documentStore = externalDocumentStore;            
        }

        private static DocumentConvention CreateConventions()
        {
            return new DocumentConvention
            {
                JsonContractResolver = new PropertiesOnlyContractResolver(),
                FindTypeTagName = x => "Events"
                //NewDocumentETagGenerator = GenerateETag
            };
        }

        private static Guid? GenerateETag(object entity)
        {
            var sourcedEvent = entity as StoredEvent;
            if (sourcedEvent != null)
            {
                return Guid.NewGuid();
            }
            return null;
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventSourceId == id)
                    .Where(x => x.EventSequence >= minVersion)
                    .Where(x => x.EventSequence <= maxVersion)
                    .ToList().OrderBy(x => x.EventSequence);
                return new CommittedEventStream(id, storedEvents.Select(ToComittedEvent));
            }
        }

        private static CommittedEvent ToComittedEvent(StoredEvent x)
        {
            return new CommittedEvent(x.CommitId, x.EventIdentifier, x.EventSourceId,x.EventSequence, x.EventTimeStamp, x.Data, x.Version);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            try
            {
                if (useAsyncSave)
                {
                    using (var session = _documentStore.OpenAsyncSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;
                        foreach (var uncommittedEvent in eventStream)
                        {
                            session.Store(ToStoredEvent(eventStream.CommitId, uncommittedEvent));
                        }
                        session.SaveChangesAsync();
                    }
                }
                else
                {
                    using (var session = _documentStore.OpenSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;
                        foreach (var uncommittedEvent in eventStream)
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


        /*/// <summary>
        /// Get some events after specified event.
        /// </summary>
        /// <param name="eventId">The id of last event not to be included in result set.</param>
        /// <param name="maxCount">Maximum number of returned events</param>
        /// <returns>A collection events starting right after <paramref name="eventId"/>.</returns>
        public IEnumerable<CommittedEvent> GetEventsAfter(Guid? eventId, int maxCount)
        {
            var result = new List<CommittedEvent>();

            using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventSequence >= minVersion)
                    .Where(x => x.EventSequence <= maxVersion)
                    .ToList().OrderBy(x => x.);
                return new CommittedEventStream(id, storedEvents.Select(ToComittedEvent));
            }

            return result;
        }*/



        /// <summary>
        /// Get some events after specified event.
        /// </summary>
        /// <param name="eventId">The id of last event not to be included in result set.</param>
        /// <param name="maxCount">Maximum number of returned events</param>
        /// <returns>A collection events starting right after <paramref name="eventId"/>.</returns>
        public IEnumerable<CommittedEvent> ReadFrom(DateTime start)
        {
            var result = new List<CommittedEvent>();

            using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventTimeStamp >= start)
                    .ToList().OrderBy(x => x.EventTimeStamp);
                result = storedEvents.Select(ToComittedEvent).ToList();
            }

            return result;
        }



    }
}
