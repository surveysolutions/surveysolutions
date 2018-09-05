using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    [RequiresPreprocessor(typeof(HistoryPreProcessor))]
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
        ICommandPostProcessor<Questionnaire, SetDefaultTranslation>,
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
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMetadata>
    {
        private IPlainStorageAccessor<User> accountStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<User>>();

        private IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeItemStorage
            => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireChangeRecord>>();

        private IPlainKeyValueStorage<QuestionnaireStateTracker> questionnaireStateTrackerStorage
            => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireStateTracker>>();

        private IQuestionnireHistoryVersionsService questionnireHistoryVersionsService 
            => ServiceLocator.Current.GetInstance<IQuestionnireHistoryVersionsService>();

        private OriginalQuestionnaireStorage originalQuestionnaireDocumentStorage
            => ServiceLocator.Current.GetInstance<OriginalQuestionnaireStorage>();

        private IPatchGenerator patchGenerator
            => ServiceLocator.Current.GetInstance<IPatchGenerator>();
        
        private QuestionnaireHistorySettings questionnaireHistorySettings 
            => ServiceLocator.Current.GetInstance<QuestionnaireHistorySettings>();

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
            this.UpdateQuestionnaireVariableIfNeed(questionnaireId, command.Variable);

            this.AddQuestionnaireChangeItem(questionnaireId, command.ResponsibleId,
                QuestionnaireActionType.Update,
                QuestionnaireItemType.Questionnaire, command.QuestionnaireId, command.Title, aggregate.QuestionnaireDocument);
        }

        private void UpdateQuestionnaireVariableIfNeed(Guid questionnaireId, string variable)
        {
            var questionnaireStateTracker = this.questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());
            questionnaireStateTracker.VariableState.TryGetValue(questionnaireId, out var variableInState);
            if (variableInState == variable) 
                return;

            questionnaireStateTracker.VariableState[questionnaireId] = variable;
            this.questionnaireStateTrackerStorage.Store(questionnaireStateTracker, questionnaireId.FormatGuid());
        }

        private void UpdateQuestionnaireTitleIfNeed(Guid questionnaireId, string title)
        {
            var questionnaireStateTracker = this.questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());
            var titleInState = questionnaireStateTracker.GroupsState[questionnaireId];
            if (titleInState != title)
            {
                questionnaireStateTracker.GroupsState[questionnaireId] = title;
                this.questionnaireStateTrackerStorage.Store(questionnaireStateTracker, questionnaireId.FormatGuid());
            }
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
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
            var questionnaireStateTracker = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.Source.CreatedBy.Value,
                questionnaireStateTracker == null ? QuestionnaireActionType.Import : QuestionnaireActionType.Replace, QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId, command.Source.Title, aggregate.QuestionnaireDocument);

            this.UpdateFullQuestionnaireState(command.Source, command.QuestionnaireId, command.Source.CreatedBy ?? Guid.Empty);
        }

        public void Process(Questionnaire aggregate, UpdateMetadata command)
        {
            var questionnaireId = command.QuestionnaireId;

            this.UpdateQuestionnaireTitleIfNeed(questionnaireId, command.Title);

            this.AddQuestionnaireChangeItem(questionnaireId, command.ResponsibleId,
                QuestionnaireActionType.Update,
                QuestionnaireItemType.Metadata, command.QuestionnaireId, command.Title, aggregate.QuestionnaireDocument);
        }
        #endregion

        #region Shared persons
        public void Process(Questionnaire aggregate, AddSharedPersonToQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserEmail(command.PersonId), null);

        public void Process(Questionnaire aggregate, RemoveSharedPersonFromQuestionnaire command)
            => this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Person, command.PersonId, this.GetUserEmail(command.PersonId), null);

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
            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string staticTitle;
            questionnaire.StaticTextState.TryGetValue(command.EntityId, out staticTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.StaticText, command.EntityId, staticTitle, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.EntityId);
            questionnaire.StaticTextState.Remove(command.EntityId);
            this.questionnaireStateTrackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
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
            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            questionnaire.VariableState.TryGetValue(command.EntityId, out var variableName);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Variable, command.EntityId, variableName, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.EntityId);
            questionnaire.VariableState.Remove(command.EntityId);
            questionnaireStateTrackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveVariable command)
            => this.MoveEntity(command.QuestionnaireId, command.EntityId, command.TargetEntityId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        #endregion

        #region Macroses
        public void Process(Questionnaire aggregate, AddMacro command)
        {
            this.AddOrUpdateMacroState(command.QuestionnaireId, command.MacroId, string.Empty);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Macro, command.MacroId, null, aggregate.QuestionnaireDocument);
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
                QuestionnaireItemType.LookupTable, command.LookupTableId, null, aggregate.QuestionnaireDocument);
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

        public void Process(Questionnaire aggregate, SetDefaultTranslation command)
        {
            var state = this.questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            bool isDirty = false;

            if (state.TranslationState.TryGetValue(command.QuestionnaireId, out var defaultTranslation))
            {
                if (defaultTranslation != command.TranslationId?.FormatGuid())
                {
                    state.TranslationState[command.QuestionnaireId] = command.TranslationId?.FormatGuid();
                    isDirty = true;
                }
            }
            else
            {
                state.TranslationState.Add(command.QuestionnaireId, command.TranslationId?.FormatGuid());
                isDirty = true;
            }

            if (isDirty)
            {
                this.questionnaireStateTrackerStorage.Store(state, command.QuestionnaireId.FormatGuid());
            }

            var translationName = aggregate.QuestionnaireDocument.Translations
                .SingleOrDefault(t => t.Id == command.TranslationId)?.Name;

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Mark,
              QuestionnaireItemType.Translation, command.QuestionnaireId, translationName, aggregate.QuestionnaireDocument);
        }

        #endregion

        #region Groups and Rosters
        public void Process(Questionnaire aggregate, AddGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle, command.ParentGroupId);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Add,
                QuestionnaireItemType.Section, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);

            if (command.Index.HasValue)
                this.MoveEntity(command.QuestionnaireId, command.GroupId, command.ParentGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            var groupTitle = command.VariableName ?? command.Title;
            this.AddOrUpdateGroupState(command.QuestionnaireId, command.GroupId, groupTitle, parentId: null);

            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Update,
                questionnaire.GroupsState.ContainsKey(command.GroupId)
                    ? QuestionnaireItemType.Section
                    : QuestionnaireItemType.Roster, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            this.UpdateRoster(command.QuestionnaireId, command.IsRoster, command.GroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());

            var isGroup = questionnaire.GroupsState.ContainsKey(command.GroupId);
            string groupTitle;
            if (isGroup)
                questionnaire.GroupsState.TryGetValue(command.GroupId, out groupTitle);
            else
                questionnaire.RosterState.TryGetValue(command.GroupId, out groupTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                isGroup ? QuestionnaireItemType.Section : QuestionnaireItemType.Roster, command.GroupId, groupTitle, aggregate.QuestionnaireDocument);

            questionnaire.RemoveCascadely(command.GroupId);
            this.questionnaireStateTrackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
        }

        public void Process(Questionnaire aggregate, MoveGroup command)
            => this.MoveEntity(command.QuestionnaireId, command.GroupId, command.TargetGroupId, command.ResponsibleId, aggregate.QuestionnaireDocument);

        private void UpdateRoster(Guid questionnaireId, bool isRoster, Guid groupId, Guid responsibleId, QuestionnaireDocument questionnaireDocument)
        {
            var questionnaire = questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());
            if (isRoster)
            {
                if (questionnaire.RosterState.ContainsKey(groupId))
                    return;

                var groupTitle = questionnaire.GroupsState[groupId];

                questionnaire.RosterState[groupId] = groupTitle;
                questionnaire.GroupsState.Remove(groupId);

                this.questionnaireStateTrackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId,
                    QuestionnaireActionType.GroupBecameARoster, QuestionnaireItemType.Section, groupId, groupTitle, questionnaireDocument);

                var rosterTitle = questionnaire.RosterState[groupId];
                this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Update,
                    QuestionnaireItemType.Roster, groupId, rosterTitle, questionnaireDocument);
            }
            else
            {
                if (questionnaire.GroupsState.ContainsKey(groupId))
                    return;

                var rosterTitle = questionnaire.RosterState[groupId];

                questionnaire.GroupsState[groupId] = rosterTitle;
                questionnaire.RosterState.Remove(groupId);

                this.questionnaireStateTrackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
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
                    entityType = entityAsGroup.IsRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Section;
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
            var questionnaire = questionnaireStateTrackerStorage.GetById(command.QuestionnaireId.FormatGuid());
            string questionTitle;
            questionnaire.QuestionsState.TryGetValue(command.QuestionId, out questionTitle);

            this.AddQuestionnaireChangeItem(command.QuestionnaireId, command.ResponsibleId, QuestionnaireActionType.Delete,
                QuestionnaireItemType.Question, command.QuestionId, questionTitle, aggregate.QuestionnaireDocument);

            questionnaire.Parents.Remove(command.QuestionId);
            questionnaire.QuestionsState.Remove(command.QuestionId);
            this.questionnaireStateTrackerStorage.Store(questionnaire, command.QuestionnaireId.FormatGuid());
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
        public void Process(Questionnaire aggregate, UpdateAreaQuestion command) => this.AddQuestionChanges(aggregate, command);
        public void Process(Questionnaire aggregate, UpdateAudioQuestion command) => this.AddQuestionChanges(aggregate, command);

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
            var questionnaire = questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());

            questionnaire.Parents[entityId] = targetGroupOrRosterId;
            this.questionnaireStateTrackerStorage.Store(questionnaire, questionnaireId.FormatGuid());

            QuestionnaireChangeReference movedReference = null;
            if (targetGroupOrRosterId.HasValue)
            {
                var targetGroupId = targetGroupOrRosterId ?? questionnaireId;
                var isTargetGroupRoster = questionnaire.RosterState.ContainsKey(targetGroupId);
                var targetGroupTitle = isTargetGroupRoster
                    ? questionnaire.RosterState[targetGroupId]
                    : questionnaire.GroupsState[targetGroupId];
                movedReference = CreateQuestionnaireChangeReference(
                    isTargetGroupRoster ? QuestionnaireItemType.Roster : QuestionnaireItemType.Section,
                    targetGroupId, targetGroupTitle);
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
                movedItemType = QuestionnaireItemType.Section;
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
                entityId, moveditemTitle, questionnaireDocument, movedReference);
        }

        private void AddQuestionnaireChangeItem(Guid questionnaireId, 
            Guid responsibleId, 
            QuestionnaireActionType actionType, 
            QuestionnaireItemType targetType, 
            Guid targetId, 
            string targetTitle, 
            QuestionnaireDocument questionnaireDocument, 
            QuestionnaireChangeReference reference = null)
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
                reference);
        }

        private void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            string targetNewTitle,
            int? affectedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument questionnaireDocument,
            QuestionnaireChangeReference reference = null)
        {
            var sQuestionnaireId = questionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage.Query(x => x
                .Where(y => y.QuestionnaireId == sQuestionnaireId).Select(y => (int?) y.Sequence).Max());

            var questionnaireChangeItem = new QuestionnaireChangeRecord
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
                AffectedEntriesCount = affectedEntries,
                TargetItemDateTime = targetDateTime,
            };

            if (reference != null)
            {
                reference.QuestionnaireChangeRecord = questionnaireChangeItem;
                questionnaireChangeItem.References.Add(reference);
            }
            
            var resultingQuestionnaire = questionnireHistoryVersionsService.GetResultingQuestionnaireDocument(questionnaireDocument);
            var left = this.originalQuestionnaireDocumentStorage.Pop();
            questionnaireChangeItem.DiffWithPreviousVersion = patchGenerator.Diff(left, resultingQuestionnaire);

            this.questionnaireChangeItemStorage.Store(questionnaireChangeItem, questionnaireChangeItem.QuestionnaireChangeRecordId);

            RemoveOldQuestionnaireHistory(sQuestionnaireId, maxSequenceByQuestionnaire);
        }

        private void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire)
        {
            var minSequence = (maxSequenceByQuestionnaire ?? 0) -
                              questionnaireHistorySettings.QuestionnaireChangeHistoryLimit + 2;
            if (minSequence < 0) return;

            List<string> changeRecordIdsToRemove = this.questionnaireChangeItemStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == sQuestionnaireId && x.Sequence < minSequence
                                                                  && (x.ResultingQuestionnaireDocument != null || x.DiffWithPreviousVersion != null))
                .OrderBy(x => x.Sequence)
                .Select(x => x.QuestionnaireChangeRecordId)
                .ToList());

            if (changeRecordIdsToRemove.Count > 0)
            {
                string lastItem = changeRecordIdsToRemove.Last();
                var change = this.questionnaireChangeItemStorage.GetById(lastItem);

                QuestionnaireDocument resultingQuestionnaireForLastStoredRecord =
                    this.questionnireHistoryVersionsService.GetByHistoryVersion(Guid.Parse(change.QuestionnaireChangeRecordId));

                change.ResultingQuestionnaireDocument = 
                    this.questionnireHistoryVersionsService.GetResultingQuestionnaireDocument(resultingQuestionnaireForLastStoredRecord);
                change.DiffWithPreviousVersion = null;
                this.questionnaireChangeItemStorage.Store(change, change.QuestionnaireChangeRecordId);

                foreach (var changeRecordIdToRemove in changeRecordIdsToRemove.Where(x => x != lastItem))
                {
                    var itemToRemoveQuestionnaire = this.questionnaireChangeItemStorage.GetById(changeRecordIdToRemove);
                    itemToRemoveQuestionnaire.ResultingQuestionnaireDocument = null;
                    itemToRemoveQuestionnaire.DiffWithPreviousVersion = null;
                    this.questionnaireChangeItemStorage.Store(itemToRemoveQuestionnaire, changeRecordIdToRemove);
                }
            }
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

        private string GetUserEmail(Guid? userId)
        {
            if (userId.HasValue)
            {
                var creator = this.accountStorage.GetById(userId.Value.FormatGuid());
                if (creator != null)
                    return creator.Email;
            }
            return null;
        }

        private void AddOrUpdateQuestionnaireStateItem(Guid questionnaireId, Guid itemId, string itemTitle,
            Guid? parentId, Action<QuestionnaireStateTracker, Guid, string> setAction)
        {
            var questionnaireStateTracker = this.questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());

            if (parentId.HasValue)
            {
                questionnaireStateTracker.Parents[itemId] = parentId;
            }

            setAction(questionnaireStateTracker, itemId, itemTitle);

            this.questionnaireStateTrackerStorage.Store(questionnaireStateTracker, questionnaireId.FormatGuid());
        }

        private void UpdateFullQuestionnaireState(QuestionnaireDocument questionnaireDocument, Guid questionnaireId, Guid createdBy)
        {
            var questionnaireStateTracker = questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaireStateTracker == null)
                questionnaireStateTracker = new QuestionnaireStateTracker() { CreatedBy = createdBy };

            questionnaireStateTracker.GroupsState[questionnaireId] = questionnaireDocument.Title;

            var compositeElements = questionnaireDocument.Find<IComposite>(c => true);
            foreach (var compositeElement in compositeElements)
            {
                questionnaireStateTracker.Parents[compositeElement.PublicKey] = compositeElement.GetParent()?.PublicKey;

                var question = compositeElement as IQuestion;
                if (question != null)
                {
                    questionnaireStateTracker.QuestionsState[question.PublicKey] = question.StataExportCaption ??
                                                                                  question.QuestionText;
                    continue;
                }
                var group = compositeElement as IGroup;
                if (group != null)
                {
                    var groupTitle = @group.VariableName ?? @group.Title;
                    if (group.IsRoster)
                        questionnaireStateTracker.RosterState[group.PublicKey] = groupTitle;
                    else
                        questionnaireStateTracker.GroupsState[group.PublicKey] = groupTitle;
                    continue;
                }
                var staticTexts = compositeElement as IStaticText;
                if (staticTexts != null)
                {
                    questionnaireStateTracker.StaticTextState[staticTexts.PublicKey] = staticTexts.Text;
                    continue;
                }
                var variable = compositeElement as IVariable;
                if (variable != null)
                {
                    questionnaireStateTracker.VariableState[variable.PublicKey] = variable.Name;
                }
            }
            questionnaireStateTrackerStorage.Store(questionnaireStateTracker, questionnaireId.FormatGuid());
        }

        private void DeleteItemFromStateAndUpdateHistory(Guid questionnaireId,
            Func<QuestionnaireStateTracker, Dictionary<Guid, string>> state, Guid itemId, QuestionnaireItemType itemType,
            Guid responsibleId, QuestionnaireDocument questionnaireDocument)
        {
            QuestionnaireStateTracker questionnaire = this.questionnaireStateTrackerStorage.GetById(questionnaireId.FormatGuid());

            string itemName = "";

            state(questionnaire).TryGetValue(itemId, out itemName);

            this.AddQuestionnaireChangeItem(questionnaireId, responsibleId, QuestionnaireActionType.Delete, itemType,
                itemId, itemName, questionnaireDocument);

            questionnaire.Parents.Remove(itemId);
            state(questionnaire).Remove(itemId);
            this.questionnaireStateTrackerStorage.Store(questionnaire, questionnaireId.FormatGuid());
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
            questionnaireStateTrackerStorage.Remove(command.QuestionnaireId.FormatGuid());
            var creatorId = aggregate.QuestionnaireDocument.CreatedBy ?? Guid.Empty;
            UpdateFullQuestionnaireState(aggregate.QuestionnaireDocument, command.QuestionnaireId, creatorId);

            var itemToRevert = this.questionnaireChangeItemStorage.GetById(command.HistoryReferenceId.FormatGuid());

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
