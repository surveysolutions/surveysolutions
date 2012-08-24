using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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
            // getting all featured questions
            var featuredQuestions = FindFeaturedQuestions(evnt);

            var browseItem = new CompleteQuestionnaireBrowseItem(
                evnt.Payload.Questionnaire.PublicKey.ToString(),
                evnt.Payload.QuestionnaireId.ToString(),
                evnt.Payload.Questionnaire.Title, evnt.Payload.CreationDate,
                DateTime.Now,
                evnt.Payload.Questionnaire.Status,
                evnt.Payload.TotalQuestionCount,
                0,
                evnt.Payload.Questionnaire.Responsible);
            browseItem.FeaturedQuestions = featuredQuestions.ToArray();
            this.documentItemStore.Store(browseItem, evnt.Payload.Questionnaire.PublicKey);
        }

        private List<QuestionStatisticView> FindFeaturedQuestions(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var featuredQuestions = new List<QuestionStatisticView>();

            var target = evnt.Payload.Questionnaire;
            var nodes = new Queue<ICompleteGroup>(new List<ICompleteGroup>() {target});
            var keys = new Queue<Guid>();
            keys.Enqueue(target.PublicKey);
            {
                ICompleteGroup group = nodes.Dequeue();
                var key = keys.Dequeue();
                ProccessQuestions(featuredQuestions, @group.Children.OfType<ICompleteQuestion>(), group.PublicKey,
                                  group.PropogationPublicKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(subGroup.PublicKey);
                }
            }
            while (nodes.Count > 0)
            {
                ICompleteGroup group = nodes.Dequeue();
                var key = keys.Dequeue();
                ProccessQuestions(featuredQuestions, group.Children.OfType<ICompleteQuestion>(), group.PublicKey,
                                  group.PropogationPublicKey, key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(key);
                }
            }
            return featuredQuestions;
        }

        protected void ProccessQuestions(List<QuestionStatisticView> featuredQuestions, 
            IEnumerable<ICompleteQuestion> questions, 
            Guid gropPublicKey, Guid? gropPropagationPublicKey, Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                if (completeQuestion.Featured)
                {
                    var statItem = new QuestionStatisticView(completeQuestion, gropPublicKey, gropPropagationPublicKey, screenPublicKey);
                    featuredQuestions.Add(statItem);
                }
            }
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
                var item = this.documentItemStore.GetByGuid(evnt.EventSourceId);

                var featuredQuestions = new List<QuestionStatisticView>();
                featuredQuestions.AddRange(item.FeaturedQuestions);
                var currentFeatured =
                    featuredQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);
                if (currentFeatured == null)
                {
                    currentFeatured = new QuestionStatisticView(
                            new TextCompleteQuestion(evnt.Payload.QuestionText) { PublicKey = evnt.Payload.QuestionPublicKey },
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
