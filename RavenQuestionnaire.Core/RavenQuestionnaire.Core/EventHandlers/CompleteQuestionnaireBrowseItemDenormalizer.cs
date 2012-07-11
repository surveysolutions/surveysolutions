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
                                                               IEventHandler<FeaturedQuestionUpdated>,
                                                               IEventHandler<PropagatableGroupAdded>,
                                                               IEventHandler<PropagatableGroupDeleted>,
                                                               IEventHandler<CompleteQuestionnaireDeleted>,
                                                               IEventHandler<QuestionnaireStatusChanged>
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
                                            "", evnt.Payload.CreationDate,
                                            DateTime.Now,
                                            evnt.Payload.Status, 
                                            evnt.Payload.TotalQuestionCount, 
                                            0, 
                                            evnt.Payload.Responsible), 
                                    evnt.Payload.CompletedQuestionnaireId);
         
        }


        #region Implementation of IEventHandler<in SetAnswerCommand>

       /* public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var items =
                this.documentItemStore.Query().Where(
                    q => q.CompleteQuestionnaireId == evnt.Payload.CompletedQuestionnaireId.ToString());
            foreach (CompleteQuestionnaireBrowseItem item in items)
            {
                item.AnsweredQuestionCount++;

                if (evnt.Payload.Question.Featured)
                {
                    var featuredQuestions = new List<QuestionStatisticView>();
                    featuredQuestions.AddRange(item.FeaturedQuestions);
                    var currentFetured =
                        featuredQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.Question.PublicKey);
                    if (currentFetured == null)
                        featuredQuestions.Add(new QuestionStatisticView(evnt.Payload.Question, Guid.Empty, Guid.Empty));
                    else
                        currentFetured.AnswerValue = currentFetured.AnswerText = evnt.Payload.Question.GetAnswerString();

                    item.FeaturedQuestions = featuredQuestions.ToArray();
                }
            }
          
        }*/

        #endregion

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

        #region Implementation of IEventHandler<in FeaturedQuestionUpdated>

        public void Handle(IPublishedEvent<FeaturedQuestionUpdated> evnt)
        {
            var item =
                this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            /* foreach (CompleteQuestionnaireBrowseItem item in items)
             {*/
            //    item.AnsweredQuestionCount++;

            var featuredQuestions = new List<QuestionStatisticView>();
            featuredQuestions.AddRange(item.FeaturedQuestions);
            var currentFetured =
                featuredQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);
            if (currentFetured == null)
            {
                currentFetured = new QuestionStatisticView(
                    new TextCompleteQuestion(evnt.Payload.QuestionText) {PublicKey = evnt.Payload.QuestionPublicKey},
                    Guid.Empty,
                    Guid.Empty);
                featuredQuestions.Add(currentFetured);
            }
            
            currentFetured.AnswerValue = currentFetured.AnswerText = evnt.Payload.Answer;

            item.FeaturedQuestions = featuredQuestions.ToArray();

            // }
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
    }
}
