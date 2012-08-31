// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the QuestionnaireDenormalizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using RavenQuestionnaire.Core.Entities.Extensions;

namespace RavenQuestionnaire.Core.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.AbstractFactories;
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Events;
    using RavenQuestionnaire.Core.Events.Questionnaire;

    /// <summary>
    /// The questionnaire denormalizer.
    /// </summary>
    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>, 
                                             IEventHandler<QuestionnaireTemplateLoaded>, 
                                             IEventHandler<NewGroupAdded>, 
                                             IEventHandler<QuestionnaireItemMoved>, 
                                             IEventHandler<QuestionDeleted>, 
                                             IEventHandler<NewQuestionAdded>, 
                                             IEventHandler<QuestionChanged>, 
                                             IEventHandler<ImageUpdated>, 
                                             IEventHandler<ImageUploaded>, 
                                             IEventHandler<ImageDeleted>, 
                                             IEventHandler<GroupDeleted>, 
                                             IEventHandler<GroupUpdated>
    {
        #region Fields

        /// <summary>
        /// The document storage.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> documentStorage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireDenormalizer"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public QuestionnaireDenormalizer(IDenormalizerStorage<QuestionnaireDocument> documentStorage)
        {
            this.documentStorage = documentStorage;
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

            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireTemplateLoaded> evnt)
        {
            this.documentStorage.Store(evnt.Payload.Template, evnt.Payload.Template.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.Payload.QuestionnairePublicKey);

            var group = new Group();
            group.Title = evnt.Payload.GroupText;
            group.Propagated = evnt.Payload.Paropagateble;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            item.Add(group, evnt.Payload.ParentGroupPublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.Payload.PublicKey);

         //   var questionnaire = new Questionnaire(item);
            item.MoveItem(evnt.Payload.PublicKey, evnt.Payload.GroupKey, evnt.Payload.AfterItemKey);
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
            item.Remove(evnt.Payload.QuestionId);
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

            AbstractQuestion result = new CompleteQuestionFactory().Create(evnt.Payload.QuestionType);
            result.QuestionType = evnt.Payload.QuestionType;
            result.QuestionText = evnt.Payload.QuestionText;
            result.StataExportCaption = evnt.Payload.StataExportCaption;
            result.ConditionExpression = evnt.Payload.ConditionExpression;
            result.ValidationExpression = evnt.Payload.ValidationExpression;
            result.ValidationMessage = evnt.Payload.ValidationMessage;
            result.AnswerOrder = evnt.Payload.AnswerOrder;
            result.Featured = evnt.Payload.Featured;
            result.Mandatory = evnt.Payload.Mandatory;
            result.Instructions = evnt.Payload.Instructions;
            result.PublicKey = evnt.Payload.PublicKey;
            result.Triggers.Add(evnt.Payload.TargetGroupKey);
            this.UpdateAnswerList(evnt.Payload.Answers, result);

            item.Add(result, evnt.Payload.GroupPublicKey);
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

            question.QuestionText = evnt.Payload.QuestionText;
            question.StataExportCaption = evnt.Payload.StataExportCaption;
            question.QuestionType = evnt.Payload.QuestionType;
            this.UpdateAnswerList(evnt.Payload.Answers, question);
            question.ConditionExpression = evnt.Payload.ConditionExpression;
            question.ValidationExpression = evnt.Payload.ValidationExpression;
            question.ValidationMessage = evnt.Payload.ValidationMessage;
            question.Instructions = evnt.Payload.Instructions;
            question.Featured = evnt.Payload.Featured;
            question.Mandatory = evnt.Payload.Mandatory;
            question.Triggers.Add(evnt.Payload.TargetGroupKey);
            question.AnswerOrder = evnt.Payload.AnswerOrder;
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

            item.Remove(evnt.Payload.GroupPublicKey);
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
                group.ConditionExpression = evnt.Payload.ConditionExpression;
                group.Update(evnt.Payload.GroupText);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The update answer list.
        /// </summary>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        protected void UpdateAnswerList(IEnumerable<Answer> answers, AbstractQuestion question)
        {
            List<Answer> enumerable = answers as List<Answer> ?? answers.ToList();
            if (answers != null && enumerable.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in enumerable)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        #endregion
    }
}