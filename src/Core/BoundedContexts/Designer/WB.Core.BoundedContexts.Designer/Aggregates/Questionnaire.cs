using Main.Core.Entities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates.Snapshots;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using NHibernate.Util;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation;
using WB.Core.BoundedContexts.Designer.Translations;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    internal class Questionnaire : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireState>
    {
        private const int MaxCountOfDecimalPlaces = 15;
        private const int MaxChapterItemsCount = 400;
        private const int MaxTitleLength = 500;
        private const int maxFilteredComboboxOptionsCount = 15000;
        private const int maxCascadingComboboxOptionsCount = 15000;
        private const int MaxGroupDepth = 10;
        private const int DefaultVariableLengthLimit = 32;
        private const int DefaultRestrictedVariableLengthLimit = 20;

        private static readonly QuestionType[] RestrictedVariableLengthQuestionTypes = 
            new QuestionType[]
            {
                QuestionType.GpsCoordinates,
                QuestionType.MultyOption,
                QuestionType.TextList
            };

        private static readonly HashSet<QuestionType> RosterSizeQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.Numeric,
            QuestionType.MultyOption,
            QuestionType.TextList,
        };

        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private HashSet<Guid> readOnlyUsers=new HashSet<Guid>();
        private HashSet<Guid> macroIds = new HashSet<Guid>();
        private HashSet<Guid> lookupTableIds = new HashSet<Guid>();

        private bool wasExpressionsMigrationPerformed = false;

        internal void AddMacro(MacroAdded e)
        {
            this.macroIds.Add(e.MacroId);
            this.innerDocument.Macros[e.MacroId] = new Macro();
        }

        internal void UpdateMacro(MacroUpdated e)
        {
            if (!innerDocument.Macros.ContainsKey(e.MacroId))
                return;

            var macro = this.innerDocument.Macros[e.MacroId];
            macro.Name = e.Name;
            macro.Content = e.Content;
            macro.Description = e.Description;
        }

        internal void DeleteMacro(MacroDeleted e)
        {
            this.macroIds.Remove(e.MacroId);
            innerDocument.Macros.Remove(e.MacroId);
        }

        internal void AddLookupTable(LookupTableAdded e)
        {
            this.lookupTableIds.Add(e.LookupTableId);
            innerDocument.LookupTables[e.LookupTableId] = new LookupTable()
            {
                TableName = e.LookupTableName,
                FileName = e.LookupTableFileName
            };
        }

        internal void UpdateLookupTable(LookupTableUpdated e)
        {
            innerDocument.LookupTables[e.LookupTableId] = new LookupTable
            {
                TableName = e.LookupTableName,
                FileName = e.LookupTableFileName
            };
        }

        internal void DeleteLookupTable(LookupTableDeleted e)
        {
            this.lookupTableIds.Remove(e.LookupTableId);
            innerDocument.LookupTables.Remove(e.LookupTableId);
        }

        internal void UpdateAttachment(AttachmentUpdated e)
        {
            innerDocument.Attachments.RemoveAll(x => x.AttachmentId == e.AttachmentId);
            innerDocument.Attachments.Add(new Attachment
            {
                AttachmentId = e.AttachmentId,
                Name = e.AttachmentName,
                ContentId = e.AttachmentContentId
            });
        }

        internal void DeleteAttachment(AttachmentDeleted e)
        {
            innerDocument.Attachments.RemoveAll(x => x.AttachmentId == e.AttachmentId);
        }

        internal void UpdateTranslation(TranslationUpdated e)
        {
            var translation = new Translation
            {
                Id = e.TranslationId,
                Name = e.Name,
            };
            innerDocument.Translations.RemoveAll(x => x.Id == e.TranslationId);
            innerDocument.Translations.Add(translation);
        }

        internal void DeleteTranslation(TranslationDeleted e)
        {
            innerDocument.Translations.RemoveAll(x => x.Id == e.TranslationId);
        }

        internal void AddSharedPersonToQuestionnaire(SharedPersonToQuestionnaireAdded e)
        {
            this.innerDocument.SharedPersons.Add(e.PersonId);
            if (e.ShareType == ShareType.View)
                this.readOnlyUsers.Add(e.PersonId);
        }

        internal void RemoveSharedPersonFromQuestionnaire(SharedPersonFromQuestionnaireRemoved e)
        {
            this.innerDocument.SharedPersons.Remove(e.PersonId);
            this.readOnlyUsers.Remove(e.PersonId);
        }

        private void UpdateQuestionnaire(QuestionnaireUpdated e)
        {
            this.innerDocument.Title = System.Web.HttpUtility.HtmlDecode(e.Title);
            this.innerDocument.IsPublic = e.IsPublic;
        }

        private void DeleteQuestionnaire(QuestionnaireDeleted e)
        {
            this.innerDocument.IsDeleted = true;
        }

        private void MigrateExpressionsToCSharp(ExpressionsMigratedToCSharp e)
        {
            this.wasExpressionsMigrationPerformed = true;
            this.innerDocument.UsesCSharp = true;

        }

        private void DeleteGroup(GroupDeleted e)
        {
            this.innerDocument.RemoveGroup(e.GroupPublicKey);
        }

        private void UpdateGroup(GroupUpdated e)
        {
            this.innerDocument.UpdateGroup(e.GroupPublicKey, 
                e.GroupText,
                e.VariableName, 
                e.Description, 
                e.ConditionExpression, 
                e.HideIfDisabled);
        }

        internal void AddGroup(NewGroupAdded e)
        {
            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(e.GroupText);
            group.VariableName = e.VariableName;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            group.HideIfDisabled = e.HideIfDisabled;

            Guid? parentGroupPublicKey = e.ParentGroupPublicKey;
            if (parentGroupPublicKey.HasValue)
            {
                var parentGroup = this.innerDocument.Find<Group>(parentGroupPublicKey.Value);
                if (parentGroup != null)
                {
                    group.SetParent(parentGroup);
                }
                else
                {
                    string errorMessage = string.Format("Fail attempt to add group {0} into group {1}. But group {1} doesnt exist in document {2}",
                        e.PublicKey,
                        e.ParentGroupPublicKey,
                        this.innerDocument.PublicKey);

                    logger.Error(errorMessage);
                }
            }

            this.innerDocument.Add(group, e.ParentGroupPublicKey, null);
        }

        internal void ImportTemplate(TemplateImported e)
        {
            var upgradedDocument = e.Source;

            upgradedDocument.ReplaceSharedPersons(this.innerDocument.SharedPersons);

            this.innerDocument = upgradedDocument;
        }

        private void CloneQuestionnaire(QuestionnaireCloned e)
        {
            this.innerDocument = e.QuestionnaireDocument;
            if (e.QuestionnaireDocument.Macros != null)
            {
                this.macroIds = e.QuestionnaireDocument.Macros.Select(x => x.Key).ToHashSet();
            }
            if (e.QuestionnaireDocument.LookupTables != null)
            {
                this.lookupTableIds = e.QuestionnaireDocument.LookupTables.Select(x => x.Key).ToHashSet();
            }
        }

        private void CloneGroup(GroupCloned e)
        {
            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(e.GroupText);
            group.VariableName = e.VariableName;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            group.HideIfDisabled = e.HideIfDisabled;

            this.innerDocument.Insert(e.TargetIndex, group, e.ParentGroupPublicKey);
        }

        internal void MarkGroupAsRoster(GroupBecameARoster e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group => group.IsRoster = true);
        }

        internal void ChangeRoster(RosterChanged e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group =>
            {
                group.RosterSizeQuestionId = e.RosterSizeQuestionId;
                group.RosterSizeSource = e.RosterSizeSource;
                group.FixedRosterTitles = e.FixedRosterTitles;
                group.RosterTitleQuestionId = e.RosterTitleQuestionId;
            });
        }

        private void RemoveRosterFlagFromGroup(GroupStoppedBeingARoster e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group =>
            {
                group.IsRoster = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.FixedRosterTitles = new FixedRosterTitle[0];
            });
        }

        internal void AddQuestion(NewQuestionAdded e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        e.AnswerOrder,
                        e.Featured,
                        e.Capital,
                        e.Instructions,
                        e.Properties,
                        e.Mask,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.LinkedToRosterId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        null,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId,
                        null,
                        e.ValidationConditions,
                        e.LinkedFilterExpression,
                        e.IsTimestamp));


            this.innerDocument.Add(question, e.GroupPublicKey, null);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }


        internal void CloneQuestion(QuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        e.AnswerOrder,
                        e.Featured,
                        e.Capital,
                        e.Instructions,
                        e.Properties,
                        e.Mask,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.LinkedToRosterId,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        e.MaxAnswerCount,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId,
                        e.YesNoView,
                        e.ValidationConditions,
                        e.LinkedFilterExpression,
                        e.IsTimestamp));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }

        private void CloneNumericQuestion(NumericQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.Numeric,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        Order.AZ,
                        e.Featured,
                        e.Capital,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }


        internal void CloneTextListQuestion(TextListQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.TextList,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        Order.AZ,
                        false,
                        false,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.MaxAnswerCount,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupId);
        }

        private void CreateNewQuestionnaire(NewQuestionnaireCreated e)
        {
            this.innerDocument.IsPublic = e.IsPublic;
            this.innerDocument.Title = System.Web.HttpUtility.HtmlDecode(e.Title);
            this.innerDocument.PublicKey = e.PublicKey;
            this.innerDocument.CreationDate = e.CreationDate;
            this.innerDocument.LastEntryDate = e.CreationDate;
            this.innerDocument.CreatedBy = e.CreatedBy;
        }

        internal void UpdateQuestion(QuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        e.AnswerOrder,
                        e.Featured,
                        e.Capital,
                        e.Instructions,
                        e.Properties,
                        e.Mask,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.LinkedToRosterId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        null,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId,
                        e.YesNoView,
                        e.ValidationConditions,
                        e.LinkedFilterExpression,
                        e.IsTimestamp));

            this.innerDocument.ReplaceEntity(question, newQuestion);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, null);
        }

        internal void UpdateNumericQuestion(NumericQuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        QuestionType.Numeric,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        Order.AZ,
                        e.Featured,
                        e.Capital,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false)
                    );

            this.innerDocument.ReplaceEntity(question, newQuestion);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, null);
        }

        internal void UpdateTextListQuestion(TextListQuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.TextList,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.HideIfDisabled,
                        Order.AZ,
                        false,
                        false,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.MaxAnswerCount,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        private void DeleteQuestion(QuestionDeleted e)
        {
            this.innerDocument.RemoveEntity(e.QuestionId);

            this.innerDocument.RemoveHeadPropertiesFromRosters(e.QuestionId);
        }

        private void MoveQuestionnaireItem(QuestionnaireItemMoved e)
        {
            this.innerDocument.MoveItem(e.PublicKey, e.GroupKey, e.TargetIndex);

            this.innerDocument.CheckIsQuestionHeadAndUpdateRosterProperties(e.PublicKey, e.GroupKey);
        }

        internal void UpdateQRBarcodeQuestion(QRBarcodeQuestionUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionId);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.QRBarcode,
                        e.QuestionScope,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        e.HideIfDisabled,
                        Order.AZ,
                        false,
                        false,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        internal void UpdateMultimediaQuestion(MultimediaQuestionUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionId);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.Multimedia,
                        e.QuestionScope,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        e.HideIfDisabled,
                        Order.AZ,
                        false,
                        false,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        internal void CloneQRBarcodeQuestion(QRBarcodeQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.QRBarcode,
                        e.QuestionScope,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        e.HideIfDisabled,
                        Order.AZ,
                        false,
                        false,
                        e.Instructions,
                        e.Properties,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.ValidationConditions,
                        null,
                        false));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.ParentGroupId);
        }

        internal void AddStaticText(StaticTextAdded e)
        {
            var staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, 
                text: e.Text, 
                attachmentName: null,
                enablementCondition: e.EnablementCondition,
                hideIfDisabled: e.HideIfDisabled,
                validationConditions: e.ValidationConditions);

            this.innerDocument.Add(c: staticText, parent: e.ParentId, parentPropagationKey: null);
        }

        internal void UpdateStaticText(StaticTextUpdated e)
        {
            var oldStaticText = this.innerDocument.Find<IStaticText>(e.EntityId);
            var newStaticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, 
                text: e.Text, 
                attachmentName: e.AttachmentName,
                enablementCondition: e.EnablementCondition,
                hideIfDisabled: e.HideIfDisabled,
                validationConditions:e.ValidationConditions);

            this.innerDocument.ReplaceEntity(oldStaticText, newStaticText);
        }

        internal void CloneStaticText(StaticTextCloned e)
        {
            var staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, 
                text: e.Text, 
                attachmentName: e.AttachmentName,
                enablementCondition: e.EnablementCondition,
                hideIfDisabled: e.HideIfDisabled,
                validationConditions: e.ValidationConditions);

            this.innerDocument.Insert(e.TargetIndex, staticText, e.ParentId);
        }

        internal void DeleteStaticText(StaticTextDeleted e)
        {
            this.innerDocument.RemoveEntity(e.EntityId);   
        }

        internal void AddVariable(VariableAdded e)
        {
            var variable = this.questionnaireEntityFactory.CreateVariable(e);
            this.innerDocument.Add(c: variable, parent: e.ParentId, parentPropagationKey: null);
        }

        internal void UpdateVariable(VariableUpdated e)
        {
            var oldVariable = this.innerDocument.Find<IVariable>(e.EntityId);
            var newVariable = this.questionnaireEntityFactory.CreateVariable(e);
            this.innerDocument.ReplaceEntity(oldVariable, newVariable);

        }

        internal void CloneVariable(VariableCloned e)
        {
            var variable = this.questionnaireEntityFactory.CreateVariable(e);
            this.innerDocument.Insert(e.TargetIndex, variable, e.ParentId);
        }

        internal void DeleteVariable(VariableDeleted e)
        {
            this.innerDocument.RemoveEntity(e.EntityId);
        }

        public QuestionnaireState CreateSnapshot()
        {
            return new QuestionnaireState
            {
                QuestionnaireDocument = this.innerDocument,
                Version = this.Version,
                WasExpressionsMigrationPerformed = wasExpressionsMigrationPerformed,
                ReadOnlyUsers = this.readOnlyUsers,
                MacroIds = this.macroIds,
                LookupTableIds = this.lookupTableIds
            };
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.innerDocument = snapshot.QuestionnaireDocument.Clone() as QuestionnaireDocument;
            this.wasExpressionsMigrationPerformed = snapshot.WasExpressionsMigrationPerformed;
            this.readOnlyUsers = snapshot.ReadOnlyUsers;
            this.macroIds = snapshot.MacroIds;
            this.lookupTableIds = snapshot.LookupTableIds;
        }

        private static int? DetermineActualMaxValueForNumericQuestion(bool isAutopropagating, int? legacyMaxValue, int? actualMaxValue)
        {
            return isAutopropagating ? legacyMaxValue : actualMaxValue;
        }

        #endregion

        #region Dependencies

        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;
        private readonly ILogger logger;
        private readonly IClock clock;
        private readonly IExpressionProcessor expressionProcessor;
        private readonly ISubstitutionService substitutionService;
        private readonly IKeywordsProvider variableNameValidator;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationService;

        #endregion

        public Questionnaire(
            IQuestionnaireEntityFactory questionnaireEntityFactory, 
            ILogger logger, 
            IClock clock, 
            IExpressionProcessor expressionProcessor, 
            ISubstitutionService substitutionService, 
            IKeywordsProvider variableNameValidator, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService,
            ITranslationsService translationService)
        {
            this.questionnaireEntityFactory = questionnaireEntityFactory;
            this.logger = logger;
            this.clock = clock;
            this.expressionProcessor = expressionProcessor;
            this.substitutionService = substitutionService;
            this.variableNameValidator = variableNameValidator;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationService = translationService;
        }

        #region Questionnaire command handlers

        public void CreateQuestionnaire(Guid publicKey, string title, Guid? createdBy, bool isPublic)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.CreateNewQuestionnaire(
                new NewQuestionnaireCreated
                {
                    IsPublic = isPublic,
                    PublicKey = publicKey,
                    Title = title,
                    CreationDate = this.clock.UtcNow(),
                    CreatedBy = createdBy
                });

            this.AddGroup(
                new NewGroupAdded
                {
                    GroupText = "New Section",
                    PublicKey = Guid.NewGuid(),
                    ResponsibleId = createdBy ?? Guid.Empty
                }
                );

            this.MigrateExpressionsToCSharp(new ExpressionsMigratedToCSharp());
        }

        public void CloneQuestionnaire(string title, bool isPublic, Guid createdBy, Guid publicKey, IQuestionnaireDocument source)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(title);

            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, "only QuestionnaireDocuments are supported for now");

            var clonedDocument = (QuestionnaireDocument)document.Clone();
            clonedDocument.PublicKey = this.EventSourceId;
            clonedDocument.CreatedBy = createdBy;
            clonedDocument.CreationDate = this.clock.UtcNow();
            clonedDocument.Title = title;
            clonedDocument.IsPublic = isPublic;
            if (clonedDocument.SharedPersons != null)
            {
                clonedDocument.SharedPersons.Clear();
            }

            foreach (var lookupTable in clonedDocument.LookupTables)
            {
                var lookupTableName = lookupTable.Value.TableName;

                if (!this.lookupTableService.IsLookupTableEmpty(document.PublicKey, lookupTable.Key, lookupTableName))
                {
                    lookupTableService.CloneLookupTable(document.PublicKey, lookupTable.Key, lookupTableName, this.EventSourceId, lookupTable.Key);
                }
            }

            foreach (var attachment in clonedDocument.Attachments)
            {
                var newAttachmentId = Guid.NewGuid();
                this.attachmentService.CloneMeta(attachment.AttachmentId, newAttachmentId, clonedDocument.PublicKey);
                attachment.AttachmentId = newAttachmentId;
            }

            foreach (var translation in clonedDocument.Translations)
            {
                var newTranslationId = Guid.NewGuid();
                this.translationService.CloneTranslation(document.PublicKey, translation.Id, clonedDocument.PublicKey, newTranslationId);
                translation.Id = newTranslationId;
            }

            this.CloneQuestionnaire(new QuestionnaireCloned
            {
                QuestionnaireDocument = clonedDocument,
                ClonedFromQuestionnaireId = document.PublicKey,
                ClonedFromQuestionnaireVersion = document.LastEventSequence
            });

            if (source.UsesCSharp)
            {
                this.MigrateExpressionsToCSharp(new ExpressionsMigratedToCSharp());
            }
        }

        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, "Only QuestionnaireDocuments are supported for now");
            if (document.IsDeleted)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, "Trying to import template of deleted questionnaire");

            document.CreatedBy = createdBy;
            this.ImportTemplate(new TemplateImported { Source = document });
        }

        public void UpdateQuestionnaire(UpdateQuestionnaire command)
#warning CRUD
        {
            if (!command.IsResponsibleAdmin) 
                this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(command.Title);

            this.UpdateQuestionnaire(new QuestionnaireUpdated()
            {
                Title = command.Title,
                IsPublic = command.IsPublic,
                ResponsibleId = command.ResponsibleId
            });
        }

        public void DeleteQuestionnaire()
        {
            this.DeleteQuestionnaire(new QuestionnaireDeleted());
        }

        #endregion

        #region Macro command handlers

        public void AddMacro(AddMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroAlreadyExist(command.MacroId);
            this.AddMacro(new MacroAdded(command.MacroId, command.ResponsibleId));
        }

        public void UpdateMacro(UpdateMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroIsAbsent(command.MacroId);
            this.ThrowDomainExceptionIfMacroContentIsEmpty(command.Content);
            this.UpdateMacro(new MacroUpdated(command.MacroId, command.Name, command.Content, command.Description, command.ResponsibleId));
        }

        public void DeleteMacro(DeleteMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroIsAbsent(command.MacroId);
            this.DeleteMacro(new MacroDeleted(command.MacroId, command.ResponsibleId));
        }

        #endregion

        #region Attachment command handlers
                
        public void AddOrUpdateAttachment(AddOrUpdateAttachment command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.UpdateAttachment(new AttachmentUpdated(
                attachmentId: command.AttachmentId, 
                attachmentName: command.AttachmentName, 
                responsibleId: command.ResponsibleId,
                attachmentContentId: command.AttachmentContentId));
        }

        public void DeleteAttachment(DeleteAttachment command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.DeleteAttachment(new AttachmentDeleted(command.AttachmentId, command.ResponsibleId));
        }

        #endregion

        #region Translation command handlers
                
        public void AddOrUpdateTranslation(AddOrUpdateTranslation command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.UpdateTranslation(new TranslationUpdated(
                translationId: command.TranslationId,
                name: command.Name,
                responsibleId: command.ResponsibleId));
        }

        public void DeleteTranslation(DeleteTranslation command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.DeleteTranslation(new TranslationDeleted(command.TranslationId, command.ResponsibleId));
        }

        #endregion

        #region Lookup table command handlers

        public void AddLookupTable(AddLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(command.LookupTableId, command.LookupTableName, DefaultVariableLengthLimit);

            if (this.lookupTableIds.Contains(command.LookupTableId))
            {
                throw new QuestionnaireException(DomainExceptionType.LookupTableAlreadyExist, ExceptionMessages.LookupTableAlreadyExist);
            }
            this.AddLookupTable(new LookupTableAdded(command.LookupTableId, command.LookupTableName, command.LookupTableFileName, command.ResponsibleId));
        }

        public void UpdateLookupTable(UpdateLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(command.LookupTableId, command.LookupTableName, DefaultVariableLengthLimit);

            if (!this.lookupTableIds.Contains(command.LookupTableId))
            {
                throw new QuestionnaireException(DomainExceptionType.LookupTableIsAbsent, ExceptionMessages.LookupTableIsAbsent);
            }
            this.UpdateLookupTable(new LookupTableUpdated(command.LookupTableId, command.LookupTableName, command.LookupTableFileName, command.ResponsibleId));
        }

        public void DeleteLookupTable(DeleteLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            if (!this.lookupTableIds.Contains(command.LookupTableId))
            {
                throw new QuestionnaireException(DomainExceptionType.LookupTableIsAbsent, ExceptionMessages.LookupTableIsAbsent);
            }
            this.DeleteLookupTable(new LookupTableDeleted(command.LookupTableId, command.ResponsibleId));
        }

        #endregion

        #region Group command handlers

        public void AddGroupAndMoveIfNeeded(Guid groupId, 
            Guid responsibleId, 
            string title, 
            string variableName, 
            Guid? rosterSizeQuestionId, 
            string description, 
            string condition, 
            bool hideIfDisabled,
            Guid? parentGroupId, 
            bool isRoster, 
            RosterSizeSourceType rosterSizeSource,
            FixedRosterTitleItem[] rosterFixedTitles, 
            Guid? rosterTitleQuestionId, 
            int? index = null)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(groupId, variableName, DefaultVariableLengthLimit);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: isRoster, rosterSizeSource: rosterSizeSource,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: fixedTitles,
                rosterTitleQuestionId: rosterTitleQuestionId, rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(parentGroupId));

            if (parentGroupId.HasValue)
            {
                this.innerDocument.ConnectChildrenWithParent();
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroupId.Value);
                this.ThrowIfTargetGroupHasReachedAllowedDepthLimit(parentGroupId.Value);
            }

            this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(
                text: title,
                entityId: groupId,
                variableName: variableName,
                parentGroup: parentGroupId.HasValue ? this.innerDocument.Find<IGroup>(parentGroupId.Value) : this.innerDocument);

            this.AddGroup(new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
                VariableName = variableName,
                ParentGroupPublicKey = parentGroupId,
                Description = description,
                ConditionExpression = condition,
                HideIfDisabled = hideIfDisabled,
                ResponsibleId = responsibleId
            });

            if (isRoster)
            {
                this.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, groupId));
                this.ChangeRoster(new RosterChanged(responsibleId, groupId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = rosterSizeSource,
                    FixedRosterTitles = fixedTitles,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });
            }
            else
            {
                this.RemoveRosterFlagFromGroup(new GroupStoppedBeingARoster(responsibleId, groupId));
            }

            if (index.HasValue)
            {
                this.MoveQuestionnaireItem(new QuestionnaireItemMoved
                {
                    PublicKey = groupId,
                    GroupKey = parentGroupId,
                    TargetIndex = index.Value,
                    ResponsibleId = responsibleId
                });
            }
        }

        private static Guid? GetIdOrReturnSameId(Dictionary<Guid, Guid> replacementIdDictionary, Guid? id)
        {
            if (!id.HasValue)
                return null;

            return replacementIdDictionary.ContainsKey(id.Value) ? replacementIdDictionary[id.Value] : id;
        }

        private void FillGroup(Guid? parentGroupId, 
            Guid responsibleId,
            IGroup sourceGroup,
            Guid sourceQuestionnaireId,
            int targetIndex,
            bool preserveVariableName,
            Dictionary<Guid, Guid> replacementIdDictionary,
            List<IEvent> events)
        {
           var groupId = replacementIdDictionary.ContainsKey(sourceGroup.PublicKey) ? replacementIdDictionary[sourceGroup.PublicKey] : Guid.NewGuid();

           events.AddRange(this.CreateCloneGroupWithoutChildrenEvents(
                groupId: groupId, 
                responsibleId: responsibleId, 
                parentGroupId: parentGroupId,
                sourceGroupId: sourceGroup.PublicKey, 
                title: sourceGroup.Title, 
                targetIndex: targetIndex,
                description: sourceGroup.Description, 
                condition: sourceGroup.ConditionExpression,
                hideIfDisabled: sourceGroup.HideIfDisabled,
                isRoster: sourceGroup.IsRoster, 
                rosterSizeSource: sourceGroup.RosterSizeSource,
                rosterSizeQuestionId: GetIdOrReturnSameId(replacementIdDictionary, sourceGroup.RosterSizeQuestionId),
                rosterTitleQuestionId: GetIdOrReturnSameId(replacementIdDictionary, sourceGroup.RosterTitleQuestionId),
                rosterFixedTitles: sourceGroup.FixedRosterTitles,
                variableName: preserveVariableName ? sourceGroup.VariableName : null,
                sourceQuestionnaireId : sourceQuestionnaireId));

            foreach (var questionnaireItem in sourceGroup.Children)
            {
                var itemId = replacementIdDictionary.ContainsKey(questionnaireItem.PublicKey) ? replacementIdDictionary[questionnaireItem.PublicKey] : Guid.NewGuid(); 
                var sourceItemId = questionnaireItem.PublicKey;
                var itemTargetIndex = sourceGroup.Children.IndexOf(questionnaireItem);

                var @group = questionnaireItem as IGroup;
                if (@group != null)
                {
                    this.FillGroup( 
                        parentGroupId: groupId, 
                        responsibleId: responsibleId,
                        sourceGroup: @group,
                        sourceQuestionnaireId: sourceQuestionnaireId,
                        targetIndex: itemTargetIndex,
                        preserveVariableName : preserveVariableName,
                        replacementIdDictionary: replacementIdDictionary,
                        events: events);
                    continue;
                }

                var question = questionnaireItem as IQuestion;
                if (question != null)
                {
                    var variableName = preserveVariableName ? question.StataExportCaption: string.Empty;
                    var variableLabel = question.VariableLabel;
                    var title = question.QuestionText;
                    var enablementCondition = question.ConditionExpression;
                    var hideIfDisabled = question.HideIfDisabled;
                    var instructions = question.Instructions;

                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion != null)
                    {
                        events.AddRange(this.CreateNumericQuestionCloneEvents(questionId: itemId, 
                            targetIndex: itemTargetIndex,
                            variableName: variableName, 
                            variableLabel: variableLabel, 
                            parentGroupId: groupId,
                            title: title,
                            isPreFilled: numericQuestion.Featured,
                            scope: numericQuestion.QuestionScope, 
                            enablementCondition: enablementCondition,
                            hideIfDisabled: hideIfDisabled,
                            validationExpression: null,
                            validationMessage: null, 
                            instructions: instructions,
                            properties: question.Properties,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId:sourceQuestionnaireId,
                            responsibleId: responsibleId,
                            isInteger: numericQuestion.IsInteger,
                            useFormatting: numericQuestion.UseFormatting,
                            countOfDecimalPlaces: numericQuestion.CountOfDecimalPlaces,
                            validationConditions: numericQuestion.ValidationConditions));
                        continue;
                    }

                    var textListQuestion = question as ITextListQuestion;
                    if (textListQuestion != null)
                    {
                        events.AddRange(this.CreateTextListQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            enablementCondition: enablementCondition, 
                            hideIfDisabled: hideIfDisabled,
                            instructions: instructions,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            responsibleId: responsibleId,
                            scope : textListQuestion.QuestionScope,
                            maxAnswerCount: textListQuestion.MaxAnswerCount,
                            validationConditions: textListQuestion.ValidationConditions));
                        continue;
                    }

                    var qrBarcodeQuestion = question as IQRBarcodeQuestion;
                    if (qrBarcodeQuestion != null)
                    {
                        events.AddRange(this.CreateQrBarcodeQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            enablementCondition: enablementCondition, 
                            hideIfDisabled: hideIfDisabled,
                            instructions: instructions,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            scope: qrBarcodeQuestion.QuestionScope,
                            responsibleId: responsibleId,
                            validationConditions: qrBarcodeQuestion.ValidationConditions));
                        continue;
                    }

                    var textQuestion = question as TextQuestion;
                    if (textQuestion != null)
                    {
                        events.AddRange(this.CreateTextQuestionClonedEvents(questionId: itemId, 
                            targetIndex: itemTargetIndex,
                            variableName: variableName, 
                            variableLabel: variableLabel,
                            parentGroupId: groupId,
                            title: title,
                            enablementCondition: enablementCondition, 
                            hideIfDisabled: hideIfDisabled,
                            responsibleId: responsibleId,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId, 
                            instructions: instructions, 
                            sourceProperties: textQuestion.Properties,
                            mask: textQuestion.Mask,
                            isPreFilled: textQuestion.Featured,
                            scope: textQuestion.QuestionScope,
                            validationConditions: textQuestion.ValidationConditions));
                        continue;
                    }

                    var geoLocationQuestion = question as GpsCoordinateQuestion;
                    if (geoLocationQuestion != null)
                    {
                        events.AddRange(this.CreateGeoLocationQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, title: title,
                            enablementCondition: enablementCondition, 
                            hideIfDisabled: hideIfDisabled,
                            instructions: instructions,
                            properties: geoLocationQuestion.Properties,
                            parentGroupId: groupId, 
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            responsibleId: responsibleId,
                            scope: geoLocationQuestion.QuestionScope,
                            featured: geoLocationQuestion.Featured,
                            capital: geoLocationQuestion.Capital,
                            validationConditions: geoLocationQuestion.ValidationConditions));
                        continue;
                    }

                    var dateTitmeQuestion = question as DateTimeQuestion;
                    if (dateTitmeQuestion != null)
                    {
                        events.AddRange(this.CreateDateTimeQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, title: title,
                            hideIfDisabled: hideIfDisabled,
                            enablementCondition: enablementCondition, 
                            instructions: instructions,
                            properties: dateTitmeQuestion.Properties,
                            parentGroupId: groupId, 
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            responsibleId: responsibleId, 
                            scope: dateTitmeQuestion.QuestionScope,
                            isPreFilled: dateTitmeQuestion.Featured,
                            validationConditions: dateTitmeQuestion.ValidationConditions,
                            isTimestamp: dateTitmeQuestion.IsTimestamp));
                        continue;
                    }

                    var categoricalMultiQuestion = question as MultyOptionsQuestion;
                    if (categoricalMultiQuestion != null)
                    {
                        events.AddRange(this.CreateCategoricalMultiAnswersQuestionClonedEvents(questionId: itemId,
                            targetIndex: itemTargetIndex, variableName: variableName, 
                            variableLabel: variableLabel,
                            title: title,
                            hideIfDisabled: hideIfDisabled,
                            enablementCondition: enablementCondition, 
                            parentGroupId: groupId,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            instructions: instructions, 
                            properties: categoricalMultiQuestion.Properties,
                            responsibleId: responsibleId,
                            scope: categoricalMultiQuestion.QuestionScope,
                            linkedToQuestionId: GetIdOrReturnSameId(replacementIdDictionary, categoricalMultiQuestion.LinkedToQuestionId),
                            linkedToRosterId: GetIdOrReturnSameId(replacementIdDictionary, categoricalMultiQuestion.LinkedToRosterId),
                            areAnswersOrdered: categoricalMultiQuestion.AreAnswersOrdered,
                            yesNoView: categoricalMultiQuestion.YesNoView,
                            maxAllowedAnswers: categoricalMultiQuestion.MaxAllowedAnswers,
                            options:
                                categoricalMultiQuestion.Answers.Select(
                                    answer => new Option(answer.PublicKey, answer.AnswerValue, answer.AnswerText))
                                    .ToArray(),
                            validationConditions: categoricalMultiQuestion.ValidationConditions, linkedFilterExpression: categoricalMultiQuestion.LinkedFilterExpression));
                        continue;
                    }

                    var categoricalSingleQuestion = question as SingleQuestion;
                    if (categoricalSingleQuestion != null)
                    {
                        events.AddRange(this.CreateCategoricalSingleAnswerQuestionEvents(questionId: itemId,
                            targetIndex: itemTargetIndex, variableName: variableName, variableLabel: variableLabel,
                            title: title, 
                            hideIfDisabled: hideIfDisabled,
                            enablementCondition: enablementCondition, 
                            parentGroupId: groupId,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId,
                            instructions: instructions, 
                            properties: categoricalSingleQuestion.Properties,
                            responsibleId: responsibleId,
                            scope: categoricalSingleQuestion.QuestionScope,
                            validationExpression: categoricalSingleQuestion.ValidationExpression,
                            validationMessage: categoricalSingleQuestion.ValidationMessage,
                            linkedToQuestionId: GetIdOrReturnSameId(replacementIdDictionary, categoricalSingleQuestion.LinkedToQuestionId),
                            linkedToRosterId: GetIdOrReturnSameId(replacementIdDictionary, categoricalSingleQuestion.LinkedToRosterId),
                            isPreFilled: categoricalSingleQuestion.Featured,
                            isFilteredCombobox: categoricalSingleQuestion.IsFilteredCombobox,
                            cascadeFromQuestionId: GetIdOrReturnSameId(replacementIdDictionary, categoricalSingleQuestion.CascadeFromQuestionId),
                            options:
                                categoricalSingleQuestion.Answers.Select(
                                    answer => new Option(answer.PublicKey, answer.AnswerValue, answer.AnswerText, answer.ParentValue))
                                    .ToArray(),
                            validationConditions: categoricalSingleQuestion.ValidationConditions, linkedFilterExpression: categoricalSingleQuestion.LinkedFilterExpression));
                        continue;
                    }

                    var multimediaQuestion = question as IMultimediaQuestion;
                    if (multimediaQuestion != null)
                    {
                        events.AddRange(this.CreateMultimediaQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            hideIfDisabled: hideIfDisabled,
                            enablementCondition: enablementCondition,
                            instructions: instructions,
                            properties: multimediaQuestion.Properties,
                            sourceQuestionId: sourceItemId,
                            sourceQuestionnaireId: sourceQuestionnaireId, 
                            scope : multimediaQuestion.QuestionScope,
                            responsibleId: responsibleId,
                            validationConditions: multimediaQuestion.ValidationConditions));
                        continue;
                    }
                }

                var staticText = questionnaireItem as IStaticText;
                if (staticText != null)
                {
                    events.AddRange(this.CreateStaticTextClonedEvents(itemId, groupId, staticText.Text,
                        staticText.AttachmentName,
                        staticText.ConditionExpression, 
                        staticText.HideIfDisabled, 
                        sourceItemId, 
                        sourceQuestionnaireId,
                        itemTargetIndex, 
                        responsibleId,
                        staticText.ValidationConditions));
                    continue;
                }

                var variable = questionnaireItem as IVariable;
                if (variable != null)
                {
                    events.AddRange(this.CreateVariableClonedEvents(
                        itemId, 
                        groupId, 
                        sourceItemId, 
                        sourceQuestionnaireId,
                        itemTargetIndex, 
                        responsibleId, variable.Type, variable.Name, variable.Expression));
                    continue;
                }
            }
        }

        public void UpdateGroup(Guid groupId, Guid responsibleId,
            string title,string variableName, Guid? rosterSizeQuestionId, string description, string condition, bool hideIfDisabled, 
            bool isRoster, RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(groupId, variableName, DefaultVariableLengthLimit);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: isRoster, rosterSizeSource: rosterSizeSource,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: fixedTitles,
                rosterTitleQuestionId: rosterTitleQuestionId, rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(groupId));

            this.innerDocument.ConnectChildrenWithParent();
            var group = this.GetGroupById(groupId);

            var wasGroupAndBecomeARoster = !@group.IsRoster && isRoster;
            var wasRosterAndBecomeAGroup = @group.IsRoster && !isRoster;

            if (wasGroupAndBecomeARoster)
            {
                this.ThrowIfGroupCantBecomeARosterBecauseOfPrefilledQuestions(group);
            }
            if (wasRosterAndBecomeAGroup)
            {
                this.ThrowIfRosterHaveAQuestionThatUsedAsRosterTitleQuestionOfOtherGroups(group);
                this.ThrowIfRosterCantBecomeAGroupBecauseContainsLinkedSourceQuestions(group);
                this.ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(group, wasRosterAndBecomeAGroup: true);
            }

            this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(
                text: title,
                entityId: groupId,
                variableName: variableName,
                parentGroup: @group.GetParent() as IGroup ?? this.innerDocument);

            this.UpdateGroup(new GroupUpdated
            {
                GroupPublicKey = groupId,
                GroupText = title,
                VariableName = variableName,
                Description = description,
                ConditionExpression = condition,
                HideIfDisabled = hideIfDisabled,
                ResponsibleId = responsibleId
            });

            if (isRoster)
            {
                this.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, groupId));
                this.ChangeRoster(new RosterChanged(responsibleId, groupId)
                    {
                        RosterSizeQuestionId = rosterSizeQuestionId,
                        RosterSizeSource = rosterSizeSource,
                        FixedRosterTitles = fixedTitles,
                        RosterTitleQuestionId = rosterTitleQuestionId
                    });
            }
            else
            {
                this.RemoveRosterFlagFromGroup(new GroupStoppedBeingARoster(responsibleId, groupId));
            }
        }

        public void DeleteGroup(Guid groupId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);
            this.ThrowDomainExceptionIfGroupQuestionsUsedAsRosterTitleQuestionOfOtherGroups(groupId);
            
            var group = this.GetGroupById(groupId);

            this.ThrowDomainExceptionIfRosterQuestionsUsedAsLinkedSourceQuestions(group);

            this.DeleteGroup(new GroupDeleted() { GroupPublicKey = groupId, ResponsibleId = responsibleId });
        }

        public void MoveGroup(Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            if (targetGroupId.HasValue)
            {
                this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId.Value);
            }
            this.innerDocument.ConnectChildrenWithParent();
            var sourceGroup = this.GetGroupById(groupId);

            if (targetGroupId.HasValue)
            {
                var sourceChapter = this.innerDocument.GetChapterOfItemById(groupId);
                var targetChapter = this.innerDocument.GetChapterOfItemById(targetGroupId.Value);

                if (sourceChapter.PublicKey != targetChapter.PublicKey)
                {
                    var numberOfMovedItems = sourceGroup.Children
                        .TreeToEnumerable(x => x.Children)
                        .Count();

                    var numberOfItemsInChapter = targetChapter.Children
                        .TreeToEnumerable(x => x.Children)
                        .Count();

                    if ((numberOfMovedItems + numberOfItemsInChapter) >= MaxChapterItemsCount)
                    {
                        throw new QuestionnaireException(string.Format("Section cannot have more than {0} elements", MaxChapterItemsCount));
                    }
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(this.GetGroupById(targetGroupId.Value)).Count();
                var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(sourceGroup);

                if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                {
                    throw new QuestionnaireException(string.Format("Sub-section or roster depth cannot be higher than {0}", MaxGroupDepth));
                }
                
            }
            
            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = targetGroupId.HasValue ? this.GetGroupById(targetGroupId.Value) : this.innerDocument;

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceGroup.GetParent() as IGroup);

            this.ThrowIfGroupFromRosterThatContainsRosterTitleQuestionMovedToAnotherGroup(sourceGroup, targetGroup);
            this.ThrowIfSourceGroupContainsInvalidRosterSizeQuestions(sourceGroup, targetGroup);
            this.ThrowIfGroupFromRosterThatContainsLinkedSourceQuestionsMovedToGroup(sourceGroup, targetGroup);
            this.ThrowIfGroupMovedFromRosterToGroupAndContainsRosterTitleInSubstitution(sourceGroup, targetGroup);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: sourceGroup.IsRoster,
                rosterSizeSource: sourceGroup.RosterSizeSource,
                rosterSizeQuestionId: sourceGroup.RosterSizeQuestionId, rosterFixedTitles: sourceGroup.FixedRosterTitles,
                rosterTitleQuestionId: sourceGroup.RosterTitleQuestionId,
                rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(targetGroup.PublicKey));

            this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(
                    text: sourceGroup.Title,
                    entityId: sourceGroup.PublicKey,
                    variableName: sourceGroup.VariableName,
                    parentGroup: targetGroup);

            this.MoveQuestionnaireItem(new QuestionnaireItemMoved
            {
                PublicKey = groupId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        #endregion

        private static QuestionCloned GetQuestionClonedEvent(IQuestion question, 
            Guid targetId, 
            Guid sourceQuestionnaireId, 
            Guid parentGroupId,
            int targetIndex,
            bool preserveVariableName, 
            Guid responsibleId)
        {
            var asTextQuestion = question as TextQuestion;
            var asMultioptions = question as IMultyOptionsQuestion;
            var asNumeric = question as NumericQuestion;
            var asListQuestion = question as TextListQuestion;
            var asDateTimeQuestion = question as DateTimeQuestion;

            var questionCloned = new QuestionCloned(
                publicKey: targetId,
                groupPublicKey: parentGroupId,
                questionText: question.QuestionText,
                questionType: question.QuestionType,
                variableLabel: question.VariableLabel,
                stataExportCaption: preserveVariableName ? question.StataExportCaption : null,
                featured: question.Featured,
                capital: question.Capital,

                questionScope: question.QuestionScope,
                conditionExpression: question.ConditionExpression,
                hideIfDisabled: question.HideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: question.Instructions,
                properties: question.Properties,

                answers: question.Answers.ToArray(),
                sourceQuestionId: question.PublicKey,
                sourceQuestionnaireId: sourceQuestionnaireId,
                targetIndex: targetIndex,
                responsibleId: responsibleId,
                linkedToQuestionId: question.LinkedToQuestionId,
                linkedToRosterId: question.LinkedToRosterId,

                areAnswersOrdered: asMultioptions?.AreAnswersOrdered,
                yesNoView: asMultioptions?.YesNoView,

                mask: asTextQuestion?.Mask,

                cascadeFromQuestionId: question.CascadeFromQuestionId,
                isFilteredCombobox: question.IsFilteredCombobox,

                isInteger: asNumeric?.IsInteger,
                countOfDecimalPlaces: asNumeric?.CountOfDecimalPlaces,
                maxAnswerCount: asListQuestion?.MaxAnswerCount,
                maxAllowedAnswers: asMultioptions?.MaxAllowedAnswers,
                answerOrder : null,
                validationConditions: question.ValidationConditions,
                linkedFilterExpression: question.LinkedFilterExpression,

                isTimestamp: asDateTimeQuestion?.IsTimestamp ?? false);

            return questionCloned;
        }

        public void AddDefaultTypeQuestionAdnMoveIfNeeded(AddDefaultTypeQuestion command)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(command.QuestionId);
            var parentGroup = this.GetGroupById(command.ParentGroupId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentGroupId);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.innerDocument.ConnectChildrenWithParent();
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }
           
            this.AddQuestion(new NewQuestionAdded(
                publicKey : command.QuestionId,
                groupPublicKey : command.ParentGroupId,
                questionText : command.Title,
                questionType : QuestionType.Text,
                stataExportCaption : null,
                variableLabel : null,
                featured : false,
                questionScope : QuestionScope.Interviewer,
                conditionExpression : null,
                hideIfDisabled: false,
                validationExpression: null,
                validationMessage : null,
                instructions : null,
                properties: new QuestionProperties(false, false), 
                responsibleId : command.ResponsibleId,
                linkedToQuestionId : null,
                areAnswersOrdered : null,
                maxAllowedAnswers : null,
                mask : null,
                isFilteredCombobox : false,
                cascadeFromQuestionId : null,
                capital:false,
                answerOrder:null,
                answers: null,
                isInteger: null,
                yesNoView: null,
                validationConditions: new List<ValidationCondition>()
            ));

            if (command.Index.HasValue)
            {
                this.MoveQuestionnaireItem(new QuestionnaireItemMoved
                {
                    PublicKey = command.QuestionId,
                    GroupKey = command.ParentGroupId,
                    TargetIndex = command.Index.Value,
                    ResponsibleId = command.ResponsibleId
                });
            }
        }

        public void DeleteQuestion(Guid questionId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfQuestionUsedInConditionOrValidationOfOtherQuestionsAndGroups(questionId);
            this.ThrowIfQuestionIsUsedAsRosterSize(questionId);
            this.ThrowIfQuestionIsUsedAsRosterTitle(questionId);
            this.ThrowIfQuestionIsUsedAsCascadingParent(questionId);

            this.DeleteQuestion(new QuestionDeleted() { QuestionId = questionId, ResponsibleId = responsibleId });
        }

        public void MoveQuestion(Guid questionId, Guid targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId);

            this.ThrowIfChapterHasMoreThanAllowedLimit(targetGroupId);

            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            var targetGroup = this.innerDocument.Find<IGroup>(targetGroupId);

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, question.GetParent() as IGroup);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(question.QuestionText, question.StataExportCaption,
                questionId, question.Featured, targetGroup);

            foreach (var validationCondition in question.ValidationConditions)
            {
                this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(
                    text: validationCondition.Message,
                    entityId: questionId,
                    variableName: question.StataExportCaption,
                    parentGroup: targetGroup);
            }

            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(question.Featured, targetGroup);
            this.ThrowDomainExceptionIfQuestionIsRosterTitleAndItsMovedToIncorrectGroup(question, targetGroup);

            this.ThrowDomainExceptionIfQuestionIsRosterSizeAndItsMovedToIncorrectGroup(question, targetGroup);

            this.MoveQuestionnaireItem(new QuestionnaireItemMoved
            {
                PublicKey = questionId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateTextQuestion(Guid questionId, string title, string variableName, string variableLabel, bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, string mask, Guid responsibleId, IList<ValidationCondition> validationCoditions, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled,
               QuestionType.Text, responsibleId, validationCoditions);

            this.UpdateQuestion(new QuestionChanged
            (
                publicKey : questionId,
                groupPublicKey : null, //?
                questionText : title,
                questionType : QuestionType.Text,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                featured : isPreFilled,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                responsibleId : responsibleId,
                mask : mask,
                capital:false,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                targetGroupKey: Guid.Empty,
                validationConditions: validationCoditions,
                linkedFilterExpression:null,
                isTimestamp: false
            ));
        }

        public void UpdateGpsCoordinatesQuestion(Guid questionId, string title, string variableName, string variableLabel, bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, IList<ValidationCondition> validationConditions, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled, QuestionType.GpsCoordinates, responsibleId, validationConditions);
            
            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: questionId,
                groupPublicKey: null, //?
                questionText: title,
                questionType: QuestionType.GpsCoordinates,
                stataExportCaption: variableName,
                variableLabel: variableLabel,
                featured: isPreFilled,
                questionScope: scope,
                conditionExpression: enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: instructions,
                properties: properties,
                responsibleId: responsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null, 
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                targetGroupKey: Guid.Empty,
                validationConditions: validationConditions,
                linkedFilterExpression: null,
                isTimestamp: false
            ));
        }

        public void UpdateDateTimeQuestion(UpdateDateTimeQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(command.QuestionId, parentGroup, title, variableName, command.IsPreFilled,
                QuestionType.DateTime, command.ResponsibleId, command.ValidationConditions);
            
            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: command.QuestionId,
                groupPublicKey: null, //?
                questionText: title,
                questionType: QuestionType.DateTime,
                stataExportCaption: variableName,
                variableLabel: command.VariableLabel,
                featured: command.IsPreFilled,
                questionScope: command.Scope,
                conditionExpression: command.EnablementCondition,
                hideIfDisabled: command.HideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: command.Instructions,
                properties: command.Properties,
                responsibleId: command.ResponsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                targetGroupKey: Guid.Empty,
                validationConditions: command.ValidationConditions,
                linkedFilterExpression: null,
                isTimestamp: command.IsTimestamp
            ));
        }

        public void UpdateMultiOptionQuestion(Guid questionId, string title, string variableName, string variableLabel, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, Option[] options, Guid? linkedToEntityId, bool areAnswersOrdered, int? maxAllowedAnswers, bool yesNoView, IList<ValidationCondition> validationConditions, string linkedFilterExpression, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, false, QuestionType.MultyOption, responsibleId, validationConditions);
            this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId, linkedToEntityId);
            this.ThrowIfCategoricalQuestionIsInvalid(questionId, options, linkedToEntityId, false, null, scope, null);
            this.ThrowIfMaxAllowedAnswersInvalid(QuestionType.MultyOption, linkedToEntityId, maxAllowedAnswers, options);
            this.ThrowIfCategoricalQuestionHasMoreThan200Options(options, linkedToEntityId.HasValue);

            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(linkedToEntityId, out linkedQuestionId, out linkedRosterId);

            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: questionId,
                groupPublicKey: null, //?
                questionText: title,
                questionType: QuestionType.MultyOption,
                stataExportCaption: variableName,
                variableLabel: variableLabel,
                featured: false,
                questionScope: scope,
                conditionExpression: enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: instructions,
                properties: properties,
                responsibleId: responsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: ConvertOptionsToAnswers(options),
                linkedToQuestionId: linkedQuestionId,
                linkedToRosterId: linkedRosterId,
                isInteger: null,
                areAnswersOrdered: areAnswersOrdered,
                yesNoView: yesNoView,
                maxAllowedAnswers: maxAllowedAnswers,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                targetGroupKey: Guid.Empty,
                validationConditions: validationConditions,
                linkedFilterExpression: linkedFilterExpression,
                isTimestamp:false
            ));
        }

        #region Question: SingleOption command handlers

        public void UpdateSingleOptionQuestion(Guid questionId, string title, string variableName, string variableLabel, bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, Option[] options, Guid? linkedToEntityId, bool isFilteredCombobox, Guid? cascadeFromQuestionId, IList<ValidationCondition> validationConditions, string linkedFilterExpression, QuestionProperties properties)
        {
            Answer[] answers;

            if (options == null && (isFilteredCombobox || cascadeFromQuestionId.HasValue))
            {
                IQuestion question = this.GetQuestion(questionId);
                answers = question.Answers.ToArray();                
            }
            else
            {
                answers = ConvertOptionsToAnswers(options);                
            }

            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled, QuestionType.SingleOption, responsibleId, validationConditions);

            if (isFilteredCombobox || cascadeFromQuestionId.HasValue)
            {
                var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
                answers = categoricalOneAnswerQuestion != null ? categoricalOneAnswerQuestion.Answers.ToArray() : null;
            }

            this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId, linkedToEntityId);
            this.ThrowIfCategoricalQuestionIsInvalid(questionId, options, linkedToEntityId, isPreFilled, isFilteredCombobox, scope, cascadeFromQuestionId);
            this.ThrowIfCascadingQuestionHasConditionOrValidation(questionId, cascadeFromQuestionId, validationConditions, enablementCondition);

            if (!isFilteredCombobox && !cascadeFromQuestionId.HasValue)
            {
                this.ThrowIfCategoricalQuestionHasMoreThan200Options(options, linkedToEntityId.HasValue);
            }
            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(linkedToEntityId, out linkedQuestionId, out linkedRosterId);

            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: questionId,
                groupPublicKey: null, //?
                questionText: title,
                questionType: QuestionType.SingleOption,
                stataExportCaption: variableName,
                variableLabel: variableLabel,
                featured: isPreFilled,
                questionScope: scope,
                conditionExpression: enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: instructions,
                properties: properties,
                responsibleId: responsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: answers,
                linkedToQuestionId: linkedQuestionId,
                linkedToRosterId:linkedRosterId,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: cascadeFromQuestionId,
                targetGroupKey: Guid.Empty,
                validationConditions: validationConditions,
                linkedFilterExpression: linkedFilterExpression,
                isTimestamp: false
            ));
        }

        private void  ExtractLinkedQuestionValues(Guid? linkedToEntityId, out Guid? linkedQuestionId, out Guid? linkedRosterId)
        {
            linkedQuestionId = linkedToEntityId.HasValue
                ? (this.innerDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == linkedToEntityId.Value) == null
                    ? (Guid?) null
                    : linkedToEntityId.Value)
                : null;

            linkedRosterId = linkedToEntityId.HasValue && !linkedQuestionId.HasValue
                ? linkedToEntityId.Value
                : (Guid?) null;
        }

        public void UpdateFilteredComboboxOptions(Guid questionId, Guid responsibleId, Option[] options)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfFilteredComboboxIsInvalid(questionId, options);
            ThrowIfNotLinkedCategoricalQuestionIsInvalid(options);

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);

            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: questionId,
                groupPublicKey: null, //?
                questionText: categoricalOneAnswerQuestion.QuestionText,
                questionType: categoricalOneAnswerQuestion.QuestionType,
                stataExportCaption: categoricalOneAnswerQuestion.StataExportCaption,
                variableLabel: categoricalOneAnswerQuestion.VariableLabel,
                featured: categoricalOneAnswerQuestion.Featured,
                questionScope: categoricalOneAnswerQuestion.QuestionScope,
                conditionExpression: categoricalOneAnswerQuestion.ConditionExpression,
                hideIfDisabled: categoricalOneAnswerQuestion.HideIfDisabled,
                validationExpression: categoricalOneAnswerQuestion.ValidationExpression,
                validationMessage: categoricalOneAnswerQuestion.ValidationMessage,
                instructions: categoricalOneAnswerQuestion.Instructions,
                properties: categoricalOneAnswerQuestion.Properties,
                responsibleId: responsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: ConvertOptionsToAnswers(options),
                linkedToQuestionId: categoricalOneAnswerQuestion.LinkedToQuestionId,
                linkedToRosterId:categoricalOneAnswerQuestion.LinkedToRosterId,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: categoricalOneAnswerQuestion.IsFilteredCombobox,
                cascadeFromQuestionId: categoricalOneAnswerQuestion.CascadeFromQuestionId,
                targetGroupKey: Guid.Empty,
                validationConditions: categoricalOneAnswerQuestion.ValidationConditions,
                linkedFilterExpression: null,
                isTimestamp: false
            ));
        }

        public void UpdateCascadingComboboxOptions(Guid questionId, Guid responsibleId, Option[] options)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfCascadingComboboxIsInvalid(questionId, options);
            ThrowIfNotLinkedCategoricalQuestionIsInvalid(options, isCascade: true);

            ThrowDomainExceptionIfOptionsHasEmptyParentValue(options);

            ThrowDomainExceptionIfOptionsHasNotDecimalParentValue(options);

            ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(options);

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);

            this.UpdateQuestion(new QuestionChanged
            (
                publicKey: questionId,
                groupPublicKey: null, //?
                questionText: categoricalOneAnswerQuestion.QuestionText,
                questionType: categoricalOneAnswerQuestion.QuestionType,
                stataExportCaption: categoricalOneAnswerQuestion.StataExportCaption,
                variableLabel: categoricalOneAnswerQuestion.VariableLabel,
                featured: categoricalOneAnswerQuestion.Featured,
                questionScope: categoricalOneAnswerQuestion.QuestionScope,
                conditionExpression: categoricalOneAnswerQuestion.ConditionExpression,
                hideIfDisabled: categoricalOneAnswerQuestion.HideIfDisabled,
                validationExpression: categoricalOneAnswerQuestion.ValidationExpression,
                validationMessage: categoricalOneAnswerQuestion.ValidationMessage,
                instructions: categoricalOneAnswerQuestion.Instructions,
                properties: categoricalOneAnswerQuestion.Properties,
                responsibleId: responsibleId,
                mask: null,
                capital: false,
                answerOrder: null,
                answers: ConvertOptionsToAnswers(options),
                linkedToQuestionId: categoricalOneAnswerQuestion.LinkedToQuestionId,
                linkedToRosterId: categoricalOneAnswerQuestion.LinkedToRosterId,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: categoricalOneAnswerQuestion.IsFilteredCombobox,
                cascadeFromQuestionId: categoricalOneAnswerQuestion.CascadeFromQuestionId,
                targetGroupKey: Guid.Empty,
                validationConditions: categoricalOneAnswerQuestion.ValidationConditions,
                linkedFilterExpression: null,
                isTimestamp: false
            ));
        }
        #endregion

        public void UpdateNumericQuestion(Guid questionId, 
            string title, 
            string variableName, 
            string variableLabel, 
            bool isPreFilled, 
            QuestionScope scope, 
            string enablementCondition,
            bool hideIfDisabled, 
            string instructions,
            QuestionProperties properties,
            Guid responsibleId, 
            bool isInteger, int? countOfDecimalPlaces, List<ValidationCondition> validationConditions)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled, QuestionType.Numeric, responsibleId, validationConditions);

            this.ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(isInteger, countOfDecimalPlaces);
            this.ThrowIfDecimalPlacesValueIsIncorrect(countOfDecimalPlaces);

            this.UpdateNumericQuestion(new NumericQuestionChanged
            (
                publicKey : questionId,
                questionText : title,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                featured : isPreFilled,
                capital : false,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression:null,
                validationMessage:null,
                instructions : instructions,
                properties: properties, 
                responsibleId : responsibleId,
                isInteger : isInteger,
                countOfDecimalPlaces : countOfDecimalPlaces,
                validationConditions: validationConditions
            ));
        }

        public void UpdateTextListQuestion(Guid questionId, string title, string variableName, string variableLabel, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, int? maxAnswerCount, QuestionScope scope, IList<ValidationCondition> validationConditions, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            var isPrefilled = false;

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPrefilled, QuestionType.TextList, responsibleId, validationConditions);

            this.UpdateTextListQuestion(new TextListQuestionChanged
            {
                PublicKey = questionId,

                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                Instructions = instructions,  
                Properties = properties,
                ResponsibleId = responsibleId,
                QuestionScope = scope,
                MaxAnswerCount = maxAnswerCount,
                ValidationConditions = validationConditions
            });
        }

        public void UpdateMultimediaQuestion(Guid questionId, string title, string variableName, string variableLabel, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, QuestionScope scope, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            var isPrefilled = false;
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPrefilled, QuestionType.Multimedia, responsibleId, null);

            this.UpdateMultimediaQuestion(new MultimediaQuestionUpdated()
            {
                QuestionId = questionId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                EnablementCondition = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                Instructions = instructions,
                Properties = properties,
                QuestionScope = scope,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateQRBarcodeQuestion(Guid questionId, string title, string variableName, string variableLabel, string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, QuestionScope scope, IList<ValidationCondition> validationConditions, 
            QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            var isPrefilled = false;
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPrefilled, QuestionType.QRBarcode, responsibleId, validationConditions);
            

            this.UpdateQRBarcodeQuestion(new QRBarcodeQuestionUpdated()
            {
                QuestionId = questionId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                EnablementCondition = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = null,
                ValidationMessage = null,
                Instructions = instructions,
                Properties = properties,
                QuestionScope = scope,
                ResponsibleId = responsibleId,
                ValidationConditions = validationConditions
            });
        }

        #region Static text command handlers
        public void AddStaticTextAndMoveIfNeeded(AddStaticText command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(command.EntityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);
            this.ThrowDomainExceptionIfStaticTextIsEmpty(command.Text);
            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentId);

            this.AddStaticText(new StaticTextAdded(command.EntityId, 
                command.ResponsibleId,
                command.ParentId, 
                command.Text,
                null,
                false,
                null));
            
            if (command.Index.HasValue)
            {
                this.MoveQuestionnaireItem(new QuestionnaireItemMoved
                {
                    PublicKey = command.EntityId,
                    GroupKey = command.ParentId,
                    TargetIndex = command.Index.Value,
                    ResponsibleId = command.ResponsibleId
                });
            }
        }

        public void UpdateStaticText(UpdateStaticText command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            
            this.ThrowDomainExceptionIfEntityDoesNotExists(command.EntityId);
            this.ThrowDomainExceptionIfStaticTextIsEmpty(command.Text);

            this.UpdateStaticText(new StaticTextUpdated(command.EntityId,
                command.ResponsibleId, 
                command.Text, 
                command.AttachmentName, 
                command.HideIfDisabled, 
                command.EnablementCondition, 
                command.ValidationConditions)
            );
        }

        public void DeleteStaticText(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);

            this.DeleteStaticText(new StaticTextDeleted() { EntityId = entityId, ResponsibleId = responsibleId });
        }

        public void MoveStaticText(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(targetEntityId);

            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = this.GetGroupById(targetEntityId);
            var sourceStaticText = this.innerDocument.Find<IStaticText>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceStaticText.GetParent() as IGroup);

            this.MoveQuestionnaireItem(new QuestionnaireItemMoved
            {
                PublicKey = entityId,
                GroupKey = targetEntityId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }
        #endregion


        #region Variable command handlers
        public void AddVariableAndMoveIfNeeded(AddVariable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(command.EntityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);
            this.ThrowDomainExceptionIfVariableNameIsInvalid(command.EntityId, command.VariableData.Name, DefaultVariableLengthLimit);
            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentId);

            this.AddVariable(new VariableAdded(
                command.EntityId, 
                command.ResponsibleId,
                command.ParentId, 
                command.VariableData));
            
            if (command.Index.HasValue)
            {
                this.MoveQuestionnaireItem(new QuestionnaireItemMoved
                {
                    PublicKey = command.EntityId,
                    GroupKey = command.ParentId,
                    TargetIndex = command.Index.Value,
                    ResponsibleId = command.ResponsibleId
                });
            }
        }

        public void UpdateVariable(UpdateVariable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            
            this.ThrowDomainExceptionIfEntityDoesNotExists(command.EntityId);
            this.ThrowDomainExceptionIfVariableNameIsInvalid(command.EntityId, command.VariableData.Name, DefaultVariableLengthLimit);

            this.UpdateVariable(new VariableUpdated(command.EntityId, command.ResponsibleId, command.VariableData));
        }

        public void DeleteVariable(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);

            this.DeleteVariable(new VariableDeleted() { EntityId = entityId, ResponsibleId = responsibleId });
        }

        public void MoveVariable(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(targetEntityId);

            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = this.GetGroupById(targetEntityId);
            var sourceVariable = this.innerDocument.Find<IVariable>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceVariable.GetParent() as IGroup);

            this.MoveQuestionnaireItem(new QuestionnaireItemMoved
            {
                PublicKey = entityId,
                GroupKey = targetEntityId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }
        #endregion

        #region Shared Person command handlers

        public void AddSharedPerson(Guid personId, string email, ShareType shareType, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (this.innerDocument.CreatedBy == personId)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.OwnerCannotBeInShareList,
                    string.Format("User {0} is an owner of this questionnaire. Please, input another email.", email));
            }

            if (this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserExistInShareList,
                    string.Format("User {0} already exist in share list.", email));
            }

            this.AddSharedPersonToQuestionnaire(new SharedPersonToQuestionnaireAdded()
            {
                PersonId = personId,
                Email = email,
                ShareType = shareType,
                ResponsibleId = responsibleId
            });
        }

        public void RemoveSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (!this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserDoesNotExistInShareList,
                    "Couldn't remove user, because it doesn't exist in share list");
            }

            this.RemoveSharedPersonFromQuestionnaire(new SharedPersonFromQuestionnaireRemoved()
            {
                PersonId = personId,
                ResponsibleId = responsibleId
            });
        }

        #endregion

        #region CopyPaste command handler
        
        public void PasteAfter(PasteAfter pasteAfter)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteAfter.ResponsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(pasteAfter.ItemToPasteAfterId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteAfter.EntityId);
            ThrowDomainExceptionIfEntityDoesNotExists(pasteAfter.SourceDocument, pasteAfter.SourceItemId);

            this.innerDocument.ConnectChildrenWithParent();
            pasteAfter.SourceDocument.ConnectChildrenWithParent();

            var itemToInsertAfter = this.innerDocument.Find<IComposite>(pasteAfter.ItemToPasteAfterId);
            var targetToPasteIn = itemToInsertAfter.GetParent();
            var entityToInsert = pasteAfter.SourceDocument.Find<IComposite>(pasteAfter.SourceItemId);
            var targetIndex = targetToPasteIn.Children.IndexOf(itemToInsertAfter) + 1;

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);

            this.GeneratePasteEvents(pasteAfter.EntityId, pasteAfter.ResponsibleId, 
                pasteAfter.SourceDocument, entityToInsert, targetToPasteIn, targetIndex);
        }

        public void PasteInto(PasteInto pasteInto)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteInto.ResponsibleId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteInto.EntityId);
            ThrowDomainExceptionIfEntityDoesNotExists(pasteInto.SourceDocument, pasteInto.SourceItemId);

            this.innerDocument.ConnectChildrenWithParent();
            pasteInto.SourceDocument.ConnectChildrenWithParent();

            this.ThrowDomainExceptionIfGroupDoesNotExist(pasteInto.ParentId);

            var entityToInsert = pasteInto.SourceDocument.Find<IComposite>(pasteInto.SourceItemId);
            var targetToPasteIn = this.GetGroupById(pasteInto.ParentId);
            var targetIndex = targetToPasteIn.Children.Count();

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);

            this.GeneratePasteEvents(pasteInto.EntityId, pasteInto.ResponsibleId, pasteInto.SourceDocument, entityToInsert, targetToPasteIn, targetIndex);
        }

        private void CheckDepthInvariants(IComposite targetToPasteIn, IComposite entityToInsert)
        {
            if (targetToPasteIn.PublicKey != this.EventSourceId)
            {
                var targetChapter = this.innerDocument.GetChapterOfItemById(targetToPasteIn.PublicKey);

                var numberOfMovedItems = entityToInsert.Children
                    .TreeToEnumerable(x => x.Children)
                    .Count();

                var numberOfItemsInChapter = targetChapter.Children
                    .TreeToEnumerable(x => x.Children)
                    .Count();

                if ((numberOfMovedItems + numberOfItemsInChapter) >= MaxChapterItemsCount - 1)
                {
                    throw new QuestionnaireException(string.Format("Section cannot have more than {0} elements",
                        MaxChapterItemsCount));
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(this.GetGroupById(targetToPasteIn.PublicKey)).Count();

                var entityToInsertAsGroup = entityToInsert as IGroup;
                if (entityToInsertAsGroup != null)
                {
                    var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(entityToInsertAsGroup);

                    if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                    {
                        throw new QuestionnaireException(string.Format("Sub-section or roster depth cannot be higher than {0}",
                            MaxGroupDepth));
                    }
                }
            }
        }

        private void GeneratePasteEvents(Guid pasteItemId, Guid responsibleId,
            QuestionnaireDocument sourceDocument, IComposite entityToInsert, IComposite targetToPasteIn, int targetIndex)
        {
            var entityToInsertAsQuestion = entityToInsert as IQuestion;
            if (entityToInsertAsQuestion != null)
            {
                if (targetToPasteIn.PublicKey == this.EventSourceId)
                    throw new QuestionnaireException(string.Format("Question cannot be pasted here."));

                var questionCloned = GetQuestionClonedEvent(entityToInsertAsQuestion, pasteItemId, sourceDocument.PublicKey,
                    targetToPasteIn.PublicKey,
                    targetIndex, true, responsibleId);

                this.CloneQuestion(questionCloned);

                return;
            }

            var entityToInsertAsStaticText = entityToInsert as IStaticText;
            if (entityToInsertAsStaticText != null)
            {
                if (targetToPasteIn.PublicKey == this.EventSourceId)
                    throw new QuestionnaireException(string.Format("Static Text cannot be pasted here."));

                this.CloneStaticText(new StaticTextCloned(
                    entityId : pasteItemId,
                    parentId : targetToPasteIn.PublicKey,
                    sourceEntityId : entityToInsert.PublicKey,
                    sourceQuestionnaireId : sourceDocument.PublicKey,
                    targetIndex : targetIndex,
                    text : entityToInsertAsStaticText.Text,
                    attachmentName : entityToInsertAsStaticText.AttachmentName,
                    responsibleId : responsibleId,
                    enablementCondition: entityToInsertAsStaticText.ConditionExpression,
                    hideIfDisabled: entityToInsertAsStaticText.HideIfDisabled,
                    validationConditions: entityToInsertAsStaticText.ValidationConditions));

                return;
            }

            var entityToInsertAsGroup = entityToInsert as IGroup;
            if (entityToInsertAsGroup != null)
            {
                //roster as chapter is forbidden
                if (entityToInsertAsGroup.IsRoster && (targetToPasteIn.PublicKey == this.EventSourceId))
                    throw new QuestionnaireException(string.Format("Roster cannot be pasted here."));

                //roster, group, chapter
                Dictionary<Guid, Guid> replacementIdDictionary = (entityToInsert).TreeToEnumerable(x => x.Children).ToDictionary(y => y.PublicKey, y => Guid.NewGuid());
                replacementIdDictionary[entityToInsert.PublicKey] = pasteItemId;

                var events = new List<IEvent>();

                this.FillGroup(
                    parentGroupId: targetToPasteIn == null ? (Guid?) null : targetToPasteIn.PublicKey,
                    responsibleId: responsibleId,
                    sourceGroup: entityToInsertAsGroup,
                    sourceQuestionnaireId: sourceDocument.PublicKey,
                    targetIndex: targetIndex,
                    preserveVariableName: true,
                    replacementIdDictionary: replacementIdDictionary,
                    events: events);

                events.ForEach(this.ApplyEvent);

                return;
            }

            var entityToInsertAsVariable = entityToInsert as IVariable;
            if (entityToInsertAsVariable != null)
            {
                if (targetToPasteIn.PublicKey == this.EventSourceId)
                    throw new QuestionnaireException(string.Format("Variable cannot be pasted here."));

                this.CloneVariable(new VariableCloned(
                    entityId: pasteItemId,
                    parentId: targetToPasteIn.PublicKey,
                    sourceEntityId: entityToInsert.PublicKey,
                    sourceQuestionnaireId: sourceDocument.PublicKey,
                    targetIndex: targetIndex,
                    responsibleId: responsibleId,
                    variableData: new VariableData(entityToInsertAsVariable.Type,
                                                   entityToInsertAsVariable.Name,
                                                   entityToInsertAsVariable.Expression)));

                return;
            }

            throw new QuestionnaireException(string.Format("Unknown item type. Paste failed."));
        }

        #endregion

        #region Questionnaire Invariants

        private void ThrowIfQuestionIsUsedAsCascadingParent(Guid questionId)
        {
            var usedInCascades = this.innerDocument.Find<SingleQuestion>(x => x.CascadeFromQuestionId == questionId).Any();
            if (usedInCascades)
            {
                throw new QuestionnaireException(ExceptionMessages.CantRemoveParentQuestionInCascading);
            }
        }

        private void ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(Guid questionId, IGroup parentGroup, string title, string variableName, bool isPrefilled, QuestionType questionType, Guid responsibleId, IList<ValidationCondition> validationCoditions)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfTitleIsEmptyOrTooLong(title);

            int variableLengthLimit = RestrictedVariableLengthQuestionTypes.Contains(questionType)
                ? DefaultRestrictedVariableLengthLimit
                : DefaultVariableLengthLimit;

            this.ThrowDomainExceptionIfVariableNameIsInvalid(questionId, variableName, variableLengthLimit);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(title, variableName, questionId, isPrefilled, parentGroup);

            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(isPrefilled, parentGroup);

            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            if (validationCoditions != null)
            {
                foreach (var validationCondition in validationCoditions)
                {
                    this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(
                        text: validationCondition.Message,
                        entityId: questionId,
                        variableName: variableName,
                        parentGroup: parentGroup);
                }
            }
        }

        private void ThrowIfChapterHasMoreThanAllowedLimit(Guid itemId)
        {
            var chapter = this.innerDocument.GetChapterOfItemById(itemId);
            if (chapter.Children.TreeToEnumerable(x => x.Children).Count() >= MaxChapterItemsCount)
            {
                throw new QuestionnaireException(string.Format("Section cannot have more than {0} child items", MaxChapterItemsCount));
            }
        }

        private void ThrowIfTargetGroupHasReachedAllowedDepthLimit(Guid itemId)
        {
            
            var entity = innerDocument.Find<IComposite>(itemId);
            if (entity != null)
            {
                var targetGroupDepth = this.GetAllParentGroups(entity).Count();

                if ((targetGroupDepth) >= MaxGroupDepth)
                {
                    throw new QuestionnaireException(string.Format("Sub-section or roster  depth cannot be higher than {0}",
                        MaxGroupDepth));
                }
            }
        }

        private void ThrowIfGroupMovedFromRosterToGroupAndContainsRosterTitleInSubstitution(IGroup sourceGroup, IGroup targetGroup)
        {
            if (this.IsRosterOrInsideRoster(targetGroup) || sourceGroup.IsRoster)
                return;

            this.ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(sourceGroup);
        }

        private void ThrowIfTargetIndexIsNotAcceptable(int targetIndex, IGroup targetGroup, IGroup parentGroup)
        {
            var maxAcceptableIndex = targetGroup.Children.Count;
            if (parentGroup != null && targetGroup.PublicKey == parentGroup.PublicKey)
                maxAcceptableIndex--;

            if (targetIndex < 0 || maxAcceptableIndex < targetIndex)
                throw new QuestionnaireException(
                   string.Format(
                       "You can't move to sub-section {0} because it position {1} in not acceptable",
                       FormatGroupForException(targetGroup.PublicKey, this.innerDocument), targetIndex));
        }

        private void ThrowIfGroupFromRosterThatContainsLinkedSourceQuestionsMovedToGroup(IGroup sourceGroup, IGroup targetGroup)
        {
            if (this.IsRosterOrInsideRoster(targetGroup)) return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(sourceGroup).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't move sub-section {0} to another sub-section because it contains linked source question(s): {1}",
                        FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowIfSourceGroupContainsInvalidRosterSizeQuestions(IGroup sourceGroup, IGroup targetGroup)
        {
            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(sourceGroup).Select(question => question.PublicKey);

            var rosterSizeQuestionsInGroup = this.GetAllRosterSizeQuestionIds().Intersect(allQuestionsIdsFromGroup);
            var rosterSizeQuestionsOfTargetGroupAndUpperRosters = GetRosterSizeQuestionsOfGroupAndUpperRosters(targetGroup);

            if (rosterSizeQuestionsOfTargetGroupAndUpperRosters.Intersect(rosterSizeQuestionsInGroup).Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't move sub-section {0} to roster {1} because it contains roster source question(s): {2}",
                        FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                        FormatGroupForException(targetGroup.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            rosterSizeQuestionsInGroup.Select(questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowIfGroupFromRosterThatContainsRosterTitleQuestionMovedToAnotherGroup(IGroup sourceGroup, IGroup targetGroup)
        {
            if (!IsRosterOrInsideRoster(sourceGroup) && !ContainsRoster(sourceGroup)) return;

            IEnumerable<IQuestion> groupQuestions = GetAllQuestionsInGroup(sourceGroup);

            var rosterTitleQuestionsWithDependentGroups =
                from question in groupQuestions
                let groupsWhereQuestionIsRosterTitle = this.GetGroupsByRosterTitleId(question.PublicKey, sourceGroup.PublicKey)
                where groupsWhereQuestionIsRosterTitle.Any()
                select new { RostertTitleQuestion = question, DependentGroups = groupsWhereQuestionIsRosterTitle };

            if (rosterTitleQuestionsWithDependentGroups.All(question => !question.DependentGroups.Any())) return;

            Func<Guid, IEnumerable<IGroup>, string> getWarningMessage = (rosterTitleQuestionId, invalidGroups) =>
            {
                return string.Format("Question {0} used as roster title question in sub-section(s):{1}{2}",
                    this.FormatQuestionForException(rosterTitleQuestionId, this.innerDocument), Environment.NewLine,
                    string.Join(Environment.NewLine,
                        invalidGroups.Select(group => FormatGroupForException(@group.PublicKey, this.innerDocument))));
            };

            if (IsRosterOrInsideRoster(targetGroup))
            {
                var rosterForTargetGroup = GetFirstRosterParentGroupOrNull(targetGroup);

                var rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion =
                    rosterTitleQuestionsWithDependentGroups.Select(
                        question =>
                            new
                            {
                                RostertTitleQuestion = question.RostertTitleQuestion,
                                DependentGroups =
                                    question.DependentGroups.Where(
                                        group => group.RosterSizeQuestionId != rosterForTargetGroup.RosterSizeQuestionId)
                            }).Where(question => question.DependentGroups.Any());

                if (!rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion.Any()) return;

                var warningsForRosterTitlesNotInRostersByRosterSize =
                    rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion.Select(
                        x => getWarningMessage(x.RostertTitleQuestion.PublicKey, x.DependentGroups));

                throw new QuestionnaireException(
                    string.Join(
                        string.Format(
                            "Sub-section {0} could not be moved to sub-section {1} because contains some questions that used as roster title questions in groups which have roster size question not the same as have target {1} group: ",
                            FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                            FormatGroupForException(targetGroup.PublicKey, this.innerDocument)), Environment.NewLine,
                        string.Join(Environment.NewLine, warningsForRosterTitlesNotInRostersByRosterSize)));
            }
            else
            {
                if (sourceGroup.IsRoster || ContainsRoster(sourceGroup)) return;

                var warningsForRosterTitlesNotInRostersByRosterSize =
                    rosterTitleQuestionsWithDependentGroups.Select(
                        x => getWarningMessage(x.RostertTitleQuestion.PublicKey, x.DependentGroups));

                throw new QuestionnaireException(
                    string.Join(
                        string.Format("Sub-section {0} could not be moved to sub-section {1} because: ",
                            FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                            FormatGroupForException(targetGroup.PublicKey, this.innerDocument)),
                        Environment.NewLine,
                        string.Join(Environment.NewLine, warningsForRosterTitlesNotInRostersByRosterSize)));
            }
        }

        private void ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(bool isPrefilled, IGroup parentGroup)
        {
            if (isPrefilled && IsRosterOrInsideRoster(parentGroup))
                throw new QuestionnaireException("Question inside roster sub-section can not be pre-filled.");
        }

        private void ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupTitleRequired,
                    "The titles of sections and sub-sections can not be empty or contains whitespace only");
            }

            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.TitleIsTooLarge,
                    string.Format("The titles of sections and sub-sections can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionnaireTitleRequired,
                    "Questionnaire's title can not be empty or contains whitespace only");
            }
            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.TitleIsTooLarge, 
                    string.Format("Questionnaire's title can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private static void ThrowDomainExceptionIfEntityDoesNotExists(QuestionnaireDocument doc, Guid entityId)
        {
            var entity = doc.Find<IComposite>(entityId);
            if (entity == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format("Questionnaire item with id {0} can't be found", entityId));
            }
        }

        private void ThrowDomainExceptionIfEntityDoesNotExists(Guid entityId)
        {
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);
        }

        private void ThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format("Question with public key {0} can't be found", publicKey));
            }
        }

        private void ThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupNotFound,
                    string.Format("sub-section with public key {0} can't be found", groupPublicKey));
            }
        }

        private void ThrowDomainExceptionIfTitleIsEmptyOrTooLong(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new QuestionnaireException(DomainExceptionType.QuestionTitleRequired, "Question text can't be empty");

            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(DomainExceptionType.TitleIsTooLarge,
                    string.Format("Question text can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private void ThrowDomainExceptionIfStaticTextIsEmpty(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new QuestionnaireException(DomainExceptionType.StaticTextIsEmpty, "Static text is empty");
        }

        private void ThrowDomainExceptionIfVariableNameIsInvalid(Guid questionPublicKey, string stataCaption, int variableLengthLimit)
        {
            if (string.IsNullOrEmpty(stataCaption))
            {
                return;
            }
            
            bool isTooLong = stataCaption.Length > variableLengthLimit;
            if (isTooLong)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameMaxLength, string.Format("This element's name or ID shouldn't be longer than {0} characters.", variableLengthLimit));
            }

            bool containsInvalidCharacters = stataCaption.Any(c => !(c == '_' || Char.IsLetterOrDigit(c)));
            if (containsInvalidCharacters)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameSpecialCharacters,
                    "Valid variable or roster ID name should contain only letters, digits and underscore character");
            }

            bool startsWithDigitOrUnderscore = Char.IsDigit(stataCaption[0]) || stataCaption[0] == '_';
            if (startsWithDigitOrUnderscore)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameStartWithDigit, "Variable name or roster ID shouldn't starts with digit or underscore");
            }

            bool endsWithUnderscore = stataCaption[stataCaption.Length-1] == '_';
            if (endsWithUnderscore)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameEndsWithUnderscore, "Variable name or roster ID shouldn't end with underscore");
            }

            bool hasConsecutiveUnderscore = stataCaption.Contains("__");
            if (hasConsecutiveUnderscore)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameHasConsecutiveUnderscores, "Variable name or roster ID shouldn't have two and more consecutive underscore characters.");
            }

            var captions = this.innerDocument.GetEntitiesByType<AbstractQuestion>()
                .Where(q => q.PublicKey != questionPublicKey)
                .Select(q => q.StataExportCaption);

            bool isNotUnique = captions.Contains(stataCaption);
            if (isNotUnique)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VarialbeNameNotUnique, "Variable name or roster ID should be unique in questionnaire's scope");
            }
            
            var keywords = this.variableNameValidator.GetAllReservedKeywords();

            foreach (var keyword in keywords.Where(keyword => stataCaption.ToLower() == keyword)) {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameShouldNotMatchWithKeywords,
                    keyword + " is a keyword. Variable name or roster ID shouldn't match with keywords");
            }
        }

        private void ThrowIfCascadingQuestionHasConditionOrValidation(Guid questionId, Guid? cascadeFromQuestionId, IList<ValidationCondition> validationExpression, string enablementCondition)
        {
            if (!cascadeFromQuestionId.HasValue )
            {
                return;
            }

            if (validationExpression.Count > 0)
            {
                throw new QuestionnaireException(ExceptionMessages.CascadingCantHaveValidationExpression);
            }

            if (!string.IsNullOrWhiteSpace(enablementCondition))
            {
                throw new QuestionnaireException(ExceptionMessages.CascadingCantHaveConditionExpression);
            }
        }
        private void ThrowIfCategoricalQuestionIsInvalid(Guid questionId, Option[] options, Guid? linkedToEntityId, bool isFeatured, bool? isFilteredCombobox, QuestionScope scope, Guid? cascadeFromQuestionId)
        {
            bool entityIsLinked = linkedToEntityId.HasValue;
            bool questionHasOptions = options != null && options.Any();

            if (entityIsLinked && questionHasOptions)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.ConflictBetweenLinkedQuestionAndOptions,
                    "Categorical question cannot be with answers and linked to another question in the same time");
            }

            if (cascadeFromQuestionId.HasValue && entityIsLinked)
            {
                throw new QuestionnaireException(ExceptionMessages.CantBeLinkedAndCascadingAtSameTime);
            }

            if (cascadeFromQuestionId.HasValue)
            {
                var cascadefrom = this.innerDocument.Find<IQuestion>(x => x.PublicKey == cascadeFromQuestionId).FirstOrDefault();
                if (cascadefrom == null)
                {
                    throw new QuestionnaireException(ExceptionMessages.ShouldCascadeFromExistingQuestion);
                }
            }

            if (entityIsLinked)
            {
                //this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId);
                this.ThrowIfLinkedEntityIsInvalid(linkedToEntityId, isFeatured);
                this.ThrowIfLinkedCategoricalQuestionIsNotFilledByInterviewer(scope);
            }
            else if (isFilteredCombobox != true && !(cascadeFromQuestionId.HasValue))
            {
                ThrowIfNotLinkedCategoricalQuestionIsInvalid(options);
            }
        }

        private void ThrowIfCategoricalQuestionHasMoreThan200Options(Option[] options, bool isLinkedQuestion)
        {
            if (!isLinkedQuestion && options.Count() > 200)
            {
                throw new QuestionnaireException(DomainExceptionType.CategoricalQuestionHasMoreThan200Options, ExceptionMessages.CategoricalQuestionHasMoreThan200Options);
            }
        }

        private void ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(Guid questionId, Guid? linkedToQuestionId)
        {
            if (!linkedToQuestionId.HasValue) return;

            var rosterTitleQuestionGroups =
                this.innerDocument.Find<IGroup>(
                    group => group.RosterTitleQuestionId.HasValue && group.RosterTitleQuestionId.Value == questionId)
                    .Select(group => group.PublicKey);

            if (rosterTitleQuestionGroups.Any())
            {
                throw new QuestionnaireException(
                    string.Format("Linked categorical multi-select question could not be used as a roster title question in sub-section(s): {0}",
                        string.Join(Environment.NewLine,
                            rosterTitleQuestionGroups.Select(groupId => FormatGroupForException(groupId, this.innerDocument)))));
            }
        }

        private void ThrowIfMaxAllowedAnswersInvalid(QuestionType questionType, Guid? linkedToQuestionId, int? maxAllowedAnswers,
            Option[] options)
        {
            if (!maxAllowedAnswers.HasValue) return;

            if (maxAllowedAnswers.Value < 2)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.MaxAllowedAnswersLessThan2,
                    "Maximum Allowed Answers for question should be more than one");
            }

            if (!linkedToQuestionId.HasValue && maxAllowedAnswers.Value > options.Length)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.MaxAllowedAnswersMoreThanOptions,
                    "Maximum Allowed Answers more than question's options");
            }
        }

        private void ThrowIfLinkedEntityIsInvalid(Guid? linkedToEntityId, bool isPrefilled)
        {
            var linkedToQuestion =
                this.innerDocument.Find<IQuestion>(x => x.PublicKey == linkedToEntityId).FirstOrDefault();

            if (linkedToQuestion == null)
            {
                var linkedToRoster =
                    this.innerDocument.Find<IGroup>(x => x.PublicKey == linkedToEntityId).FirstOrDefault();

                if (linkedToRoster == null)
                    throw new QuestionnaireException(
                        DomainExceptionType.LinkedEntityDoesNotExist,
                        "Entity that you are linked to does not exist");

                if(!linkedToRoster.IsRoster)
                    throw new QuestionnaireException(
                       DomainExceptionType.GroupYouAreLinkedToIsNotRoster,
                       "Group that you are linked to is not a roster");

                return;
            }

            bool typeOfLinkedQuestionIsNotSupported = !(
                linkedToQuestion.QuestionType == QuestionType.DateTime ||
                    linkedToQuestion.QuestionType == QuestionType.Numeric ||
                    linkedToQuestion.QuestionType == QuestionType.Text);

            if (typeOfLinkedQuestionIsNotSupported)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.NotSupportedQuestionForLinkedQuestion,
                    "Linked question can be only type of number, text or date");
            }

            if (isPrefilled)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionWithLinkedQuestionCanNotBeFeatured,
                    "Question that linked to another question can not be pre-filled");
            }

            if (!this.IsUnderPropagatableGroup(linkedToQuestion))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.LinkedQuestionIsNotInPropagateGroup,
                    "Question that you are linked to is not in the roster group");
            }
        }

        private void ThrowIfLinkedCategoricalQuestionIsNotFilledByInterviewer(QuestionScope scope)
        {
            if (scope == QuestionScope.Supervisor)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.LinkedCategoricalQuestionCanNotBeFilledBySupervisor,
                    "Linked categorical questions cannot be filled by supervisor");
            }
        }

        private static void ThrowIfNotLinkedCategoricalQuestionIsInvalid(Option[] options, bool isCascade = false)
        {
            if ((options == null || !options.Any() || options.Count() < 2))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorEmpty, "Question with options should have two options at least");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Value)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueRequired, "Answer option value is required");
            }

            var tooLongValues = options.Where(option => option.Value.Length > 16).Select(option => option.Value).ToList();
            if (tooLongValues.Any())
            {
                throw new QuestionnaireException(string.Format("Following option values are too long: {0}", string.Join(", ", tooLongValues)));
            }

            if (options.Any(x => !x.Value.IsDecimal()))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Option value should have only number characters");
            }

            if (!AreElementsUnique(options.Select(x => x.Value)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueNotUnique,
                    "Option values must be unique for categorical question");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Title)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorTextRequired, "Answer title can't be empty");
            }

            if (!isCascade && !AreElementsUnique(options.Select(x => x.Title)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorTextNotUnique, "Answer title is not unique");
            }
        }

        private void ThrowDomainExceptionIfEntityAlreadyExists(Guid entityId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                elementId: entityId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.EntityWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more questionnaire item(s) with same ID {0} already exists",
                        entityId));
        }

        private void ThrowDomainExceptionIfQuestionAlreadyExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfGroupAlreadyExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.GroupWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more sub-section(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfMoreThanOneEntityExists(Guid entityId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                elementId: entityId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneEntityWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more questionnaire item(s) with same ID {0} already exists",
                        entityId));
        }

        private void ThrowDomainExceptionIfMoreThanOneQuestionExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneQuestionsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(bool isInteger, int? countOfDecimalPlaces)
        {
            if (isInteger && countOfDecimalPlaces.HasValue)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.IntegerQuestionCantHaveDecimalPlacesSettings,
                    "Roster size question can't have decimal places settings");
            }
        }

        private void ThrowIfDecimalPlacesValueIsIncorrect(int? countOfDecimalPlaces)
        {
            if (!countOfDecimalPlaces.HasValue)
                return;

            if (countOfDecimalPlaces.Value > MaxCountOfDecimalPlaces)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Number of decimal places '{0}' exceeded maximum '{1}'.", countOfDecimalPlaces, MaxCountOfDecimalPlaces));
            }

            if (countOfDecimalPlaces.Value < 0)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Number of decimal places cant be negative."));
            }

            if (countOfDecimalPlaces.Value == 0)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("If you want number of decimal places equals to 0 please select integer."));
            }
        }

        private void ThrowDomainExceptionIfMoreThanOneGroupExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneGroupsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more sub-section(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(Guid elementId, int expectedCount,
            DomainExceptionType exceptionType, Func<IEnumerable<T>, string> getExceptionDescription)
            where T : class, IComposite
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(x => x.PublicKey == elementId, expectedCount,
                exceptionType, getExceptionDescription);
        }

        private void ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(Func<T, bool> condition, int expectedCount,
            DomainExceptionType exceptionType, Func<IEnumerable<T>, string> getExceptionDescription)
            where T : class, IComposite
        {
            List<T> elementsWithSameId = this.innerDocument.Find(condition).ToList();

            if (elementsWithSameId.Count > expectedCount)
            {
                throw new QuestionnaireException(exceptionType, getExceptionDescription(elementsWithSameId));
            }
        }

        private void ThrowDomainExceptionIfMacroAlreadyExist(Guid macroId)
        {
            if (this.macroIds.Contains(macroId))
            {
                throw new QuestionnaireException(DomainExceptionType.MacroAlreadyExist, ExceptionMessages.MacroAlreadyExist);
            }
        }

        private void ThrowDomainExceptionIfMacroContentIsEmpty(string macroContent)
        {
            if (string.IsNullOrWhiteSpace(macroContent))
            {
                throw new QuestionnaireException(DomainExceptionType.MacroContentIsEmpty, ExceptionMessages.MacroContentIsEmpty);
            }
        }

        private void ThrowDomainExceptionIfMacroIsAbsent(Guid macroId)
        {
            if (!this.macroIds.Contains(macroId))
            {
                throw new QuestionnaireException(DomainExceptionType.MacroIsAbsent, ExceptionMessages.MacroIsAbsent);
            }
        }

        private void ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(Guid viewerId)
        {
            if (this.innerDocument.CreatedBy != viewerId && !this.innerDocument.SharedPersons.Contains(viewerId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit,
                    "You don't have permissions for changing this questionnaire");
            }
            if (this.readOnlyUsers.Contains(viewerId))
            {
                throw new QuestionnaireException(
                   DomainExceptionType.DoesNotHavePermissionsForEdit,
                   "You don't have permissions for changing this questionnaire");
            }
        }

        private void ThrowDomainExceptionIfOptionsHasEmptyParentValue(Option[] options)
        {
            if (options.Select(x => x.ParentValue).Any(string.IsNullOrWhiteSpace))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsCantContainsEmptyParentValueField,
                    ExceptionMessages.CategoricalCascadingOptionsCantContainsEmptyParentValueField);
            }
        }

        private void ThrowDomainExceptionIfOptionsHasNotDecimalParentValue(Option[] options)
        {
            decimal d;
            if (options.Select(x => x.ParentValue).Any(number => !Decimal.TryParse(number, out d)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsCantContainsNotDecimalParentValueField,
                    ExceptionMessages.CategoricalCascadingOptionsCantContainsNotDecimalParentValueField);
            }
        }

        private void ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(Option[] options)
        {

            if (options.Select(x => x.ParentValue + "$" + x.Title).Distinct().Count() != options.Length)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair,
                    ExceptionMessages.CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair);
            }
        }

        private void ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(string text, string variableName,
            Guid questionId, bool prefilled, IGroup parentGroup)
        {
            if (this.substitutionService.GetAllSubstitutionVariableNames(text).Length > 0  && prefilled)
            {
                throw new QuestionnaireException(DomainExceptionType.FeaturedQuestionTitleContainsSubstitutionReference,
                    "Pre-filled question text contains substitution references. It's illegal");
            }    

            this.ThrowDomainExceptionIfTextContainsIncorrectSubstitution(text, variableName, questionId, parentGroup);
        }

        private void ThrowDomainExceptionIfTextContainsIncorrectSubstitution(string text, string variableName,
            Guid entityId, IGroup parentGroup)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(text);
            if (substitutionReferences.Length == 0)
                return;

            if (substitutionReferences.Contains(variableName))
            {
                throw new QuestionnaireException(DomainExceptionType.TextContainsSubstitutionReferenceToSelf,
                    "Text contains illegal substitution references to self");
            }

            List<string> unknownReferences, questionsIncorrectTypeOfReferenced, questionsIllegalPropagationScope, variablesIllegalPropagationScope;

            this.innerDocument.ConnectChildrenWithParent(); //find all references and do it only once

            this.ValidateSubstitutionReferences(entityId, parentGroup, substitutionReferences,
                out unknownReferences,
                out questionsIncorrectTypeOfReferenced,
                out questionsIllegalPropagationScope,
                out variablesIllegalPropagationScope);

            if (unknownReferences.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.TextContainsUnknownSubstitutionReference,
                    "Text contains unknown substitution references: " + String.Join(", ", unknownReferences.ToArray()));

            if (questionsIncorrectTypeOfReferenced.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.TextContainsSubstitutionReferenceQuestionOfInvalidType,
                    "Text contains substitution references to questions of illegal type: " +
                    String.Join(", ", questionsIncorrectTypeOfReferenced.ToArray()));

            if (questionsIllegalPropagationScope.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.TextContainsInvalidSubstitutionReference,
                    "Text contains illegal substitution references to questions: " +
                    String.Join(", ", questionsIllegalPropagationScope.ToArray()));

            if (variablesIllegalPropagationScope.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.TextContainsInvalidSubstitutionReference,
                    "Text contains illegal substitution references to variables: " +
                    String.Join(", ", variablesIllegalPropagationScope.ToArray()));
        }

        private void ThrowDomainExceptionIfQuestionUsedInConditionOrValidationOfOtherQuestionsAndGroups(Guid questionId)
        {
            var question = this.innerDocument.FirstOrDefault<IQuestion>(x => x.PublicKey == questionId);

            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                condition:
                    x => this.IsGroupAndHasQuestionIdInCondition(x, question) || this.IsQuestionAndHasQuestionIdInConditionOrValidation(x, question),
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion,
                getExceptionDescription:
                    groupsAndQuestions => string.Format("One or more questions/sub-sections depend on {0}:{1}{2}",
                        question.QuestionText,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, GetTitleList(groupsAndQuestions))));
        }

        private void ThrowDomainExceptionIfGroupQuestionsUsedAsRosterTitleQuestionOfOtherGroups(Guid groupId)
        {
            var groupQuestions = this.innerDocument.Find<IQuestion>(x => IsQuestionParent(groupId, x));

            var referencedQuestions = groupQuestions.ToDictionary(question => question.PublicKey,
                question => this.GetGroupsByRosterTitleId(question.PublicKey, groupId).Select(GetTitle));

            if (referencedQuestions.Values.Count(x => x.Any()) > 0)
            {
                throw new QuestionnaireException(DomainExceptionType.QuestionUsedAsRosterTitleOfOtherGroup, string.Join(Environment.NewLine,
                    referencedQuestions.Select(x => string.Format("Question {0} used as roster title question in sub-section(s):{1}{2}",
                        FormatQuestionForException(x.Key, this.innerDocument),
                        Environment.NewLine,
                        string.Join(Environment.NewLine, x.Value)))));
            }
        }

        
        private void ThrowDomainExceptionIfQuestionIsRosterTitleAndItsMovedToIncorrectGroup(IQuestion question, IGroup targetGroup)
        {

            if (!GetGroupsByRosterTitleId(question.PublicKey).Any())
                return;

            this.innerDocument.ConnectChildrenWithParent();

            IGroup sourceRoster = GetFirstRosterParentGroupOrNull(question);
            IGroup targetRoster = GetFirstRosterParentGroupOrNull(targetGroup);

            if (targetRoster == null || (sourceRoster != null && targetRoster != null && sourceRoster.RosterSizeQuestionId.HasValue &&
                targetRoster.RosterSizeQuestionId.HasValue && sourceRoster.RosterSizeQuestionId != targetRoster.RosterSizeQuestionId))
                throw new QuestionnaireException(
                    string.Format(
                        "You can move a roster title question {0} only to a roster that has a roster source question {1}",
                        this.FormatQuestionForException(question.PublicKey, this.innerDocument),
                        this.FormatQuestionForException(sourceRoster.RosterSizeQuestionId.Value, this.innerDocument)));
        }

        private void ThrowDomainExceptionIfQuestionIsRosterSizeAndItsMovedToIncorrectGroup(AbstractQuestion question, IGroup targetGroup)
        {
            var groupsByRosterSizeQuestion =
                this.GetGroupsByRosterSizeQuestion(question.PublicKey).Select(x => x.PublicKey);

            if (!groupsByRosterSizeQuestion.Any())
                return;

            foreach (var groupByRosterSizeQuestion in groupsByRosterSizeQuestion)
            {
                if (
                    !this.IsReferencedItemInTheSameScopeWithReferencesItem(GetQuestionnaireItemDepthAsVector(targetGroup.PublicKey),
                        GetQuestionnaireItemDepthAsVector(groupByRosterSizeQuestion)))
                    //   if (GetQuestionnaireItemDepth(targetGroup.PublicKey) > GetQuestionnaireItemDepth(groupByRosterSizeQuestion) - 1)
                    throw new QuestionnaireException(string.Format(
                        "Roster source question {0} cannot be placed deeper then roster.",
                        FormatQuestionForException(question.PublicKey, this.innerDocument)));
            }
        }

        private void ThrowIfRosterInformationIsIncorrect(Guid groupId, bool isRoster, RosterSizeSourceType rosterSizeSource,
            Guid? rosterSizeQuestionId, FixedRosterTitle[] rosterFixedTitles, Guid? rosterTitleQuestionId, Func<Guid[]> rosterDepthFunc)
        {
            if (!isRoster) return;

            switch (rosterSizeSource)
            {
                case RosterSizeSourceType.Question:
                    this.ThrowIfRosterSizeQuestionIsIncorrect(groupId, rosterSizeQuestionId, rosterTitleQuestionId, rosterFixedTitles,
                        rosterDepthFunc);
                    break;
                case RosterSizeSourceType.FixedTitles:
                    this.ThrowIfRosterByFixedTitlesIsIncorrect(rosterSizeQuestionId, rosterTitleQuestionId, rosterFixedTitles);
                    break;
            }
        }

        private void ThrowIfRosterByFixedTitlesIsIncorrect(Guid? rosterSizeQuestionId, Guid? rosterTitleQuestionId,
            FixedRosterTitle[] rosterFixedTitles)
        {
            if (rosterFixedTitles == null || rosterFixedTitles.Length < 2)
            {
                throw new QuestionnaireException("List of titles for fixed set of items roster should contain at least two items");
            }
            
            if (rosterFixedTitles.Length > 250)
            {
                throw new QuestionnaireException("Number of titles for fixed set of items roster could not be more than 250");
            }

            if (rosterFixedTitles.Any(item => string.IsNullOrWhiteSpace(item.Title)))
            {
                throw new QuestionnaireException("Fixed set of items roster title could not have empty titles");
            }
            
            if (rosterSizeQuestionId.HasValue)
            {
                throw new QuestionnaireException("Fixed set of items roster could not have roster source question");
            }

            if (rosterTitleQuestionId.HasValue)
            {
                throw new QuestionnaireException("Fixed set of items roster could not have roster title question");
            }
        }

        private void ThrowIfRosterSizeQuestionIsIncorrect(Guid groupId, Guid? rosterSizeQuestionId, Guid? rosterTitleQuestionId,
            FixedRosterTitle[] rosterFixedTitles, Func<Guid[]> rosterDepthFunc)
        {
            if (!rosterSizeQuestionId.HasValue)
                throw new QuestionnaireException("Roster source question is empty");

            var localRosterSizeQuestionId = rosterSizeQuestionId.Value;
            var rosterSizeQuestion = this.innerDocument.Find<IQuestion>(localRosterSizeQuestionId);
            if (rosterSizeQuestion == null)
                // TODO: Guid should be replaced, but question is missing, so title or variable name cannot be found 
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} is missing in questionnaire.",
                    rosterSizeQuestionId));

            if (!RosterSizeQuestionTypes.Contains(rosterSizeQuestion.QuestionType))
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} should have numeric or categorical multi-select Answers or List type.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (
                !this.IsReferencedItemInTheSameScopeWithReferencesItem(this.GetQuestionnaireItemDepthAsVector(rosterSizeQuestionId),
                    rosterDepthFunc()))
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} cannot be placed deeper then roster.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));


            if (rosterSizeQuestion.QuestionType == QuestionType.MultyOption && rosterSizeQuestion.LinkedToQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} should not be linked.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.MultyOption && rosterTitleQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster having categorical multi-select question {0} as roster size source cannot have roster title question.",
                    this.FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.TextList && rosterTitleQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster having list question {0} as roster size source cannot have roster title question.",
                    this.FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.Numeric)
            {
                var numericQuestion = (INumericQuestion)rosterSizeQuestion;

                if (!numericQuestion.IsInteger)
                    throw new QuestionnaireException(string.Format(
                        "Roster source question {0} should be Integer.",
                        FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

                if (rosterTitleQuestionId.HasValue)
                {
                    var rosterTitleQuestion = this.innerDocument.Find<IQuestion>(rosterTitleQuestionId.Value);
                    if (rosterTitleQuestion == null)
                        // TODO: Guid should be replaced, but question is missing, so title or variable name cannot be found 
                        throw new QuestionnaireException(string.Format(
                            "Roster title question {0} is missing in questionnaire.", rosterTitleQuestionId));

                    if (
                        !IsRosterTitleInRosterByRosterSize(rosterTitleQuestion: rosterTitleQuestion,
                            rosterSizeQuestionId: localRosterSizeQuestionId, currentRosterId: groupId))
                        throw new QuestionnaireException(string.Format(
                            "Question for roster titles {0} should be placed only inside groups where roster source question is {1}",
                            FormatQuestionForException(rosterTitleQuestionId.Value, this.innerDocument),
                            FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));
                }
            }

            if (rosterFixedTitles != null && rosterFixedTitles.Any())
            {
                throw new QuestionnaireException(string.Format("Roster fixed items list should be empty for roster by question: {0}.",
                    FormatGroupForException(groupId, this.innerDocument)));
            }
        }

        private void ThrowIfQuestionIsUsedAsRosterSize(Guid questionId)
        {
            var referencingRoster = this.innerDocument.Find<IGroup>(group => @group.RosterSizeQuestionId == questionId).FirstOrDefault();

            if (referencingRoster != null)
                throw new QuestionnaireException(
                    string.Format("Question {0} is referenced as roster source question by sub-section {1}.",
                        FormatQuestionForException(questionId, this.innerDocument),
                        FormatGroupForException(referencingRoster.PublicKey, this.innerDocument)));
        }

        private void ThrowIfQuestionIsUsedAsRosterTitle(Guid questionId)
        {
            var referencingRosterTitle =
                this.innerDocument.Find<IGroup>(
                    group =>
                        @group.RosterTitleQuestionId == questionId && group.RosterSizeQuestionId.HasValue &&
                            this.innerDocument.FirstOrDefault<IQuestion>(
                                question =>
                                    question.PublicKey == @group.RosterSizeQuestionId.Value && question.QuestionType == QuestionType.Numeric) != null)
                    .FirstOrDefault();

            if (referencingRosterTitle != null)
                throw new QuestionnaireException(
                    string.Format("Question {0} is referenced as roster title question by sub-section {1}.",
                        FormatQuestionForException(questionId, this.innerDocument),
                        FormatGroupForException(referencingRosterTitle.PublicKey, this.innerDocument)));
        }

        private void ThrowIfRosterHaveAQuestionThatUsedAsRosterTitleQuestionOfOtherGroups(IGroup group)
        {
            var allRosterTitleQuestions =
                this.innerDocument.Find<IGroup>(g => g.PublicKey != group.PublicKey && g.RosterTitleQuestionId.HasValue)
                    .Select(g => g.RosterTitleQuestionId.Value);


            if (!allRosterTitleQuestions.Any()) return;

            var allQuestionsInGroup = GetAllQuestionsInGroup(group).Select(q => q.PublicKey);

            var rosterTitleQuestionsOfOtherGroups =
                allQuestionsInGroup.Intersect(allRosterTitleQuestions);

            if (!rosterTitleQuestionsOfOtherGroups.Any()) return;

            throw new QuestionnaireException(
                string.Format(
                    "This roster can't become a sub-section because contains a roster title questions of other sub-section(s): {0}",
                    string.Join(Environment.NewLine,
                        rosterTitleQuestionsOfOtherGroups.Select(
                            questionId => FormatQuestionForException(questionId, this.innerDocument)))));
        }

        private void ThrowIfGroupCantBecomeARosterBecauseOfPrefilledQuestions(IGroup group)
        {
            var hasAnyPrefilledQuestion = this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group).Any(question => question.Featured);

            if (!hasAnyPrefilledQuestion) return;

            var questionVariables = GetFilteredQuestionForException(@group, question => question.Featured);

            throw new QuestionnaireException(
                string.Format(
                    "This sub-section can't become a roster because contains pre-filled questions: {0}. Toggle off pre-filled property for that questions to complete this operation",
                    string.Join(Environment.NewLine, questionVariables)));
        }

        private void ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(IGroup group, bool wasRosterAndBecomeAGroup = false)
        {
            var hasAnyQuestionsWithRosterTitleInSubstitutions =
                this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group)
                    .Where(x => wasRosterAndBecomeAGroup || GetFirstRosterParentGroupOrNull(x, group) == null)
                    .Any(
                        question =>
                            this.substitutionService.GetAllSubstitutionVariableNames(question.QuestionText)
                                .Contains(this.substitutionService.RosterTitleSubstitutionReference));

            if (!hasAnyQuestionsWithRosterTitleInSubstitutions) return;

            var questionVariables = GetFilteredQuestionForException(@group, question => this.substitutionService.GetAllSubstitutionVariableNames(question.QuestionText)
                                .Contains(this.substitutionService.RosterTitleSubstitutionReference));

            throw new QuestionnaireException(
                string.Format(
                    "This sub-section can't become a roster because contains questions with reference on roster title in substitution: {0}.",
                    string.Join(Environment.NewLine, questionVariables)));
        }

        private void ThrowIfRosterCantBecomeAGroupBecauseContainsLinkedSourceQuestions(IGroup group)
        {
            if (GetFirstRosterParentGroupOrNull(group.GetParent()) != null)
                return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(@group).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "This {0} roster can't become a sub-section because contains linked source question(s): {1}",
                        FormatGroupForException(@group.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowDomainExceptionIfRosterQuestionsUsedAsLinkedSourceQuestions(IGroup group)
        {
            if (!this.IsRosterOrInsideRoster(@group)) return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(@group).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't delete {0} sub-section because it contains linked source question(s): {1}",
                        FormatGroupForException(@group.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowDomainExceptionIfFilteredComboboxIsInvalid(Guid questionId, Option[] options)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.FilteredComboboxQuestionNotFound,
                    string.Format("Combo box with public key {0} can't be found", questionId));
            }

            if (!categoricalOneAnswerQuestion.IsFilteredCombobox.HasValue || !categoricalOneAnswerQuestion.IsFilteredCombobox.Value)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionIsNotAFilteredCombobox,
                    string.Format("Question {0} is not a combo box", FormatQuestionForException(questionId, this.innerDocument)));
            }

            if (options != null && options.Length > maxFilteredComboboxOptionsCount)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.FilteredComboboxQuestionOptionsMaxLength,
                    string.Format("Combo box question {0} contains more than {1} options",
                        FormatQuestionForException(questionId, this.innerDocument), maxFilteredComboboxOptionsCount));
            }
        }

        private void ThrowDomainExceptionIfCascadingComboboxIsInvalid(Guid questionId, Option[] options)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format("Combo box with public key {0} can't be found", questionId));
            }
            
            if (options != null && options.Length > maxCascadingComboboxOptionsCount)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingQuestionOptionsMaxLength,
                    string.Format("Combo box question {0} contains more than {1} options",
                        FormatQuestionForException(questionId, this.innerDocument), maxCascadingComboboxOptionsCount));
            }
        }

        private void ThrowIfExpressionsAreAlreadyMigrated()
        {
            if (this.wasExpressionsMigrationPerformed)
                throw new QuestionnaireException("Expressions are already migrated to C#.");
        }

        #endregion

        #region Utilities

        private static void PrepareGeneralProperties(ref string title, ref string variableName)
        {
            if (variableName != null)
                variableName = variableName.Trim();

            if (title != null)
                title = title.Trim();
        }

        private void ValidateSubstitutionReferences(Guid entityId, IGroup parentGroup, string[] substitutionReferences,
            out List<string> unknownReferences, 
            out List<string> questionsIncorrectTypeOfReferenced,
            out List<string> questionsIllegalPropagationScope,
            out List<string> variablesIllegalPropagationScope)
        {
            unknownReferences = new List<string>();
            questionsIncorrectTypeOfReferenced = new List<string>();
            questionsIllegalPropagationScope = new List<string>();
            variablesIllegalPropagationScope = new List<string>();

            var questions = this.innerDocument.GetEntitiesByType<AbstractQuestion>()
                .Where(q => q.PublicKey != entityId)
                .Where(q => !string.IsNullOrEmpty(q.StataExportCaption))
                .GroupBy(q => q.StataExportCaption, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var variables = this.innerDocument.GetEntitiesByType<Variable>()
                .Where(v => v.PublicKey != entityId && !string.IsNullOrEmpty(v.Name))
                .GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);


            var rosterVectorOfEntity = GetQuestionnaireItemDepthAsVector(parentGroup.PublicKey);

            foreach (var substitutionReference in substitutionReferences)
            {
                if (substitutionReference == this.substitutionService.RosterTitleSubstitutionReference)
                {
                    if (rosterVectorOfEntity.Length > 0)
                        continue;
                }
                //extract validity of variable name to separate method and make check validity of substitutionReference  
                if (substitutionReference.Length > 32)
                {
                    unknownReferences.Add(substitutionReference);
                    continue;
                }

                bool isQuestionReference = questions.ContainsKey(substitutionReference);
                bool isVariableReference = variables.ContainsKey(substitutionReference);

                if (!isQuestionReference && !isVariableReference)
                {
                    unknownReferences.Add(substitutionReference);
                }
                else if (isQuestionReference)
                {
                    var currentQuestion = questions[substitutionReference];
                    bool typeOfRefQuestionIsNotSupported = !(currentQuestion.QuestionType == QuestionType.DateTime ||
                        currentQuestion.QuestionType == QuestionType.Numeric ||
                        currentQuestion.QuestionType == QuestionType.SingleOption ||
                        currentQuestion.QuestionType == QuestionType.Text ||
                        currentQuestion.QuestionType == QuestionType.AutoPropagate ||
                        currentQuestion.QuestionType == QuestionType.QRBarcode);

                    if (typeOfRefQuestionIsNotSupported)
                        questionsIncorrectTypeOfReferenced.Add(substitutionReference);

                    if (!this.IsReferencedItemInTheSameScopeWithReferencesItem(this.GetQuestionnaireItemDepthAsVector(currentQuestion.PublicKey), rosterVectorOfEntity))
                        questionsIllegalPropagationScope.Add(substitutionReference);
                }
                else if (isVariableReference)
                {
                    var currentVariable = variables[substitutionReference];

                    if (!this.IsReferencedItemInTheSameScopeWithReferencesItem(this.GetQuestionnaireItemDepthAsVector(currentVariable.PublicKey), rosterVectorOfEntity))
                        variablesIllegalPropagationScope.Add(substitutionReference);
                }
            }
        }

        private static Answer[] ConvertOptionsToAnswers(Option[] options)
        {
            if (options == null)
                return null;

            return options.Select(ConvertOptionToAnswer).ToArray();
        }

        private static Answer ConvertOptionToAnswer(Option option)
        {
            return new Answer
            {
                PublicKey = option.Id,
                AnswerValue = option.Value,
                AnswerText = option.Title,
                ParentValue = option.ParentValue
            };
        }

        private IEnumerable<Guid> GetRosterSizeQuestionsOfGroupAndUpperRosters(IGroup group)
        {
            var targerGroup = @group;
            while (targerGroup != null)
            {
                if (targerGroup.IsRoster && targerGroup.RosterSizeQuestionId.HasValue)
                    yield return targerGroup.RosterSizeQuestionId.Value;

                targerGroup = targerGroup.GetParent() as IGroup;
            }
        }

        private IEnumerable<IQuestion> GetAllCategoricalLinkedQuestions()
        {
            return this.innerDocument.Find<IQuestion>(question => question.LinkedToQuestionId.HasValue);
        }

        private IEnumerable<Guid> GetAllLinkedSourceQuestions()
        {
            return this.GetAllCategoricalLinkedQuestions().Select(question => question.LinkedToQuestionId.Value);
        }

        private IEnumerable<Guid> GetAllRosterSizeQuestionIds()
        {
            return
                this.innerDocument.Find<IGroup>(group => group.RosterSizeQuestionId.HasValue)
                    .Select(group => group.RosterSizeQuestionId.Value);
        }

        private bool IsUnderPropagatableGroup(IComposite item)
        {
            this.innerDocument.ConnectChildrenWithParent();

            return this.GetFirstRosterParentGroupOrNull(item) != null;
        }

        private IGroup GetFirstRosterParentGroupOrNull(IComposite item, IGroup stopGroup = null)
        {
            while (item != null)
            {
                var parentGroup = item as IGroup;
                if (stopGroup != null && parentGroup != null && parentGroup == stopGroup)
                {
                    return parentGroup.IsRoster ? parentGroup : (IGroup) null;
                }
                if (parentGroup != null && parentGroup.IsRoster)
                    return parentGroup;

                item = item.GetParent();
            }
            return null;
        }

        private Guid GetScopeOrRoster(IGroup entityAsGroup)
        {
            if (entityAsGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                return entityAsGroup.PublicKey;

            if (!entityAsGroup.RosterSizeQuestionId.HasValue)
                return entityAsGroup.PublicKey;

            var rosterSizeQuestion = innerDocument.Find<IQuestion>(entityAsGroup.RosterSizeQuestionId.Value);
            if (rosterSizeQuestion == null)
                return entityAsGroup.PublicKey;

            return rosterSizeQuestion.PublicKey;
        }

        private Guid[] GetQuestionnaireItemDepthAsVector(Guid? itemId)
        {
            if (!itemId.HasValue)
                return new Guid[0];

            var entity = innerDocument.Find<IComposite>(itemId.Value);
            if (entity == null)
                return new Guid[0];

            var scopeIds = new List<Guid>();

            var entityAsGroup = entity as IGroup;
            if (entityAsGroup != null && entityAsGroup.IsRoster)
            {
                scopeIds.Add(GetScopeOrRoster(entityAsGroup));
            }

            this.innerDocument.ConnectChildrenWithParent();
            var currentParent = (IGroup)entity.GetParent();
            while (currentParent != null)
            {
                if (currentParent.IsRoster)
                    scopeIds.Add(GetScopeOrRoster(currentParent));

                currentParent = (IGroup)currentParent.GetParent();
            }
            return scopeIds.ToArray();
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption == stataCaption);
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionId);
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private IEnumerable<IGroup> GetGroupsByRosterTitleId(Guid questionId, params Guid[] exceptGroups)
        {
            return this.innerDocument.Find<IGroup>(
                group => !exceptGroups.Contains(group.PublicKey) && (group.RosterTitleQuestionId == questionId));
        }

        private bool IsRosterOrInsideRoster(IGroup group)
        {
            while (group != null)
            {
                if (group.IsRoster)
                    return true;

                group = (IGroup)group.GetParent();
            }

            return false;
        }

        private bool ContainsRoster(IGroup group)
        {
            if (group != null)
            {
                foreach (var subGroup in group.Children.OfType<IGroup>())
                {
                    if (subGroup.IsRoster || ContainsRoster(subGroup))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AreElementsUnique(IEnumerable<string> elements)
        {
            return elements.Distinct().Count() == elements.Count();
        }

        private bool IsReferencedItemInTheSameScopeWithReferencesItem(Guid[] referencedItemRosterVector, Guid[] referencesItemRosterVector)
        {
            if (referencedItemRosterVector.Length > referencesItemRosterVector.Length)
                return false;

            return referencedItemRosterVector.All(referencesItemRosterVector.Contains);
        }

        private bool IsRosterTitleInRosterByRosterSize(IQuestion rosterTitleQuestion, Guid rosterSizeQuestionId, Guid currentRosterId)
        {
            var groupsByRosterSizeQuestion =
                this.GetGroupsByRosterSizeQuestion(rosterSizeQuestionId).Select(x => x.PublicKey).ToHashSet();
            groupsByRosterSizeQuestion.Add(currentRosterId);

            var parentForRosterTitleQuestion = rosterTitleQuestion.GetParent();
            while (parentForRosterTitleQuestion != null)
            {
                if (groupsByRosterSizeQuestion.Contains(parentForRosterTitleQuestion.PublicKey))
                    return true;
                var parentGroup = parentForRosterTitleQuestion as IGroup;
                if (parentGroup != null && parentGroup.IsRoster)
                    break;

                parentForRosterTitleQuestion = parentForRosterTitleQuestion.GetParent();
            }

            return false;
        }

        private IGroup GetRosterByrVariableName(string rosterName)
        {
            return this.innerDocument.Find<IGroup>(group => group.VariableName == rosterName && group.IsRoster).FirstOrDefault();
        }

        private IEnumerable<IGroup> GetGroupsByRosterSizeQuestion(Guid rosterSizeQuestionId)
        {
            return this.innerDocument.Find<IGroup>(
                group => group.RosterSizeQuestionId == rosterSizeQuestionId);
        }

        private IEnumerable<IQuestion> GetAllQuestionsInGroup(IGroup group)
        {
            var questionsInGroup = new List<IQuestion>();

            foreach (var groupItem in group.Children)
            {
                var itemAsGroup = groupItem as IGroup;
                if (itemAsGroup != null)
                {
                    questionsInGroup.AddRange(GetAllQuestionsInGroup(itemAsGroup));
                }

                var itemAsQuestion = groupItem as IQuestion;
                if (itemAsQuestion != null)
                {
                    questionsInGroup.Add(itemAsQuestion);
                }
            }
            return questionsInGroup;
        }

        private string[] GetFilteredQuestionForException(IGroup @group, Func<AbstractQuestion, bool> filter)
        {
            return this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group)
                .Where(filter)
                .Select(question => string.Format("'{0}', [{1}]", question.QuestionText, question.StataExportCaption))
                .ToArray();
        }

        private string FormatQuestionForException(Guid questionId, QuestionnaireDocument document)
        {
            var question = document.Find<IQuestion>(questionId);

            return string.Format("'{0}', {1}", question.QuestionText, question.StataExportCaption);
        }

        private IEnumerable<IGroup> GetAllParentGroups(IComposite entity)
        {
            this.innerDocument.ConnectChildrenWithParent();

            var currentParent = (IGroup)entity.GetParent();

            while (currentParent != null)
            {
                yield return currentParent;

                currentParent = (IGroup)currentParent.GetParent();
            }
        }

        private bool IsQuestionParent(Guid groupId, IQuestion question)
        {
            return GetAllParentGroups(question).Any(x => x.PublicKey == groupId);
        }

        private IGroup GetGroupById(Guid groupId)
        {
            return this.innerDocument.Find<IGroup>(groupId);
        }

        private static string[] GetTitleList(IEnumerable<IComposite> groupsAndQuestions)
        {
            return groupsAndQuestions.Select(GetTitle).ToArray();
        }

        private static string GetTitle(IComposite composite)
        {
            var question = composite as IQuestion;
            var group = composite as IGroup;
            if (group != null)
            {
                return group.Title;
            }
            if (question != null)
            {
                return question.QuestionText;
            }
            return "<untitled>";
        }

        private bool IsQuestionAndHasQuestionIdInConditionOrValidation(IComposite composite, IQuestion sourceQuestion)
        {
            var question = composite as IQuestion;
            bool isSelfReferenceIsChecking = composite.PublicKey == sourceQuestion.PublicKey;

            if (isSelfReferenceIsChecking)
            {
                // we should allow do delete questions that reference itself in condition or validation expression
                return false;
            }

            if (question != null)
            {
                string questionId = sourceQuestion.PublicKey.ToString();
                string alias = sourceQuestion.StataExportCaption;

                IEnumerable<string> conditionIds = new List<string>();
                if (IsExpressionDefined(question.ConditionExpression))
                {
                    conditionIds = this.expressionProcessor.GetIdentifiersUsedInExpression(question.ConditionExpression);
                }

                List<string> validationIds = new List<string>();
                foreach (var validationCondition in question.ValidationConditions)
                {
                    if (IsExpressionDefined(validationCondition.Expression))
                    {
                        validationIds.AddRange(this.expressionProcessor.GetIdentifiersUsedInExpression(validationCondition.Expression));
                    }
                }

                return validationIds.Contains(questionId) || validationIds.Contains(alias) ||
                    conditionIds.Contains(questionId) || conditionIds.Contains(alias);
            }
            return false;
        }

        private bool IsGroupAndHasQuestionIdInCondition(IComposite composite, IQuestion question)
        {
            var group = composite as IGroup;
            if (group != null && IsExpressionDefined(group.ConditionExpression))
            {
                string alias = question.StataExportCaption;

                IEnumerable<string> conditionIds = this.expressionProcessor.GetIdentifiersUsedInExpression(group.ConditionExpression).ToList();
               
                return conditionIds.Contains(alias);
            }
            return false;
        }

        private static string FormatGroupForException(Guid groupId, QuestionnaireDocument questionnaireDocument)
        {
            return string.Format("'{0}'", GetGroupTitleForException(groupId, questionnaireDocument));
        }

        private static string GetGroupTitleForException(Guid groupId, QuestionnaireDocument questionnaireDocument)
        {
            var @group = questionnaireDocument.Find<IGroup>(groupId);

            return @group != null
                ? @group.Title ?? "<<NO GROUP TITLE>>"
                : "<<MISSING GROUP>>";
        }

        private static void UpdateCustomMappingsWithContextQuestion(Dictionary<string, string> customMappings, IQuestion contextQuestion)
        {
            customMappings["this"] = contextQuestion.StataExportCaption;
        }

        private Dictionary<string, string> BuildCustomMappingsFromIdsToIdentifiers()
        {
            return this.innerDocument
                .Find<IQuestion>(question => !string.IsNullOrWhiteSpace(question.StataExportCaption))
                .SelectMany(question => new []
                {
                    new KeyValuePair<string, string>(question.PublicKey.ToString(), question.StataExportCaption),
                    new KeyValuePair<string, string>(question.PublicKey.FormatGuid(), question.StataExportCaption),
                })
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);
        }

        private static bool HasEnablementCondition(IGroup group)
        {
            return !string.IsNullOrWhiteSpace(@group.ConditionExpression);
        }

        
        private FixedRosterTitle[] GetRosterFixedTitlesOrThrow(FixedRosterTitleItem[] rosterFixedTitles)
        {
            if (rosterFixedTitles == null)
                return new FixedRosterTitle[0];

            if (rosterFixedTitles.Any(x => x == null))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Invalid title list");
            }

            if (rosterFixedTitles.Any(x => String.IsNullOrWhiteSpace(x.Value)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Fixed set of items roster value is required");
            }

            if (rosterFixedTitles.Any(x => !x.Value.IsDecimal()))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Fixed set of items roster value should have only number characters");
            }

            if (rosterFixedTitles.Select(x => x.Value).Distinct().Count() != rosterFixedTitles.Length)
            {
                throw new QuestionnaireException("Fixed set of items roster values must be unique");
            }

            return rosterFixedTitles.Select(item => new FixedRosterTitle(decimal.Parse(item.Value), item.Title)).ToArray();                
                
        }

        private static int GetMaxChildGroupNestingDepth(IGroup group)
        {
            int maxDepth = 1;
            Queue<Tuple<IGroup, int>> queue = new Queue<Tuple<IGroup, int>>();
            queue.Enqueue(Tuple.Create(group, 1));

            while (queue.Any())
            {
                var item = queue.Dequeue();
                foreach (var subGroup in item.Item1.Children.OfType<IGroup>())
                {
                    queue.Enqueue(Tuple.Create(subGroup, item.Item2 + 1));
                }

                if (item.Item2 > maxDepth)
                    maxDepth = item.Item2;
            }

            return maxDepth;
        }

        #endregion

        #region Create clone events

        private IEnumerable<IEvent> CreateTextQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel,
            bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, QuestionProperties sourceProperties,
            string mask, Guid parentGroupId, Guid sourceQuestionId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId,
            IList<ValidationCondition> validationConditions)
        {
            yield return new QuestionCloned(
                publicKey: questionId,
                groupPublicKey: parentGroupId,
                questionText: title,
                questionType: QuestionType.Text,
                stataExportCaption: variableName,
                variableLabel: variableLabel,
                questionScope: scope,
                conditionExpression: enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression: null,
                validationMessage: null,
                instructions: instructions,
                properties: sourceProperties,
                sourceQuestionId: sourceQuestionId,
                sourceQuestionnaireId: sourceQuestionnaireId,
                targetIndex: targetIndex,
                responsibleId: responsibleId,
                featured: isPreFilled,
                capital: false,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId:null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: mask,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null,
                isTimestamp: false
            );
        }

        private IEnumerable<IEvent> CreateGeoLocationQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, 
            string enablementCondition, bool hideIfDisabled, string instructions, QuestionProperties properties, Guid parentGroupId, Guid sourceQuestionId,
            Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId, QuestionScope scope, bool featured, bool capital,
            IList<ValidationCondition> validationConditions)
        {
            yield return new QuestionCloned(
                publicKey: questionId,
                groupPublicKey: parentGroupId,
                questionText: title,
                questionType: QuestionType.GpsCoordinates,
                stataExportCaption: variableName,
                variableLabel: variableLabel,
                questionScope: scope,
                conditionExpression: enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                sourceQuestionId : sourceQuestionId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                responsibleId : responsibleId,
                featured:featured,
                capital: capital,
                answerOrder: null,
                answers: null,
                linkedToQuestionId : null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers : null,
                mask : null,
                isFilteredCombobox : null,
                cascadeFromQuestionId : null,
                maxAnswerCount : null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null,
                isTimestamp: false
            );
        }

        private IEnumerable<IEvent> CreateDateTimeQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, 
            bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string instructions, QuestionProperties properties,
            Guid parentGroupId, Guid sourceQuestionId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId,
            IList<ValidationCondition> validationConditions, bool isTimestamp)
        {
            yield return new QuestionCloned(
                publicKey : questionId,
                groupPublicKey : parentGroupId,
                questionText : title,
                questionType : QuestionType.DateTime,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                featured : isPreFilled,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                sourceQuestionId : sourceQuestionId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                responsibleId : responsibleId,
                capital: false,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null,
                isTimestamp: isTimestamp);
        }

        private IEnumerable<IEvent> CreateCategoricalMultiAnswersQuestionClonedEvents(
            Guid questionId, 
            string title, 
            string variableName, 
            string variableLabel, 
            QuestionScope scope, 
            string enablementCondition, 
            bool hideIfDisabled,
            string instructions,
            QuestionProperties properties,
            Guid parentGroupId, 
            Guid sourceQuestionId, 
            Guid sourceQuestionnaireId,
            int targetIndex, 
            Guid responsibleId, 
            Option[] options, 
            Guid? linkedToQuestionId,
            Guid? linkedToRosterId,
            bool areAnswersOrdered, 
            int? maxAllowedAnswers,
            bool yesNoView,
            IList<ValidationCondition> validationConditions,
            string linkedFilterExpression)
        {
            yield return new QuestionCloned(
            
                publicKey : questionId,
                groupPublicKey : parentGroupId,
                questionText : title,
                questionType : QuestionType.MultyOption,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                answers : ConvertOptionsToAnswers(options),
                sourceQuestionId : sourceQuestionId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                responsibleId : responsibleId,
                linkedToQuestionId : linkedToQuestionId,
                linkedToRosterId: linkedToRosterId,
                areAnswersOrdered : areAnswersOrdered,
                maxAllowedAnswers : maxAllowedAnswers,
                yesNoView : yesNoView,
                featured: false,
                capital: false,
                answerOrder: null,
                isInteger: null,
                mask: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: linkedFilterExpression,
                isTimestamp: false);
        }

        private IEnumerable<IEvent> CreateCategoricalSingleAnswerQuestionEvents(Guid questionId, string title, string variableName, string variableLabel, 
            bool isPreFilled, QuestionScope scope, string enablementCondition, bool hideIfDisabled, string validationExpression, string validationMessage, string instructions,
            QuestionProperties properties,
            Guid parentGroupId, Guid sourceQuestionId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId, Option[] options, 
            Guid? linkedToQuestionId, Guid? linkedToRosterId, bool? isFilteredCombobox, Guid? cascadeFromQuestionId,
            IList<ValidationCondition> validationConditions, string linkedFilterExpression)
        {
            yield return new QuestionCloned(
                publicKey : questionId,
                groupPublicKey : parentGroupId,
                questionText : title,
                questionType : QuestionType.SingleOption,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                featured : isPreFilled,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                answers : ConvertOptionsToAnswers(options),
                sourceQuestionId : sourceQuestionId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                responsibleId : responsibleId,
                linkedToQuestionId : linkedToQuestionId,
                linkedToRosterId: linkedToRosterId,
                isFilteredCombobox : isFilteredCombobox,
                cascadeFromQuestionId : cascadeFromQuestionId,
                capital: false,
                answerOrder: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                maxAllowedAnswers: null,
                mask: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: linkedFilterExpression,
                isTimestamp: false);
        }

        private IEnumerable<IEvent> CreateNumericQuestionCloneEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, bool isPreFilled, QuestionScope scope, 
            string enablementCondition, bool hideIfDisabled, string validationExpression, string validationMessage, string instructions,
            QuestionProperties properties, Guid sourceQuestionId, 
            Guid sourceQuestionnaireId,
            int targetIndex, Guid responsibleId, bool isInteger, bool useFormatting, int? countOfDecimalPlaces,
            IList<ValidationCondition> validationConditions)
        {
            yield return new NumericQuestionCloned
            (
                publicKey : questionId,
                groupPublicKey : parentGroupId,
                questionText : title,
                stataExportCaption : variableName,
                variableLabel : variableLabel,
                featured : isPreFilled,
                capital : false,
                questionScope : scope,
                conditionExpression : enablementCondition,
                hideIfDisabled: hideIfDisabled,
                validationExpression : null,
                validationMessage : null,
                instructions : instructions,
                properties: properties,
                sourceQuestionId : sourceQuestionId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                responsibleId : responsibleId,
                isInteger : isInteger,
                countOfDecimalPlaces : countOfDecimalPlaces,
                validationConditions: validationConditions
            );
        }

        private IEnumerable<IEvent> CreateTextListQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, 
            string variableLabel, string enablementCondition, bool hideIfDisabled, string instructions, 
            Guid sourceQuestionId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId, QuestionScope scope, int? maxAnswerCount,
            IList<ValidationCondition> validationConditions)
        {
            yield return new TextListQuestionCloned
            {
                PublicKey = questionId,
                GroupId = parentGroupId,
                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = null,
                ValidationMessage = null,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                SourceQuestionnaireId = sourceQuestionnaireId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                MaxAnswerCount = maxAnswerCount,
                QuestionScope = scope,
                ValidationConditions = validationConditions
            };
        }

        private IEnumerable<IEvent> CreateQrBarcodeQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, 
            string enablementCondition, bool hideIfDisabled, string instructions, Guid sourceQuestionId, Guid sourceQuestionnaireId,
            int targetIndex, QuestionScope scope, Guid responsibleId, IList<ValidationCondition> validationConditions)
        {
            yield return new QRBarcodeQuestionCloned
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                EnablementCondition = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = null,
                ValidationMessage = null,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                SourceQuestionnaireId = sourceQuestionnaireId,
                TargetIndex = targetIndex,
                QuestionScope = scope,
                ResponsibleId = responsibleId,
                ValidationConditions = validationConditions
            };
        }

        private IEnumerable<IEvent> CreateMultimediaQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, 
            string enablementCondition, bool hideIfDisabled, string instructions, QuestionProperties properties,
            Guid sourceQuestionId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId, QuestionScope scope,
            IList<ValidationCondition> validationConditions)
        {
            yield return
                new QuestionCloned(
                    publicKey : questionId,
                    groupPublicKey : parentGroupId,
                    questionText : title,
                    questionType : QuestionType.Multimedia,
                    stataExportCaption : variableName,
                    variableLabel : variableLabel,
                    featured : false,
                    capital : false,
                    questionScope : scope,
                    conditionExpression : enablementCondition,
                    hideIfDisabled: hideIfDisabled,
                    instructions : instructions,
                    properties: properties,
                    sourceQuestionId : sourceQuestionId,
                    sourceQuestionnaireId : sourceQuestionnaireId,
                    targetIndex : targetIndex,
                    responsibleId : responsibleId,
                    validationExpression: null,
                    validationMessage: null,
                    answerOrder: null,
                    answers: null,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    isInteger: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: null,
                    cascadeFromQuestionId: null,
                    maxAnswerCount: null,
                    countOfDecimalPlaces: null,
                    validationConditions: validationConditions,
                    linkedFilterExpression: null,
                    isTimestamp: false);
            yield return
                new MultimediaQuestionUpdated
                {
                    QuestionId = questionId,
                    Title = title,
                    VariableName = variableName,
                    VariableLabel = variableLabel,
                    EnablementCondition = enablementCondition,
                    HideIfDisabled = hideIfDisabled,
                    Instructions = instructions,
                    ResponsibleId = responsibleId,
                    QuestionScope = scope
                };
        }

        public IEnumerable<IEvent> CreateCloneGroupWithoutChildrenEvents(Guid groupId, Guid responsibleId, string title, string variableName, 
            Guid? rosterSizeQuestionId, string description, string condition, bool hideIfDisabled, Guid? parentGroupId, Guid sourceGroupId, int targetIndex, bool isRoster,
            RosterSizeSourceType rosterSizeSource, FixedRosterTitle[] rosterFixedTitles, Guid? rosterTitleQuestionId, Guid? sourceQuestionnaireId)
        {
            yield return
                new GroupCloned
                {
                    PublicKey = groupId,
                    GroupText = title,
                    VariableName = variableName,
                    ParentGroupPublicKey = parentGroupId,
                    Description = description,
                    ConditionExpression = condition,
                    HideIfDisabled = hideIfDisabled,
                    SourceGroupId = sourceGroupId,
                    TargetIndex = targetIndex,
                    ResponsibleId = responsibleId,
                    SourceQuestionnaireId = sourceQuestionnaireId
                };

            if (isRoster)
            {
                yield return new GroupBecameARoster(responsibleId, groupId);
                yield return new RosterChanged(responsibleId, groupId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = rosterSizeSource,
                    FixedRosterTitles = rosterFixedTitles,
                    RosterTitleQuestionId = rosterTitleQuestionId
                };
            }
            else
            {
                yield return new GroupStoppedBeingARoster(responsibleId, groupId);
            }
        }

        private IEnumerable<IEvent> CreateStaticTextClonedEvents(Guid itemId, Guid parentGroupId, string text, string attachmentName, 
            string enablementCondition, bool hideIfDisabled, Guid sourceItemId, Guid sourceQuestionnaireId,
            int targetIndex, Guid responsibleId, IList<ValidationCondition> validationConditions)
        {
            yield return new StaticTextCloned(
                entityId : itemId,
                parentId : parentGroupId,
                sourceEntityId : sourceItemId,
                sourceQuestionnaireId : sourceQuestionnaireId,
                targetIndex : targetIndex,
                text : text,
                attachmentName : attachmentName,
                responsibleId : responsibleId,
                enablementCondition : enablementCondition,
                validationConditions : validationConditions,
                hideIfDisabled: hideIfDisabled
            );
        }

        private IEnumerable<IEvent> CreateVariableClonedEvents(Guid itemId, Guid parentGroupId, Guid sourceItemId, Guid sourceQuestionnaireId, int targetIndex, Guid responsibleId, VariableType type, string name, string expression)
        {
            yield return new VariableCloned(
                entityId: itemId,
                parentId: parentGroupId,
                sourceEntityId: sourceItemId,
                sourceQuestionnaireId: sourceQuestionnaireId,
                targetIndex: targetIndex,
                responsibleId: responsibleId,
                variableData: new VariableData(type, name, expression));
        }

        #endregion

        protected override void OnEventApplied(UncommittedEvent appliedEvent) { }
    }
}