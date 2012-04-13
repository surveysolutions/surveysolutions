using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Repositories
{
    public interface ICollectionRepository:IEntityRepository<Collection, CollectionDocument>
    {
    }
}
