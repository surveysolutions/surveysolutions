using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireChangeHistoryDenormalizer :
        BaseDenormalizer,
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
        IEventHandler<StaticTextDeleted>,

        IEventHandler<SharedPersonToQuestionnaireAdded>,
        IEventHandler<SharedPersonFromQuestionnaireRemoved>
    {
        private readonly IReadSideRepositoryWriter<AccountDocument> accountStorage;
        private readonly IReadSideRepositoryWriter<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireStateTacker> questionnaireStateTackerStorage; 

        public QuestionnaireChangeHistoryDenormalizer(
            IReadSideRepositoryWriter<AccountDocument> accountStorage,
            IReadSideRepositoryWriter<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IReadSideKeyValueStorage<QuestionnaireStateTacker> questionnaireStateTackerStorage)
        {
            this.accountStorage = accountStorage;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.questionnaireStateTackerStorage = questionnaireStateTackerStorage;
        }

        public override object[] Writers
        {
            get { return new object[] { questionnaireChangeItemStorage, questionnaireStateTackerStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] { accountStorage }; }
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.CreatedBy, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Questionnaire,
                evnt.EventSourceId, evnt.Payload.Title, evnt.EventSequence);

            questionnaireStateTackerStorage.Store(new QuestionnaireStateTacker() { CreatedBy = evnt.Payload.CreatedBy ?? Guid.Empty }, evnt.EventSourceId);

            AddOrUpdateGroupState(evnt.EventSourceId, evnt.EventSourceId, evnt.Payload.Title);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            var creatorId = evnt.Payload.QuestionnaireDocument.CreatedBy ?? Guid.Empty;

            UpdateFullQuestionnaireState(evnt.Payload.QuestionnaireDocument, evnt.EventSourceId, creatorId);

#warning don't remove this line, it is here because clone event had been risen incorrectly for some time. ClonedFromQuestionnaireId were set equal EventSourceId, but should be Id of the colned questionnaire
            if (evnt.Payload.ClonedFromQuestionnaireId == evnt.EventSourceId)
            {
                AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, creatorId, evnt.EventTimeStamp,
                    QuestionnaireActionType.Add, QuestionnaireItemType.Questionnaire,
                    evnt.EventSourceId, evnt.Payload.QuestionnaireDocument.Title, evnt.EventSequence);
            }
            else
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(evnt.Payload.ClonedFromQuestionnaireId);

                AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, creatorId, evnt.EventTimeStamp,
                    QuestionnaireActionType.Clone, QuestionnaireItemType.Questionnaire,
                    evnt.EventSourceId, evnt.Payload.QuestionnaireDocument.Title, evnt.EventSequence,
                    CreateQuestionnaireChangeReference(QuestionnaireItemType.Questionnaire,
                        evnt.Payload.ClonedFromQuestionnaireId, questionnaire.GroupsState[evnt.Payload.ClonedFromQuestionnaireId]));
            }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.Source.CreatedBy, evnt.EventTimeStamp,
                QuestionnaireActionType.Replace, QuestionnaireItemType.Questionnaire,
                evnt.EventSourceId, evnt.Payload.Source.Title, evnt.EventSequence);

            UpdateFullQuestionnaireState(evnt.Payload.Source, evnt.EventSourceId, evnt.Payload.Source.CreatedBy ?? Guid.Empty);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            if (questionnaire == null)
                return;

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, questionnaire.CreatedBy, evnt.EventTimeStamp,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Questionnaire,
                evnt.EventSourceId, questionnaire.GroupsState[evnt.EventSourceId], evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Questionnaire, evnt.EventSourceId,
                evnt.Payload.Title, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<SharedPersonToQuestionnaireAdded> evnt)
        {
            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Person, evnt.Payload.PersonId,
                GetUserName(evnt.Payload.PersonId), evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<SharedPersonFromQuestionnaireRemoved> evnt)
        {
            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Person, evnt.Payload.PersonId,
                GetUserName(evnt.Payload.PersonId), evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            var groupTitle = CreateGroupTitleFromEvent(evnt.Payload);
            AddOrUpdateGroupState(evnt.EventSourceId, evnt.Payload.PublicKey, groupTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Group, evnt.Payload.PublicKey,
                groupTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            var groupTitle = CreateGroupTitleFromEvent(evnt.Payload);
            AddOrUpdateGroupState(evnt.EventSourceId, evnt.Payload.PublicKey, groupTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.Group, evnt.Payload.PublicKey,
                groupTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.Group, evnt.Payload.SourceGroupId,
                    groupTitle));
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            var groupTitle = CreateGroupTitleFromEvent(evnt.Payload);
            AddOrUpdateGroupState(evnt.EventSourceId, evnt.Payload.GroupPublicKey, groupTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Group, evnt.Payload.GroupPublicKey,
                groupTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<GroupBecameARoster> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var groupTitle = questionnaire.GroupsState[evnt.Payload.GroupId];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.GroupBecameARoster, QuestionnaireItemType.Group,
                evnt.Payload.GroupId, groupTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<RosterChanged> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var groupTitle = questionnaire.GroupsState[evnt.Payload.GroupId];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Roster, evnt.Payload.GroupId,
                groupTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<GroupStoppedBeingARoster> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var groupTitle = questionnaire.GroupsState[evnt.Payload.GroupId];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.RosterBecameAGroup, QuestionnaireItemType.Roster, evnt.Payload.GroupId,
                groupTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var groupTitle = questionnaire.GroupsState[evnt.Payload.GroupPublicKey];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Group, evnt.Payload.GroupPublicKey,
                groupTitle, evnt.EventSequence);

            questionnaire.GroupsState.Remove(evnt.Payload.GroupPublicKey);
            questionnaireStateTackerStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.Question,
                    evnt.Payload.SourceQuestionId, questionTitle));
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var questionTitle = questionnaire.QuestionsState[evnt.Payload.QuestionId];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
               QuestionnaireActionType.Delete, QuestionnaireItemType.Question, evnt.Payload.QuestionId, questionTitle, evnt.EventSequence);

            questionnaire.QuestionsState.Remove(evnt.Payload.QuestionId);
            questionnaireStateTackerStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.Question,
                    evnt.Payload.SourceQuestionId, questionTitle));
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.Question,
                    evnt.Payload.SourceQuestionId, questionTitle));
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.PublicKey, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.QuestionId, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.Question, evnt.Payload.QuestionId,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.QuestionId, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Question, evnt.Payload.QuestionId,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.QuestionId, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.Question, evnt.Payload.QuestionId,
                questionTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.Question,
                    evnt.Payload.SourceQuestionId, questionTitle));
        }

        public void Handle(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            var questionTitle = CreateQuestionTitleFromEvent(evnt.Payload);
            AddOrUpdateQuestionState(evnt.EventSourceId, evnt.Payload.QuestionId, questionTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.Question, evnt.Payload.QuestionId,
                questionTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<StaticTextAdded> evnt)
        {
            var staticTextTitle = evnt.Payload.Text;
            AddOrUpdateStaticTextState(evnt.EventSourceId, evnt.Payload.EntityId, staticTextTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Add, QuestionnaireItemType.StaticText, evnt.Payload.EntityId,
                staticTextTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<StaticTextUpdated> evnt)
        {
            var staticTextTitle = evnt.Payload.Text;
            AddOrUpdateStaticTextState(evnt.EventSourceId, evnt.Payload.EntityId, staticTextTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Update, QuestionnaireItemType.StaticText, evnt.Payload.EntityId,
                staticTextTitle, evnt.EventSequence);
        }

        public void Handle(IPublishedEvent<StaticTextCloned> evnt)
        {
            var staticTextTitle = evnt.Payload.Text;
            AddOrUpdateStaticTextState(evnt.EventSourceId, evnt.Payload.EntityId, staticTextTitle);

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Clone, QuestionnaireItemType.StaticText, evnt.Payload.EntityId,
                staticTextTitle, evnt.EventSequence,
                CreateQuestionnaireChangeReference(QuestionnaireItemType.StaticText,
                    evnt.Payload.SourceEntityId, staticTextTitle));
        }

        public void Handle(IPublishedEvent<StaticTextDeleted> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);
            var staticTitle = questionnaire.StaticTextState[evnt.Payload.EntityId];

            AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId, evnt.EventTimeStamp,
                QuestionnaireActionType.Delete, QuestionnaireItemType.StaticText, evnt.Payload.EntityId,
                staticTitle, evnt.EventSequence);


            questionnaire.StaticTextState.Remove(evnt.Payload.EntityId);
            questionnaireStateTackerStorage.Store(questionnaire, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.EventSourceId);

            var targetGroupId = evnt.Payload.GroupKey ?? evnt.EventSourceId;

            if (questionnaire.QuestionsState.ContainsKey(evnt.Payload.PublicKey))
            {
                AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId,
                    evnt.EventTimeStamp,
                    QuestionnaireActionType.Move, QuestionnaireItemType.Question, evnt.Payload.PublicKey,
                    questionnaire.QuestionsState[evnt.Payload.PublicKey], evnt.EventSequence,
                    CreateQuestionnaireChangeReference(QuestionnaireItemType.Group,
                        targetGroupId, questionnaire.GroupsState[targetGroupId]));
                return;
            }
            if (questionnaire.GroupsState.ContainsKey(evnt.Payload.PublicKey))
            {
                AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId,
                    evnt.EventTimeStamp,
                    QuestionnaireActionType.Move, QuestionnaireItemType.Group, evnt.Payload.PublicKey,
                    questionnaire.GroupsState[evnt.Payload.PublicKey], evnt.EventSequence,
                    CreateQuestionnaireChangeReference(QuestionnaireItemType.Group,
                        targetGroupId, questionnaire.GroupsState[targetGroupId]));
                return;
            }
            if (questionnaire.StaticTextState.ContainsKey(evnt.Payload.PublicKey))
            {
                AddQuestionnaireChangeItem(evnt.EventIdentifier, evnt.EventSourceId, evnt.Payload.ResponsibleId,
                    evnt.EventTimeStamp,
                    QuestionnaireActionType.Move, QuestionnaireItemType.StaticText, evnt.Payload.PublicKey,
                    questionnaire.StaticTextState[evnt.Payload.PublicKey], evnt.EventSequence,
                    CreateQuestionnaireChangeReference(QuestionnaireItemType.Group,
                        targetGroupId, questionnaire.GroupsState[targetGroupId]));
            }
        }

        private string GetUserName(Guid? userId)
        {
            if (userId.HasValue)
            {
                var creator = accountStorage.GetById(userId);
                if (creator != null)
                    return creator.UserName;
            }
            return null;
        }

        private void UpdateFullQuestionnaireState(QuestionnaireDocument questionnaireDocument, Guid questionnaireId, Guid createdBy)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(questionnaireId);
            if (questionnaireStateTacker == null)
                questionnaireStateTacker = new QuestionnaireStateTacker() { CreatedBy = createdBy };

            questionnaireStateTacker.GroupsState[questionnaireId] = questionnaireDocument.Title;

            var compositeElements = questionnaireDocument.Find<IComposite>(c => true);
            foreach (var compositeElement in compositeElements)
            {
                var question = compositeElement as IQuestion;
                if (question != null)
                {
                    questionnaireStateTacker.QuestionsState[question.PublicKey] = CreateTitle(question,
                        q => q.StataExportCaption, q => q.QuestionText);
                    continue;
                }
                var group = compositeElement as IGroup;
                if (group != null)
                {
                    questionnaireStateTacker.GroupsState[group.PublicKey] = CreateTitle(group,
                        g => g.VariableName, g => g.Title);
                    continue;
                }
                var staticTexts = compositeElement as IStaticText;
                if (staticTexts != null)
                {
                    questionnaireStateTacker.StaticTextState[staticTexts.PublicKey] = staticTexts.Text;
                    continue;
                }
            }
            questionnaireStateTackerStorage.Store(questionnaireStateTacker, questionnaireId);
        }

        private void AddOrUpdateQuestionState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.QuestionsState[id] = title);
        }

        private void AddOrUpdateGroupState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.GroupsState[id] = title);
        }

        private void AddOrUpdateStaticTextState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.StaticTextState[id] = title);
        }

        private void AddOrUpdateQuestionnaireStateItem(Guid questionnaireId, Guid itemId, string itemTitle,
            Action<QuestionnaireStateTacker, Guid, string> setAction)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(questionnaireId);

            setAction(questionnaireStateTacker, itemId, itemTitle);

            questionnaireStateTackerStorage.Store(questionnaireStateTacker, questionnaireId);
        }

        private void AddQuestionnaireChangeItem(
            Guid id,
            Guid questionnaireId,
            Guid? creatorId,
            DateTime timestamp,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            int sequence,
            params QuestionnaireChangeReference[] references)
        {
            var questionnaireChangeItem = new QuestionnaireChangeRecord()
            {
                QuestionnaireChangeRecordId = id.FormatGuid(),
                QuestionnaireId = questionnaireId.FormatGuid(),
                UserId = creatorId ?? Guid.Empty,
                UserName = GetUserName(creatorId),
                Timestamp = timestamp,
                ActionType = actionType,
                TargetItemId = targetId,
                TargetItemTitle = targetTitle,
                TargetItemType = targetType,
                Sequence = sequence,
                References = references.ToHashSet()
            };
            questionnaireChangeItemStorage.Store(questionnaireChangeItem,
                questionnaireChangeItem.QuestionnaireChangeRecordId);
        }

        private QuestionnaireChangeReference CreateQuestionnaireChangeReference(
            QuestionnaireItemType referenceType, Guid id, string title)
        {
            return new QuestionnaireChangeReference() { ReferenceId = id, ReferenceType = referenceType, ReferenceTitle = title };
        }

        private string CreateTitle<T>(T target, Func<T, string> getVariableName, Func<T, string> getTitle)
        {
            return string.IsNullOrEmpty(getVariableName(target)) ? getTitle(target) : getVariableName(target);
        }

        private string CreateGroupTitleFromEvent(FullGroupDataEvent evnt)
        {
            return CreateTitle(evnt, q => q.VariableName, q => q.GroupText);
        }

        private string CreateQuestionTitleFromEvent(AbstractQuestionDataEvent evnt)
        {
            return CreateTitle(evnt, q => q.StataExportCaption, q => q.QuestionText);
        }

        private string CreateQuestionTitleFromEvent(AbstractQuestionUpdated evnt)
        {
            return CreateTitle(evnt, q => q.VariableName, q => q.Title);
        }

        private string CreateQuestionTitleFromEvent(AbstractListQuestionDataEvent evnt)
        {
            return CreateTitle(evnt, q => q.StataExportCaption, q => q.QuestionText);
        }

    }
}
