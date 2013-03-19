// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the QuestionnaireDenormalizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace Main.Core.EventHandlers
{
    using Ncqrs.Eventing;

    /// <summary>
    /// The questionnaire denormalizer.
    /// </summary>
    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>,
                                             IEventHandler<SnapshootLoaded>,
                                             IEventHandler<NewGroupAdded>,
                                             IEventHandler<QuestionnaireItemMoved>,
                                             IEventHandler<QuestionDeleted>,
                                             IEventHandler<NewQuestionAdded>,
                                             IEventHandler<QuestionChanged>,
                                             IEventHandler<ImageUpdated>,
                                             IEventHandler<ImageUploaded>,
                                             IEventHandler<ImageDeleted>,
                                             IEventHandler<GroupDeleted>,
                                             IEventHandler<GroupUpdated>,
                                             IEventHandler<QuestionnaireUpdated>,
                                             IEventHandler<QuestionnaireDeleted>
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> documentStorage;

        private readonly ICompleteQuestionFactory questionFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireDenormalizer"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public QuestionnaireDenormalizer(
            IDenormalizerStorage<QuestionnaireDocument> documentStorage, ICompleteQuestionFactory questionFactory)
        {
            this.documentStorage = documentStorage;
            this.questionFactory = questionFactory;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireDocument();

            item.Title = evnt.Payload.Title;
            item.PublicKey = evnt.Payload.PublicKey;
            item.CreationDate = evnt.Payload.CreationDate;
            item.CreatedBy = evnt.Payload.CreatedBy;

            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as QuestionnaireDocument;
            if (document == null)
            {
                return;
            }

            this.documentStorage.Store(document.Clone() as QuestionnaireDocument, document.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            var group = new Group();
            group.Title = evnt.Payload.GroupText;
            group.Propagated = evnt.Payload.Paropagateble;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            group.Description = evnt.Payload.Description;
            item.Add(group, evnt.Payload.ParentGroupPublicKey, null);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            // var questionnaire = new Questionnaire(item);
            item.MoveItem(evnt.Payload.PublicKey, evnt.Payload.GroupKey, evnt.Payload.AfterItemKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            item.RemoveQuestion(evnt.Payload.QuestionId);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            AbstractQuestion result = new CompleteQuestionFactory().Create(evnt.Payload);
            if (result == null)
            {
                return;
            }

            item.Add(result, evnt.Payload.GroupPublicKey, null);
            this.UpdateQuestionnaire(evnt, item);
        }

        //// move it out of there

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            var question = item.Find<AbstractQuestion>(evnt.Payload.PublicKey);
            if (question == null)
            {
                return;
            }

            this.questionFactory.UpdateQuestionByEvent(question, evnt.Payload);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ImageUpdated> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);
            question.UpdateCard(evnt.Payload.ImageKey, evnt.Payload.Title, evnt.Payload.Description);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ImageUploaded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            var newImage = new Image
                               {
                                   PublicKey = evnt.Payload.ImagePublicKey,
                                   Title = evnt.Payload.Title,
                                   Description = evnt.Payload.Description,
                                   CreationDate = DateTime.Now
                               };
            var question = item.Find<AbstractQuestion>(evnt.Payload.PublicKey);
            question.AddCard(newImage);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ImageDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);

            question.RemoveCard(evnt.Payload.ImageKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.RemoveGroup(evnt.Payload.GroupPublicKey);

            this.UpdateQuestionnaire(evnt, item);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);
            var group = item.Find<Group>(evnt.Payload.GroupPublicKey);
            if (group != null)
            {
                group.Propagated = evnt.Payload.Propagateble;

                ////if(e.Triggers!=null)
                // group.Triggers = e.Triggers;
                group.Description = evnt.Payload.Description;
                group.ConditionExpression = evnt.Payload.ConditionExpression;
                group.Update(evnt.Payload.GroupText);
            }
            this.UpdateQuestionnaire(evnt, item);
        }

        #endregion

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetByGuid(evnt.EventSourceId);
            if (document == null) return;
            document.Title = evnt.Payload.Title;
            this.UpdateQuestionnaire(evnt, document);
        }

        /// <summary>
        /// Updates questionnaire with event' service information
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        /// <param name="document">
        /// The document.
        /// </param>
        private void UpdateQuestionnaire(IEvent evnt, QuestionnaireDocument document)
        {
            document.LastEntryDate = evnt.EventTimeStamp;
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetByGuid(evnt.EventSourceId);
            if (document == null) return;
            document.IsDeleted = true;
        }
    }
}