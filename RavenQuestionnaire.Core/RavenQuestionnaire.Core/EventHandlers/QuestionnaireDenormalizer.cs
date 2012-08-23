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

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class QuestionnaireDenormalizer : IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<QuestionnaireTemplateLoaded>, IEventHandler<NewGroupAdded>,
        IEventHandler<QuestionnaireItemMoved>, IEventHandler<QuestionDeleted>,
        IEventHandler<NewQuestionAdded>, IEventHandler<QuestionChanged>,
        IEventHandler<ImageUpdated>, IEventHandler<ImageUploaded>,
        IEventHandler<ImageDeleted>, IEventHandler<GroupDeleted>, 
        IEventHandler<GroupUpdated>
    {

        private readonly IDenormalizerStorage<QuestionnaireDocument> _documentStorage;

        public QuestionnaireDenormalizer(IDenormalizerStorage<QuestionnaireDocument> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IEventHandler<in NewQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            QuestionnaireDocument item = new QuestionnaireDocument();

            item.Title = evnt.Payload.Title;
            item.PublicKey = evnt.Payload.PublicKey;
            item.CreationDate = evnt.Payload.CreationDate;

            this._documentStorage.Store(item, item.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireTemplateLoaded>

        public void Handle(IPublishedEvent<QuestionnaireTemplateLoaded> evnt)
        {
            this._documentStorage.Store(evnt.Payload.Template, evnt.Payload.Template.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in NewGroupAdded>

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.QuestionnairePublicKey);

            Group group = new Group();
            group.Title = evnt.Payload.GroupText;
            group.Propagated = evnt.Payload.Paropagateble;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            item.Add(group, evnt.Payload.ParentGroupPublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireItemMoved>

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.PublicKey);

            var questionnaire = new Questionnaire(item);
            questionnaire.MoveItem(evnt.Payload.PublicKey, evnt.Payload.GroupKey, evnt.Payload.AfterItemKey);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionDeleted>

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);
            item.Remove(evnt.Payload.QuestionId);
        }

        #endregion

        #region Implementation of IEventHandler<in NewQuestionAdded>

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            var result = new CompleteQuestionFactory().Create(evnt.Payload.QuestionType);
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
            UpdateAnswerList(evnt.Payload.Answers, result);

            item.Add(result, evnt.Payload.GroupPublicKey);
        }
        //move it out of there
        protected void UpdateAnswerList(IEnumerable<Answer> answers, AbstractQuestion question)
        {
            if (answers != null && answers.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in answers)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionChanged>

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            var question = item.Find<AbstractQuestion>(evnt.Payload.PublicKey);
            if (question == null)
                return;

            question.QuestionText = evnt.Payload.QuestionText;
            question.StataExportCaption = evnt.Payload.StataExportCaption;
            question.QuestionType = evnt.Payload.QuestionType;
            UpdateAnswerList(evnt.Payload.Answers, question);
            question.ConditionExpression = evnt.Payload.ConditionExpression;
            question.ValidationExpression = evnt.Payload.ValidationExpression;
            question.ValidationMessage = evnt.Payload.ValidationMessage;
            question.Instructions = evnt.Payload.Instructions;
            question.Featured = evnt.Payload.Featured;
            question.Mandatory = evnt.Payload.Mandatory;
            question.Triggers.Add(evnt.Payload.TargetGroupKey);
            question.AnswerOrder = evnt.Payload.AnswerOrder;
        }

        #endregion

        #region Implementation of IEventHandler<in ImageUpdated>

        public void Handle(IPublishedEvent<ImageUpdated> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);
            question.UpdateCard(evnt.Payload.ImageKey, evnt.Payload.Title, evnt.Payload.Description);
        }

        #endregion

        #region Implementation of IEventHandler<in ImageUploaded>

        public void Handle(IPublishedEvent<ImageUploaded> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);
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

        #endregion

        #region Implementation of IEventHandler<in ImageDeleted>

        public void Handle(IPublishedEvent<ImageDeleted> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);

            question.RemoveCard(evnt.Payload.ImageKey);
        }

        #endregion

        #region Implementation of IEventHandler<in GroupDeleted>

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            item.Remove(evnt.Payload.GroupPublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in GroupUpdated>

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.EventSourceId);
            Group group = item.Find<Group>(evnt.Payload.GroupPublicKey);
            if (group != null)
            {
                group.Propagated = evnt.Payload.Propagateble;
                //if(e.Triggers!=null)
                //    group.Triggers = e.Triggers;
                group.ConditionExpression = evnt.Payload.ConditionExpression;
                group.Update(evnt.Payload.GroupText);
            }
        }

        #endregion
    }
}
