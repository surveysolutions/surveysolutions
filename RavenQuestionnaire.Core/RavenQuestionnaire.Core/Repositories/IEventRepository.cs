using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public interface IEventRepository : IEntityRepository<Event, EventDocument>
    {
    }
}
