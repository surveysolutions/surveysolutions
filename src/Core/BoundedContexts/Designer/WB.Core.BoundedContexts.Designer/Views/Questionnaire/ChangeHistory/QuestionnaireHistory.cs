using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireHistory : IQuestionnaireHistory
    {
        private readonly IReadSideRepositoryWriter<AccountDocument> accountStorage;
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage;

        public QuestionnaireHistory(
            IReadSideRepositoryWriter<AccountDocument> accountStorage,
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage,
            IPlainKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage)
        {
            this.accountStorage = accountStorage;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.questionnaireStateTackerStorage = questionnaireStateTackerStorage;
        }

        public void Write(QuestionnaireCommand questionnaireCommand)
        {
            TypeSwitch.Do(questionnaireCommand, 
                //Questionnaire
                TypeSwitch.Case<CreateQuestionnaire>(this.Handle),
                TypeSwitch.Case<UpdateQuestionnaire>(this.Handle),
                TypeSwitch.Case<DeleteQuestionnaire>(this.Handle),
                TypeSwitch.Case<CloneQuestionnaire>(this.Handle),
                TypeSwitch.Case<ImportQuestionnaire>(this.Handle),
                //Shared persons
                TypeSwitch.Case<AddSharedPersonToQuestionnaire>(this.Handle),
                TypeSwitch.Case<RemoveSharedPersonFromQuestionnaire>(this.Handle),
                //Static texts
                TypeSwitch.Case<AddStaticText>(this.Handle),
                TypeSwitch.Case<UpdateStaticText>(this.Handle),
                TypeSwitch.Case<MoveStaticText>(this.Handle),
                TypeSwitch.Case<DeleteStaticText>(this.Handle),
                //Variables
                TypeSwitch.Case<AddVariable>(this.Handle),
                TypeSwitch.Case<UpdateVariable>(this.Handle),
                TypeSwitch.Case<MoveVariable>(this.Handle),
                TypeSwitch.Case<DeleteVariable>(this.Handle),
                //Macroses
                TypeSwitch.Case<AddMacro>(this.Handle),
                TypeSwitch.Case<UpdateMacro>(this.Handle),
                TypeSwitch.Case<DeleteMacro>(this.Handle),
                //Attachments
                TypeSwitch.Case<AddOrUpdateAttachment>(this.Handle),
                TypeSwitch.Case<DeleteAttachment>(this.Handle),
                //Translations
                TypeSwitch.Case<AddOrUpdateTranslation>(this.Handle),
                TypeSwitch.Case<DeleteTranslation>(this.Handle),
                //Groups and Roster
                TypeSwitch.Case<AddGroup>(this.Handle),
                TypeSwitch.Case<UpdateGroup>(this.Handle),
                TypeSwitch.Case<MoveGroup>(this.Handle),
                TypeSwitch.Case<DeleteGroup>(this.Handle),
                //Paste
                TypeSwitch.Case<PasteAfter>(this.Handle),
                TypeSwitch.Case<PasteInto>(this.Handle),
                //Questions
                TypeSwitch.Case<AddDefaultTypeQuestion>(this.Handle),
                TypeSwitch.Case<DeleteQuestion>(this.Handle),
                TypeSwitch.Case<MoveQuestion>(this.Handle),
                TypeSwitch.Case<UpdateMultimediaQuestion>(this.Handle),
                TypeSwitch.Case<UpdateDateTimeQuestion>(this.Handle),
                TypeSwitch.Case<UpdateNumericQuestion>(this.Handle),
                TypeSwitch.Case<UpdateQRBarcodeQuestion>(this.Handle),
                TypeSwitch.Case<UpdateGpsCoordinatesQuestion>(this.Handle),
                TypeSwitch.Case<UpdateTextListQuestion>(this.Handle),
                TypeSwitch.Case<UpdateTextQuestion>(this.Handle),
                TypeSwitch.Case<UpdateMultiOptionQuestion>(this.Handle),
                TypeSwitch.Case<UpdateSingleOptionQuestion>(this.Handle));
        }

        #region Questionnaire

        private void Handle(CreateQuestionnaire command)
        {
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title);

            this.questionnaireStateTackerStorage.Store(new QuestionnaireStateTracker() { CreatedBy = command.ResponsibleId }, command.QuestionnaireId.FormatGuid());

            this.AddOrUpdateGroupState(command.QuestionnaireId, command.QuestionnaireId, command.Title);
        }

        private void Handle(UpdateQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title);

        private void Handle(DeleteQuestionnaire command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            if (questionnaire == null)
                return;

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, questionnaire.CreatedBy,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Questionnaire, command.QuestionnaireId,
                questionnaire.GroupsState[command.QuestionnaireId]);
        }

        private void Handle(CloneQuestionnaire command)
        {
            var creatorId = command.Source.CreatedBy ?? Guid.Empty;

            UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, creatorId);

            #warning don't remove this line, it is here because clone event had been risen incorrectly for some time. ClonedFromQuestionnaireId were set equal EventSourceId, but should be Id of the colned questionnaire
            if (command.Source.PublicKey == command.QuestionnaireId)
            {
                this.AddQuestionnaireChangeItem(command.QuestionnaireId, creatorId, QuestionnaireActionType.Clone,
                    QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Source.Title);
            }
            else
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(command.Source.PublicKey.FormatGuid());

                var linkToQuestionnaire = this.CreateQuestionnaireChangeReference(QuestionnaireItemType.Questionnaire,
                    command.Source.PublicKey, questionnaire.GroupsState[command.Source.PublicKey]);

                this.AddQuestionnaireChangeItem(command.QuestionnaireId, creatorId, QuestionnaireActionType.Clone, QuestionnaireItemType.Questionnaire,
                    command.QuestionnaireId, command.Source.Title, linkToQuestionnaire);
            }
        }

        private void Handle(ImportQuestionnaire command)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.Source.CreatedBy.Value, 
                questionnaireStateTacker == null ? QuestionnaireActionType.Import : QuestionnaireActionType.Replace, QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Source.Title);

            this.UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, command.Source.CreatedBy ?? Guid.Empty);
        }
        #endregion

        #region Shared persons
        private void Handle(AddSharedPersonToQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId));

        private void Handle(RemoveSharedPersonFromQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId));

        #endregion

        #region Static Texts
        private void Handle(AddStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle);

            if(command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId);
        }

        private void Handle(UpdateStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle);
        }

        private void Handle(DeleteStaticText evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.QuestionnaireId.FormatGuid());
            var staticTitle = questionnaire.StaticTextState[evnt.EntityId];

            this.AddQuestionnaireChangeItem(evnt.QuestionnaireId, evnt.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.StaticText, evnt.EntityId, staticTitle);


            questionnaire.StaticTextState.Remove(evnt.EntityId);
            this.questionnaireStateTackerStorage.Store(questionnaire, evnt.QuestionnaireId.FormatGuid());
        }

        private void Handle(MoveStaticText command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId);
        #endregion

        #region Variables
        private void Handle(AddVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Variable, command.EntityId, variableName);

            if(command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId);
        }

        private void Handle(UpdateVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Variable, command.EntityId, variableName);
        }

        private void Handle(DeleteVariable command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            var variableName = questionnaire.VariableState[command.EntityId];

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Variable, command.EntityId, variableName);

            questionnaire.VariableState.Remove(command.EntityId);
            questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        private void Handle(MoveVariable command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId);
        #endregion

        #region Macroses
        private void Handle(AddMacro evnt)
        {
            this.AddOrUpdateMacroState(evnt.QuestionnaireId, evnt.MacroId, string.Empty);

            this.AddQuestionnaireChangeItem(evnt.QuestionnaireId, evnt.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Macro, evnt.MacroId, "Empty macro added");
        }

        private void Handle(UpdateMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, command.Name);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Macro, command.MacroId, command.Name);
        }

        private void Handle(DeleteMacro command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.MacroState, command.MacroId,
                QuestionnaireItemType.Macro, command.ResponsibleId);

        #endregion

        #region Lookup tables
        private void Handle(AddLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, string.Empty);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.LookupTable, command.LookupTableId, "Empty lookup table added");
        }

        private void Handle(UpdateLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, command.LookupTableName);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.LookupTable, command.LookupTableId, command.LookupTableName);
        }

        private void Handle(DeleteLookupTable command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.LookupState,
                command.LookupTableId, QuestionnaireItemType.LookupTable, command.ResponsibleId);

        #endregion

        #region Attachments
        private void Handle(AddOrUpdateAttachment command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.AttachmentId, command.AttachmentName, (s, id, title) => s.AttachmentState[id] = title);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Attachment, command.AttachmentId, command.AttachmentName);
        }

        private void Handle(DeleteAttachment command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.AttachmentState, command.AttachmentId,
                QuestionnaireItemType.Attachment, command.ResponsibleId);

        #endregion

        #region Translations
        private void Handle(AddOrUpdateTranslation command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.TranslationId, command.Name, (s, id, title) => s.TranslationState[id] = title);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Translation, command.TranslationId, command.Name);
        }

        private void Handle(DeleteTranslation command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.TranslationState, command.TranslationId,
                QuestionnaireItemType.Translation, command.ResponsibleId);

        #endregion

        #region Groups and Rosters
        private void Handle(AddGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Group, command.GroupId, groupTitle);

            this.UpdateRoster(command);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.GroupId, command.ParentGroupId, command.ResponsibleId);
        }

        private void Handle(UpdateGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle);

            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                questionnaire.GroupsState.ContainsKey(command.GroupId)
                    ? QuestionnaireItemType.Group
                    : QuestionnaireItemType.Roster, command.GroupId, groupTitle);

            this.UpdateRoster(command);
        }

        private void Handle(DeleteGroup command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());

            var isGroup = questionnaire.GroupsState.ContainsKey(command.GroupId);
            var groupTitle = isGroup ? questionnaire.GroupsState[command.GroupId] : questionnaire.RosterState[command.GroupId];

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                isGroup ? QuestionnaireItemType.Group : QuestionnaireItemType.Roster, command.GroupId, groupTitle);

            questionnaire.GroupsState.Remove(command.GroupId);
            questionnaire.RosterState.Remove(command.GroupId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        private void Handle(MoveGroup command)
            => this.MoveEntity(command.QuestionnaireId, command.GroupId, command.TargetGroupId, command.ResponsibleId);

        private void UpdateRoster(FullGroupDataCommand command)
        {
            if (command.IsRoster)
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
                if (questionnaire.RosterState.ContainsKey(command.GroupId))
                    return;

                var groupTitle = questionnaire.GroupsState[command.GroupId];

                questionnaire.RosterState[command.GroupId] = groupTitle;
                questionnaire.GroupsState.Remove(command.GroupId);

                this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
                this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId,
                    QuestionnaireActionType.GroupBecameARoster, QuestionnaireItemType.Group, command.GroupId, groupTitle);

                var rosterTitle = questionnaire.RosterState[command.GroupId];
                this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                    QuestionnaireItemType.Roster, command.GroupId, rosterTitle);
            }
            else
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());

                if (questionnaire.GroupsState.ContainsKey(command.GroupId))
                    return;

                var rosterTitle = questionnaire.RosterState[command.GroupId];

                questionnaire.GroupsState[command.GroupId] = rosterTitle;
                questionnaire.RosterState.Remove(command.GroupId);

                this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
                this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId,
                    QuestionnaireActionType.RosterBecameAGroup, QuestionnaireItemType.Roster, command.GroupId, rosterTitle);
            }
        }
        #endregion

        #region Paste
        private void Handle(PasteAfter evnt)
            => this.CloneEntity(evnt.SourceDocument, evnt.QuestionnaireId, evnt.EntityId, evnt.SourceItemId, evnt.ResponsibleId);

        private void Handle(PasteInto evnt)
            => this.CloneEntity(evnt.SourceDocument, evnt.QuestionnaireId, evnt.EntityId, evnt.SourceItemId, evnt.ResponsibleId);

        private void CloneEntity(QuestionnaireDocument sourceQuestionnaire, Guid questionnaireId, Guid targetEntityId, Guid sourceEntityId, Guid responsibleId)
        {
            QuestionnaireItemType entityType = (QuestionnaireItemType) (-1);
            string entityTitle = "";

            var sourceEntity = sourceQuestionnaire.Find<IComposite>(sourceEntityId);

            var entityAsStaticText = sourceEntity as IStaticText;
            var entityAsVariable = sourceEntity as IVariable;
            var entityAsGroup = sourceEntity as IGroup;
            var entityAsQuestion = sourceEntity as IQuestion;

            if (entityAsStaticText != null)
            {
                entityType = QuestionnaireItemType.StaticText;
                entityTitle = entityAsStaticText.Text;
            }

            if (entityAsQuestion != null)
            {
                entityType = QuestionnaireItemType.Question;
                entityTitle = entityAsQuestion.QuestionText ?? entityAsQuestion.StataExportCaption;
            }

            if (entityAsVariable != null)
            {
                entityType = QuestionnaireItemType.Variable;
                entityTitle = entityAsVariable.Name;
            }

            if (entityAsGroup != null)
            {
                entityType = entityAsGroup.IsRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Group;
                entityTitle = entityAsGroup.Title ?? entityAsGroup.VariableName;
            }


            this.AddOrUpdateQuestionState(questionnaireId, targetEntityId, entityTitle);

            var linkToEntity = this.CreateQuestionnaireChangeReference(entityType, sourceEntityId, entityTitle);
            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Clone, entityType,
                targetEntityId, entityTitle, linkToEntity);
        }
        #endregion

        #region Questions
        private void Handle(AddDefaultTypeQuestion command)
        {
            var questionTitle = command.Title ?? "";
            this.AddOrUpdateQuestionState(command.QuestionnaireId, command.QuestionId, questionTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle);

            if(command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.ParentGroupId, command.ResponsibleId);
        }

        private void Handle(DeleteQuestion evnt)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(evnt.QuestionnaireId.FormatGuid());
            var questionTitle = questionnaire.QuestionsState[evnt.QuestionId];

            this.AddQuestionnaireChangeItem(evnt.QuestionnaireId, evnt.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Question, evnt.QuestionId, questionTitle);

            questionnaire.QuestionsState.Remove(evnt.QuestionId);
            this.questionnaireStateTackerStorage.Store(questionnaire, evnt.QuestionnaireId.FormatGuid());
        }

        private void Handle(MoveQuestion command)
            => this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.TargetGroupId, command.ResponsibleId);

        private void Handle(AbstractUpdateQuestionCommand evnt)
        {
            var questionTitle = evnt.VariableName ?? evnt.Title;

            this.AddOrUpdateQuestionState(evnt.QuestionnaireId, evnt.QuestionId, questionTitle);
            this.AddQuestionnaireChangeItem(evnt.QuestionnaireId, evnt.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Question, evnt.QuestionId, questionTitle);
        }

        #endregion

        private void MoveEntity(Guid questionnaireId, Guid entityId, Guid? targetGroupOrRosterId, Guid responsibleId)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            var moveReferences = new List<QuestionnaireChangeReference>();
            if (targetGroupOrRosterId.HasValue)
            {
                var targetGroupId = targetGroupOrRosterId ?? questionnaireId;
                var isTargetGroupRoster = questionnaire.RosterState.ContainsKey(targetGroupId);
                var targetGroupTitle = isTargetGroupRoster
                    ? questionnaire.RosterState[targetGroupId]
                    : questionnaire.GroupsState[targetGroupId];
                moveReferences.Add(CreateQuestionnaireChangeReference(
                    isTargetGroupRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Group,
                    targetGroupId, targetGroupTitle));
            }

            QuestionnaireItemType movedItemType;
            string moveditemTitle;

            if (questionnaire.QuestionsState.ContainsKey(entityId))
            {
                movedItemType = QuestionnaireItemType.Question;
                moveditemTitle = questionnaire.QuestionsState[entityId];
            }
            else if (questionnaire.GroupsState.ContainsKey(entityId))
            {
                movedItemType = QuestionnaireItemType.Group;
                moveditemTitle = questionnaire.GroupsState[entityId];
            }
            else if (questionnaire.RosterState.ContainsKey(entityId))
            {
                movedItemType = QuestionnaireItemType.Roster;
                moveditemTitle = questionnaire.RosterState[entityId];
            }
            else if (questionnaire.StaticTextState.ContainsKey(entityId))
            {
                movedItemType = QuestionnaireItemType.StaticText;
                moveditemTitle = questionnaire.StaticTextState[entityId];
            }
            else if (questionnaire.VariableState.ContainsKey(entityId))
            {
                movedItemType = QuestionnaireItemType.Variable;
                moveditemTitle = questionnaire.VariableState[entityId];
            }
            else
            {
                return;
            }

            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Move, movedItemType,
                entityId, moveditemTitle, moveReferences.ToArray());
        }

        private void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            params QuestionnaireChangeReference[] references)
        {
            var sQuestionnaireId = questionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage.Query(x => x.
                    Where(y => y.QuestionnaireId == sQuestionnaireId).
                    Select(y => (int?)y.Sequence).
                    Max());

            var questionnaireChangeItem = new QuestionnaireChangeRecord()
            {
                QuestionnaireChangeRecordId = Guid.NewGuid().FormatGuid(),
                QuestionnaireId = questionnaireId.FormatGuid(),
                UserId = responsibleId,
                UserName = this.GetUserName(responsibleId),
                Timestamp = DateTime.UtcNow,
                Sequence = maxSequenceByQuestionnaire + 1 ?? 0,
                ActionType = actionType,
                TargetItemId = targetId,
                TargetItemTitle = targetTitle,
                TargetItemType = targetType,
                References = references.ToHashSet()
            };
            this.questionnaireChangeItemStorage.Store(questionnaireChangeItem,
                questionnaireChangeItem.QuestionnaireChangeRecordId);
        }

        private string GetUserName(Guid? userId)
        {
            if (userId.HasValue)
            {
                var creator = this.accountStorage.GetById(userId);
                if (creator != null)
                    return creator.UserName;
            }
            return null;
        }

        private void AddOrUpdateQuestionnaireStateItem(Guid questionnaireId, Guid itemId, string itemTitle,
            Action<QuestionnaireStateTracker, Guid, string> setAction)
        {
            var questionnaireStateTacker = this.questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            setAction(questionnaireStateTacker, itemId, itemTitle);

            this.questionnaireStateTackerStorage.Store(questionnaireStateTacker, questionnaireId.FormatGuid());
        }

        private void UpdateFullQuestionnaireState(QuestionnaireDocument questionnaireDocument, Guid questionnaireId, Guid createdBy)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaireStateTacker == null)
                questionnaireStateTacker = new QuestionnaireStateTracker() { CreatedBy = createdBy };

            questionnaireStateTacker.GroupsState[questionnaireId] = questionnaireDocument.Title;

            var compositeElements = questionnaireDocument.Find<IComposite>(c => true);
            foreach (var compositeElement in compositeElements)
            {
                var question = compositeElement as IQuestion;
                if (question != null)
                {
                    questionnaireStateTacker.QuestionsState[question.PublicKey] = question.StataExportCaption ??
                                                                                  question.QuestionText;
                    continue;
                }
                var group = compositeElement as IGroup;
                if (group != null)
                {
                    var groupTitle = @group.VariableName ?? @group.Title;
                    if (group.IsRoster)
                        questionnaireStateTacker.RosterState[group.PublicKey] = groupTitle;
                    else
                        questionnaireStateTacker.GroupsState[group.PublicKey] = groupTitle;
                    continue;
                }
                var staticTexts = compositeElement as IStaticText;
                if (staticTexts != null)
                {
                    questionnaireStateTacker.StaticTextState[staticTexts.PublicKey] = staticTexts.Text;
                    continue;
                }
                var variable = compositeElement as IVariable;
                if (variable != null)
                {
                    questionnaireStateTacker.VariableState[variable.PublicKey] = variable.Name;
                }
            }
            questionnaireStateTackerStorage.Store(questionnaireStateTacker, questionnaireId.FormatGuid());
        }

        private void DeleteItemFromStateAndUpdateHistory(Guid questionnaireId,
            Func<QuestionnaireStateTracker, Dictionary<Guid, string>> state, Guid itemId, QuestionnaireItemType itemType,
            Guid responsibleId)
        {
            QuestionnaireStateTracker questionnaire = this.questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            string itemName = "";

            state(questionnaire).TryGetValue(itemId, out itemName);

            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Delete, itemType,
                itemId, itemName);

            state(questionnaire).Remove(itemId);
            this.questionnaireStateTackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
        }

        private void AddOrUpdateQuestionState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.QuestionsState[id] = title);
        }

        private void AddOrUpdateGroupState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) =>
                {
                    if (s.RosterState.ContainsKey(id))
                        s.RosterState[id] = title;
                    else
                        s.GroupsState[id] = title;
                });
        }

        private void AddOrUpdateLookupTableState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.LookupState[id] = title);
        }

        private void AddOrUpdateMacroState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => s.MacroState[id] = title);
        }

        private void AddOrUpdateStaticTextState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => { s.StaticTextState[id] = title; });
        }

        private void AddOrUpdateVariableState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                (s, id, title) => { s.VariableState[id] = title; });
        }
        
        private QuestionnaireChangeReference CreateQuestionnaireChangeReference(QuestionnaireItemType referenceType, Guid id, string title)
            => new QuestionnaireChangeReference()
            {
                ReferenceId = id,
                ReferenceType = referenceType,
                ReferenceTitle = title
            };
    }
}