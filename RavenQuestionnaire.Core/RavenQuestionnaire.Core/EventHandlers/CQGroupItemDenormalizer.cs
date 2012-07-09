using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class CQGroupItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                           IEventHandler<NewQuestionnaireCreated>, 
                                           IEventHandler<QuestionnaireTemplateLocaded>, 
                                           IEventHandler<CompleteQuestionnaireDeleted>
    {
        
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;

        public CQGroupItemDenormalizer(IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        #region Implementation of IEventHandler<in CreateCompleteQuestionnaireCommand>

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var group =
                this.documentGroupSession.Query().Where(g => g.SurveyId == evnt.Payload.QuestionnaireId.ToString());
            foreach (CQGroupItem cqGroupItem in group)
            {
                cqGroupItem.TotalCount++;
            }
        }

        #endregion


        #region Implementation of IEventHandler<in CreateQuestionnaireCommand>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var questionnaire = new CQGroupItem(0, 100, 0, evnt.Payload.Title, evnt.Payload.PublicKey.ToString());
            this.documentGroupSession.Store(questionnaire, evnt.Payload.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireTemplateLocaded>

        public void Handle(IPublishedEvent<QuestionnaireTemplateLocaded> evnt)
        {
            var questionnaire = new CQGroupItem(0, 100, 0, evnt.Payload.Template.Title, evnt.Payload.Template.PublicKey.ToString());
            this.documentGroupSession.Store(questionnaire, evnt.Payload.Template.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in CompleteQuestionnaireDeleted>

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            var group =
                this.documentGroupSession.Query().Where(g => Guid.Parse(g.SurveyId) == evnt.Payload.TemplateId);
            foreach (CQGroupItem cqGroupItem in group)
            {
                cqGroupItem.TotalCount--;
            }
        }

        #endregion

        
    }
}
