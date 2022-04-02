using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.Questionnaire.Documents;
using Group = Main.Core.Entities.SubEntities.Group;


namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    internal class Questionnaire : IPlainAggregateRoot
    {
        private const int MaxChapterItemsCount = 1000;
        private const int MaxSubSectionItemsCount = 400;
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
            foreach (var question in this.innerDocument.Children.TreeToEnumerable(x => x.Children).OfType<AbstractQuestion>())
            {
                question.MigrateValidationConditions();
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
                }
            }

            this.innerDocument.Add(newGroup, parentId);
        }

        private void RemoveRosterFlagFromGroup(Guid groupId)
        {
            this.innerDocument.UpdateGroup(groupId, group =>
            {
                group.IsRoster = false;
                group.CustomRosterTitle = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.FixedRosterTitles = new FixedRosterTitle[0];
            });
        }
        
        #endregion

        #region Dependencies

        private readonly IClock clock;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly IDesignerTranslationService translationService;
        private readonly ICategoriesService categoriesService;
        private readonly IFindReplaceService findReplaceService;
        private readonly IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService;
        private int affectedByReplaceEntries;

        #endregion

        public Questionnaire(
            IClock clock, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService,
            IDesignerTranslationService translationService,
            IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService,
            ICategoriesService categoriesService,
            IFindReplaceService findReplaceService)
        {
            this.clock = clock;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationService = translationService;
            this.questionnaireHistoryVersionsService = questionnaireHistoryVersionsService;
            this.categoriesService = categoriesService;
            this.findReplaceService = findReplaceService;
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
                VariableName = createQuestionnaire.Variable,
                CoverPageSectionId = Guid.NewGuid(),
            };

            this.AddGroup(CreateGroup(QuestionnaireDocument.CoverPageSectionId, QuestionnaireEditor.CoverPageSection, String.Empty, String.Empty, String.Empty, false), null);
            this.AddGroup(CreateGroup(Guid.NewGuid(), QuestionnaireEditor.NewSection, String.Empty,
                String.Empty, String.Empty, false), null);
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
                    lookupTableService.CloneLookupTable(document.PublicKey, lookupTable.Key,  this.Id, lookupTable.Key);
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

            foreach (var categories in clonedDocument.Categories)
                this.categoriesService.CloneCategories(document.PublicKey, categories.Id, clonedDocument.PublicKey, categories.Id);

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
        {
            if (!command.IsResponsibleAdmin) 
                this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(command.Title);

            this.innerDocument.Title = System.Web.HttpUtility.HtmlDecode(command.Title);
            this.innerDocument.VariableName = System.Web.HttpUtility.HtmlDecode(command.Variable);
            this.innerDocument.IsPublic = command.IsPublic;
            this.innerDocument.HideIfDisabled = command.HideIfDisabled;
            this.innerDocument.DefaultLanguageName = command.DefaultLanguageName;
        }

        public void DeleteQuestionnaire()
        {
            this.innerDocument.IsDeleted = true;
        }

        public IEnumerable<QuestionnaireEntityReference> FindAllTexts(string searchFor, bool matchCase,
            bool matchWholeWord, bool useRegex)
        {
            return findReplaceService.FindAll(this.innerDocument, searchFor, matchCase, matchWholeWord, useRegex);
        }

        public void ReplaceTexts(ReplaceTextsCommand command)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.affectedByReplaceEntries = findReplaceService.ReplaceTexts(this.innerDocument, command.ResponsibleId,
                command.SearchFor, command.ReplaceWith, command.MatchCase, command.MatchWholeWord, command.UseRegex);
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

        #region Categories command handlers

        public void AddOrUpdateCategories(AddOrUpdateCategories command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var categories = new Categories()
            {
                Id = command.CategoriesId,
                Name = command.Name,
            };
            innerDocument.Categories.RemoveAll(x => x.Id == command.CategoriesId);

            if (command.OldCategoriesId.HasValue)
            {
                innerDocument.Categories.RemoveAll(x => x.Id == command.OldCategoriesId.Value);

                var categoricalQuestionsWithOldId = innerDocument.Find<ICategoricalQuestion>(c => c.CategoriesId == command.OldCategoriesId);
                categoricalQuestionsWithOldId.ForEach(c => c.CategoriesId = command.CategoriesId);
            }

            innerDocument.Categories.Add(categories);
        }

        public void DeleteCategories(DeleteCategories command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.innerDocument.Categories.RemoveAll(x => x.Id == command.CategoriesId);

            this.innerDocument.Find<ICategoricalQuestion>(x => x.CategoriesId == command.CategoriesId)
                .ForEach(x => x.CategoriesId = null);
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

        public void AddGroupAndMoveIfNeeded(Guid groupId, Guid responsibleId, string? title, 
            string? variableName, Guid? rosterSizeQuestionId, string? description, string condition, 
            bool hideIfDisabled, Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource,
            FixedRosterTitleItem[]? rosterFixedTitles, Guid? rosterTitleQuestionId, int? index = null)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);
            this.ThrowDomainExceptionIfTryAddEntityInCoverPage(parentGroupId);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);

            if (parentGroupId.HasValue)
            {
                this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(parentGroupId.Value);
                this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(parentGroupId.Value);
                this.ThrowIfTargetGroupHasReachedAllowedDepthLimit(parentGroupId.Value);
            }

            this.AddGroup(CreateGroup(groupId, title, variableName ?? String.Empty, description?? String.Empty, condition, hideIfDisabled),
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
                    group.CustomRosterTitle = true;
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
            string? title,string? variableName, 
            Guid? rosterSizeQuestionId, string? description, string condition, bool hideIfDisabled, 
            bool isRoster, 
            RosterSizeSourceType rosterSizeSource, 
            FixedRosterTitleItem[]? rosterFixedTitles, 
            Guid? rosterTitleQuestionId,
            RosterDisplayMode displayMode)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            var group = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            var fixedTitles = GetRosterFixedTitlesOrThrow(rosterFixedTitles);
            
            var wasGroupAndBecomeARoster = !@group.IsRoster && isRoster;
            var wasRosterAndBecomeAGroup = @group.IsRoster && !isRoster;

            this.innerDocument.UpdateGroup(groupId,
                title ?? String.Empty,
                variableName ?? String.Empty,
                description ?? String.Empty,
                condition,
                hideIfDisabled,
                displayMode);

            if (isRoster)
            {
                this.innerDocument.UpdateGroup(groupId, groupToUpdate =>
                {
                    groupToUpdate.RosterSizeQuestionId = rosterSizeQuestionId;
                    groupToUpdate.RosterSizeSource = rosterSizeSource;
                    groupToUpdate.FixedRosterTitles = fixedTitles;
                    groupToUpdate.RosterTitleQuestionId = rosterTitleQuestionId;
                    groupToUpdate.IsRoster = true;
                    groupToUpdate.CustomRosterTitle = true;
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
            this.ThrowDomainExceptionIfTryDeleteCoverPage(groupId);
            this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            var isSection = this.QuestionnaireDocument.Children.Any(s => s.PublicKey == groupId);
            var isLastUserSection = this.QuestionnaireDocument.Children.Count == 1;
            if (isSection && isLastUserSection)
            {
                throw new QuestionnaireException(DomainExceptionType.Undefined, ExceptionMessages.CantRemoveLastSectionInQuestionnaire);
            }

            this.innerDocument.RemoveEntity(groupId);
        }

        public void MoveGroup(Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfTryDeleteCoverPage(groupId);
            this.ThrowDomainExceptionIfCoverPageNotFirst(groupId, targetGroupId, targetIndex);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            var sourceGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            IGroup? targetGroup = null;
            
            if (targetGroupId.HasValue)
            {
                targetGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(targetGroupId.Value);

                var sourceChapter = this.innerDocument.GetChapterOfItemByIdOrThrow(groupId);
                var targetChapter = this.innerDocument.GetChapterOfItemByIdOrThrow(targetGroupId.Value);

                if (IsCoverPage(targetChapter.PublicKey))
                {
                    bool isContainsNotAllowedEntities = sourceGroup.Children
                        .Any(c => !(c is IQuestion || c is IStaticText || c is IVariable));
                    if (isContainsNotAllowedEntities)
                    {
                        throw new QuestionnaireException(DomainExceptionType.CanNotAddElementToCoverPage, ExceptionMessages.CoverPageCanContainsOnlyQuestionsAndStaticTextsAndVariables);
                    }
                }

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
                    
                    if (targetGroup.Children.Count >= MaxSubSectionItemsCount)
                    {
                        throw new QuestionnaireException(string.Format(ExceptionMessages.SubsectionCantHaveMoreThan_DirectChildren, MaxSubSectionItemsCount));
                    }
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(targetGroup).Count();
                var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(sourceGroup);

                if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                {
                    throw new QuestionnaireException(string.Format(ExceptionMessages.SubSectionDepthLimit, MaxGroupDepth));
                }
            }
            
            // if we don't have a target group we would like to move source group into root of questionnaire
            
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup ?? this.innerDocument, sourceGroup.GetParent() as IGroup);

            var targetIsCoverPage = targetGroupId.HasValue && IsCoverPage(targetGroupId.Value);
            if (targetIsCoverPage)
            {
                var elementsToCopy = sourceGroup.Children
                        .Where(el => el is IQuestion || el is StaticText || el is IVariable)
                        .Select(el => el.PublicKey)
                        .ToList();
                foreach (var compositeId in elementsToCopy)
                {
                    this.innerDocument.MoveItem(compositeId, targetGroupId, targetIndex);
                }
            }
            else
            {
                this.innerDocument.MoveItem(groupId, targetGroupId, targetIndex);
            }
        }

        #endregion

        public void AddDefaultTypeQuestionAdnMoveIfNeeded(AddDefaultTypeQuestion command)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            
            var parentGroup = this.GetGroupById(command.ParentGroupId);
            if (parentGroup != null)
            {
                this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(parentGroup.PublicKey);
                this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(parentGroup.PublicKey);
            }

            var featured = IsCoverPage(command.ParentGroupId);

            IQuestion question = CreateQuestion(command.QuestionId,
                questionText: command.Title,
                questionType: QuestionType.Text,
                stataExportCaption: null,
                variableLabel: null,
                featured: featured,
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
                isTimestamp: false,
                showAsList:null,
                showAsListThreshold: null,
                categoriesId: null);

            this.innerDocument.Add(question, command.ParentGroupId);
            
            if (command.Index.HasValue)
            {
                this.innerDocument.MoveItem(command.QuestionId, command.ParentGroupId, command.Index.Value);
            }
        }

        public void DeleteQuestion(Guid questionId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            this.innerDocument.RemoveEntity(questionId);
           
        }

        public void MoveQuestion(Guid questionId, Guid targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfTryMoveQuestionInCoverPageForOldQuestionnaire(targetGroupId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            var question = this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            var targetGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(targetGroupId);

            this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(targetGroupId);
            this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(targetGroupId);

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, question.GetParent() as IGroup);

            this.innerDocument.MoveItem(questionId, targetGroupId, targetIndex);
        }

        public void UpdateTextQuestion(UpdateTextQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);
            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
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
                        false,
                        null,
                        null,
                        null);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateGpsCoordinatesQuestion(UpdateGpsCoordinatesQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            var question = this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
            IQuestion newQuestion = CreateQuestion(
                        question.PublicKey,
                        QuestionType.GpsCoordinates,
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
                        null, null, null, null,
                        null, null, null, null,
                        null, null, null, null,
                        command.ValidationConditions,
                        null,
                        false,
                        null,
                        null,
                        null);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateDateTimeQuestion(UpdateDateTimeQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);

            var question = this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

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
                command.IsTimestamp,
                null,
                null,
                null);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateMultiOptionQuestion(UpdateMultiOptionQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            
            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(command.LinkedToEntityId, out linkedQuestionId, out linkedRosterId);

            var answers = command.Options == null && command.IsFilteredCombobox
                ? this.GetQuestion(command.QuestionId)?.Answers.ToArray()
                : ConvertOptionsToAnswers(command.Options);

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(
                command.QuestionId,
                QuestionType.MultyOption,
                command.Scope,
                title,
                variableName,
                command.VariableLabel,
                command.EnablementCondition,
                command.HideIfDisabled,
                null, false,
                command.Instructions,
                command.Properties,
                null,
                answers,
                linkedQuestionId,
                linkedRosterId,
                null,
                null,
                command.AreAnswersOrdered,
                command.MaxAllowedAnswers,
                null,
                command.IsFilteredCombobox, 
                null,
                command.YesNoView,
                command.ValidationConditions,
                command.LinkedFilterExpression,
                false,
                null,
                null,
                categoriesId: command.CategoriesId);

            this.innerDocument.ReplaceEntity(question, newQuestion);
            
        }

        #region Question: SingleOption command handlers

        public void UpdateSingleOptionQuestion(UpdateSingleOptionQuestion command)
        {
            if(command == null)
                throw new ArgumentException(nameof(command));

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var title = command.Title;
            var variableName = command.VariableName;

            Answer[]? answers;

            if ((command.IsFilteredCombobox || command.CascadeFromQuestionId.HasValue) && command.CategoriesId == null)
            {
                IQuestion? originalQuestion = this.GetQuestion(command.QuestionId);
                answers = originalQuestion?.Answers.ToArray();                
            }
            else
            {
                answers = ConvertOptionsToAnswers(command.Options);                
            }

            PrepareGeneralProperties(ref title, ref variableName);

            Guid? linkedRosterId;
            Guid? linkedQuestionId;

            this.ExtractLinkedQuestionValues(command.LinkedToEntityId, out linkedQuestionId, out linkedRosterId);

            var question = this.innerDocument.Find<AbstractQuestion>(command.QuestionId);
            IQuestion newQuestion = CreateQuestion(command.QuestionId,
                QuestionType.SingleOption,
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
                null,
                answers,
                linkedQuestionId,
                linkedRosterId,
                null, null, null, null, null,
                command.IsFilteredCombobox,
                command.CascadeFromQuestionId,
                null,
                command.ValidationConditions,
                command.LinkedFilterExpression,
                false,
                showAsList:command.ShowAsList,
                showAsListThreshold: command.ShowAsListThreshold,
                categoriesId: command.CategoriesId);

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

        public void ReplaceOptionsWithClassification(ReplaceOptionsWithClassification command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            var categoricalQuestion = GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId) as ICategoricalQuestion;

            if(categoricalQuestion == null)
                throw new QuestionnaireException(DomainExceptionType.QuestionNotFound, string.Format(ExceptionMessages.QuestionCannotBeFound, command.QuestionId));

            IQuestion newQuestion = CreateQuestion(command.QuestionId,
                categoricalQuestion.QuestionType,
                categoricalQuestion.QuestionScope,
                categoricalQuestion.QuestionText,
                categoricalQuestion.StataExportCaption,
                categoricalQuestion.VariableLabel,
                categoricalQuestion.ConditionExpression,
                categoricalQuestion.HideIfDisabled,
                null,
                categoricalQuestion.Featured,
                categoricalQuestion.Instructions,
                categoricalQuestion.Properties,
                null,
                ConvertOptionsToAnswers(command.Options),
                categoricalQuestion.LinkedToQuestionId,
                categoricalQuestion.LinkedToRosterId,
                null, null, null, null, null,
                true,
                null/*categoricalOneAnswerQuestion.CascadeFromQuestionId*/,
                null,
                categoricalQuestion.ValidationConditions,
                null,
                false,
                null,
                null,
                categoricalQuestion.CategoriesId);

            this.innerDocument.ReplaceEntity(categoricalQuestion, newQuestion);
        }

        public void UpdateFilteredComboboxOptions(Guid questionId, Guid responsibleId, QuestionnaireCategoricalOption[] options)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            var categoricalOneAnswerQuestion = this.GetQuestionOrThrowDomainExceptionIfFilteredComboboxIsInvalid(questionId);
            
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
                null, null, null, 
                (categoricalOneAnswerQuestion as IMultyOptionsQuestion)?.MaxAllowedAnswers, 
                null,
                categoricalOneAnswerQuestion.IsFilteredCombobox,
                categoricalOneAnswerQuestion.CascadeFromQuestionId,
                null,
                categoricalOneAnswerQuestion.ValidationConditions,
                null,
                false,
                null,
                null,
                categoricalOneAnswerQuestion.CategoriesId);

            this.innerDocument.ReplaceEntity(categoricalOneAnswerQuestion, newQuestion);
        }

        public void UpdateCascadingComboboxOptions(Guid questionId, Guid responsibleId, QuestionnaireCategoricalOption[] options)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(options);

            var categoricalOneAnswerQuestion = GetQuestionOrThrowDomainExceptionIfCascadingComboboxIsInvalid(questionId);

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
                false,
                categoricalOneAnswerQuestion.ShowAsList,
                categoricalOneAnswerQuestion.ShowAsListThreshold,
                categoricalOneAnswerQuestion.CategoriesId);

            this.innerDocument.ReplaceEntity(categoricalOneAnswerQuestion, newQuestion);
        }
        #endregion


        public void UpdateNumericQuestion(UpdateNumericQuestion command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            var question = this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);

            var title = command.Title;
            var variableName = command.VariableName;
            PrepareGeneralProperties(ref title, ref variableName);

            IQuestion newQuestion = CreateQuestion(
                question.PublicKey,
                QuestionType.Numeric,
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
                false,
                null,
                null,
                null);

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        public void UpdateTextListQuestion(UpdateTextListQuestion command)
        {
            var title = command.Title;
            var variableName = command.VariableName;

            PrepareGeneralProperties(ref title, ref variableName);

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
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
                    null,
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
                    false,
                    null,
                    null);

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

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);

            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
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
                    null,
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
                    false,
                    null,
                    null,
                    null);

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

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
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
                null,
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
                false,
                null,
                null,
                null);

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

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);

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
                null,
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
                false,
                null,
                null,
                null);
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

            this.GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(command.QuestionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(command.QuestionId);
            IGroup? parentGroup = this.innerDocument.GetParentById(command.QuestionId);
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
        
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
                    null,
                    false,
                    command.Instructions,
                    command.Properties,
                    null, null, null, null, null, null, null, null, null,
                    null,null,null,
                    command.ValidationConditions,
                    null,
                    false,
                    null,
                    null,
                    null);

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
            this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);
            
            this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(command.ParentId);
            this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(command.ParentId);

            var staticText = new StaticText(publicKey: command.EntityId,
                text: System.Web.HttpUtility.HtmlDecode(command.Text),
                conditionExpression: String.Empty, 
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

            GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, command.EntityId);

            var oldStaticText = this.innerDocument.Find<IStaticText>(command.EntityId);
            var newStaticText = new StaticText(publicKey: command.EntityId,
                text: System.Web.HttpUtility.HtmlDecode(command.Text),
                conditionExpression: command.EnablementCondition,
                hideIfDisabled: command.HideIfDisabled,
                validationConditions: command.ValidationConditions,
                attachmentName: command.AttachmentName);

            this.innerDocument.ReplaceEntity(oldStaticText, newStaticText);
        }

        public void DeleteStaticText(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);

            this.innerDocument.RemoveEntity(entityId);
        }

        public void MoveStaticText(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            var sourceStaticText = GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);
            var targetGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(targetEntityId);
            this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(targetEntityId);
            
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceStaticText.GetParent() as IGroup);

            this.innerDocument.MoveItem(entityId, targetEntityId, targetIndex);
        }
        #endregion


        #region Variable command handlers
        public void AddVariableAndMoveIfNeeded(AddVariable command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(command.EntityId);
            this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(command.ParentId);

            this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(command.ParentId);
            this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(command.ParentId);

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
            GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, command.EntityId);

            var oldVariable = this.innerDocument.Find<IVariable>(command.EntityId);

            var newVariable = new Variable(command.EntityId, command.VariableData);
            this.innerDocument.ReplaceEntity(oldVariable, newVariable);
        }

        public void DeleteVariable(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);

            this.innerDocument.RemoveEntity(entityId);
        }

        public void MoveVariable(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, entityId);
            var targetGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            
            this.ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(targetEntityId);
            this.ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(targetEntityId);

            var sourceVariable = this.innerDocument.Find<IVariable>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceVariable != null ? sourceVariable.GetParent() as IGroup : null);

            this.innerDocument.MoveItem(entityId, targetEntityId, targetIndex);
        }
        #endregion

        #region Shared Person command handlers

        public void AddSharedPerson(Guid personId, string emailOrLogin, ShareType shareType, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (personId == Guid.Empty || string.IsNullOrEmpty(emailOrLogin))
                throw new QuestionnaireException(DomainExceptionType.InvalidUserInfo,
                    ExceptionMessages.InvalidUserInfo);

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
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsToUnsharePersonForQuestionnaire(personId, responsibleId);

            if (!this.SharedUsersIds.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserDoesNotExistInShareList, ExceptionMessages.CantRemoveUserFromTheList);
            }

            this.sharedPersons.RemoveAll(sp => sp.UserId == personId);
        }

        public void TransferOwnership(Guid ownerId, Guid newOwnerId, string ownerEmail, string newOwnerEmail)
        {
            this.ThrowDomainExceptionIfViewerIsNotOwnerOfQuestionnaire(ownerId);

            this.RemoveSharedPerson(newOwnerId, newOwnerEmail, ownerId);
            
            this.QuestionnaireDocument.CreatedBy = newOwnerId;

            this.AddSharedPerson(ownerId, ownerEmail, ShareType.Edit, newOwnerId);
        }

        #endregion

        #region CopyPaste command handler
        
        public void PasteAfter(PasteAfter pasteAfter)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteAfter.ResponsibleId);
            var itemToInsertAfter = GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(this.innerDocument, pasteAfter.ItemToPasteAfterId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteAfter.EntityId);
            var entityToInsert = GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(pasteAfter.SourceDocument, pasteAfter.SourceItemId);

            var targetToPasteIn = itemToInsertAfter.GetParent();
            if(targetToPasteIn == null)
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format(ExceptionMessages.UnknownTypeCantBePaste, "unknown"));
            
            var targetIndex = targetToPasteIn.Children.IndexOf(itemToInsertAfter) + 1;

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);
            this.CopyElementInTree(pasteAfter.EntityId, entityToInsert, targetToPasteIn, targetIndex, pasteAfter.SourceDocument);
        }

        public void PasteInto(PasteInto pasteInto)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(pasteInto.ResponsibleId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(pasteInto.EntityId);

            var entityToInsert =
                GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(pasteInto.SourceDocument, pasteInto.SourceItemId);
            var targetToPasteIn = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(pasteInto.ParentId);

            var targetIndex = targetToPasteIn.Children.Count();

            this.CheckDepthInvariants(targetToPasteIn, entityToInsert);
            this.CopyElementInTree(pasteInto.EntityId, entityToInsert, targetToPasteIn, targetIndex, pasteInto.SourceDocument);
        }

        private void CheckDepthInvariants(IComposite targetToPasteIn, IComposite entityToInsert)
        {
            if (targetToPasteIn.PublicKey != this.Id)
            {
                var targetChapter = this.innerDocument.GetChapterOfItemByIdOrThrow(targetToPasteIn.PublicKey);

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
                
                if (targetToPasteIn.Children.Count >= MaxSubSectionItemsCount)
                {
                    throw new QuestionnaireException(string.Format(ExceptionMessages.SubsectionCantHaveMoreThan_DirectChildren, MaxSubSectionItemsCount));
                }

                var targetGroupDepthLevel = this.GetAllParentGroups(targetToPasteIn).Count();

                if (entityToInsert is IGroup entityToInsertAsGroup)
                {
                    var sourceGroupMaxChildNestingDepth = GetMaxChildGroupNestingDepth(entityToInsertAsGroup);

                    if ((targetGroupDepthLevel + sourceGroupMaxChildNestingDepth) > MaxGroupDepth)
                    {
                        throw new QuestionnaireException(string.Format(ExceptionMessages.SubSectionDepthLimit, MaxGroupDepth));
                    }
                }
            }
        }

        internal void CopyElementInTree(Guid pasteItemId, IComposite entityToInsert, IComposite targetToPasteIn, int targetIndex, QuestionnaireDocument? sourceQuestionnaire)
        {
            if(sourceQuestionnaire == null)
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format(ExceptionMessages.QuestionnaireCantBeFound, "unknown"));

            switch (entityToInsert)
            {
                case IQuestion entityToInsertAsQuestion:
                    this.CopyQuestion(pasteItemId, targetToPasteIn, targetIndex, entityToInsertAsQuestion, sourceQuestionnaire);
                    break;
                case IStaticText entityToInsertAsStaticText:
                    this.CopyStaticText(pasteItemId, targetToPasteIn, targetIndex, entityToInsertAsStaticText);
                    break;
                case IGroup entityToInsertAsGroup:
                    this.CopyGroup(pasteItemId, entityToInsert, targetToPasteIn, targetIndex, entityToInsertAsGroup, sourceQuestionnaire);
                    break;
                case IVariable entityToInsertAsVariable:
                    this.CopyVariable(pasteItemId, targetToPasteIn, targetIndex, entityToInsertAsVariable);
                    break;
                default:
                    throw new QuestionnaireException(string.Format(ExceptionMessages.UnknownTypeCantBePaste));
            }
        }

        private void CopyVariable(Guid pasteItemId, IComposite targetToPasteIn, int targetIndex, IVariable entityToInsertAsVariable)
        {
            if (targetToPasteIn.PublicKey == this.Id)
                throw new QuestionnaireException(string.Format(ExceptionMessages.VariableCantBePaste));
            if (IsCoverPage(targetToPasteIn.PublicKey))
                throw new QuestionnaireException(string.Format(ExceptionMessages.VariableCantBePaste));

            var variable = (Variable) entityToInsertAsVariable.Clone();
            variable.PublicKey = pasteItemId;
            this.innerDocument.Insert(targetIndex, variable, targetToPasteIn.PublicKey);
        }

        private void CopyGroup(Guid pasteItemId, IComposite entityToInsert, IComposite targetToPasteIn, int targetIndex,
            IGroup entityToInsertAsGroup, QuestionnaireDocument sourceQuestionnaire)
        {
            //roster as chapter is forbidden
            if (entityToInsertAsGroup.IsRoster && (targetToPasteIn.PublicKey == this.Id))
                throw new QuestionnaireException(string.Format(ExceptionMessages.RosterCantBePaste));

            //roster, group, chapter
            Dictionary<Guid, Guid> replacementIdDictionary = (entityToInsert).TreeToEnumerable(x => x.Children)
                .ToDictionary(y => y.PublicKey, y => Guid.NewGuid());
            replacementIdDictionary[entityToInsert.PublicKey] = pasteItemId;

            var clonedGroup = entityToInsertAsGroup.Clone();
            var targetIsCoverPage = IsCoverPage(targetToPasteIn.PublicKey);
            var elementsToCopy = targetIsCoverPage
                ? clonedGroup.Children.Where(el => el is IQuestion || el is StaticText)
                : clonedGroup.TreeToEnumerable(x => x.Children);
            elementsToCopy.ForEach(c =>
            {
                switch (c)
                {
                    case Group g:
                        g.PublicKey = replacementIdDictionary[g.PublicKey];
                        g.RosterSizeQuestionId = GetIdOrReturnSameId(replacementIdDictionary, g.RosterSizeQuestionId);
                        g.RosterTitleQuestionId = GetIdOrReturnSameId(replacementIdDictionary, g.RosterTitleQuestionId);
                        break;
                    case IQuestion q:
                        ((AbstractQuestion) q).PublicKey = replacementIdDictionary[q.PublicKey];
                        q.CascadeFromQuestionId = GetIdOrReturnSameId(replacementIdDictionary, q.CascadeFromQuestionId);
                        q.LinkedToQuestionId = GetIdOrReturnSameId(replacementIdDictionary, q.LinkedToQuestionId);
                        q.LinkedToRosterId = GetIdOrReturnSameId(replacementIdDictionary, q.LinkedToRosterId);
                        q.Featured = QuestionnaireDocument.IsCoverPageSupported ? targetIsCoverPage : q.Featured;
                        q.ConditionExpression = targetIsCoverPage ? string.Empty : q.ConditionExpression; 
                        this.CopyCategories(q, sourceQuestionnaire);
                        break;
                    case Variable v:
                        v.PublicKey = replacementIdDictionary[v.PublicKey];
                        break;
                    case StaticText st:
                        st.PublicKey = replacementIdDictionary[st.PublicKey];
                        break;
                }
            });

            if (targetIsCoverPage)
            {
                foreach (var entity in elementsToCopy.Reverse())
                {
                    this.innerDocument.Insert(targetIndex, entity, targetToPasteIn.PublicKey);
                }
            }
            else
            {
                this.innerDocument.Insert(targetIndex, clonedGroup, targetToPasteIn.PublicKey);
            }
        }

        private void CopyStaticText(Guid pasteItemId, IComposite targetToPasteIn, int targetIndex,
            IStaticText entityToInsertAsStaticText)
        {
            if (targetToPasteIn.PublicKey == this.Id)
                throw new QuestionnaireException(string.Format(ExceptionMessages.StaticTextCantBePaste));

            var staticText = (StaticText) entityToInsertAsStaticText.Clone();
            staticText.PublicKey = pasteItemId;

            if (IsCoverPage(targetToPasteIn.PublicKey))
            {
                staticText.ConditionExpression = String.Empty;
            }
            
            this.innerDocument.Insert(targetIndex, staticText, targetToPasteIn.PublicKey);
        }

        private void CopyQuestion(Guid pasteItemId, IComposite targetToPasteIn, int targetIndex,
            IQuestion entityToInsertAsQuestion, QuestionnaireDocument sourceQuestionnaire)
        {
            if (targetToPasteIn.PublicKey == this.Id)
                throw new QuestionnaireException(string.Format(ExceptionMessages.CantPasteQuestion));

            var question = (AbstractQuestion) entityToInsertAsQuestion.Clone();
            question.PublicKey = pasteItemId;

            if (QuestionnaireDocument.IsCoverPageSupported)
            {
                var targetIsCoverPage = IsCoverPage(targetToPasteIn.PublicKey);
                question.Featured = targetIsCoverPage;

                if (targetIsCoverPage)
                {
                    question.ConditionExpression = string.Empty;
                }
            }

            this.CopyCategories(entityToInsertAsQuestion, sourceQuestionnaire);

            this.innerDocument.Insert(targetIndex, question, targetToPasteIn.PublicKey);
        }

        private void CopyCategories(IQuestion entityToInsertAsQuestion, QuestionnaireDocument sourceQuestionnaire)
        {
            if (!(entityToInsertAsQuestion is ICategoricalQuestion categoricalQuestion)) return;
            if (!categoricalQuestion.CategoriesId.HasValue) return;
            if (this.innerDocument.Categories.Exists(x => x.Id == categoricalQuestion.CategoriesId.Value)) return;

            var sourceCategories = sourceQuestionnaire.Categories.Find(x => x.Id == categoricalQuestion.CategoriesId);
            if(sourceCategories != null)
                this.innerDocument.Categories.Add(sourceCategories);
        }

        #endregion

        #region Questionnaire Invariants

        private void ThrowIfChapterHasMoreNestedChildrenThanAllowedLimit(Guid itemId)
        {
            var chapter = this.innerDocument.GetChapterOfItemByIdOrThrow(itemId);
            if (chapter.Children.TreeToEnumerable(x => x.Children).Count() >= MaxChapterItemsCount)
            {
                throw new QuestionnaireException(string.Format(ExceptionMessages.SectionCantHaveMoreThan_Items, MaxChapterItemsCount));
            }
        }
        
        private void ThrowIfTargetSubsectionHasMoreDirectChildrenThanAllowedLimit(Guid groupId)
        {
            var targetGroup = this.GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(groupId);
            if (targetGroup.Children.Count >= MaxSubSectionItemsCount)
            {
                throw new QuestionnaireException(string.Format(ExceptionMessages.SubsectionCantHaveMoreThan_DirectChildren, MaxSubSectionItemsCount));
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

        private void ThrowIfTargetIndexIsNotAcceptable(int targetIndex, IGroup targetGroup, IGroup? parentGroup)
        {
            if (parentGroup == null)
                throw new QuestionnaireException(
                string.Format(ExceptionMessages.CantMoveSubsectionInWrongPosition, FormatGroupForException(targetGroup.PublicKey, this.innerDocument), targetIndex));

            var maxAcceptableIndex = targetGroup.Children.Count;
            if (targetGroup.PublicKey == parentGroup.PublicKey)
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

        private static IComposite GetEntityOrThrowDomainExceptionIfEntityDoesNotExists(QuestionnaireDocument? doc, Guid entityId)
        {
            if(doc == null)
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format(ExceptionMessages.QuestionnaireCantBeFound, "unknown"));

            var entity = doc.Find<IComposite>(entityId);
            if (entity == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format(ExceptionMessages.QuestionnaireCantBeFound, entityId));
            }

            return entity;
        }

        private AbstractQuestion GetQuestionOrThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new QuestionnaireException(DomainExceptionType.QuestionNotFound, string.Format(ExceptionMessages.QuestionCannotBeFound, publicKey));
            }

            return question;
        }

        private Group GetGroupOrThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupNotFound,
                    string.Format(ExceptionMessages.SubSectionCantBeFound, groupPublicKey));
            }

            return group;
        }

        private void ThrowDomainExceptionIfTryMoveQuestionInCoverPageForOldQuestionnaire(Guid groupPublicKey)
        {
            if (groupPublicKey == QuestionnaireDocument.CoverPageSectionId &&
                !QuestionnaireDocument.IsCoverPageSupported)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CanNotEditElementIntoCoverPage,
                    ExceptionMessages.CantEditCoverPageInOldQuestionnaire);
            }
        }

        private void ThrowDomainExceptionIfTryDeleteCoverPage(Guid groupPublicKey)
        {
            var isCoverPage = IsCoverPage(groupPublicKey);
            if (isCoverPage)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.TryToDeleteCoverPage,
                    ExceptionMessages.CantRemoveCoverPageInQuestionnaire);
            }
        }

        private void ThrowDomainExceptionIfCoverPageNotFirst(Guid groupPublicKey, Guid? targetGroupId, in int targetIndex)
        {
            if (!QuestionnaireDocument.IsCoverPageSupported)
                return;

            if (IsCoverPage(groupPublicKey)
                || (targetGroupId == null && targetIndex == 0))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CoverSectionMustBeFirst,
                    ExceptionMessages.CoverPageMustBeFirstInQuestionnaire);
            }
        }

        private void ThrowDomainExceptionIfTryAddEntityInCoverPage(Guid? targetGroupId)
        {
            if (!QuestionnaireDocument.IsCoverPageSupported || !targetGroupId.HasValue)
                return;

            if (IsCoverPage(targetGroupId.Value))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CanNotAddElementToCoverPage,
                    ExceptionMessages.CoverPageCanContainsOnlyQuestionsAndStaticTextsAndVariables);
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

        private void ThrowDomainExceptionIfViewerIsNotOwnerOfQuestionnaire(Guid viewerId)
        {
            if (this.innerDocument.CreatedBy != viewerId)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit, ExceptionMessages.NoPremissionsToEditQuestionnaire);
            }
        }

        private void ThrowDomainExceptionIfViewerDoesNotHavePermissionsToUnsharePersonForQuestionnaire(Guid personId, Guid viewerId)
        {
            if (this.innerDocument.CreatedBy != viewerId && !this.SharedUsersIds.Contains(viewerId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit, ExceptionMessages.NoPremissionsToEditQuestionnaire);
            }
            if (this.ReadOnlyUsersIds.Contains(viewerId) && personId != viewerId)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit, ExceptionMessages.NoPremissionsToEditQuestionnaire);
            }
        }

        private ICategoricalQuestion GetQuestionOrThrowDomainExceptionIfFilteredComboboxIsInvalid(Guid questionId)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<ICategoricalQuestion>(questionId);
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

            return categoricalOneAnswerQuestion;
        }

        private SingleQuestion GetQuestionOrThrowDomainExceptionIfCascadingComboboxIsInvalid(Guid questionId)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format(ExceptionMessages.ComboboxCannotBeFound, questionId));
            }

            return categoricalOneAnswerQuestion;
        }

        private void ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(QuestionnaireCategoricalOption[] options)
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

        private static void PrepareGeneralProperties(ref string? title, ref string? variableName)
        {
            variableName = variableName?.Trim();
            title = title?.Trim();
        }


        private static Answer[]? ConvertOptionsToAnswers(QuestionnaireCategoricalOption[] options)
            => options?.Select(ConvertOptionToAnswer).ToArray();

        private static Answer ConvertOptionToAnswer(QuestionnaireCategoricalOption option) => new Answer
        {
            AnswerCode = option.Value,
            AnswerText = option.Title,
            ParentCode = option.ParentValue,
            ParentValue = option.ParentValue?.ToString(),
            AnswerValue = option.Value.ToString()
        };

        private static Answer[]? ConvertOptionsToAnswers(Option[]? options) 
            => options?.Select(ConvertOptionToAnswer).ToArray();

        private static Answer ConvertOptionToAnswer(Option option) => new Answer
        {
            AnswerValue = option.Value,
            AnswerText = option.Title,
            ParentValue = option.ParentValue
        };

        private IQuestion? GetQuestion(Guid questionId)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionId);
        }

        private string FormatQuestionForException(Guid questionId, QuestionnaireDocument document)
        {
            var question = document.Find<IQuestion>(questionId);

            return string.Format("'{0}', {1}", question?.QuestionText ?? "Unknown question", question?.StataExportCaption);
        }

        private IEnumerable<IGroup> GetAllParentGroups(IComposite entity)
        {
            var currentParent = entity.GetParent() as IGroup;

            while (currentParent != null)
            {
                yield return currentParent;

                currentParent = currentParent.GetParent() as IGroup;
            }
        }

        private IGroup? GetGroupById(Guid groupId)
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

        
        private FixedRosterTitle[] GetRosterFixedTitlesOrThrow(FixedRosterTitleItem[]? rosterFixedTitles)
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
            string? questionText, string? stataExportCaption, string? variableLabel, string? conditionExpression,
            bool hideIfDisabled, Order? answerOrder, bool featured, string? instructions,
            QuestionProperties? questionProperties, string? mask, Answer[]? answers,
            Guid? linkedToQuestionId, Guid? linkedToRosterId, bool? isInteger,
            int? countOfDecimalPlaces, bool? areAnswersOrdered, int? maxAllowedAnswers,
            int? maxAnswerCount, bool? isFilteredCombobox, Guid? cascadeFromQuestionId,
            bool? yesNoView, IList<ValidationCondition> validationConditions,
            string? linkedFilterExpression, bool isTimestamp,
            bool? showAsList, int? showAsListThreshold, Guid? categoriesId = null)
        {
            AbstractQuestion question;

            switch (questionType)
            {
                case QuestionType.MultyOption:
                    question = new MultyOptionsQuestion
                    {
                        AreAnswersOrdered = areAnswersOrdered ?? false,
                        MaxAllowedAnswers = maxAllowedAnswers,
                        YesNoView = yesNoView ?? false,
                        CategoriesId = categoriesId
                    };
                    UpdateAnswerList(answers, question, linkedToQuestionId);
                    break;
                case QuestionType.SingleOption:
                    question = new SingleQuestion()
                    {
                        ShowAsList = showAsList ?? false,
                        ShowAsListThreshold = showAsListThreshold,
                        CategoriesId = categoriesId
                    };

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
            question.QuestionScope = questionScope;
            question.QuestionText = System.Web.HttpUtility.HtmlDecode(questionText);
            question.StataExportCaption = stataExportCaption ?? String.Empty;
            question.VariableLabel = variableLabel;
            question.ConditionExpression = conditionExpression ?? String.Empty;
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
        private static void UpdateAnswerList(IEnumerable<Answer>? answers, IQuestion question, Guid? linkedToQuestionId)
        {
            question.Answers?.Clear();
            if (linkedToQuestionId.HasValue || answers == null) return;
            question.Answers = answers.Select(x=> x).ToList();
        }

        private static IGroup CreateGroup(Guid id, string? title, string variableName, string description, string enablingCondition, bool hideIfDisabled)
        {
            return new Group
            {
                Title = System.Web.HttpUtility.HtmlDecode(title) ?? String.Empty,
                VariableName = variableName,
                PublicKey = id,
                Description = description,
                ConditionExpression = enablingCondition,
                HideIfDisabled = hideIfDisabled
            };
        }

        #endregion

        public void RevertVersion(RevertVersionQuestionnaire command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            var historyReferenceId = command.HistoryReferenceId;
            var questionnaire = questionnaireHistoryVersionsService.GetByHistoryVersion(historyReferenceId);

            this.innerDocument = questionnaire 
                                 ?? throw new ArgumentException(string.Format(ExceptionMessages.QuestionnaireRevisionCantBeFound, Id, historyReferenceId));
        }

        public void UpdateMetaInfo(UpdateMetadata command)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmpty(command.Title);

            this.innerDocument.Title = command.Title;
            this.innerDocument.Metadata = command.Metadata;
        }

        private bool IsCoverPage(Guid publicKey) => publicKey == QuestionnaireDocument.CoverPageSectionId;

        public void MigrateToNewVersion(MigrateToNewVersion command)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            if (QuestionnaireDocument.IsCoverPageSupported)
                throw new QuestionnaireException(DomainExceptionType.MigrateToNewVersion, ExceptionMessages.QuestionnaireAlreadySupportedCover);

            var cover = CreateGroup(QuestionnaireDocument.CoverPageSectionId, QuestionnaireEditor.CoverPageSection, String.Empty, String.Empty, String.Empty, false);
            this.innerDocument.Insert(0, cover, null);

            var featuredQuestions = QuestionnaireDocument.Children
                .TreeToEnumerableDepthFirst(c => c.Children)
                .Where(entity => entity is IQuestion question && question.Featured)
                .Reverse()
                .ToList();

            var coverId = QuestionnaireDocument.CoverPageSectionId;
            foreach (var question in featuredQuestions)
            {
                QuestionnaireDocument.MoveItem(question.PublicKey, coverId, 0);
            }
        }
    }
}
