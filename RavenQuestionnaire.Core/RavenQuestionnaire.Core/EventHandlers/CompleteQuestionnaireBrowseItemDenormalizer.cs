// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
    using RavenQuestionnaire.Core.Events;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
    using RavenQuestionnaire.Core.Views.Statistics;

    /// <summary>
    /// The complete questionnaire browse item denormalizer.
    /// </summary>
    public class CompleteQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                               IEventHandler<AnswerSet>, 
                                                               IEventHandler<PropagatableGroupAdded>, 
                                                               IEventHandler<PropagatableGroupDeleted>, 
                                                               IEventHandler<CompleteQuestionnaireDeleted>, 
                                                               IEventHandler<QuestionnaireStatusChanged>, 
                                                               IEventHandler<QuestionnaireAssignmentChanged>
    {
        #region Fields

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseItemDenormalizer"/> class.
        /// </summary>
        /// <param name="documentItemStore">
        /// The document item store.
        /// </param>
        public CompleteQuestionnaireBrowseItemDenormalizer(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            // getting all featured questions
            List<QuestionStatisticView> featuredQuestions = this.FindFeaturedQuestions(evnt);

            var browseItem = new CompleteQuestionnaireBrowseItem(
                evnt.Payload.Questionnaire.PublicKey.ToString(), 
                evnt.Payload.QuestionnaireId.ToString(), 
                evnt.Payload.Questionnaire.Title, 
                evnt.Payload.CreationDate, 
                DateTime.Now, 
                evnt.Payload.Questionnaire.Status, 
                evnt.Payload.TotalQuestionCount, 
                0, 
                evnt.Payload.Questionnaire.Responsible);
            browseItem.FeaturedQuestions = featuredQuestions.ToArray();
            this.documentItemStore.Store(browseItem, evnt.Payload.Questionnaire.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            if (evnt.Payload.Featured)
            {
                CompleteQuestionnaireBrowseItem item = this.documentItemStore.GetByGuid(evnt.EventSourceId);

                var featuredQuestions = new List<QuestionStatisticView>();
                featuredQuestions.AddRange(item.FeaturedQuestions);
                QuestionStatisticView currentFeatured =
                    featuredQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);
                if (currentFeatured == null)
                {
                    currentFeatured =
                        new QuestionStatisticView(
                            new TextCompleteQuestion(evnt.Payload.QuestionText)
                                {
                                   PublicKey = evnt.Payload.QuestionPublicKey 
                                }, 
                            Guid.Empty, 
                            null, 
                            Guid.Empty);
                    featuredQuestions.Add(currentFeatured);
                }

                currentFeatured.AnswerValue = currentFeatured.AnswerText = evnt.Payload.AnswerString;

                item.FeaturedQuestions = featuredQuestions.ToArray();
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this.documentItemStore.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            CompleteQuestionnaireBrowseItem item =
                this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            item.Status = evnt.Payload.Status;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            CompleteQuestionnaireBrowseItem item =
                this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The proccess questions.
        /// </summary>
        /// <param name="featuredQuestions">
        /// The featured questions.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        /// <param name="gropPropagationPublicKey">
        /// The grop propagation public key.
        /// </param>
        /// <param name="screenPublicKey">
        /// The screen public key.
        /// </param>
        protected void ProccessQuestions(
            List<QuestionStatisticView> featuredQuestions, 
            IEnumerable<ICompleteQuestion> questions, 
            Guid gropPublicKey, 
            Guid? gropPropagationPublicKey, 
            Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                if (completeQuestion.Featured)
                {
                    var statItem = new QuestionStatisticView(
                        completeQuestion, gropPublicKey, gropPropagationPublicKey, screenPublicKey);
                    featuredQuestions.Add(statItem);
                }
            }
        }

        /// <summary>
        /// The find featured questions.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; RavenQuestionnaire.Core.Views.Statistics.QuestionStatisticView].
        /// </returns>
        private List<QuestionStatisticView> FindFeaturedQuestions(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var featuredQuestions = new List<QuestionStatisticView>();

            CompleteQuestionnaireDocument target = evnt.Payload.Questionnaire;
            var nodes = new Queue<ICompleteGroup>(new List<ICompleteGroup> { target });
            var keys = new Queue<Guid>();
            keys.Enqueue(target.PublicKey);
            {
                ICompleteGroup group = nodes.Dequeue();
                Guid key = keys.Dequeue();
                this.ProccessQuestions(
                    featuredQuestions, 
                    @group.Children.OfType<ICompleteQuestion>(), 
                    group.PublicKey, 
                    group.PropogationPublicKey, 
                    key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(subGroup.PublicKey);
                }
            }

            while (nodes.Count > 0)
            {
                ICompleteGroup group = nodes.Dequeue();
                Guid key = keys.Dequeue();
                this.ProccessQuestions(
                    featuredQuestions, 
                    group.Children.OfType<ICompleteQuestion>(), 
                    group.PublicKey, 
                    group.PropogationPublicKey, 
                    key);
                foreach (ICompleteGroup subGroup in group.Children.OfType<ICompleteGroup>())
                {
                    nodes.Enqueue(subGroup);
                    keys.Enqueue(key);
                }
            }

            return featuredQuestions;
        }

        #endregion
    }
}