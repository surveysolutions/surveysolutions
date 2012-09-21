// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Complete.Question;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace Main.Core.EventHandlers
{
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
            HandleNewQuestionnaire(evnt.Payload.Questionnaire);
        }
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if(document==null)
                return;
            HandleNewQuestionnaire(document);
        }

        protected void HandleNewQuestionnaire(CompleteQuestionnaireDocument document)
        {
            // getting all featured questions
            var browseItem = this.documentItemStore.GetByGuid(document.PublicKey);
            if (browseItem == null)
            {
                browseItem = new CompleteQuestionnaireBrowseItem(document);
                this.documentItemStore.Store(browseItem, document.PublicKey);
            }
            List<ICompleteQuestion> featuredQuestions = this.FindFeaturedQuestions(document);

            browseItem.FeaturedQuestions =
                featuredQuestions.Select(
                    q => new CompleteQuestionView() {PublicKey = q.PublicKey, Answer = q.GetAnswerString(), Title = q.QuestionText}).ToArray();
            browseItem.Status = document.Status;
            browseItem.Responsible = document.Responsible;
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
                if (item == null)
                    return;
                CompleteQuestionView currentFeatured =
                    item.FeaturedQuestions.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionPublicKey);

                if (currentFeatured != null)
                    currentFeatured.Answer = evnt.Payload.AnswerString;
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
            List<ICompleteQuestion> featuredQuestions,
            IEnumerable<ICompleteQuestion> questions, 
            Guid gropPublicKey, 
            Guid? gropPropagationPublicKey, 
            Guid screenPublicKey)
        {
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                if (completeQuestion.Featured)
                {
                    featuredQuestions.Add(completeQuestion);
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
        private List<ICompleteQuestion> FindFeaturedQuestions(CompleteQuestionnaireDocument target)
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