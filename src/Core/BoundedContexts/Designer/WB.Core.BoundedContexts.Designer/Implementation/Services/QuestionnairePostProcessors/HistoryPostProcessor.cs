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
using WB.Core.BoundedContexts.Designer.Services;
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
        ICommandPostProcessor<Questionnaire, DeleteLookupTable>,
        ICommandPostProcessor<Questionnaire, ReplaceTextsCommand>,
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>
    {
        private IPlainStorageAccessor<User> accountStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<User>>();

        private IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireChangeRecord>>();

        private IPlainKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTackerStorage
            => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireStateTracker>>();

        private IQuestionnireHistotyVersionsService questionnireHistotyVersionsService 
            => ServiceLocator.Current.GetInstance<IQuestionnireHistotyVersionsService>();

        #region Questionnaire

        public void Process(Questionnaire aggregate, CreateQuestionnaire command)
        {
            UpdateFullQuestionnaireState(aggregate.QuestionnaireDocument, command.QuestionnaireId, command.ResponsibleId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
        {
            var questionnaireId = command.QuestionnaireId;

            this.UpdateQuestionnaireTitleIfNeed(questionnaireId, command.Title);

            this.AddQuestionnaireChangeItem(questionnaireId, command.ResponsibleId,
                QuestionnaireActionType.Update,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title, aggregate.QuestionnaireDocument);
        }

        private void UpdateQuestionnaireTitleIfNeed(Guid questionnaireId, string title)
        {

            var questionnaireStateTacker = this.questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());
            var titleInState = questionnaireStateTacker.GroupsState[questionnaireId];
            if (titleInState != title)
            {
                questionnaireStateTacker.GroupsState[questionnaireId] = title;
                this.questionnaireStateTackerStorage.Store(questionnaireStateTacker, questionnaireId.FormatGuid());
            }
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            if (questionnaire == null)
                return;

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, questionnaire.CreatedBy,
                QuestionnaireActionType.Delete, QuestionnaireItemType.Questionnaire, command.QuestionnaireId,
                questionnaire.GroupsState[command.QuestionnaireId], null);
        }

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
        {
            var creatorId = command.Source.CreatedBy ?? Guid.Empty;

            UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, creatorId);

            var linkToQuestionnaire = this.CreateQuestionnaireChangeReference(QuestionnaireItemType.Questionnaire,
                command.Source.PublicKey, command.Source.Title);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, creatorId, QuestionnaireActionType.Clone,
                QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Title, null, linkToQuestionnaire);
        }

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
        {
            var questionnaireStateTacker = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.Source.CreatedBy.Value,
                questionnaireStateTacker == null ? QuestionnaireActionType.Import : QuestionnaireActionType.Replace, QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Source.Title, aggregate.QuestionnaireDocument);

            this.UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, command.Source.CreatedBy ?? Guid.Empty);
        }
        #endregion

        #region Shared persons
        public void Process(Questionnaire aggregate, AddSharedPersonToQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId), null);

        public void Process(Questionnaire aggregate, RemoveSharedPersonFromQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserName(command.PersonId), null);

        #endregion

        #region Static Texts
        public void Process(Questionnaire aggregate, AddStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle, command.ParentId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle, aggregate.QuestionnaireDocument);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateStaticText command)
        {
            var staticTextTitle = command.Text;
            this.AddOrUpdateStaticTextState(command.QuestionnaireId, command.EntityId, staticTextTitle, parentId: null);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.StaticText, command.EntityId, staticTextTitle, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string staticTitle;
            questionnaire.StaticTextState.TryGetValue(command.EntityId, out staticTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.StaticText, command.EntityId, staticTitle, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.EntityId);
            questionnaire.StaticTextState.Remove(command.EntityId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveStaticText command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        #endregion

        #region Variables
        public void Process(Questionnaire aggregate, AddVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName, command.ParentId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Variable, command.EntityId, variableName, aggregate.QuestionnaireDocument);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.EntityId, command.ParentId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateVariable command)
        {
            var variableName = command.VariableData.Name;
            this.AddOrUpdateVariableState(command.QuestionnaireId, command.EntityId, variableName, parentId: null);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Variable, command.EntityId, variableName, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteVariable command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string variableName;
            questionnaire.VariableState.TryGetValue(command.EntityId, out variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Variable, command.EntityId, variableName, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.EntityId);
            questionnaire.VariableState.Remove(command.EntityId);
            questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveVariable command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        #endregion

        #region Macroses
        public void Process(Questionnaire aggregate, AddMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, string.Empty);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Macro, command.MacroId, "Empty macro added", aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, command.Name);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Macro, command.MacroId, command.Name, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteMacro command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.MacroState, command.MacroId,
                QuestionnaireItemType.Macro, command.ResponsibleId, aggregate.QuestionnaireDocument);

        #endregion

        #region Lookup tables
        public void Process(Questionnaire aggregate, AddLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, string.Empty);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.LookupTable, command.LookupTableId, "Empty lookup table added", aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateLookupTable command)
        {
            this.AddOrUpdateLookupTableState(command.QuestionnaireId, command.LookupTableId, command.LookupTableName);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.LookupTable, command.LookupTableId, command.LookupTableName, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteLookupTable command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.LookupState,
                command.LookupTableId, QuestionnaireItemType.LookupTable, command.ResponsibleId, aggregate.QuestionnaireDocument);

        #endregion

        #region Attachments
        public void Process(Questionnaire aggregate, AddOrUpdateAttachment command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.AttachmentId, command.AttachmentName,
                parentId: null, setAction: (s, id, title) => s.AttachmentState[id] = title);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Attachment, command.AttachmentId, command.AttachmentName, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteAttachment command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.AttachmentState, command.AttachmentId,
                QuestionnaireItemType.Attachment, command.ResponsibleId, aggregate.QuestionnaireDocument);

        #endregion

        #region Translations
        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command)
        {
            this.AddOrUpdateQuestionnaireStateItem(command.QuestionnaireId, command.TranslationId, command.Name,
                parentId: null, setAction: (s, id, title) => s.TranslationState[id] = title);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Translation, command.TranslationId, command.Name, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteTranslation command)
            => this.DeleteItemFromStateAndUpdateHistory(command.QuestionnaireId, q => q.TranslationState, command.TranslationId,
                QuestionnaireItemType.Translation, command.ResponsibleId, aggregate.QuestionnaireDocument);

        #endregion

        #region Groups and Rosters
        public void Process(Questionnaire aggregate, AddGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle, command.ParentGroupId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Group, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.GroupId, command.ParentGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle, parentId: null);

            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                questionnaire.GroupsState.ContainsKey(command.GroupId)
                    ? QuestionnaireItemType.Group
                    : QuestionnaireItemType.Roster, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);
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
                isGroup ? QuestionnaireItemType.Group : QuestionnaireItemType.Roster, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            questionnaire.RemoveCascadely(command.GroupId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveGroup command)
            => this.MoveEntity(command.QuestionnaireId, command.GroupId, command.TargetGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);

        private void UpdateRoster(Guid questionnaireId, bool isRoster, Guid groupId, Guid responsibleId, QuestionnaireDocument questionnaireDocument)
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
                    QuestionnaireActionType.GroupBecameARoster, QuestionnaireItemType.Group, groupId, groupTitle, questionnaireDocument);

                var rosterTitle = questionnaire.RosterState[groupId];
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Update,
                    QuestionnaireItemType.Roster, groupId, rosterTitle, questionnaireDocument);
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
                    QuestionnaireActionType.RosterBecameAGroup, QuestionnaireItemType.Roster, groupId, rosterTitle, questionnaireDocument);
            }
        }
        #endregion

        #region Paste
        public void Process(Questionnaire aggregate, PasteAfter command)
            => this.CloneEntity(aggregate, command.SourceDocument, command.QuestionnaireId, command.EntityId, command.SourceItemId, command.ResponsibleId);

        public void Process(Questionnaire aggregate, PasteInto command)
            => this.CloneEntity(aggregate, command.SourceDocument, command.QuestionnaireId, command.EntityId, command.SourceItemId, command.ResponsibleId);

        private void CloneEntity(Questionnaire aggregate, QuestionnaireDocument sourceQuestionnaire, Guid questionnaireId, Guid targetEntityId, Guid sourceEntityId, Guid responsibleId)
        {
            QuestionnaireItemType entityType = (QuestionnaireItemType)(-1);
            string entityTitle = "";

            var entities = new List<IComposite>();

            var pasteEntity = aggregate.QuestionnaireDocument.Find<IComposite>(targetEntityId);

            if (pasteEntity is IGroup)
            {
                entities.AddRange(pasteEntity.TreeToEnumerable(x => x.Children).Reverse().ToList());
            }
            else
            {
                entities.Add(pasteEntity);
            }


            foreach (var entity in entities)
            {
                var parentId = entity.GetParent().PublicKey;

                var entityAsStaticText = entity as IStaticText;
                var entityAsVariable = entity as IVariable;
                var entityAsGroup = entity as IGroup;
                var entityAsQuestion = entity as IQuestion;

                if (entityAsGroup != null)
                {
                    entityType = entityAsGroup.IsRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Group;
                    entityTitle = entityAsGroup.Title ?? entityAsGroup.VariableName;
                    this.AddOrUpdateGroupState(questionnaireId, entityAsGroup.PublicKey, entityTitle, parentId);
                    continue;
                }

                if (entityAsStaticText != null)
                {
                    entityType = QuestionnaireItemType.StaticText;
                    entityTitle = entityAsStaticText.Text;
                    this.AddOrUpdateStaticTextState(questionnaireId, entityAsStaticText.PublicKey, entityTitle, parentId);
                    continue;
                }

                if (entityAsQuestion != null)
                {
                    entityType = QuestionnaireItemType.Question;
                    entityTitle = entityAsQuestion.QuestionText ?? entityAsQuestion.StataExportCaption;
                    this.AddOrUpdateQuestionState(questionnaireId, entityAsQuestion.PublicKey, entityTitle, parentId);
                    continue;
                }

                if (entityAsVariable != null)
                {
                    entityType = QuestionnaireItemType.Variable;
                    entityTitle = entityAsVariable.Name;
                    this.AddOrUpdateVariableState(questionnaireId, entityAsVariable.PublicKey, entityTitle, parentId);
                    continue;
                }

                this.AddOrUpdateGroupState(questionnaireId, targetEntityId, entityTitle, parentId);
            }

            var linkToEntity = this.CreateQuestionnaireChangeReference(entityType, sourceEntityId, entityTitle);
            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Clone, entityType,
                targetEntityId, entityTitle, aggregate.QuestionnaireDocument, linkToEntity);
        }
        #endregion

        #region Questions
        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command)
        {
            var questionTitle = command.Title ?? "";
            this.AddOrUpdateQuestionState(command.QuestionnaireId, command.QuestionId, questionTitle, command.ParentGroupId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle, aggregate.QuestionnaireDocument);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.ParentGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            var questionnaire = questionnaireStateTackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string questionTitle;
            questionnaire.QuestionsState.TryGetValue(command.QuestionId, out questionTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.QuestionId);
            questionnaire.QuestionsState.Remove(command.QuestionId);
            this.questionnaireStateTackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveQuestion command)
            => this.MoveEntity(command.QuestionnaireId, command.QuestionId, command.TargetGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);

        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateNumericQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateTextListQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateTextQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command)=> this.AddQuestionChanges(aggregate, command);

        private void AddQuestionChanges(Questionnaire aggregate, AbstractUpdateQuestionCommand command)
        {
            var questionTitle = command.VariableName ?? command.Title;

            this.AddOrUpdateQuestionState(command.QuestionnaireId, command.QuestionId, questionTitle, parentId: null);
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle, aggregate.QuestionnaireDocument);
        }

        #endregion

        private void MoveEntity(Guid questionnaireId, Guid entityId, Guid? targetGroupOrRosterId, Guid responsibleId, QuestionnaireDocument questionnaireDocument)
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
                entityId, moveditemTitle, questionnaireDocument, moveReferences.ToArray());
        }

        private void AddQuestionnaireChangeItem(Guid questionnaireId, Guid responsibleId, QuestionnaireActionType actionType, QuestionnaireItemType targetType, Guid targetId, string targetTitle, QuestionnaireDocument questionnaireDocument, params QuestionnaireChangeReference[] references)
        {
            AddQuestionnaireChangeItem(questionnaireId,
                responsibleId,
                actionType,
                targetType,
                targetId,
                targetTitle,
                null,
                null,
                null,
                questionnaireDocument,
                references);
        }

        private void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            string targetNewTitle,
            int? affecedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument questionnaireDocument,
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
                TargetItemNewTitle = targetNewTitle,
                AffectedEntriesCount = affecedEntries,
                TargetItemDateTime = targetDateTime,
            };

            references.ForEach(r => r.QuestionnaireChangeRecord = questionnaireChangeItem);
            questionnaireChangeItem.References = references.ToHashSet();
            questionnaireChangeItem.ResultingQuestionnaireDocument = questionnireHistotyVersionsService.GetResultingQuestionnaireDocument(questionnaireDocument);

            this.questionnaireChangeItemStorage.Store(questionnaireChangeItem, questionnaireChangeItem.QuestionnaireChangeRecordId);
        }

        private string GetUserName(Guid? userId)
        {
            if (userId.HasValue)
            {
                var creator = this.accountStorage.GetById(userId.Value.FormatGuid());
                if (creator != null)
                    return creator.UserName;
            }
            return null;
        }

        private void AddOrUpdateQuestionnaireStateItem(Guid questionnaireId, Guid itemId, string itemTitle,
            Guid? parentId, Action<QuestionnaireStateTracker, Guid, string> setAction)
        {
            var questionnaireStateTacker = this.questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            if (parentId.HasValue)
            {
                questionnaireStateTacker.Parents[itemId] = parentId;
            }

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
                questionnaireStateTacker.Parents[compositeElement.PublicKey] = compositeElement.GetParent()?.PublicKey;

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
            Guid responsibleId, QuestionnaireDocument questionnaireDocument)
        {
            QuestionnaireStateTracker questionnaire = this.questionnaireStateTackerStorage.GetById(questionnaireId.FormatGuid());

            string itemName = "";

            state(questionnaire).TryGetValue(itemId, out itemName);

            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Delete, itemType,
                itemId, itemName, questionnaireDocument);

            questionnaire.Parents.Remove(itemId);
            state(questionnaire).Remove(itemId);
            this.questionnaireStateTackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
        }

        private void AddOrUpdateQuestionState(Guid questionnaireId, Guid itemId, string itemTitle, Guid? parentId)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                parentId: parentId, setAction: (s, id, title) => s.QuestionsState[id] = title);
        }

        private void AddOrUpdateGroupState(Guid questionnaireId, Guid itemId, string itemTitle, Guid? parentId)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                parentId: parentId, setAction: (s, id, title) =>
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
                parentId: null, setAction: (s, id, title) => s.LookupState[id] = title);
        }

        private void AddOrUpdateMacroState(Guid questionnaireId, Guid itemId, string itemTitle)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                parentId: null, setAction: (s, id, title) => s.MacroState[id] = title);
        }

        private void AddOrUpdateStaticTextState(Guid questionnaireId, Guid itemId, string itemTitle, Guid? parentId)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                parentId: parentId, setAction: (s, id, title) => { s.StaticTextState[id] = title; });
        }

        private void AddOrUpdateVariableState(Guid questionnaireId, Guid itemId, string itemTitle, Guid? parentId)
        {
            AddOrUpdateQuestionnaireStateItem(questionnaireId, itemId, itemTitle,
                parentId: parentId, setAction: (s, id, title) => { s.VariableState[id] = title; });
        }

        private QuestionnaireChangeReference CreateQuestionnaireChangeReference(QuestionnaireItemType referenceType, Guid id, string title)
            => new QuestionnaireChangeReference
            {
                ReferenceId = id,
                ReferenceType = referenceType,
                ReferenceTitle = title
            };

        public void Process(Questionnaire aggregate, ReplaceTextsCommand command)
        {
            AddQuestionnaireChangeItem(command.QuestionnaireId, 
                command.ResponsibleId, 
                QuestionnaireActionType.ReplaceAllTexts, 
                QuestionnaireItemType.Questionnaire, 
                command.QuestionnaireId,
                command.SearchFor,
                command.ReplaceWith, 
                aggregate.GetLastReplacedEntriesCount(),
                null,
                aggregate.QuestionnaireDocument
                );
        }

        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
        {
            questionnaireStateTackerStorage.Remove(command.QuestionnaireId.FormatGuid());
            var creatorId = aggregate.QuestionnaireDocument.CreatedBy ?? Guid.Empty;
            UpdateFullQuestionnaireState(aggregate.QuestionnaireDocument, command.QuestionnaireId, creatorId);

            var itemToRevert = this.questionnaireChangeItemStorage.GetById(command.HistoryReferanceId.FormatGuid());

            AddQuestionnaireChangeItem(command.QuestionnaireId,
                command.ResponsibleId,
                QuestionnaireActionType.Revert,
                QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId,
                aggregate.QuestionnaireDocument.Title,
                null,
                null,
                itemToRevert.Timestamp,
                aggregate.QuestionnaireDocument
                );
        }
    }
}