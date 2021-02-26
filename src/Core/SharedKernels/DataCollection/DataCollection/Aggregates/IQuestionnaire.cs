using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }
        int Revision { get; }

        Guid QuestionnaireId { get; }

        string Title { get; }
        string DefaultLanguageName { get; }
        
        Translation Translation { get; }

        string VariableName { get; }
        Type ExpressionStorageType { get; set; }
        IReadOnlyList<Translation> Translations { get; }

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        QuestionScope GetQuestionScope(Guid questionId);

        AnswerType GetAnswerType(Guid questionId);

        bool IsQuestionLinked(Guid questionId);

        bool IsLinkedToListQuestion(Guid questionId);
        bool IsUsingExpressionStorage();
        Guid[] GetQuestionsLinkedToRoster();

        Guid[] GetQuestionsLinkedToQuestion();

        Guid? GetQuestionIdByVariable(string variable);

        Guid GetVariableIdByVariableName(string variableName);

        Guid? GetRosterIdByVariableName(string variableName, bool ignoreCase = false);

        Guid? GetSectionIdByVariable(string variable);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetQuestionExportDescription(Guid questionId);

        string GetGroupTitle(Guid groupId);

        string GetStaticText(Guid staticTextId);

        Attachment GetAttachmentForEntity(Guid entityId);
        Guid? GetAttachmentIdByName(string name);
        Attachment GetAttachmentById(Guid attachmentId);

        Guid? GetCascadingQuestionParentId(Guid questionId);

        IEnumerable<int> GetMultiSelectAnswerOptionsAsValues(Guid questionId);

        IEnumerable<CategoricalOption> GetCategoricalMultiOptionsByValues(Guid questionId, int[] values);

        IEnumerable<CategoricalOption> GetOptionsForQuestion(Guid questionId, int? parentQuestionValue,
            string searchFor, int[] excludedOptionIds);

        bool DoesSupportReusableCategories(Guid questionId);

        Guid? GetReusableCategoriesForQuestion(Guid questionId);

        CategoricalOption GetOptionForQuestionByOptionText(Guid questionId, string optionText, int? parentQuestionValue);

        CategoricalOption GetOptionForQuestionByOptionValue(Guid questionId, decimal optionValue, int? answerParentValue);

        IEnumerable<CategoricalOption> GetOptionsForQuestionFromStructure(Guid questionId, int? parentQuestionValue, string filter, int[] excludedOptionIds = null);

        CategoricalOption GetOptionForQuestionByOptionTextFromStructure(Guid questionId, string optionText, int? parentQuestionValue);

        CategoricalOption GetOptionForQuestionByOptionValueFromStructure(Guid questionId, decimal optionValue, int? parentQuestionValue);

        string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue, int? answerParentValue);

        int? GetMaxSelectedAnswerOptions(Guid questionId);

        int GetMaxRosterRowCount();

        int GetMaxLongRosterRowCount();

        bool IsQuestion(Guid entityId);

        bool IsStaticText(Guid entityId);

        bool IsInterviewierQuestion(Guid questionId);

        ReadOnlyCollection<Guid> GetPrefilledQuestions();
        
        ReadOnlyCollection<Guid> GetPrefilledEntities();

        ReadOnlyCollection<Guid> GetHiddenQuestions();

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        ReadOnlyCollection<Guid> GetParentsStartingFromTop(Guid entityId);

        Guid? GetParentGroup(Guid entityId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        bool IsRosterSizeQuestion(Guid questionId);

        bool IsRosterTitleQuestion(Guid questionId);

        IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId);

        int? GetListSizeForListQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedEntity(Guid entityId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId);

        IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null);

        IEnumerable<Guid> GetRostersWithTitlesToChange();

        Guid[] GetRosterSizeSourcesForEntity(Guid entityId);

        IReadOnlyCollection<Guid> GetAllRosterSizeQuestions();

        int GetRosterLevelForQuestion(Guid questionId);

        int GetRosterLevelForGroup(Guid groupId);

        int GetRosterLevelForEntity(Guid entityId);

        bool IsRosterGroup(Guid groupId);

        ReadOnlyCollection<Guid> GetAllEntities();
        
        ReadOnlyCollection<Guid> GetAllQuestions();

        ReadOnlyCollection<Guid> GetAllVariables();

        ReadOnlyCollection<Guid> GetAllStaticTexts();

        ReadOnlyCollection<Guid> GetAllGroups();

        ReadOnlyCollection<Guid> GetAllRosters();

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestionsOutsideRosters(Guid? groupId);
        IEnumerable<Guid> GetAllUnderlyingVariablesOutsideRosters(Guid? groupId);

        IEnumerable<Guid> GetAllUnderlyingStaticTexts(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildGroupsAndRosters(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildGroups(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildRosters(Guid groupId);

        Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId);

        Guid GetRosterReferencedByLinkedQuestion(Guid linkedQuestionId);

        bool IsQuestionLinkedToRoster(Guid linkedQuestionId);

        bool IsQuestionInteger(Guid questionId);

        bool IsQuestionYesNo(Guid questionId);

        int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId);

        FixedRosterTitle[] GetFixedRosterTitles(Guid groupId);

        string GetFixedRosterTitle(Guid groupId, decimal fixedTitleValue);

        bool DoesQuestionSpecifyRosterTitle(Guid questionId);

        IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId);

        bool IsRosterTitleQuestionAvailable(Guid rosterId);

        IEnumerable<Guid> GetNestedRostersOfGroupById(Guid rosterId);

        Guid GetRosterSizeQuestion(Guid rosterId);

        Guid? GetRosterTitleQuestionId(Guid rosterId);

        IEnumerable<Guid> GetCascadingQuestionsThatDependUponQuestion(Guid questionId);

        IEnumerable<Guid> GetCascadingQuestionsThatDirectlyDependUponQuestion(Guid id);

        IEnumerable<Guid> GetAllChildCascadingQuestions();

        IEnumerable<Guid> GetAllSections();

        /// <summary>
        /// Gets list of question ids that use question with provided <param name="questionId">questionId</param> as a substitution
        /// </summary>
        /// <param name="questionId">Substituted question id</param>
        /// <returns>List of questions that depend on provided question</returns>
        IEnumerable<Guid> GetSubstitutedQuestions(Guid questionId);

        /// <summary>
        /// Gets list of static text ids that use question with provided <param name="questionId">questionId</param> as a substitution.
        /// </summary>
        IEnumerable<Guid> GetSubstitutedStaticTexts(Guid questionId);

        /// <summary>
        /// Gets list of <see cref="IGroup"/> ids that use <see cref="IQuestion"/> with provided <param name="questionId">questionId</param> as a substitution.
        /// </summary>
        IEnumerable<Guid> GetSubstitutedGroups(Guid questionId);

        /// <summary>
        /// Gets first level child questions of a group
        /// </summary>
        ReadOnlyCollection<Guid> GetChildQuestions(Guid groupId);

        /// <summary>
        /// Gets first level child entities of a group
        /// </summary>
        IReadOnlyCollection<Guid> GetChildEntityIds(Guid groupId);

        ReadOnlyCollection<Guid> GetChildInterviewerQuestions(Guid groupId);

        ReadOnlyCollection<Guid> GetChildStaticTexts(Guid groupId);

        IReadOnlyList<Guid> GetAllUnderlyingInterviewerEntities(Guid id);

        IReadOnlyList<Guid> GetSubSectionsWithEnablementCondition(Guid groupId);

        bool IsPrefilled(Guid entityId);
        bool ShouldBeHiddenIfDisabled(Guid entityId);

        string GetValidationMessage(Guid questionId, int conditionIndex);
        string[] GetValidationMessages(Guid entityId);

        bool HasMoreThanOneValidationRule(Guid questionId);
        string GetQuestionInstruction(Guid questionId);
        bool IsQuestionFilteredCombobox(Guid questionId);
        bool IsQuestionCascading(Guid questionId);
        bool ShouldQuestionRecordAnswersOrder(Guid questionId);
        string GetTextQuestionMask(Guid questionId);
        bool GetHideInstructions(Guid questionId);
        bool ShouldUseFormatting(Guid questionId);
        bool HasVariable(string variableName);
        bool HasQuestion(string variableName);
        bool HasRoster(string variableName);
        bool IsTimestampQuestion(Guid questionId);
        bool IsSupportFilteringForOptions(Guid questionId);
        bool IsFixedRoster(Guid id);
        bool IsNumericRoster(Guid id);

        decimal[] GetFixedRosterCodes(Guid rosterId);
        IReadOnlyCollection<string> GetTranslationLanguages();
        string GetDefaultTransation();
        bool IsQuestionIsRosterSizeForLongRoster(Guid questionId);
        bool IsSubSection(Guid groupId);
        bool IsVariable(Guid id);
        bool HasCustomRosterTitle(Guid id);
        IReadOnlyCollection<Guid> GetChildEntityIdsWithVariablesWithoutChache(Guid groupId);

        IEnumerable<QuestionnaireItemReference> GetChidrenReferences(Guid groupId);
        Guid? GetCommonParentRosterForLinkedQuestionAndItSource(Guid linkedQuestionId);
        string GetVariableLabel(Guid variableId);
        string GetVariableName(Guid variableId);
        bool HasVariable(Guid variableId);
        bool HasStaticText(Guid entityId);
        Guid GetFirstSectionId();
        IEnumerable<Guid> GetLinkedToSourceEntity(Guid linkedSourceEntityId);

        List<Guid> GetExpressionsPlayOrder();
        bool SupportsExpressionsGraph();
        List<Guid> GetExpressionsPlayOrder(Guid changedEntity);
        List<Guid> GetValidationExpressionsPlayOrder(IEnumerable<Guid> entities);

        bool HasAnyCascadingOptionsForSelectedParentOption(Guid cascadingQuestionId, Guid parenQuestionId, int selectedParentValue);
        string GetRosterVariableName(Guid id);
        IReadOnlyCollection<int> GetValidationWarningsIndexes(Guid entityId);
        bool IsSignature(Guid entityIdentityId);
        bool IsRosterTriggeredByOrderedMultiQuestion(Guid rosterId);
        DateTime? GetDefaultDateForDateQuestion(Guid dateQuestionId);
        bool IsFlatRoster(Guid groupId);
        bool IsTableRoster(Guid groupId);
        bool IsMatrixRoster(Guid groupId);
        bool IsCustomViewRoster(Guid groupId);

        bool ShowCascadingAsList(Guid id);
        int? GetCascadingAsListThreshold(Guid id);
        bool HasAnyMultimediaQuestion();

        /// <summary>
        ///  Gets variable name for any entity in questionnaire
        /// </summary>
        /// <param name="id">Entity Id</param>
        string GetEntityVariableOrThrow(Guid id);

        string ApplyMarkDownTransformation(string text);

        GeometryType? GetQuestionGeometryType(Guid questionId);
        int GetEntityIdMapValue(Guid entityId);
        bool IsCoverPage(Guid identityId);
        bool IsCoverPageSupported { get; }
        Guid CoverPageSectionId { get; }
        string GetAttachmentNameForEntity(Guid entityId);
        IEnumerable<Guid> GetStaticTextsThatUseVariableAsAttachment(Guid variableId);
        Guid GetEntityReferencedByLinkedQuestion(Guid linkedQuestionId);
        bool IsInsideRoster(Guid entityId);
    }
}
