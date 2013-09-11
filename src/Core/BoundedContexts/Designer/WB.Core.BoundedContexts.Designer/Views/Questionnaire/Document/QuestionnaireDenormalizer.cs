using System;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document
{
    using Main.Core.Entities;

    public class QuestionnaireDenormalizer :
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<NewGroupAdded>,
        IEventHandler<GroupCloned>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionChanged>,
        IEventHandler<ImageUpdated>,
        IEventHandler<ImageUploaded>,
        IEventHandler<ImageDeleted>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;
        private readonly ICompleteQuestionFactory questionFactory;
        private readonly ILogger logger;

        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage,
            ICompleteQuestionFactory questionFactory, ILogger logger)
        {
            this.documentStorage = documentStorage;
            this.questionFactory = questionFactory;
            this.logger = logger;
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireDocument();

            item.Title = evnt.Payload.Title;
            item.IsPublic = evnt.Payload.IsPublic;
            item.PublicKey = evnt.Payload.PublicKey;
            item.CreationDate = evnt.Payload.CreationDate;
            item.CreatedBy = evnt.Payload.CreatedBy;

            this.documentStorage.Store(item, item.PublicKey);
        }


        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            var group = new Group();
            group.Title = evnt.Payload.GroupText;
            group.Propagated = evnt.Payload.Paropagateble;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            group.Description = evnt.Payload.Description;

            Guid? parentGroupPublicKey = evnt.Payload.ParentGroupPublicKey;
            if (parentGroupPublicKey.HasValue)
            {
                var parentGroup = item.Find<Group>(parentGroupPublicKey.Value);
                if (parentGroup != null)
                {
                    group.SetParent(parentGroup);
                }
                else
                {
                    string errorMessage = string.Format("Event {0} attempted to add group {1} into group {2}. But group {2} doesnt exist in document {3}",
                        evnt.EventIdentifier, 
                        evnt.Payload.PublicKey, 
                        evnt.Payload.ParentGroupPublicKey, 
                        item.PublicKey);

                    logger.Error(errorMessage);
                }
            }


            item.Add(group, parentGroupPublicKey, null);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            var group = new Group();
            group.Title = evnt.Payload.GroupText;
            group.Propagated = evnt.Payload.Paropagateble;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            group.Description = evnt.Payload.Description;

            #warning Slava: uncomment this line and get rig of read and write side objects
            item.Insert(evnt.Payload.TargetIndex, group, evnt.Payload.ParentGroupPublicKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            QuestionnaireDocument questionnaire = this.documentStorage.GetById(evnt.EventSourceId);

            bool isLegacyEvent = evnt.Payload.AfterItemKey != null;

            if (isLegacyEvent)
            {
                logger.Warn(string.Format("Ignored legacy MoveItem event {0} from event source {1}", evnt.EventIdentifier, evnt.EventSourceId));
                return;
            }

            questionnaire.MoveItem(evnt.Payload.PublicKey, evnt.Payload.GroupKey, evnt.Payload.TargetIndex);

            this.UpdateQuestionnaire(evnt, questionnaire);
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.RemoveQuestion(evnt.Payload.QuestionId);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            FullQuestionDataEvent e = evnt.Payload;
            AbstractQuestion result =
                new CompleteQuestionFactory().CreateQuestion(
                    new DataQuestion(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId));

            if (result == null)
            {
                return;
            }
            #warning Slava: uncomment this line and get rig of read and write side objects
            item.Insert(evnt.Payload.TargetIndex, result, evnt.Payload.GroupPublicKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            FullQuestionDataEvent e = evnt.Payload;
            AbstractQuestion result =
                new CompleteQuestionFactory().CreateQuestion(
                    new DataQuestion(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId));
            if (result == null)
            {
                return;
            }

            item.Add(result, evnt.Payload.GroupPublicKey, null);
            this.UpdateQuestionnaire(evnt, item);
        }

        //// move it out of there
        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            var question = item.Find<AbstractQuestion>(evnt.Payload.PublicKey);
            if (question == null)
            {
                return;
            }

            QuestionChanged e = evnt.Payload;
            IQuestion newQuestion =
                this.questionFactory.CreateQuestion(
                    new DataQuestion(
                        question.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Triggers,
                        e.MaxValue,
                        e.Answers,
                        e.LinkedToQuestionId));

            item.ReplaceQuestionWithNew(question, newQuestion);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<ImageUpdated> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);
            question.UpdateCard(evnt.Payload.ImageKey, evnt.Payload.Title, evnt.Payload.Description);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<ImageUploaded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
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

        public void Handle(IPublishedEvent<ImageDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            var question = item.Find<AbstractQuestion>(evnt.Payload.QuestionKey);

            question.RemoveCard(evnt.Payload.ImageKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.RemoveGroup(evnt.Payload.GroupPublicKey);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.UpdateGroup(
                evnt.Payload.GroupPublicKey,
                evnt.Payload.GroupText,
                evnt.Payload.Description,
                evnt.Payload.Propagateble,
                evnt.Payload.ConditionExpression);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.Title = evnt.Payload.Title;
            document.IsPublic = evnt.Payload.IsPublic;
            this.UpdateQuestionnaire(evnt, document);
        }

        private void UpdateQuestionnaire(IEvent evnt, QuestionnaireDocument document)
        {
            document.LastEntryDate = evnt.EventTimeStamp;
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.IsDeleted = true;
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var document = evnt.Payload.Source;
            this.documentStorage.Store(document.Clone() as QuestionnaireDocument, document.PublicKey);
        }
    }
}