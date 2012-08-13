using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class CompleteQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                               IEventHandler<AnswerSet>,
                                                               IEventHandler<PropagatableGroupAdded>,
                                                               IEventHandler<PropagatableGroupDeleted>,
                                                               IEventHandler<CompleteQuestionnaireDeleted>,
                                                               IEventHandler<QuestionnaireStatusChanged>,
                                                               IEventHandler<QuestionnaireAssignmentChanged>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;
        public CompleteQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }


        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.documentItemStore.Store(new CompleteQuestionnaireBrowseItem(
                                            evnt.Payload.CompletedQuestionnaireId.ToString(),
                                            evnt.Payload.QuestionnaireId.ToString(),
                                            evnt.Payload.Questionnaire.Title, evnt.Payload.CreationDate,
                                            DateTime.Now,
                                            evnt.Payload.Status, 
                                            evnt.Payload.TotalQuestionCount, 
                                            0, 
                                            evnt.Payload.Responsible), 
                                    evnt.Payload.CompletedQuestionnaireId);
         
        }

        #region Implementation of IEventHandler<in AddPropagatableGroupCommand>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
          //  throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IEventHandler<in DeletePropagatableGroupCommand>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
          //  throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IEventHandler<in AnswerSet>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            if (evnt.Payload.Featured)
            {
                var item =
                this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            
            var featuredQuestions = new List<QuestionStatisticView>();
            featuredQuestions.AddRange(item.FeaturedQuestions);
            var currentFeatured =
                featuredQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);
            if (currentFeatured == null)
            {
                currentFeatured = new QuestionStatisticView(
                        new TextCompleteQuestion(evnt.Payload.QuestionText) {PublicKey = evnt.Payload.QuestionPublicKey},
                        Guid.Empty,
                        null,
                        Guid.Empty);
                    featuredQuestions.Add(currentFeatured);
                }

                currentFeatured.AnswerValue = currentFeatured.AnswerText = evnt.Payload.AnswerString;

                item.FeaturedQuestions = featuredQuestions.ToArray();
            }
        }
    

        #endregion

        #region Implementation of IEventHandler<in CompleteQuestionnaireDeleted>

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this.documentItemStore.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var item =
                this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            item.Status = evnt.Payload.Status;

        }

        #endregion

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var item = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
        }
    }
}
