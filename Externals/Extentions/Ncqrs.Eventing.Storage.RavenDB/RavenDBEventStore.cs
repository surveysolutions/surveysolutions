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
        private bool useAsyncSave = false;//research

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


        /// <summary>
        /// Get some events after specified event.
        /// </summary>
        /// <param name="eventId">The id of last event not to be included in result set.</param>
        /// <param name="maxCount">Maximum number of returned events</param>
        /// <returns>A collection events starting right after <paramref name="eventId"/>.</returns>
        /*public IEnumerable<CommittedEvent> GetEventsAfter(Guid? eventId, int maxCount)
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
        protected IList<StoredEvent> AccumulateWithPaging(Func<StoredEvent, bool> predicate)
        {
            List<StoredEvent> retval = new List<StoredEvent>();
            using (var session = _documentStore.OpenSession())
            {

                int count;
                Raven.Client.Linq.RavenQueryStatistics stats;
                session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults())
                    .Statistics(out stats).Where(predicate).ToList();

                count = stats.TotalResults;
                if (count == 0)
                    return retval;
                int queryLimit = 128;
                int step = 0;
                while (step < count)
                {
                    retval.AddRange(
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).Skip(step).Take(queryLimit).Where(predicate).ToList());
                    step += queryLimit;
                }
            }
            return retval;
        }


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
               /* var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventTimeStamp >= start)
                    .ToList().OrderBy(x => x.EventTimeStamp);*/
                var storedEvents = AccumulateWithPaging(x => x.EventTimeStamp >= start).OrderBy(x=>x.EventTimeStamp);
                result = storedEvents.Select(ToComittedEvent).ToList();
            }

            return result;
        }



    }
}
