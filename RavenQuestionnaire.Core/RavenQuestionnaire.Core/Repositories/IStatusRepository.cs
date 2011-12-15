using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public interface IStatusRepository : IEntityRepository<Status, StatusDocument>
    {
    }
}
