using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class CompleteQuestionnaireRepository : EntityRepository<CompleteQuestionnaire, CompleteQuestionnaireDocument>, ICompleteQuestionnaireRepository
    {
        private IIteratorContainer iteratorContainer;
        public CompleteQuestionnaireRepository(IDocumentSession documentSession, IIteratorContainer iteratorContainer) : base(documentSession)
        {
            this.iteratorContainer = iteratorContainer;
        }

        protected override CompleteQuestionnaire Create(CompleteQuestionnaireDocument doc)
        {
            return new CompleteQuestionnaire(doc, iteratorContainer);
        }
    }
}
