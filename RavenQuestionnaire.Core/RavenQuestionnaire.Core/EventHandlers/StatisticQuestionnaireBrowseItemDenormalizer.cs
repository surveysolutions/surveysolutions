using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class StatisticQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                                IEventHandler<CompleteQuestionnaireDeleted>,
                                                                IEventHandler<QuestionnaireStatusChanged>,
                                                                IEventHandler<QuestionnaireAssignmentChanged>
    {

        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;
        public Dictionary<Guid, SurveyStatus> AllQuestionnaire { get; set; }
        public int UnAssignment { get; set; }


        public StatisticQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore, Dictionary<Guid, SurveyStatus> AllQuestionnaire)
        {
            this.documentItemStore = documentItemStore;
            this.AllQuestionnaire = AllQuestionnaire;
        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            if(AllQuestionnaire.ContainsKey(evnt.Payload.CompletedQuestionnaireId))
                return;
            AllQuestionnaire.Add(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.Status);
            if (evnt.Payload.Responsible == null)
                UnAssignment++;
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            UnAssignment--;
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            if (!AllQuestionnaire.ContainsKey(evnt.Payload.CompletedQuestionnaireId))
                return;
            AllQuestionnaire.Remove(evnt.Payload.CompletedQuestionnaireId);
        }
    }
}
