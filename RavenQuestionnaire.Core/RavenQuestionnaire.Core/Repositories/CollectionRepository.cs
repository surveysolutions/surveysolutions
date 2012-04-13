using Raven.Client;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Repositories
{
    public class CollectionRepository:EntityRepository<Collection, CollectionDocument>, ICollectionRepository
    {
        public CollectionRepository(IDocumentSession documentSession) : base(documentSession)
        {
        }

        protected override Collection Create(CollectionDocument doc)
        {
            return new Collection(doc);
        }
    }
}
