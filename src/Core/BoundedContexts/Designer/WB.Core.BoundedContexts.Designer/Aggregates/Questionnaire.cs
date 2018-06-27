using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using Group = Main.Core.Entities.SubEntities.Group;


namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    internal class Questionnaire : IPlainAggregateRoot
    {
        private const int MaxChapterItemsCount = 400;
        private const int MaxGroupDepth = 10;

        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private List<SharedPerson> sharedPersons = new List<SharedPerson>();
        private IEnumerable<Guid> ReadOnlyUsersIds => sharedPersons.Where(p => p.ShareType == ShareType.View).Select(p => p.UserId);
        private IEnumerable<Guid> SharedUsersIds => sharedPersons.Select(p => p.UserId);

        public QuestionnaireDocument QuestionnaireDocument => this.innerDocument;
        public IEnumerable<SharedPerson> SharedPersons => this.sharedPersons;

        internal void Initialize(Guid aggregateId, QuestionnaireDocument document, IEnumerable<SharedPerson> sharedPersons)
        {
            this.innerDocument = document ?? new QuestionnaireDocument() { PublicKey = aggregateId };
            this.sharedPersons = sharedPersons?.ToList() ?? new List<SharedPerson>();

            // Migrate single validation conditions to multiple
            foreach (var question in this.innerDocument.Children.TreeToEnumerable(x => x.Children).OfType<IQuestion>())
            {
                question.ValidationConditions = question.ValidationConditions;
                question.ValidationExpression = null;
                question.ValidationMessage = null;
            }
        }
        
        public Guid Id => this.innerDocument.PublicKey;
        
        private void AddGroup(IGroup newGroup, Guid? parentId)
        {
            if (parentId.HasValue)
            {
                var parentGroup = this.innerDocument.Find<Group>(parentId.Value);
                if (parentGroup != null)
                {
                    newGroup.SetParent(parentGroup);
                }
                else
                {
                    string errorMessage = string.Format(ExceptionMessages.FailedToAddGroup,
                        newGroup.PublicKey,
                        parentId,
                        this.innerDocument.PublicKey);

                    logger.Error(errorMessage);
                }
            }

            this.innerDocument.Add(newGroup, parentId);
        }

        private void RemoveRosterFlagFromGroup(Guid groupId)
        {
            this.innerDocument.UpdateGroup(groupId, group =>
            {
                group.IsRoster = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.FixedRosterTitles = new FixedRosterTitle[0];
            });
        }
        
        #endregion

        #region Dependencies

        private readonly ILogger logger;
        private readonly IClock clock;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationService;
        private readonly IQuestionnireHistoryVersionsService questionnireHistoryVersionsService;
        private int affectedByReplaceEntries;

        #endregion

        public Questionnaire(
            ILogger logger, 
            IClock clock, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService,
            ITranslationsService translationService,
            IQuestionnireHistoryVersionsService questionnireHistoryVersionsService)
        {
            this.logger = logger;
            this.clock = clock;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationService = translationService;
            this.questionnireHistoryVersionsService = questionnireHistoryVersionsService;
        }

        #region Questionnaire command handlers

        public void CreateQuestionnaire(CreateQuestionnaire createQuestionnaire)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(createQuestionnaire.Title);

            this.innerDocument = new QuestionnaireDocument
            {
                IsPublic = createQuestionnaire.IsPublic,
                Title = System.Web.HttpUtility.HtmlDecode(createQuestionnaire.Title),
                PublicKey = createQuestionnaire.QuestionnaireId,
                CreationDate = this.clock.UtcNow(),
                LastEntryDate = this.clock.UtcNow(),
                CreatedBy = createQuestionnaire.ResponsibleId,
            };

            this.AddGroup(CreateGroup(Guid.NewGuid(), QuestionnaireEditor.NewSection, null, null, null,false), null);
        }

        public void CloneQuestionnaire(string title, bool isPublic, Guid createdBy, Guid publicKey, IQuestionnaireDocument source)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(title);

            if (!(source is QuestionnaireDocument document))
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, ExceptionMessages.OnlyQuestionnaireDocumentsAreSupported);

            var clonedDocument = document.Clone();
            clonedDocument.PublicKey = this.Id;
            clonedDocument.CreatedBy = createdBy;
            clonedDocument.CreationDate = this.clock.UtcNow();
            clonedDocument.Title = title;
            clonedDocument.IsPublic = isPublic;

            foreach (var lookupTable in clonedDocument.LookupTables)
            {
                var lookupTableName = lookupTable.Value.TableName;

                if (!this.lookupTableService.IsLookupTableEmpty(document.PublicKey, lookupTable.Key, lookupTableName))
                {
                    lookupTableService.CloneLookupTable(document.PublicKey, lookupTable.Key, lookupTableName, this.Id, lookupTable.Key);
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
                if (translation.Id == clonedDocument.DefaultTranslation)
                {
                    clonedDocument.DefaultTranslation = newTranslationId;
                }

                translation.Id = newTranslationId;
            }

            this.innerDocument = clonedDocument;
        }

        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, ExceptionMessages.OnlyQuestionnaireDocumentsAreSupported);
            if (document.IsDeleted)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, ExceptionMessages.ImportOfDeletedQuestionnaire);

            document.CreatedBy = createdBy;
            
            this.innerDocument = document;
        }

        public void UpdateQuestionnaire(UpdateQuestionnaire command)
#warning CRUD
        {
            if (!command.IsResponsibleAdmin) 
                this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(command.Title);

            this.innerDocument.Title = System.Web.HttpUtility.HtmlDecode(command.Title);
            this.innerDocument.VariableName = System.Web.HttpUtility.HtmlDecode(command.Variable);
            this.innerDocument.IsPublic = command.IsPublic;
        }

        public void DeleteQuestionnaire()
        {
            this.innerDocument.IsDeleted = true;
        }

        public IEnumerable<QuestionnaireEntityReference> FindAllTexts(string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            Regex searchRegex = BuildSearchRegex(searchFor, matchCase, matchWholeWord, useRegex);
            IEnumerable<IComposite> allEntries = this.innerDocument.Children.TreeToEnumerableDepthFirst(x => x.Children);
            foreach (var questionnaireItem in allEntries)
            {
                var title = questionnaireItem.GetTitle();
                var variable = questionnaireItem.GetVariable();

                if (MatchesSearchTerm(variable, searchRegex) || questionnaireItem.PublicKey.ToString().Equals(searchFor, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.VariableName);
                }
                if (MatchesSearchTerm(title, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.Title);
                }
                
                var question = questionnaireItem as IQuestion;
                if (question != null)
                {
                    if (question.Answers != null && !(question.IsFilteredCombobox.GetValueOrDefault() || question.CascadeFromQuestionId.HasValue))
                    {
                        for (int i = 0; i < question.Answers.Count; i++)
                        {
                            if (MatchesSearchTerm(question.Answers[i].AnswerText, searchRegex))
                            {
                                yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.Option, i);
                            }
                        }
                    }

                    if (MatchesSearchTerm(question.Properties.OptionsFilterExpression, searchRegex) || MatchesSearchTerm(question.LinkedFilterExpression, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.OptionsFilter);
                    }
                }

                var group = questionnaireItem as IGroup;
                if (group != null)
                {
                    if (group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    {
                        for (int i = 0; i < group.FixedRosterTitles.Length; i++)
                        {
                            if (MatchesSearchTerm(group.FixedRosterTitles[i].Title, searchRegex))
                            {
                                yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.FixedRosterItem, i);
                            }
                        }
                    }
                }

                var conditional = questionnaireItem as IConditional;
                if (MatchesSearchTerm(conditional?.ConditionExpression, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.EnablingCondition);
                }

                var validatable = questionnaireItem as IValidatable;
                if (validatable != null)
                {
                    for (int i = 0; i < validatable.ValidationConditions.Count; i++)
                    {
                        if(MatchesSearchTerm(validatable.ValidationConditions[i].Expression, searchRegex))
                        {
                            yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.ValidationExpression, i);
                        }
                        if (MatchesSearchTerm(validatable.ValidationConditions[i].Message, searchRegex))
                        {
                            yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.ValidationMessage, i);
                        }
                    }
                }

                var questionnaireVariable = questionnaireItem as IVariable;
                if (questionnaireVariable != null)
                {
                    if (MatchesSearchTerm(questionnaireVariable.Label, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireVariable, QuestionnaireVerificationReferenceProperty.VariableLabel);
                    }

                    if (MatchesSearchTerm(questionnaireVariable.Expression, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireVariable, QuestionnaireVerificationReferenceProperty.VariableContent);
                    }
                }

                var staticText = questionnaireItem as IStaticText;
                if (staticText != null && MatchesSearchTerm(staticText.AttachmentName, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(staticText, QuestionnaireVerificationReferenceProperty.AttachmentName);
                }
            }

            foreach (var macro in this.innerDocument.Macros.OrderBy(x => x.Value.Name))
            {
                if (MatchesSearchTerm(macro.Value.Content, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateForMacro(macro.Key);
                }
            }
        }

        private static Regex BuildSearchRegex(string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!matchCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            string encodedSearchPattern = useRegex ? searchFor : Regex.Escape(searchFor);
            string pattern = matchWholeWord ? $@"\b{encodedSearchPattern}\b" : encodedSearchPattern;

            Regex searchRegex = new Regex(pattern, options);
            return searchRegex;
        }

        private static bool MatchesSearchTerm(string target, Regex searchRegex)
        {
            if (target.IsNullOrEmpty()) return false;

            return searchRegex.IsMatch(target);
        }

        private static string ReplaceUsingSearchTerm(string target, Regex searchFor, string replaceWith)
        {
            return searchFor.Replace(target, replaceWith);
        }

        public void ReplaceTexts(ReplaceTextsCommand command)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var allEntries = this.innerDocument.Children.TreeToEnumerable(x => x.Children);
            this.affectedByReplaceEntries = 0;
            var searchRegex = BuildSearchRegex(command.SearchFor, command.MatchCase, command.MatchWholeWord, command.UseRegex);
            foreach (var questionnaireItem in allEntries)
            {
                bool replacedAny = false;
                var title = questionnaireItem.GetTitle();
                if (MatchesSearchTerm(title, searchRegex))
                {
                    replacedAny = true;
                    questionnaireItem.SetTitle(ReplaceUsingSearchTerm(title, searchRegex, command.ReplaceWith));
                }

                var variableName = questionnaireItem.GetVariable();
                if (MatchesSearchTerm(variableName, searchRegex))
                {
                    replacedAny = true;
                    questionnaireItem.SetVariable(ReplaceUsingSearchTerm(variableName, searchRegex, command.ReplaceWith));
                }

                var conditional = questionnaireItem as IConditional;
                if (MatchesSearchTerm(conditional?.ConditionExpression, searchRegex))
                {
                    replacedAny = true;
                    string newCondition = ReplaceUsingSearchTerm(conditional.ConditionExpression, searchRegex, command.ReplaceWith);
                    conditional.ConditionExpression = newCondition;
                }

                var validatable = questionnaireItem as IValidatable;
                if (validatable != null)
                {
                    foreach (var validationCondition in validatable.ValidationConditions)
                    {
                        if (MatchesSearchTerm(validationCondition.Expression, searchRegex))
                        {
                            replacedAny = true;
                            string newValidationCondition = ReplaceUsingSearchTerm(validationCondition.Expression, searchRegex, command.ReplaceWith);
                            validationCondition.Expression = newValidationCondition;
                        }
                        if (MatchesSearchTerm(validationCondition.Message, searchRegex))
                        {
                            replacedAny = true;
                            string newMessage = ReplaceUsingSearchTerm(validationCondition.Message, searchRegex, command.ReplaceWith);
                            validationCondition.Message = newMessage;
                        }
                    }
                }

                var questionnaireVariable = questionnaireItem as IVariable;
                if (questionnaireVariable != null)
                {
                    if (MatchesSearchTerm(questionnaireVariable.Label, searchRegex))
                    {
                        replacedAny = true;
                        questionnaireVariable.Label = ReplaceUsingSearchTerm(questionnaireVariable.Label, searchRegex, command.ReplaceWith);
                    }

                    if (MatchesSearchTerm(questionnaireVariable.Expression, searchRegex))
                    {
                        replacedAny = true;
                        questionnaireVariable.Expression = ReplaceUsingSearchTerm(questionnaireVariable.Expression, searchRegex, command.ReplaceWith);
                    }
                }

                var question = questionnaireItem as IQuestion;
                if (question != null)
                {
                    if (question.Answers != null && !(question.IsFilteredCombobox.GetValueOrDefault() || question.CascadeFromQuestionId.HasValue))
                    {
                        foreach (var questionAnswer in question.Answers)
                        {
                            if (MatchesSearchTerm(questionAnswer.AnswerText, searchRegex))
                            {
                                replacedAny = true;
                                questionAnswer.AnswerText = ReplaceUsingSearchTerm(questionAnswer.AnswerText, searchRegex, command.ReplaceWith);
                            }
                        }
                    }

                    if (MatchesSearchTerm(question.Properties.OptionsFilterExpression, searchRegex))
                    {
                        replacedAny = true;
                        question.Properties.OptionsFilterExpression = ReplaceUsingSearchTerm(question.Properties.OptionsFilterExpression, searchRegex, command.ReplaceWith);
                    }

                    if (MatchesSearchTerm(question.LinkedFilterExpression, searchRegex))
                    {
                        replacedAny = true;
                        question.LinkedFilterExpression = ReplaceUsingSearchTerm(question.LinkedFilterExpression, searchRegex, command.ReplaceWith);
                    }
                }

                var group = questionnaireItem as IGroup;
                if (group != null)
                {
                    if (group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    {
                        foreach (var fixedRosterTitle in group.FixedRosterTitles)
                        {
                            if (MatchesSearchTerm(fixedRosterTitle.Title, searchRegex))
                            {
                                replacedAny = true;
                                fixedRosterTitle.Title = ReplaceUsingSearchTerm(fixedRosterTitle.Title, searchRegex, command.ReplaceWith);
                            }
                        }
                    }
                }

                var staticText = questionnaireItem as IStaticText;
                if (staticText != null)
                {
                    if (MatchesSearchTerm(staticText.AttachmentName, searchRegex))
                    {
                        replacedAny = true;
                        staticText.AttachmentName = ReplaceUsingSearchTerm(staticText.AttachmentName, searchRegex, command.ReplaceWith);
                    }
                }

                if (replacedAny)
                {
                    this.affectedByReplaceEntries++;
                }
            }

            foreach (var macro in this.innerDocument.Macros.Values)
            {
                if (MatchesSearchTerm(macro.Content, searchRegex))
                {
                    this.affectedByReplaceEntries++;
                    macro.Content = ReplaceUsingSearchTerm(macro.Content, searchRegex, command.ReplaceWith);
                }
            }
        }

        public int GetLastReplacedEntriesCount()
        {
            return this.affectedByReplaceEntries;
        }

        #endregion

        #region Macro command handlers

        public void AddMacro(AddMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroAlreadyExist(command.MacroId);

            this.innerDocument.Macros[command.MacroId] = new Macro();
        }

        public void UpdateMacro(UpdateMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroIsAbsent(command.MacroId);
            
            if (!innerDocument.Macros.ContainsKey(command.MacroId))
                return;

            var macro = this.innerDocument.Macros[command.MacroId];
            macro.Name = command.Name;
            macro.Content = command.Content;
            macro.Description = command.Description;
        }

        public void DeleteMacro(DeleteMacro command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfMacroIsAbsent(command.MacroId);

            innerDocument.Macros.Remove(command.MacroId);
        }

        #endregion

        #region Attachment command handlers
                
        public void AddOrUpdateAttachment(AddOrUpdateAttachment command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            innerDocument.Attachments.RemoveAll(x => x.AttachmentId == command.AttachmentId);

            if(command.OldAttachmentId.HasValue)
                innerDocument.Attachments.RemoveAll(x => x.AttachmentId == command.OldAttachmentId.Value);

            innerDocument.Attachments.Add(new Attachment
            {
                AttachmentId = command.AttachmentId,
                Name = command.AttachmentName,
                ContentId = command.AttachmentContentId
            });
        }

        public void DeleteAttachment(DeleteAttachment command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.innerDocument.Attachments.RemoveAll(x => x.AttachmentId == command.AttachmentId);
        }

        #endregion

        #region Translation command handlers
                
        public void AddOrUpdateTranslation(AddOrUpdateTranslation command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var translation = new Translation
            {
                Id = command.TranslationId,
                Name = command.Name,
            };
            innerDocument.Translations.RemoveAll(x => x.Id == command.TranslationId);

            if (command.OldTranslationId.HasValue)
            {
                innerDocument.Translations.RemoveAll(x => x.Id == command.OldTranslationId.Value);

                if (innerDocument.DefaultTranslation.HasValue &&
                    innerDocument.DefaultTranslation == command.OldTranslationId)
                    innerDocument.DefaultTranslation = command.TranslationId;
            }

            innerDocument.Translations.Add(translation);
        }

        public void DeleteTranslation(DeleteTranslation command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.innerDocument.Translations.RemoveAll(x => x.Id == command.TranslationId);

            if (innerDocument.DefaultTranslation.HasValue && innerDocument.DefaultTranslation == command.TranslationId)
                innerDocument.DefaultTranslation = null;
        }

        public void SetDefaultTranslation(SetDefaultTranslation command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.innerDocument.DefaultTranslation = command.TranslationId;
        }

        #endregion

        #region Lookup table command handlers

        public void AddLookupTable(AddLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            if (this.innerDocument.LookupTables.ContainsKey(command.LookupTableId))
            {
                throw new QuestionnaireException(DomainExceptionType.LookupTableAlreadyExist, ExceptionMessages.LookupTableAlreadyExist);
            }

            innerDocument.LookupTables[command.LookupTableId] = new LookupTable()
            {
                TableName = command.LookupTableName,
                FileName = command.LookupTableFileName
            };
        }

        public void UpdateLookupTable(UpdateLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            if (command.OldLookupTableId.HasValue)
            {
                if (innerDocument.LookupTables.ContainsKey(command.OldLookupTableId.Value))
                    innerDocument.LookupTables.Remove(command.OldLookupTableId.Value);
            }

            innerDocument.LookupTables[command.LookupTableId] = new LookupTable
            {
                TableName = command.LookupTableName,
                FileName = command.LookupTableFileName
            };
        }

        public void DeleteLookupTable(DeleteLookupTable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            if (!this.innerDocument.LookupTables.ContainsKey(command.LookupTableId))
            {
                throw new QuestionnaireException(DomainExceptionType.LookupTableIsAbsent, ExceptionMessages.LookupTableIsAbsent);
            }
            innerDocument.LookupTables.Remove(command.LookupTableId);
        }

        #endregion

        #region Group command handlers

        public void AddGroupAndMoveIfNeeded(Guid groupId, Guid responsibleId, string title, 
            string variableName, Guid? rosterSizeQuestionId, string description, string condition, 
            bool hideIfDisabled, Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource,
            FixedRosterTitleItem[] rosterFixedTitles, Guid? rosterTitleQuestionId, int? index = null)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);

            if (parentGroupId.HasValue)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroupId.Value);
                this.ThrowIfTargetGroupHasReachedAllowedDepthLimit(parentGroupId.Value);
            }

            this.AddGroup(CreateGroup(groupId, title, variableName, description, condition, hideIfDisabled),
                parentGroupId);

            if (isRoster)
            {
                this.innerDocument.UpdateGroup(groupId, group =>
                {
                    group.RosterSizeQuestionId = rosterSizeQuestionId;
                    group.RosterSizeSource = rosterSizeSource;
                    group.FixedRosterTitles = fixedTitles;
                    group.RosterTitleQuestionId = rosterTitleQuestionId;
                    group.IsRoster = true;
                });
            }
            else
            {
                this.RemoveRosterFlagFromGroup( groupId);
            }

            if (index.HasValue)
            {
                this.innerDocument.MoveItem(groupId, parentGroupId, index.Value);
            }
        }

        private static Guid? GetIdOrReturnSameId(Dictionary<Guid, Guid> replacementIdDictionary, Guid? id)
        {
            if (!id.HasValue)
                return null;

            return replacementIdDictionary.ContainsKey(id.Value) ? replacementIdDictionary[id.Value] : id;
        }

        public void UpdateGroup(Guid groupId, Guid responsibleId,
            string title,string variableName, Guid? rosterSizeQuestionId, string description, string condition, bool hideIfDisabled, 
            bool isRoster, RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);

            var group = this.GetGroupById(groupId);

            var wasGroupAndBecomeARoster = !@group.IsRoster && isRoster;
            var wasRosterAndBecomeAGroup = @group.IsRoster && !isRoster;

            this.innerDocument.UpdateGroup(groupId,
                title,
                variableName,
                description,
                condition,
                hideIfDisabled);

            if (isRoster)
            {
                this.innerDocument.UpdateGroup(groupId, groupToUpdate =>
                {
                    groupToUpdate.RosterSizeQuestionId = rosterSizeQuestionId;
                    groupToUpdate.RosterSizeSource = rosterSizeSource;
                    groupToUpdate.FixedRosterTitles = fixedTitles;
                    groupToUpdate.RosterTitleQuestionId = rosterTitleQuestionId;
                    groupToUpdate.IsRoster = true;
                });
            }
            else
            {
                this.RemoveRosterFlagFromGroup(groupId);
            }
        }

        public void DeleteGroup(Guid groupId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            if (this.QuestionnaireDocument.Children.Count == 1 &&
                this.QuestionnaireDocument.Children[0].PublicKey == groupId)
            {
                throw new QuestionnaireException(DomainExceptionType.Undefined, ExceptionMessages.CantRemoveLastSectionInQuestionnaire);
            }

            this.innerDocument.RemoveEntity(groupId);
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
                        throw new QuestionnaireException(string.Format(ExceptionMessages.SectionCantHaveMoreThan_Items, MaxChapterItemsCount));
                    }
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(this.GetGroupById(targetGroupId.Value)).Count();
                var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(sourceGroup);

                if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                {
                    throw new QuestionnaireException(string.Format(ExceptionMessages.SubSectionDepthLimit, MaxGroupDepth));
                }
                
            }
            
            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = targetGroupId.HasValue ? this.GetGroupById(targetGroupId.Value) : this.innerDocument;

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceGroup.GetParent() as IGroup);

            this.innerDocument.MoveItem(groupId, targetGroupId, targetIndex);
        }

        #endregion

        public void AddDefaultTypeQuestionAdnMoveIfNeeded(AddDefaultTypeQuestion command)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(command.QuestionId);
            var parentGroup = this.GetGroupById(command.ParentGroupId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentGroupId);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            IQuestion question = CreateQuestion(command.QuestionId,
                questionText: command.Title,
                questionType: QuestionType.Text,
                stataExportCaption: null,
                variableLabel: null,
                featured: false,
                questionScope: QuestionScope.Interviewer,
                conditionExpression: null,
                hideIfDisabled: false,
                validationConditions: new List<ValidationCondition>(),
                instructions: null,
                questionProperties: new QuestionProperties(false, false),
                linkedToQuestionId: null,
                areAnswersOrdered: null,
                maxAllowedAnswers: null,
                mask: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: null,
                answerOrder: null,
                answers: null,
                isInteger: null,
                yesNoView: null,
                linkedToRosterId: null,
                countOfDecimalPlaces: null,
                maxAnswerCount: null,
                linkedFilterExpression: null,
                isTimestamp: false);

            this.innerDocument.Add(question, command.ParentGroupId);
            
            if (command.Index.HasValue)
            {
                this.innerDocument.MoveItem(command.QuestionId, command.ParentGroupId, command.Index.Value);
            }
        }

        public void DeleteQuestion(Guid questionId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            this.innerDocument.RemoveEntity(questionId);
           
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

            this.innerDocument.MoveItem(questionId, targetGroupId, targetIndex);
        }

        public void UpdateTextQuestion(UpdateTextQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                        command.QuestionId,
                        QuestionType.Text,
                        command.Scope,
                        title,
                        variableName,
                        command.VariableLabel,
                        command.EnablementCondition,
                        command.HideIfDisabled,
                        null,
                        command.IsPreFilled,
                        command.Instructions,
                        command.Properties,
                        command.Mask,
                        null, null, null, null,
                        null, null, null, null,
                        null, null, null,
                        command.ValidationConditions,
                        null,
                        false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateGpsCoordinatesQuestion(UpdateGpsCoordinatesQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                        question.PublicKey,
                        QuestionType.GpsCoordinates,
                        command.Scope,
                        title,
                        variableName,
                        command.VariableLabel,
                        command.EnablementCondition,
                        command.HideIfDisabled,
                        Order.AZ,
                        command.IsPreFilled,
                        command.Instructions,
                        command.Properties,
                        null, null, null, null,
                        null, null, null, null,
                        null, null, null, null,
                        command.ValidationConditions,
                        null,
                        false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateDateTimeQuestion(UpdateDateTimeQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }


            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                question.PublicKey,
                QuestionType.DateTime,
                command.Scope,
                command.Title,
                command.VariableName,
                command.VariableLabel,
                command.EnablementCondition,
                command.HideIfDisabled,
                null,
                command.IsPreFilled,
                command.Instructions,
                command.Properties,
                null, null, null, null, null,null, null, null, null, null, null,null,
                command.ValidationConditions,
                null,
                command.IsTimestamp);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateMultiOptionQuestion(Guid questionId, string title, string variableName, string variableLabel, QuestionScope scope, string enablementCondition,
            bool hideIfDisabled, string instructions, Guid responsibleId, Option[] options, Guid? linkedToEntityId, bool areAnswersOrdered, int? maxAllowedAnswers, 
            bool yesNoView, IList<ValidationCondition> validationConditions, string linkedFilterExpression, QuestionProperties properties)
        {
            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(linkedToEntityId, out linkedQuestionId, out linkedRosterId);


            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            IQuestion newQuestion = CreateQuestion(questionId,
                QuestionType.MultyOption,
                scope,
                title,
                variableName,
                variableLabel,
                enablementCondition,
                hideIfDisabled,
                null, false, 
                instructions,
                properties,
                null,
                ConvertOptionsToAnswers(options),
                linkedQuestionId,
                linkedRosterId,
                null,
                null,
                areAnswersOrdered,
                maxAllowedAnswers,
                null, null, null,
                yesNoView,
                validationConditions,
                linkedFilterExpression,
                false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
            
        }

        #region Question: SingleOption command handlers

        public void UpdateSingleOptionQuestion(Guid questionId, string title, string variableName, string variableLabel, bool isPreFilled, QuestionScope scope,
            string enablementCondition, bool hideIfDisabled, string instructions, Guid responsibleId, Option[] options, Guid? linkedToEntityId, bool isFilteredCombobox, 
            Guid? cascadeFromQuestionId, IList<ValidationCondition> validationConditions, string linkedFilterExpression, QuestionProperties properties)
        {
            Answer[] answers;

            if (options == null && (isFilteredCombobox || cascadeFromQuestionId.HasValue))
            {
                IQuestion originalQuestion = this.GetQuestion(questionId);
                answers = originalQuestion.Answers.ToArray();                
            }
            else
            {
                answers = ConvertOptionsToAnswers(options);                
            }

            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            if (isFilteredCombobox || cascadeFromQuestionId.HasValue)
            {
                var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
                answers = categoricalOneAnswerQuestion?.Answers.ToArray();
            }

            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(linkedToEntityId, out linkedQuestionId, out linkedRosterId);

            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            IQuestion newQuestion = CreateQuestion(questionId,
                QuestionType.SingleOption,
                scope,
                title,
                variableName,
                variableLabel,
                enablementCondition,
                hideIfDisabled,
                null,
                isPreFilled,
                instructions,
                properties,
                null,
                answers,
                linkedQuestionId,
                linkedRosterId,
                null, null, null, null, null,
                isFilteredCombobox,
                cascadeFromQuestionId,
                null,
                validationConditions,
                linkedFilterExpression,
                false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
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

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            IQuestion newQuestion = CreateQuestion(questionId,
                categoricalOneAnswerQuestion.QuestionType,
                categoricalOneAnswerQuestion.QuestionScope,
                categoricalOneAnswerQuestion.QuestionText,
                categoricalOneAnswerQuestion.StataExportCaption,
                categoricalOneAnswerQuestion.VariableLabel,
                categoricalOneAnswerQuestion.ConditionExpression,
                categoricalOneAnswerQuestion.HideIfDisabled,
                null,
                categoricalOneAnswerQuestion.Featured,
                categoricalOneAnswerQuestion.Instructions,
                categoricalOneAnswerQuestion.Properties,
                null,
                ConvertOptionsToAnswers(options),
                categoricalOneAnswerQuestion.LinkedToQuestionId,
                categoricalOneAnswerQuestion.LinkedToRosterId,
                null, null, null, null, null,
                categoricalOneAnswerQuestion.IsFilteredCombobox,
                categoricalOneAnswerQuestion.CascadeFromQuestionId,
                null,
                categoricalOneAnswerQuestion.ValidationConditions,
                null,
                false);

            this.innerDocument.ReplaceEntity(categoricalOneAnswerQuestion, newQuestion);
        }

        public void UpdateCascadingComboboxOptions(Guid questionId, Guid responsibleId, Option[] options)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfCascadingComboboxIsInvalid(questionId, options);
            ThrowDomainExceptionIfOptionsHasEmptyParentValue(options);
            ThrowDomainExceptionIfOptionsHasNotDecimalParentValue(options);
            ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(options);

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);

            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            IQuestion newQuestion = CreateQuestion(
                questionId,
                categoricalOneAnswerQuestion.QuestionType,
                categoricalOneAnswerQuestion.QuestionScope,
                categoricalOneAnswerQuestion.QuestionText,
                categoricalOneAnswerQuestion.StataExportCaption,
                categoricalOneAnswerQuestion.VariableLabel,
                categoricalOneAnswerQuestion.ConditionExpression,
                categoricalOneAnswerQuestion.HideIfDisabled,
                null,
                categoricalOneAnswerQuestion.Featured,
                categoricalOneAnswerQuestion.Instructions,
                categoricalOneAnswerQuestion.Properties,
                null,
                ConvertOptionsToAnswers(options),
                categoricalOneAnswerQuestion.LinkedToQuestionId,
                categoricalOneAnswerQuestion.LinkedToRosterId,
                null, null, null, null, null,
                categoricalOneAnswerQuestion.IsFilteredCombobox,
                categoricalOneAnswerQuestion.CascadeFromQuestionId,
                null,
                categoricalOneAnswerQuestion.ValidationConditions,
                null,
                false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }
        #endregion


        public void UpdateNumericQuestion(UpdateNumericQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                question.PublicKey,
                QuestionType.Numeric,
                command.Scope,
                title,
                variableName,
                command.VariableLabel,
                command.EnablementCondition,
                command.HideIfDisabled,
                Order.AZ,
                command.IsPreFilled,
                command.Instructions,
                command.Properties,
                null,
                ConvertOptionsToAnswers(command.Options),
                null,
                null,
                command.IsInteger,
                command.CountOfDecimalPlaces,
                null,
                null,
                null,
                null,
                null,
                null,
                command.ValidationConditions,
                null,
                false);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateTextListQuestion(UpdateTextListQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                    command.QuestionId,
                    QuestionType.TextList,
                    command.Scope,
                    command.Title,
                    command.VariableName,
                    command.VariableLabel,
                    command.EnablementCondition,
                    command.HideIfDisabled,
                    Order.AZ,
                    false,
                    command.Instructions,
                    command.Properties,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    command.MaxAnswerCount,
                    null,
                    null,
                    null,
                    command.ValidationConditions,
                    null,
                    false);

            if (question != null)
            {
                this.innerDocument.ReplaceEntity(question, newQuestion);
            }
        }

        public void UpdateAreaQuestion(UpdateAreaQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                    command.QuestionId,
                    QuestionType.Area,
                    command.Scope,
                    command.Title,
                    command.VariableName,
                    command.VariableLabel,
                    command.EnablementCondition,
                    command.HideIfDisabled,
                    Order.AZ,
                    false,
                    command.Instructions,
                    command.Properties,
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
                    command.ValidationConditions,
                    null,
                    false);

            if (question != null)
            {
                this.innerDocument.ReplaceEntity(question, newQuestion);
            }
        }


        public void UpdateAudioQuestion(UpdateAudioQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            AudioQuestion newQuestion = (AudioQuestion)CreateQuestion(
                command.QuestionId,
                QuestionType.Audio,
                command.Scope,
                command.Title,
                command.VariableName,
                command.VariableLabel,
                command.EnablementCondition,
                command.HideIfDisabled,
                Order.AZ,
                false,
                command.Instructions,
                command.Properties,
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
                new List<ValidationCondition>(),
                null,
                false);

            if (question != null)
            {
                this.innerDocument.ReplaceEntity(question, newQuestion);
            }
        }

        public void UpdateMultimediaQuestion(
            UpdateMultimediaQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);

            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            MultimediaQuestion newQuestion = (MultimediaQuestion) CreateQuestion(
                command.QuestionId,
                QuestionType.Multimedia,
                command.Scope,
                title,
                variableName,
                command.VariableLabel,
                command.EnablementCondition,
                command.HideIfDisabled,
                Order.AZ,
                false,
                command.Instructions,
                command.Properties,
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
                new List<ValidationCondition>(),
                null,
                false);
            newQuestion.IsSignature = command.IsSignature;
            if (question != null)
            {
                this.innerDocument.ReplaceEntity(question, newQuestion);
            }
        }

        public void UpdateQRBarcodeQuestion(UpdateQRBarcodeQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                    command.QuestionId,
                    QuestionType.QRBarcode,
                    command.Scope,
                    command.Title,
                    command.VariableName,
                    command.VariableLabel,
                    command.EnablementCondition,
                    command.HideIfDisabled,
                    Order.AZ,
                    false,
                    command.Instructions,
                    command.Properties,
                    null, null, null, null, null, null, null, null, null,
                    null,null,null,
                    command.ValidationConditions,
                    null,
                    false);

            if (question != null)
            {
                this.innerDocument.ReplaceEntity(question, newQuestion);
            }
        }

        #region Static text command handlers
        public void AddStaticTextAndMoveIfNeeded(AddStaticText command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(command.EntityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);
            
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentId);

            var staticText = new StaticText(publicKey: command.EntityId,
                text: System.Web.HttpUtility.HtmlDecode(command.Text),
                enablementCondition: null,
                hideIfDisabled: false,
                validationConditions: null,
                attachmentName: null);

            this.innerDocument.Add(staticText, command.ParentId);

            if (command.Index.HasValue)
            {
                this.innerDocument.MoveItem(command.EntityId, command.ParentId, command.Index.Value);
            }
        }

        public void UpdateStaticText(UpdateStaticText command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, command.EntityId);

            var oldStaticText = this.innerDocument.Find<IStaticText>(command.EntityId);
            var newStaticText = new StaticText(publicKey: command.EntityId,
                text: System.Web.HttpUtility.HtmlDecode(command.Text),
                enablementCondition: command.EnablementCondition,
                hideIfDisabled: command.HideIfDisabled,
                validationConditions: command.ValidationConditions,
                attachmentName: command.AttachmentName);

            this.innerDocument.ReplaceEntity(oldStaticText, newStaticText);
        }

        public void DeleteStaticText(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);

            this.innerDocument.RemoveEntity(entityId);
        }

        public void MoveStaticText(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(targetEntityId);

            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = this.GetGroupById(targetEntityId);
            var sourceStaticText = this.innerDocument.Find<IStaticText>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceStaticText.GetParent() as IGroup);

            this.innerDocument.MoveItem(entityId, targetEntityId, targetIndex);
        }
        #endregion


        #region Variable command handlers
        public void AddVariableAndMoveIfNeeded(AddVariable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(command.EntityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);

            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentId);

            var variable = new Variable(command.EntityId, command.VariableData);

            this.innerDocument.Add(variable, command.ParentId);
            
            if (command.Index.HasValue)
            {
                this.innerDocument.MoveItem(command.EntityId, command.ParentId, command.Index.Value);
            }
        }

        public void UpdateVariable(UpdateVariable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, command.EntityId);

            var oldVariable = this.innerDocument.Find<IVariable>(command.EntityId);

            var newVariable = new Variable(command.EntityId, command.VariableData);
            this.innerDocument.ReplaceEntity(oldVariable, newVariable);
        }

        public void DeleteVariable(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);

            this.innerDocument.RemoveEntity(entityId);
        }

        public void MoveVariable(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(targetEntityId);

            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = this.GetGroupById(targetEntityId);
            var sourceVariable = this.innerDocument.Find<IVariable>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceVariable.GetParent() as IGroup);

            this.innerDocument.MoveItem(entityId, targetEntityId, targetIndex);
        }
        #endregion

        #region Shared Person command handlers

        public void AddSharedPerson(Guid personId, string emailOrLogin, ShareType shareType, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (this.innerDocument.CreatedBy == personId)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.OwnerCannotBeInShareList,
                    string.Format(ExceptionMessages.UserIsOwner, emailOrLogin));
            }

            if (this.SharedUsersIds.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserExistInShareList,
                    string.Format(ExceptionMessages.UserIsInTheList, emailOrLogin));
            }

            this.sharedPersons.Add(new SharedPerson
            {
                UserId = personId,
                ShareType = shareType,
                Email = emailOrLogin,
            });
        }

        public void RemoveSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (!this.SharedUsersIds.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserDoesNotExistInShareList, ExceptionMessages.CantRemoveUserFromTheList);
            }

            this.sharedPersons.RemoveAll(sp => sp.UserId == personId);

        }

        #endregion

        #region CopyPaste command handler
        
        public void PasteAfter(PasteAfter pasteAfter)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteAfter.ResponsibleId);
            ThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, pasteAfter.ItemToPasteAfterId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteAfter.EntityId);
            ThrowDomainExceptionIfEntityDoesNotExists(pasteAfter.SourceDocument, pasteAfter.SourceItemId);
            
            var itemToInsertAfter = this.innerDocument.Find<IComposite>(pasteAfter.ItemToPasteAfterId);
            var targetToPasteIn = itemToInsertAfter.GetParent();
            var entityToInsert = pasteAfter.SourceDocument.Find<IComposite>(pasteAfter.SourceItemId);
            var targetIndex = targetToPasteIn.Children.IndexOf(itemToInsertAfter) + 1;

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);

            this.CopyElementInTree(pasteAfter.EntityId, entityToInsert, targetToPasteIn, targetIndex);
        }

        public void PasteInto(PasteInto pasteInto)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteInto.ResponsibleId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteInto.EntityId);
            ThrowDomainExceptionIfEntityDoesNotExists(pasteInto.SourceDocument, pasteInto.SourceItemId);
            
            this.ThrowDomainExceptionIfGroupDoesNotExist(pasteInto.ParentId);

            var entityToInsert = pasteInto.SourceDocument.Find<IComposite>(pasteInto.SourceItemId);
            var targetToPasteIn = this.GetGroupById(pasteInto.ParentId);
            var targetIndex = targetToPasteIn.Children.Count();

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);

            this.CopyElementInTree(pasteInto.EntityId, entityToInsert, targetToPasteIn, targetIndex);
        }

        private void CheckDepthInvariants(IComposite targetToPasteIn, IComposite entityToInsert)
        {
            if (targetToPasteIn.PublicKey != this.Id)
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
                    throw new QuestionnaireException(string.Format(ExceptionMessages.SectionCantHaveMoreThan_Items, MaxChapterItemsCount));
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(this.GetGroupById(targetToPasteIn.PublicKey)).Count();

                var entityToInsertAsGroup = entityToInsert as IGroup;
                if (entityToInsertAsGroup != null)
                {
                    var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(entityToInsertAsGroup);

                    if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                    {
                        throw new QuestionnaireException(string.Format(ExceptionMessages.SubSectionDepthLimit, MaxGroupDepth));
                    }
                }
            }
        }

        internal void CopyElementInTree(Guid pasteItemId, IComposite entityToInsert, IComposite targetToPasteIn, int targetIndex)
        {
            var entityToInsertAsQuestion = entityToInsert as IQuestion;
            if (entityToInsertAsQuestion != null)
            {
                if (targetToPasteIn.PublicKey == this.Id)
                    throw new QuestionnaireException(string.Format(ExceptionMessages.CantPasteQuestion));

                var question = (AbstractQuestion)entityToInsertAsQuestion.Clone();
                question.PublicKey = pasteItemId;
                this.innerDocument.Insert(targetIndex, question, targetToPasteIn.PublicKey);
                return;
            }

            var entityToInsertAsStaticText = entityToInsert as IStaticText;
            if (entityToInsertAsStaticText != null)
            {
                if (targetToPasteIn.PublicKey == this.Id)
                    throw new QuestionnaireException(string.Format(ExceptionMessages.StaticTextCantBePaste));

                var staticText = (StaticText)entityToInsertAsStaticText.Clone();
                staticText.PublicKey = pasteItemId;
                this.innerDocument.Insert(targetIndex, staticText, targetToPasteIn.PublicKey);
                return;
            }

            var entityToInsertAsGroup = entityToInsert as IGroup;
            if (entityToInsertAsGroup != null)
            {
                //roster as chapter is forbidden
                if (entityToInsertAsGroup.IsRoster && (targetToPasteIn.PublicKey == this.Id))
                    throw new QuestionnaireException(string.Format(ExceptionMessages.RosterCantBePaste));

                //roster, group, chapter
                Dictionary<Guid, Guid> replacementIdDictionary = (entityToInsert).TreeToEnumerable(x => x.Children).ToDictionary(y => y.PublicKey, y => Guid.NewGuid());
                replacementIdDictionary[entityToInsert.PublicKey] = pasteItemId;

                var clonedGroup = entityToInsertAsGroup.Clone();
                clonedGroup.TreeToEnumerable(x => x.Children).ForEach(c =>
                {
                    switch (c)
                    {
                        case Group g:
                            g.PublicKey = replacementIdDictionary[g.PublicKey];
                            g.RosterSizeQuestionId = GetIdOrReturnSameId(replacementIdDictionary, g.RosterSizeQuestionId);
                            g.RosterTitleQuestionId = GetIdOrReturnSameId(replacementIdDictionary, g.RosterTitleQuestionId);
                            break;
                        case IQuestion q:
                            ((AbstractQuestion)q).PublicKey = replacementIdDictionary[q.PublicKey];
                            q.CascadeFromQuestionId = GetIdOrReturnSameId(replacementIdDictionary, q.CascadeFromQuestionId);
                            q.LinkedToQuestionId = GetIdOrReturnSameId(replacementIdDictionary, q.LinkedToQuestionId);
                            q.LinkedToRosterId = GetIdOrReturnSameId(replacementIdDictionary, q.LinkedToRosterId);
                            break;
                        case Variable v:
                            v.PublicKey = replacementIdDictionary[v.PublicKey];
                            break;
                        case StaticText st:
                            st.PublicKey = replacementIdDictionary[st.PublicKey];
                            break;
                    }
                });
                this.innerDocument.Insert(targetIndex, clonedGroup, targetToPasteIn.PublicKey);
                return;
            }

            var entityToInsertAsVariable = entityToInsert as IVariable;
            if (entityToInsertAsVariable != null)
            {
                if (targetToPasteIn.PublicKey == this.Id)
                    throw new QuestionnaireException(string.Format(ExceptionMessages.VariableCantBePaste));

                var variable = (Variable)entityToInsertAsVariable.Clone();
                variable.PublicKey = pasteItemId;
                this.innerDocument.Insert(targetIndex, variable, targetToPasteIn.PublicKey);

                return;
            }

            throw new QuestionnaireException(string.Format(ExceptionMessages.UnknownTypeCantBePaste));
        }

        #endregion

        #region Questionnaire Invariants

        private void ThrowIfChapterHasMoreThanAllowedLimit(Guid itemId)
        {
            var chapter = this.innerDocument.GetChapterOfItemById(itemId);
            if (chapter.Children.TreeToEnumerable(x => x.Children).Count() >= MaxChapterItemsCount)
            {
                throw new QuestionnaireException(string.Format(ExceptionMessages.SectionCantHaveMoreThan_Items, MaxChapterItemsCount));
            }
        }

        private void ThrowIfTargetGroupHasReachedAllowedDepthLimit(Guid itemId)
        {
            var entity = innerDocument.Find<IComposite>(itemId);
            if (entity != null)
            {
                var targetGroupDepth = this.GetAllParentGroups(entity).Count();

                if (targetGroupDepth >= MaxGroupDepth)
                {
                    throw new QuestionnaireException(string.Format(ExceptionMessages.SubSectionDepthLimit, MaxGroupDepth));
                }
            }
        }

        private void ThrowIfTargetIndexIsNotAcceptable(int targetIndex, IGroup targetGroup, IGroup parentGroup)
        {
            var maxAcceptableIndex = targetGroup.Children.Count;
            if (parentGroup != null && targetGroup.PublicKey == parentGroup.PublicKey)
                maxAcceptableIndex--;

            if (targetIndex < 0 || maxAcceptableIndex < targetIndex)
                throw new QuestionnaireException(
                   string.Format(ExceptionMessages.CantMoveSubsectionInWrongPosition, FormatGroupForException(targetGroup.PublicKey, this.innerDocument), targetIndex));
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new QuestionnaireException(DomainExceptionType.QuestionnaireTitleRequired, ExceptionMessages.QuestionnaireTitleIsEmpty);
            }
        }

        private static void ThrowDomainExceptionIfEntityDoesNotExists(QuestionnaireDocument doc, Guid entityId)
        {
            var entity = doc.Find<IComposite>(entityId);
            if (entity == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format(ExceptionMessages.QuestionnaireCantBeFound, entityId));
            }
        }

        private void ThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new QuestionnaireException(DomainExceptionType.QuestionNotFound, string.Format(ExceptionMessages.QuestionCannotBeFound, publicKey));
            }
        }

        private void ThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupNotFound,
                    string.Format(ExceptionMessages.SubSectionCantBeFound, groupPublicKey));
            }
        }

        private void ThrowDomainExceptionIfEntityAlreadyExists(Guid entityId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                elementId: entityId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.EntityWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format(ExceptionMessages.ItemWithIdExistsAlready, entityId));
        }

        private void ThrowDomainExceptionIfQuestionAlreadyExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format(ExceptionMessages.MoreThanOneQuestionWithSameId,
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
                    elementsWithSameId => string.Format(ExceptionMessages.MoreThanOneSubSectionWithSameId,
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfMoreThanOneQuestionExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneQuestionsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format(ExceptionMessages.MoreThanOneQuestionWithSameId,
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfMoreThanOneGroupExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneGroupsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format(ExceptionMessages.MoreThanOneSubSectionWithSameId,
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
            if (this.innerDocument.Macros.ContainsKey(macroId))
            {
                throw new QuestionnaireException(DomainExceptionType.MacroAlreadyExist, ExceptionMessages.MacroAlreadyExist);
            }
        }

        private void ThrowDomainExceptionIfMacroIsAbsent(Guid macroId)
        {
            if (!this.innerDocument.Macros.ContainsKey(macroId))
            {
                throw new QuestionnaireException(DomainExceptionType.MacroIsAbsent, ExceptionMessages.MacroIsAbsent);
            }
        }

        private void ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(Guid viewerId) 
        {
            if (this.innerDocument.CreatedBy != viewerId && !this.SharedUsersIds.Contains(viewerId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit, ExceptionMessages.NoPremissionsToEditQuestionnaire);
            }
            if (this.ReadOnlyUsersIds.Contains(viewerId))
            {
                throw new QuestionnaireException(
                   DomainExceptionType.DoesNotHavePermissionsForEdit, ExceptionMessages.NoPremissionsToEditQuestionnaire);
            }
        }

        private void ThrowDomainExceptionIfFilteredComboboxIsInvalid(Guid questionId, Option[] options)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.FilteredComboboxQuestionNotFound,
                    string.Format(ExceptionMessages.ComboboxCannotBeFound, questionId));
            }

            if (!categoricalOneAnswerQuestion.IsFilteredCombobox.HasValue || !categoricalOneAnswerQuestion.IsFilteredCombobox.Value)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionIsNotAFilteredCombobox,
                    string.Format(ExceptionMessages.QuestionIsNotCombobox, FormatQuestionForException(questionId, this.innerDocument)));
            }

            ThrowIfOptionsCanNotBeParsed(options);
        }

        private void ThrowDomainExceptionIfCascadingComboboxIsInvalid(Guid questionId, Option[] options)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format(ExceptionMessages.ComboboxCannotBeFound, questionId));
            }
            
            ThrowIfOptionsCanNotBeParsed(options);
        }

        private static void ThrowIfOptionsCanNotBeParsed(Option[] options)
        {
            var numberStyles = NumberStyles.None | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite;
            if (options.Any(x => !int.TryParse(x.Value,
                numberStyles,
                CultureInfo.InvariantCulture, out int _)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorValueSpecialCharacters,
                    ExceptionMessages.OptionValuesShouldBeNumbers);
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

        #endregion

        #region Utilities

        private static void PrepareGeneralProperties(ref string title, ref string variableName)
        {
            variableName = variableName?.Trim();
            title = title?.Trim();
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
                AnswerValue = option.Value,
                AnswerText = option.Title,
                ParentValue = option.ParentValue
            };
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionId);
        }

        private string FormatQuestionForException(Guid questionId, QuestionnaireDocument document)
        {
            var question = document.Find<IQuestion>(questionId);

            return string.Format("'{0}', {1}", question.QuestionText, question.StataExportCaption);
        }

        private IEnumerable<IGroup> GetAllParentGroups(IComposite entity)
        {
            var currentParent = (IGroup)entity.GetParent();

            while (currentParent != null)
            {
                yield return currentParent;

                currentParent = (IGroup)currentParent.GetParent();
            }
        }

        private IGroup GetGroupById(Guid groupId)
        {
            return this.innerDocument.Find<IGroup>(groupId);
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

        
        private FixedRosterTitle[] GetRosterFixedTitlesOrThrow(FixedRosterTitleItem[] rosterFixedTitles)
        {
            if (rosterFixedTitles == null)
                return new FixedRosterTitle[0];

            if (rosterFixedTitles.Any(x => x == null))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorValueSpecialCharacters, ExceptionMessages.InvalidFixedTitle);
            }

            if (rosterFixedTitles.Any(x => String.IsNullOrWhiteSpace(x.Value)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorValueSpecialCharacters, ExceptionMessages.InvalidValueOfFixedTitle);
            }

            if (rosterFixedTitles.Any(x => !x.Value.IsDecimal()))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorValueSpecialCharacters, ExceptionMessages.ValueOfFixedTitleCantBeParsed);
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

        public void SetId(Guid id)
        {
            this.innerDocument.PublicKey = id;
        }

        #region factory methods
        
        private static IQuestion CreateQuestion(Guid publicKey, QuestionType questionType, QuestionScope questionScope,
            string questionText, string stataExportCaption, string variableLabel, string conditionExpression,
            bool hideIfDisabled, Order? answerOrder, bool featured, string instructions,
            QuestionProperties questionProperties, string mask, Answer[] answers,
            Guid? linkedToQuestionId, Guid? linkedToRosterId, bool? isInteger,
            int? countOfDecimalPlaces, bool? areAnswersOrdered, int? maxAllowedAnswers,
            int? maxAnswerCount, bool? isFilteredCombobox, Guid? cascadeFromQuestionId,
            bool? yesNoView, IList<ValidationCondition> validationConditions,
            string linkedFilterExpression, bool isTimestamp)
        {
            AbstractQuestion question;

            switch (questionType)
            {
                case QuestionType.MultyOption:
                    question = new MultyOptionsQuestion
                    {
                        AreAnswersOrdered = areAnswersOrdered ?? false,
                        MaxAllowedAnswers = maxAllowedAnswers,
                        YesNoView = yesNoView ?? false
                    };
                    UpdateAnswerList(answers, question, linkedToQuestionId);
                    break;
                case QuestionType.SingleOption:
                    question = new SingleQuestion();
                    UpdateAnswerList(answers, question, linkedToQuestionId);
                    break;

                case QuestionType.Text:
                    question = new TextQuestion
                    {
                        Mask = mask
                    };
                    break;
                case QuestionType.DateTime:
                    question = new DateTimeQuestion
                    {
                        IsTimestamp = isTimestamp
                    };
                    break;
                case QuestionType.Numeric:
                case QuestionType.AutoPropagate:
                    question = new NumericQuestion
                    {
                        IsInteger = questionType == QuestionType.AutoPropagate ? true : isInteger ?? false,
                        CountOfDecimalPlaces = countOfDecimalPlaces,
                        QuestionType = QuestionType.Numeric,
                        UseFormatting = questionProperties?.UseFormatting ?? false
                    };
                    UpdateAnswerList(answers, question, linkedToQuestionId);
                    break;
                case QuestionType.GpsCoordinates:
                    question = new GpsCoordinateQuestion();
                    break;

                case QuestionType.TextList:
                    question = new TextListQuestion
                    {
                        MaxAnswerCount = maxAnswerCount
                    };
                    break;
                case QuestionType.QRBarcode:
                    question = new QRBarcodeQuestion();
                    break;

                case QuestionType.Multimedia:
                    question = new MultimediaQuestion();
                    break;

                case QuestionType.Area:
                    question = new AreaQuestion();
                    break;

                case QuestionType.Audio:
                    question = new AudioQuestion();
                    break;

                default:
                    throw new NotSupportedException(string.Format(ExceptionMessages.QuestionTypeIsNotSupported, questionType));
            }

            question.PublicKey = publicKey;
            question.QuestionType = questionType;
            question.QuestionScope = questionScope;
            question.QuestionText = System.Web.HttpUtility.HtmlDecode(questionText);
            question.StataExportCaption = stataExportCaption;
            question.VariableLabel = variableLabel;
            question.ConditionExpression = conditionExpression;
            question.HideIfDisabled = hideIfDisabled;
            question.ValidationExpression = null;
            question.ValidationMessage = null;
            question.AnswerOrder = answerOrder;
            question.Featured = featured;
            question.Instructions = instructions;
            question.Properties = questionProperties ?? new QuestionProperties(false, false);
            question.LinkedToQuestionId = linkedToQuestionId;
            question.LinkedToRosterId = linkedToRosterId;
            question.LinkedFilterExpression = linkedFilterExpression;
            question.IsFilteredCombobox = isFilteredCombobox;
            question.CascadeFromQuestionId = cascadeFromQuestionId;
            question.ValidationConditions = validationConditions ?? new List<ValidationCondition>();

            return question;
        }
        private static void UpdateAnswerList(IEnumerable<Answer> answers, IQuestion question, Guid? linkedToQuestionId)
        {
            question.Answers?.Clear();

            if (linkedToQuestionId.HasValue || answers == null || !answers.Any()) return;

            foreach (var answer in answers)
            {
                question.AddAnswer(answer);
            }
        }

        private static IGroup CreateGroup(Guid id, string title, string variableName, string description, string enablingCondition, bool hideIfDisabled)
        {
            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(title);
            group.VariableName = variableName;
            group.PublicKey = id;
            group.Description = description;
            group.ConditionExpression = enablingCondition;
            group.HideIfDisabled = hideIfDisabled;
            return group;
        }

        #endregion

        public void RevertVersion(RevertVersionQuestionnaire command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var historyReferanceId = command.HistoryReferanceId;
            var questionnire = questionnireHistoryVersionsService.GetByHistoryVersion(historyReferanceId);
            if (questionnire == null)
                throw new ArgumentException(string.Format(ExceptionMessages.QuestionnaireRevisionCantBeFound, Id, historyReferanceId));

            this.innerDocument = questionnire;
        }

        public void UpdateMetaInfo(UpdateMetadata command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(command.Title);

            this.innerDocument.Title = command.Title;
            this.innerDocument.Metadata = command.Metadata;
        }
    }
}
