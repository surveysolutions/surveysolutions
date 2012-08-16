using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Document;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenDBEventStore : IEventStore
    {
        private bool useAsyncSave = false;//research. in the embedded mode true is not valid  

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
            /*  using (var session = _documentStore.OpenSession())
            {
                var storedEvents = session.Query<StoredEvent>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.EventSourceId == id)
                    .Where(x => x.EventSequence >= minVersion)
                    .Where(x => x.EventSequence <= maxVersion)
                    .ToList().OrderBy(x => x.EventSequence);*/

            var storedEvents =
                AccumulateEvents(
                    x => x.EventSourceId == id && x.EventSequence >= minVersion && x.EventSequence <= maxVersion).
                    OrderBy(x => x.EventSequence);
            return new CommittedEventStream(id, storedEvents.Select(ToComittedEvent));
            // }
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
        /// Reads all committed events from storage. 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected IList<StoredEvent> AccumulateEvents(Expression<Func<StoredEvent, bool>> query)
        {
            List<StoredEvent> retval = new List<StoredEvent>();
            int maxPageSize = 1024;
            int page = 0;
            while (true)
            {


                using (var session = _documentStore.OpenSession())
                {
                    Raven.Client.Linq.RavenQueryStatistics stats;
                    var chunk =
                        session.Query<StoredEvent>().Customize(x => x.WaitForNonStaleResults()).Statistics(out stats).Skip(page * maxPageSize).
                        Take(maxPageSize).Where(query).OrderBy(x => x.EventTimeStamp).ToList();
                    if (chunk.Count == 0)
                        break;

                    retval.AddRange(chunk);
                    page++;
                }
            }

            return retval;

        }

        public IEnumerable<CommittedEvent> ReadFrom(DateTime start)
        {
            var result = new List<CommittedEvent>();

            //var storedEvents = AccumulateWithPaging(x => x.EventTimeStamp >= start).OrderBy(x => x.EventTimeStamp);

            var storedEvents = AccumulateEvents(x => x.EventTimeStamp >= start);
            
            result = storedEvents.Select(ToComittedEvent).ToList();

            return result;
        }

    }
}
