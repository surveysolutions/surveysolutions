using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class StatusRepository : EntityRepository<Status, StatusDocument>, IStatusRepository
    {
        public StatusRepository(IDocumentSession documentSession) : base(documentSession)
        {
        }

        protected override Status Create(StatusDocument doc)
        {
            return new Status(doc);
        }
    }
}
