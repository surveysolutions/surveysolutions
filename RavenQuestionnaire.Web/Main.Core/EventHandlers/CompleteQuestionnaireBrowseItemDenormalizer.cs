using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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

    public class CompleteQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                               IEventHandler<AnswerSet>,
                                                               IEventHandler<CompleteQuestionnaireDeleted>,
                                                               IEventHandler<QuestionnaireStatusChanged>,
                                                               IEventHandler<QuestionnaireAssignmentChanged>,
                                                               IEventHandler<InterviewDeleted>
    {
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> documentItemStore;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public CompleteQuestionnaireBrowseItemDenormalizer(IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> documentItemStore, IReadSideRepositoryWriter<UserDocument> users)
        {
            this.documentItemStore = documentItemStore;
            this.users = users;
        }
        
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.HandleNewSurvey(evnt.Payload.Questionnaire);
        }

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

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this.documentItemStore.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            CompleteQuestionnaireBrowseItem item =
                this.documentItemStore.GetById(evnt.EventSourceId);

            item.Status = evnt.Payload.Status;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var responsible = this.FillResponsiblesName(evnt.Payload.Responsible);

            CompleteQuestionnaireBrowseItem item = 
                this.documentItemStore.GetById(evnt.EventSourceId);

            item.Responsible = responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            CompleteQuestionnaireBrowseItem item =
                this.documentItemStore.GetById(evnt.EventSourceId);

            item.IsDeleted = true;
            this.documentItemStore.Store(item, item.CompleteQuestionnaireId);
        }

        private UserLight FillResponsiblesName(UserLight responsible)
        {
            var user = this.users.GetById(responsible.Id);
            return new UserLight
                {
                    Id = responsible.Id,
                    Name = string.IsNullOrWhiteSpace(responsible.Name)
                               ? user == null ? "" : user.UserName
                               : responsible.Name
                };
        }

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