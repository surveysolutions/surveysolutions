using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class CQGroupItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
        IEventHandler<NewQuestionnaireCreated>, IEventHandler<QuestionnaireTemplateLocaded>
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
            //  this.storage.Commit();
        }

        #endregion


        #region Implementation of IEventHandler<in CreateQuestionnaireCommand>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
           /* var questionnaire = this.storage.GetByGuid<CQGroupItem>(evnt.Payload.PublicKey);
            if (questionnaire == null)
            {*/
                var questionnaire = new CQGroupItem(0, 100, 0, evnt.Payload.Title, evnt.Payload.PublicKey.ToString());
                this.documentGroupSession.Store(questionnaire, evnt.Payload.PublicKey);
           /* }
            grid.Groups.Add(new CQGroupItem(0, 100, 100, evnt.Payload.Title, evnt.Payload.PublicKey.ToString()));*/
        //    this.storage.Commit();
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireTemplateLocaded>

        public void Handle(IPublishedEvent<QuestionnaireTemplateLocaded> evnt)
        {
            /* var questionnaire = this.storage.GetByGuid<CQGroupItem>(evnt.Payload.PublicKey);
             if (questionnaire == null)
             {*/
            var questionnaire = new CQGroupItem(0, 100, 0, evnt.Payload.Template.Title, evnt.Payload.Template.PublicKey.ToString());
            this.documentGroupSession.Store(questionnaire, evnt.Payload.Template.PublicKey);
            /* }
             grid.Groups.Add(new CQGroupItem(0, 100, 100, evnt.Payload.Title, evnt.Payload.PublicKey.ToString()));*/
            //    this.storage.Commit();
        }

        #endregion
    }
}
