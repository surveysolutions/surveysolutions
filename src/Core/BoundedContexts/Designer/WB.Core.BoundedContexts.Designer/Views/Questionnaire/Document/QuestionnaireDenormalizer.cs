using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document
{
    using Main.Core.Entities;

    internal class QuestionnaireDenormalizer :BaseDenormalizer,
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<ExpressionsMigratedToCSharp>,

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

        IEventHandler<MultimediaQuestionUpdated>,

        IEventHandler<StaticTextAdded>,
        IEventHandler<StaticTextUpdated>,
        IEventHandler<StaticTextCloned>,
        IEventHandler<StaticTextDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;
        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;
        private readonly ILogger logger;

        public QuestionnaireDenormalizer(IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage,
            IQuestionnaireEntityFactory questionnaireEntityFactory, ILogger logger)
        {
            this.documentStorage = documentStorage;
            this.questionnaireEntityFactory = questionnaireEntityFactory;
            this.logger = logger;
        }

        public override object[] Writers
        {
            get { return new object[] { documentStorage}; }
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireDocument();

            item.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.Title);
            item.IsPublic = evnt.Payload.IsPublic;
            item.PublicKey = evnt.Payload.PublicKey;
            item.CreationDate = evnt.Payload.CreationDate;
            item.CreatedBy = evnt.Payload.CreatedBy;

            this.documentStorage.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<ExpressionsMigratedToCSharp> evnt)
        {
            QuestionnaireDocument questionnaire = this.documentStorage.GetById(evnt.EventSourceId);

            questionnaire.UsesCSharp = true;

            this.UpdateQuestionnaire(evnt, questionnaire);
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            if (item == null)
            {
                return;
            }
            
            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.GroupText);
            group.VariableName = evnt.Payload.VariableName;
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
            group.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.GroupText);
            group.VariableName = evnt.Payload.VariableName;
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
            item.RemoveEntity(evnt.Payload.QuestionId);

            item.RemoveHeadPropertiesFromRosters(evnt.Payload.QuestionId);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            QuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey.Value, e.TargetIndex,EventConverter.QuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            AddQuestion(evnt, evnt.Payload.GroupPublicKey.Value, EventConverter.NewQuestionAddedToQuestionData(evnt));
        }

        //// move it out of there
        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.QuestionChangedToQuestionData(evnt));
        }

        protected void AddQuestion(IPublishableEvent evnt, Guid groupId, QuestionData data)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            if (item == null)
            {
                return;
            }
            IQuestion result = questionnaireEntityFactory.CreateQuestion(data);
            IGroup group = item.Find<IGroup>(groupId);

            if (result == null || group == null)
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
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            if (document == null)
            {
                return;
            }

            var question = document.Find<AbstractQuestion>(data.PublicKey);

            if (question == null)
            {
                return;
            }

            IQuestion newQuestion = this.questionnaireEntityFactory.CreateQuestion(data);

            document.ReplaceEntity(question, newQuestion);

            document.UpdateRosterGroupsIfNeeded(data.Triggers, data.PublicKey);

            if (newQuestion.Capital)
                document.MoveHeadQuestionPropertiesToRoster(question.PublicKey, null);

            this.UpdateQuestionnaire(evnt, document);
        }

        protected void CloneQuestion(IPublishableEvent evnt, Guid groupId,int index, QuestionData data)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            IQuestion result = questionnaireEntityFactory.CreateQuestion(data);
            IGroup group = document.Find<IGroup>(groupId);

            if (result == null || group == null)
            {
                return;
            }

            document.Insert(index, result, groupId);

            document.UpdateRosterGroupsIfNeeded(data.Triggers, data.PublicKey);

            if (result.Capital)
                document.MoveHeadQuestionPropertiesToRoster(result.PublicKey, groupId);

            this.UpdateQuestionnaire(evnt, document);
        }

        #region Static text modifications
        private void AddStaticText(IPublishableEvent evnt, Guid entityId, Guid parentId, string text)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);
            if (questionnaireDocument == null)
            {
                return;
            }

            var parentGroup = questionnaireDocument.Find<IGroup>(parentId);
            if (parentGroup == null)
            {
                return;
            }

            var staticText = questionnaireEntityFactory.CreateStaticText(entityId: entityId, text: text);

            questionnaireDocument.Add(staticText, parentId, null);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        private void UpdateStaticText(IPublishableEvent evnt, Guid entityId, string text)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);
            if (questionnaireDocument == null)
            {
                return;
            }

            var oldStaticText = questionnaireDocument.Find<IStaticText>(entityId);
            if (oldStaticText == null)
            {
                return;
            }

            var newStaticText = this.questionnaireEntityFactory.CreateStaticText(entityId: entityId, text: text);

            questionnaireDocument.ReplaceEntity(oldStaticText, newStaticText);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        private void CloneStaticText(IPublishableEvent evnt, Guid entityId, Guid parentId, int targetIndex, string text)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);
            if (questionnaireDocument == null)
            {
                return;
            }

            var parentGroup = questionnaireDocument.Find<IGroup>(parentId);
            if (parentGroup == null)
            {
                return;
            }

            var staticText = questionnaireEntityFactory.CreateStaticText(entityId: entityId, text: text);

            questionnaireDocument.Insert(index: targetIndex, c: staticText, parent: parentId);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }
        #endregion

        public void Handle(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            AddQuestion(evnt, evnt.Payload.GroupPublicKey,EventConverter.NumericQuestionAddedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            NumericQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey, e.TargetIndex, EventConverter.NumericQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.NumericQuestionChangedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            AddQuestion(evnt, evnt.Payload.GroupId, EventConverter.TextListQuestionAddedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            TextListQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupId, e.TargetIndex, EventConverter.TextListQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.TextListQuestionChangedToQuestionData(evnt));
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
                evnt.Payload.GroupText, evnt.Payload.VariableName,
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
                group.FixedRosterTitles = @event.Payload.FixedRosterTitles;
                group.RosterTitleQuestionId = @event.Payload.RosterTitleQuestionId;
            });

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<GroupStoppedBeingARoster> @event)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(@event.EventSourceId);
            if (document == null)
            {
                return;
            }
            document.UpdateGroup(@event.Payload.GroupId, group =>
            {
                group.IsRoster = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.FixedRosterTitles = new FixedRosterTitle[0];
            });

            this.UpdateQuestionnaire(@event, document);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.Title);
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
            var upgradedDocument = evnt.Payload.Source;

            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document != null)
                upgradedDocument.ReplaceSharedPersons(document.SharedPersons);

            this.AddNewQuestionnaire(upgradedDocument);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.AddNewQuestionnaire(evnt.Payload.QuestionnaireDocument);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            AddQuestion(evnt, evnt.Payload.ParentGroupId,EventConverter.QRBarcodeQuestionAddedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            UpdateQuestion(evnt, EventConverter.QRBarcodeQuestionUpdatedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            QRBarcodeQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.ParentGroupId, e.TargetIndex,EventConverter.QRBarcodeQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            UpdateQuestion(evnt, EventConverter.MultimediaQuestionUpdatedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<StaticTextAdded> evnt)
        {
            this.AddStaticText(evnt: evnt, entityId: evnt.Payload.EntityId, parentId: evnt.Payload.ParentId,
                text: evnt.Payload.Text);
        }

        public void Handle(IPublishedEvent<StaticTextUpdated> evnt)
        {
            this.UpdateStaticText(evnt: evnt, entityId: evnt.Payload.EntityId, text: evnt.Payload.Text);
        }

        public void Handle(IPublishedEvent<StaticTextCloned> evnt)
        {
            this.CloneStaticText(evnt: evnt, entityId: evnt.Payload.EntityId, parentId: evnt.Payload.ParentId,
                targetIndex: evnt.Payload.TargetIndex, text: evnt.Payload.Text);
        }

        public void Handle(IPublishedEvent<StaticTextDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.RemoveEntity(evnt.Payload.EntityId);
            
            this.UpdateQuestionnaire(evnt, item);
        }

        private void AddNewQuestionnaire(QuestionnaireDocument questionnaireDocument)
        {
            this.documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);
        }
    }
}