using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Subscribers;

namespace RavenQuestionnaire.Core.Repositories
{
    public class CompleteQuestionnaireRepository : EntityRepository<CompleteQuestionnaire, CompleteQuestionnaireDocument>, ICompleteQuestionnaireRepository
    {
        private ISubscriber subscriber;
        public CompleteQuestionnaireRepository(IDocumentSession documentSession, ISubscriber subscriber)
            : base(documentSession)
        {
            this.subscriber = subscriber;
        }

        protected override CompleteQuestionnaire Create(CompleteQuestionnaireDocument doc)
        {
            return new CompleteQuestionnaire(doc, this.subscriber);
        }
    }
}
