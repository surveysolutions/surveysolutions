using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
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
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class HistoryPostProcessor : 
        ICommandPostProcessor<Questionnaire, ImportQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CloneQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CreateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>,
        ICommandPostProcessor<Questionnaire, AddSharedPersonToQuestionnaire>,
        ICommandPostProcessor<Questionnaire, RemoveSharedPersonFromQuestionnaire>,
        ICommandPostProcessor<Questionnaire, AddStaticText>,
        ICommandPostProcessor<Questionnaire, UpdateStaticText>,
        ICommandPostProcessor<Questionnaire, MoveStaticText>,
        ICommandPostProcessor<Questionnaire, DeleteStaticText>,
        ICommandPostProcessor<Questionnaire, AddVariable>,
        ICommandPostProcessor<Questionnaire, UpdateVariable>,
        ICommandPostProcessor<Questionnaire, MoveVariable>,
        ICommandPostProcessor<Questionnaire, DeleteVariable>,
        ICommandPostProcessor<Questionnaire, AddMacro>,
        ICommandPostProcessor<Questionnaire, UpdateMacro>,
        ICommandPostProcessor<Questionnaire, DeleteMacro>,
        ICommandPostProcessor<Questionnaire, AddOrUpdateAttachment>,
        ICommandPostProcessor<Questionnaire, DeleteAttachment>,
        ICommandPostProcessor<Questionnaire, AddOrUpdateTranslation>,
        ICommandPostProcessor<Questionnaire, DeleteTranslation>,
        ICommandPostProcessor<Questionnaire, AddGroup>,
        ICommandPostProcessor<Questionnaire, UpdateGroup>,
        ICommandPostProcessor<Questionnaire, MoveGroup>,
        ICommandPostProcessor<Questionnaire, DeleteGroup>,
        ICommandPostProcessor<Questionnaire, PasteAfter>,
        ICommandPostProcessor<Questionnaire, PasteInto>,
        ICommandPostProcessor<Questionnaire, AddDefaultTypeQuestion>,
        ICommandPostProcessor<Questionnaire, DeleteQuestion>,
        ICommandPostProcessor<Questionnaire, MoveQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultimediaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateDateTimeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateNumericQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateQRBarcodeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateGpsCoordinatesQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextListQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultiOptionQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateSingleOptionQuestion>,
        ICommandPostProcessor<Questionnaire, AddLookupTable>,
        ICommandPostProcessor<Questionnaire, UpdateLookupTable>,
        ICommandPostProcessor<Questionnaire, DeleteLookupTable>
    {
        private IReadSideRepositoryWriter<AccountDocument> accountStorage
            => ServiceLocator.Current.GetInstance<IReadSideRepositoryWriter<AccountDocument>>();

        private IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireChangeRecord>>();

        private IPlainKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage
            => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireStateTracker>>();

        #region Questionnaire

        public void Process(Questionnaire aggregate, CreateQuestionnaire command)
        {
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title);

            this.questionnaireStateTackerStorage.Store(new QuestionnaireStateTracker() { CreatedBy = command.ResponsibleId }, command.QuestionnaireId.FormatGuid());

            this.AddOrUpdateGroupState(command.QuestionnaireId, command.QuestionnaireId, command.Title);
        }

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title);

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            if (questionnaire == null)
                return;

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, questionnaire.CreatedBy,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Questionnaire, command.QuestionnaireId,
                questionnaire.GroupsState[command.QuestionnaireId]);
        }

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
        {
            var creatorId = command.Source.CreatedBy ?? Guid.Empty;

            UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, creatorId);

            var linkToQuestionnaire = this.CreateQuestionnaireChangeReference(QuestionnaireItemType.Questionnaire,
                command.Source.PublicKey, command.Source.Title);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, creatorId, QuestionnaireActionType.Clone,
                QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Title, linkToQuestionnaire);
        }

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.Source.CreatedBy.Value,
                questionnaireStateTacker == null ? QuestionnaireActionType.Import : QuestionnaireActionType.Replace, QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Source.Title);

            this.UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, command.Source.CreatedBy ?? Guid.Empty);
        }
        #endregion

        #region Shared persons
        public void Process(Questionnaire aggregate, AddSharedPersonToQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId));

        public void Process(Questionnaire aggregate, RemoveSharedPersonFromQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId));

        #endregion

        #region Static Texts
        public void Process(Questionnaire aggregate, AddStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId);
        }

        public void Process(Questionnaire aggregate, UpdateStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string staticTitle;
            questionnaire.StaticTextState.TryGetValue(command.EntityId, out staticTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.StaticText, command.EntityId, staticTitle);


            questionnaire.StaticTextState.Remove(command.EntityId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveStaticText command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId);
        #endregion

        #region Variables
        public void Process(Questionnaire aggregate, AddVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Variable, command.EntityId, variableName);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId);
        }

        public void Process(Questionnaire aggregate, UpdateVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Variable, command.EntityId, variableName);
        }

        public void Process(Questionnaire aggregate, DeleteVariable command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string variableName;
            questionnaire.VariableState.TryGetValue(command.EntityId, out variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Variable, command.EntityId, variableName);

            questionnaire.VariableState.Remove(command.EntityId);
            questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveVariable command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId);
        #endregion

        #region Macroses
        public void Process(Questionnaire aggregate, AddMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, string.Empty);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Macro, command.MacroId, "Empty macro added");
        }

        public void Process(Questionnaire aggregate, UpdateMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, command.Name);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Macro, command.MacroId, command.Name);
        }

        public void Process(Questionnaire aggregate, DeleteMacro command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.MacroState, command.MacroId,
                QuestionnaireItemType.Macro, command.ResponsibleId);

        #endregion

        #region Lookup tables
        public void Process(Questionnaire aggregate, AddLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, string.Empty);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.LookupTable, command.LookupTableId, "Empty lookup table added");
        }

        public void Process(Questionnaire aggregate, UpdateLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, command.LookupTableName);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.LookupTable, command.LookupTableId, command.LookupTableName);
        }

        public void Process(Questionnaire aggregate, DeleteLookupTable command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.LookupState,
                command.LookupTableId, QuestionnaireItemType.LookupTable, command.ResponsibleId);

        #endregion

        #region Attachments
        public void Process(Questionnaire aggregate, AddOrUpdateAttachment command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.AttachmentId, command.AttachmentName, (s, id, title) => s.AttachmentState[id] = title);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Attachment, command.AttachmentId, command.AttachmentName);
        }

        public void Process(Questionnaire aggregate, DeleteAttachment command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.AttachmentState, command.AttachmentId,
                QuestionnaireItemType.Attachment, command.ResponsibleId);

        #endregion

        #region Translations
        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.TranslationId, command.Name, (s, id, title) => s.TranslationState[id] = title);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Translation, command.TranslationId, command.Name);
        }

        public void Process(Questionnaire aggregate, DeleteTranslation command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.TranslationState, command.TranslationId,
                QuestionnaireItemType.Translation, command.ResponsibleId);

        #endregion

        #region Groups and Rosters
        public void Process(Questionnaire aggregate, AddGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Group, command.GroupId, groupTitle);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.GroupId, command.ParentGroupId, command.ResponsibleId);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle);

            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                questionnaire.GroupsState.ContainsKey(command.GroupId)
                    ? QuestionnaireItemType.Group
                    : QuestionnaireItemType.Roster, command.GroupId, groupTitle);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());

            var isGroup = questionnaire.GroupsState.ContainsKey(command.GroupId);
            string groupTitle;
            if (isGroup)
                questionnaire.GroupsState.TryGetValue(command.GroupId, out groupTitle);
            else
                questionnaire.RosterState.TryGetValue(command.GroupId, out groupTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                isGroup ? QuestionnaireItemType.Group : QuestionnaireItemType.Roster, command.GroupId, groupTitle);

            questionnaire.GroupsState.Remove(command.GroupId);
            questionnaire.RosterState.Remove(command.GroupId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveGroup command)
            => this.MoveEntity(command.QuestionnaireId, command.GroupId, command.TargetGroupId, command.ResponsibleId);

        private void UpdateRoster(Guid questionnaireId, bool isRoster, Guid groupId, Guid responsibleId)
        {
            if (isRoster)
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());
                if (questionnaire.RosterState.ContainsKey(groupId))
                    return;

                var groupTitle = questionnaire.GroupsState[groupId];

                questionnaire.RosterState[groupId] = groupTitle;
                questionnaire.GroupsState.Remove(groupId);

                this.questionnaireStateTackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId,
                    QuestionnaireActionType.GroupBecameARoster, QuestionnaireItemType.Group, groupId, groupTitle);

                var rosterTitle = questionnaire.RosterState[groupId];
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Update,
                    QuestionnaireItemType.Roster, groupId, rosterTitle);
            }
            else
            {
                var questionnaire = questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

                if (questionnaire.GroupsState.ContainsKey(groupId))
                    return;

                var rosterTitle = questionnaire.RosterState[groupId];

                questionnaire.GroupsState[groupId] = rosterTitle;
                questionnaire.RosterState.Remove(groupId);

                this.questionnaireStateTackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId,
                    QuestionnaireActionType.RosterBecameAGroup, QuestionnaireItemType.Roster, groupId, rosterTitle);
            }
        }
        #endregion

        #region Paste
        public void Process(Questionnaire aggregate, PasteAfter command)
            => this.CloneEntity(command.SourceDocument, command.QuestionnaireId, command.EntityId, command.SourceItemId, command.ResponsibleId);

        public void Process(Questionnaire aggregate, PasteInto command)
            => this.CloneEntity(command.SourceDocument, command.QuestionnaireId, command.EntityId, command.SourceItemId, command.ResponsibleId);

        private void CloneEntity(QuestionnaireDocument sourceQuestionnaire, Guid questionnaireId, Guid targetEntityId, Guid sourceEntityId, Guid responsibleId)
        {
            QuestionnaireItemType entityType = (QuestionnaireItemType)(-1);
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
                this.AddOrUpdateStaticTextState(questionnaireId, targetEntityId, entityTitle);
            }

            if (entityAsQuestion != null)
            {
                entityType = QuestionnaireItemType.Question;
                entityTitle = entityAsQuestion.QuestionText ?? entityAsQuestion.StataExportCaption;
                this.AddOrUpdateQuestionState(questionnaireId, targetEntityId, entityTitle);
            }

            if (entityAsVariable != null)
            {
                entityType = QuestionnaireItemType.Variable;
                entityTitle = entityAsVariable.Name;
                this.AddOrUpdateVariableState(questionnaireId, targetEntityId, entityTitle);
            }

            if (entityAsGroup != null)
            {
                entityType = entityAsGroup.IsRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Group;
                entityTitle = entityAsGroup.Title ?? entityAsGroup.VariableName;
                this.AddOrUpdateGroupState(questionnaireId, targetEntityId, entityTitle);
            }

            var linkToEntity = this.CreateQuestionnaireChangeReference(entityType, sourceEntityId, entityTitle);
            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Clone, entityType,
                targetEntityId, entityTitle, linkToEntity);
        }
        #endregion

        #region Questions
        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command)
        {
            var questionTitle = command.Title ?? "";
            this.AddOrUpdateQuestionState(command.QuestionnaireId, command.QuestionId, questionTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.ParentGroupId, command.ResponsibleId);
        }

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string questionTitle;
            questionnaire.QuestionsState.TryGetValue(command.QuestionId, out questionTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle);

            questionnaire.QuestionsState.Remove(command.QuestionId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveQuestion command)
            => this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.TargetGroupId, command.ResponsibleId);

        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateNumericQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateTextListQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateTextQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command) => this.AddQuestionChanges(command);
        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command)=> this.AddQuestionChanges(command);

        private void AddQuestionChanges(AbstractUpdateQuestionCommand command)
        {
            var questionTitle = command.VariableName ?? command.Title;

            this.AddOrUpdateQuestionState(command.QuestionnaireId, command.QuestionId, questionTitle);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle);
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

            this.questionnaireChangeItemStorage.Store(questionnaireChangeItem, questionnaireChangeItem.QuestionnaireChangeRecordId);
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