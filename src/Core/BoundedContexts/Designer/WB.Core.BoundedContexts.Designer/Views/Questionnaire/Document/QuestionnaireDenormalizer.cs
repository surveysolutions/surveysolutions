using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document
{
    using Main.Core.Entities;

    internal class QuestionnaireDenormalizer :
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<NewGroupAdded>,
        IEventHandler<GroupCloned>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionChanged>,
        IEventHandler<NumericQuestionAdded>,
        IEventHandler<NumericQuestionCloned>,
        IEventHandler<NumericQuestionChanged>,
        IEventHandler<ImageUpdated>,
        IEventHandler<ImageUploaded>,
        IEventHandler<ImageDeleted>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<GroupBecameARoster>,
        IEventHandler<RosterChanged>,
        IEventHandler<GroupStoppedBeingARoster>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireCloned>,

        IEventHandler<TextListQuestionAdded>,
        IEventHandler<TextListQuestionCloned>,
        IEventHandler<TextListQuestionChanged>,

        IEventHandler<QRBarcodeQuestionAdded>,
        IEventHandler<QRBarcodeQuestionUpdated>,
        IEventHandler<QRBarcodeQuestionCloned>,

        IEventHandler
    {
        private readonly IQuestionnaireDocumentUpgrader upgrader;
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage;
        private readonly IQuestionFactory questionFactory;
        private readonly ILogger logger;

        public QuestionnaireDenormalizer(IReadSideRepositoryWriter<QuestionnaireDocument> documentStorage,
            IQuestionFactory questionFactory, ILogger logger, IQuestionnaireDocumentUpgrader upgrader)
        {
            this.upgrader = upgrader;
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

            item.RemoveHeadPropertiesFromRosters(evnt.Payload.QuestionId);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            QuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey.Value, e.TargetIndex,
                new QuestionData(
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
                    DetermineActualMaxValueForGenericQuestion(e.QuestionType, legacyMaxValue: e.MaxValue),
                    e.Answers,
                    e.LinkedToQuestionId,
                    e.IsInteger,
                    null,
                    e.AreAnswersOrdered,
                    e.MaxAllowedAnswers,
                    null));
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            FullQuestionDataEvent e = evnt.Payload;
            AddQuestion(evnt, evnt.Payload.GroupPublicKey.Value,
                new QuestionData(
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
                    DetermineActualMaxValueForGenericQuestion(e.QuestionType, legacyMaxValue: e.MaxValue),
                    e.Answers,
                    e.LinkedToQuestionId,
                    e.IsInteger,
                    null,
                    e.AreAnswersOrdered,
                    e.MaxAllowedAnswers,
                    null));
        }

        //// move it out of there
        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            QuestionChanged e = evnt.Payload;
            UpdateQuestion(evnt, new QuestionData(
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
                DetermineActualMaxValueForGenericQuestion(e.QuestionType, legacyMaxValue: e.MaxValue),
                e.Answers,
                e.LinkedToQuestionId,
                e.IsInteger,
                null,
                e.AreAnswersOrdered,
                e.MaxAllowedAnswers,
                null));
        }

        protected void AddQuestion(IPublishableEvent evnt, Guid groupId, QuestionData data)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            IQuestion result = questionFactory.CreateQuestion(data);

            if (result == null)
            {
                return;
            }

            item.Add(result, groupId, null);

            item.UpdateRosterGroupsIfNeeded(data.Triggers, data.PublicKey);

            if (result.Capital)
                item.MoveHeadQuestionPropertiesToRoster(result.PublicKey, groupId);

            this.UpdateQuestionnaire(evnt, item);
        }

        protected void UpdateQuestion(IPublishableEvent evnt, QuestionData data)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            var question = item.Find<AbstractQuestion>(data.PublicKey);
            if (question == null)
            {
                return;
            }

            IQuestion newQuestion = this.questionFactory.CreateQuestion(data);

            item.ReplaceQuestionWithNew(question, newQuestion);

            item.UpdateRosterGroupsIfNeeded(data.Triggers, data.PublicKey);

            if (newQuestion.Capital)
                item.MoveHeadQuestionPropertiesToRoster(question.PublicKey, null);

            this.UpdateQuestionnaire(evnt, item);
        }

        protected void CloneQuestion(IPublishableEvent evnt, Guid groupId,int index, QuestionData data)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            IQuestion result = questionFactory.CreateQuestion(data);

            if (result == null)
            {
                return;
            }
            item.Insert(index, result, groupId);

            item.UpdateRosterGroupsIfNeeded(data.Triggers, data.PublicKey);

            if (result.Capital)
                item.MoveHeadQuestionPropertiesToRoster(result.PublicKey, groupId);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            NumericQuestionAdded e = evnt.Payload;
            AddQuestion(evnt, evnt.Payload.GroupPublicKey,
                new QuestionData(
                    e.PublicKey,
                    QuestionType.Numeric,
                    e.QuestionScope,
                    e.QuestionText,
                    e.StataExportCaption,
                    e.ConditionExpression,
                    e.ValidationExpression,
                    e.ValidationMessage,
                    Order.AZ, 
                    e.Featured,
                    e.Mandatory,
                    e.Capital,
                    e.Instructions,
                    e.Triggers,
                    DetermineActualMaxValueForNumericQuestion(e.IsAutopropagating, legacyMaxValue: e.MaxValue, actualMaxValue: e.MaxAllowedValue),
                    null,
                    null,
                    e.IsInteger, 
                    e.CountOfDecimalPlaces,
                    null,
                    null,
                    null));
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            NumericQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey, e.TargetIndex,
                new QuestionData(
                    e.PublicKey,
                    QuestionType.Numeric,
                    e.QuestionScope,
                    e.QuestionText,
                    e.StataExportCaption,
                    e.ConditionExpression,
                    e.ValidationExpression,
                    e.ValidationMessage,
                    Order.AZ,
                    e.Featured,
                    e.Mandatory,
                    e.Capital,
                    e.Instructions,
                    e.Triggers,
                    DetermineActualMaxValueForNumericQuestion(e.IsAutopropagating, legacyMaxValue: e.MaxValue, actualMaxValue: e.MaxAllowedValue),
                    null,
                    null,
                    e.IsInteger, 
                    e.CountOfDecimalPlaces,
                    null,
                    null,
                    null));
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            NumericQuestionChanged e = evnt.Payload;
            UpdateQuestion(evnt, new QuestionData(
                e.PublicKey,
                QuestionType.Numeric,
                e.QuestionScope,
                e.QuestionText,
                e.StataExportCaption,
                e.ConditionExpression,
                e.ValidationExpression,
                e.ValidationMessage,
                Order.AZ,
                e.Featured,
                e.Mandatory,
                e.Capital,
                e.Instructions,
                e.Triggers,
                DetermineActualMaxValueForNumericQuestion(e.IsAutopropagating, legacyMaxValue: e.MaxValue, actualMaxValue: e.MaxAllowedValue),
                null, 
                null,
                e.IsInteger, 
                e.CountOfDecimalPlaces,
                null,
                null,
                null));
        }


        public void Handle(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            TextListQuestionAdded e = evnt.Payload;
            AddQuestion(evnt, evnt.Payload.GroupId,
                new QuestionData(
                    e.PublicKey,
                    QuestionType.TextList,
                    QuestionScope.Interviewer,
                    e.QuestionText,
                    e.StataExportCaption,
                    e.ConditionExpression,
                    null,
                    null,
                    Order.AZ,
                    false,
                    e.Mandatory,
                    false,
                    e.Instructions,
                    new List<Guid>(), 
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    e.MaxAnswerCount));
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            TextListQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupId, e.TargetIndex,
                new QuestionData(
                    e.PublicKey,
                    QuestionType.TextList,
                    QuestionScope.Interviewer,
                    e.QuestionText,
                    e.StataExportCaption,
                    e.ConditionExpression,
                    null,
                    null,
                    Order.AZ,
                    false,
                    e.Mandatory,
                    false,
                    e.Instructions,
                    new List<Guid>(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    e.MaxAnswerCount));
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            TextListQuestionChanged e = evnt.Payload;
            UpdateQuestion(evnt, new QuestionData(
                e.PublicKey,
                QuestionType.TextList,
                QuestionScope.Interviewer,
                e.QuestionText,
                e.StataExportCaption,
                e.ConditionExpression,
                null,
                null,
                Order.AZ,
                false,
                e.Mandatory,
                false,
                e.Instructions,
                new List<Guid>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                e.MaxAnswerCount));
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
                evnt.Payload.ConditionExpression);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupBecameARoster> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            item.UpdateGroup(@event.Payload.GroupId,  group => group.IsRoster = true);

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<RosterChanged> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            item.UpdateGroup(@event.Payload.GroupId, group =>
            {
                group.RosterSizeQuestionId = @event.Payload.RosterSizeQuestionId;
                group.RosterSizeSource = @event.Payload.RosterSizeSource;
                group.RosterFixedTitles = @event.Payload.RosterFixedTitles;
                group.RosterTitleQuestionId = @event.Payload.RosterTitleQuestionId;
            });

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<GroupStoppedBeingARoster> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            item.UpdateGroup(@event.Payload.GroupId, group =>
            {
                group.IsRoster = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.RosterFixedTitles = null;
            });

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.Title = evnt.Payload.Title;
            document.IsPublic = evnt.Payload.IsPublic;
            this.UpdateQuestionnaire(evnt, document);
        }

        private void UpdateQuestionnaire(IPublishableEvent evnt, QuestionnaireDocument document)
        {
            document.LastEntryDate = evnt.EventTimeStamp;
            document.LastEventSequence = evnt.EventSequence;
            this.documentStorage.Store(document, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.IsDeleted = true;
            this.documentStorage.Store(document, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.AddNewQuestionnaire(evnt.Payload.Source);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.AddNewQuestionnaire(evnt.Payload.QuestionnaireDocument);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAdded> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            var question = new QRBarcodeQuestion()
            {
                PublicKey = @event.Payload.QuestionId,
                QuestionText = @event.Payload.Title,
                StataExportCaption = @event.Payload.VariableName,
                Mandatory = @event.Payload.IsMandatory,
                ConditionExpression = @event.Payload.ConditionExpression,
                Instructions = @event.Payload.Instructions
            };

            item.Add(c: question, parent: @event.Payload.ParentGroupId, parentPropagationKey: null);

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            var oldQuestion = item.Find<IQuestion>(@event.Payload.QuestionId);

            item.ReplaceQuestionWithNew(oldQuestion, new QRBarcodeQuestion()
            {
                PublicKey = @event.Payload.QuestionId,
                QuestionText = @event.Payload.Title,
                StataExportCaption = @event.Payload.VariableName,
                Mandatory = @event.Payload.IsMandatory,
                ConditionExpression = @event.Payload.ConditionExpression,
                Instructions = @event.Payload.Instructions
            });

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            var question = new QRBarcodeQuestion()
            {
                PublicKey = @event.Payload.QuestionId,
                QuestionText = @event.Payload.Title,
                StataExportCaption = @event.Payload.VariableName,
                Mandatory = @event.Payload.IsMandatory,
                ConditionExpression = @event.Payload.ConditionExpression,
                Instructions = @event.Payload.Instructions
            };

            item.Insert(index: @event.Payload.TargetIndex, c: question, parent: @event.Payload.ParentGroupId);

            this.UpdateQuestionnaire(@event, item);
        }

        private void AddNewQuestionnaire(QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument = this.upgrader.TranslatePropagatePropertiesToRosterProperties(questionnaireDocument);
            questionnaireDocument = this.upgrader.CleanExpressionCaches(questionnaireDocument);
            this.documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireDocument) }; }
        }

        private static int? DetermineActualMaxValueForGenericQuestion(QuestionType questionType, int legacyMaxValue)
        {
            return questionType == QuestionType.AutoPropagate ? legacyMaxValue as int? : null;
        }

        private static int? DetermineActualMaxValueForNumericQuestion(bool isAutopropagating, int? legacyMaxValue, int? actualMaxValue)
        {
            return isAutopropagating ? legacyMaxValue : actualMaxValue;
        }
    }
}