using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class CompleteQuestionnaireRepository : EntityRepository<CompleteQuestionnaire, CompleteQuestionnaireDocument>, ICompleteQuestionnaireRepository
    {
        public CompleteQuestionnaireRepository(IDocumentSession documentSession) : base(documentSession)
        {
        }

        protected override CompleteQuestionnaire Create(CompleteQuestionnaireDocument doc)
        {
            return new CompleteQuestionnaire(doc);
        }
    }
}
