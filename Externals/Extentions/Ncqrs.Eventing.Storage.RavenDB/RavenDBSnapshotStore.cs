using System;
using System.Linq;
using Raven.Abstractions.Json;
using Raven.Client;
using Raven.Client.Document;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenDBSnapshotStore : RavenWriteSideStore, ISnapshotStore
    {
        private const string CollectionName = "Snapshots";
        private readonly IDocumentStore _documentStore;

        public RavenDBSnapshotStore(string ravenUrl)
        {
            _documentStore = new DocumentStore
            {
                Url = ravenUrl,                
                Conventions = CreateStoreConventions(CollectionName)
            }.Initialize(); 
        }

        public RavenDBSnapshotStore(DocumentStore externalDocumentStore)
        {
            externalDocumentStore.Conventions = CreateStoreConventions(CollectionName);
            _documentStore = externalDocumentStore;            
        }

        public void SaveShapshot(Snapshot source)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(new StoredSnaphot
                                  {
                                      Id = source.EventSourceId.ToString(),
                                      Data = source.Payload,
                                      EventSourceId = source.EventSourceId,
                                      Version = source.Version
                                  });
                session.SaveChanges();
            }
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            using (var session = _documentStore.OpenSession())
            {
                var snapshot = session.Load<StoredSnaphot>(eventSourceId.ToString());
                if (snapshot == null)
                {
                    return null;
                }
                return snapshot.Version <= maxVersion
                           ? new Snapshot(eventSourceId, snapshot.Version, snapshot.Data)
                           : null;
            }
        }
    }
}