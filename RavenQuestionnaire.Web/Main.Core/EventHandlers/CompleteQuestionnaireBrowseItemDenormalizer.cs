// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.Core.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Question;
    using Main.DenormalizerStorage;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// The complete questionnaire browse item denormalizer.
    /// </summary>
    public class CompleteQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                               IEventHandler<AnswerSet>,
                                                               IEventHandler<CompleteQuestionnaireDeleted>,
                                                               IEventHandler<QuestionnaireStatusChanged>,
                                                               IEventHandler<QuestionnaireAssignmentChanged>,
                                                               IEventHandler<SnapshootLoaded>
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
        public CompleteQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore)
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
            this.HandleNewSurvey(evnt.Payload.Questionnaire);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
            {
                return;
            }

            this.HandleNewSurvey(document.Clone() as CompleteQuestionnaireDocument);
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
                CompleteQuestionnaireBrowseItem item = this.documentItemStore.GetById(evnt.EventSourceId);
                if (item == null)
                {
                    return;
                }

                item.LastEntryDate = evnt.EventTimeStamp;
                CompleteQuestionView currentFeatured =
                    item.FeaturedQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);

                if (currentFeatured != null)
                {
                    currentFeatured.Answer = evnt.Payload.AnswerString;
                }
                

                this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
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
                this.documentItemStore.GetById(evnt.Payload.CompletedQuestionnaireId);

            item.Status = evnt.Payload.Status;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
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
                this.documentItemStore.GetById(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
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
            List<ICompleteQuestion> featuredQuestions,
            IEnumerable<ICompleteQuestion> questions,
            Guid gropPublicKey,
            Guid? gropPropagationPublicKey,
            Guid screenPublicKey)
        {
            featuredQuestions.AddRange(questions.Where(completeQuestion => completeQuestion.Featured));
        }

        /// <summary>
        /// Handle new complete questionnaire
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        protected void HandleNewSurvey(CompleteQuestionnaireDocument document)
        {
            var browseItem = new CompleteQuestionnaireBrowseItem(document);
            
            // IEnumerable<ICompleteQuestion> featuredQuestions = this.FindFeaturedQuestions(document);
            IEnumerable<ICompleteQuestion> featuredQuestions = document.GetFeaturedQuestions().ToList();

            browseItem.FeaturedQuestions =
                featuredQuestions.Select(
                    q =>
                    new CompleteQuestionView
                    {
                        PublicKey = q.PublicKey,
                        Answer = q.GetAnswerString(),
                        Title = q.QuestionText
                    }).ToArray();

            this.documentItemStore.Store(browseItem, document.PublicKey);
        }

        /// <summary>
        /// The find featured questions.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.List`1[T -&gt; RavenQuestionnaire.Core.Views.Statistics.QuestionStatisticView].
        /// </returns>
        private IEnumerable<ICompleteQuestion> FindFeaturedQuestions(CompleteQuestionnaireDocument target)
        {
            var featuredQuestions = new List<ICompleteQuestion>();
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
                    group.PropagationPublicKey,
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
                    group.PropagationPublicKey,
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