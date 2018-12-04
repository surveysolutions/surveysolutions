using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }

        Guid QuestionnaireId { get; }

        string Title { get; }
        
        Translation Translation { get; }

        string VariableName { get; }

        IQuestion GetQuestionByVariable(string variable);

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        QuestionScope GetQuestionScope(Guid questionId);

        AnswerType GetAnswerType(Guid questionId);

        bool IsQuestionLinked(Guid questionId);

        bool IsLinkedToListQuestion(Guid questionId);

        Guid[] GetQuestionsLinkedToRoster();

        Guid[] GetQuestionsLinkedToQuestion();

        Guid? GetQuestionIdByVariable(string variable);

        Guid GetVariableIdByVariableName(string variableName);

        Guid? GetRosterIdByVariableName(string variableName, bool ignoreCase = false);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetGroupTitle(Guid groupId);

        string GetStaticText(Guid staticTextId);

        Attachment GetAttachmentForEntity(Guid entityId);

        Guid? GetCascadingQuestionParentId(Guid questionId);

        IEnumerable<decimal> GetMultiSelectAnswerOptionsAsValues(Guid questionId);

        IEnumerable<CategoricalOption> GetOptionsForQuestion(Guid questionId, int? parentQuestionValue, string searchFor);

        CategoricalOption GetOptionForQuestionByOptionText(Guid questionId, string optionText, int? parentQuestionValue);

        CategoricalOption GetOptionForQuestionByOptionValue(Guid questionId, decimal optionValue);

        IEnumerable<CategoricalOption> GetOptionsForQuestionFromStructure(Guid questionId, int? parentQuestionValue, string filter);

        CategoricalOption GetOptionForQuestionByOptionTextFromStructure(Guid questionId, string optionText, int? parentQuestionValue);

        CategoricalOption GetOptionForQuestionByOptionValueFromStructure(Guid questionId, decimal optionValue);

        string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue);

        int GetCascadingParentValue(Guid questionId, decimal answerOptionValue);

        int? GetMaxSelectedAnswerOptions(Guid questionId);

        int GetMaxRosterRowCount();

        int GetMaxLongRosterRowCount();

        bool IsQuestion(Guid entityId);

        bool IsStaticText(Guid entityId);

        bool IsInterviewierQuestion(Guid questionId);

        ReadOnlyCollection<Guid> GetPrefilledQuestions();

        ReadOnlyCollection<Guid> GetHiddenQuestions();

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        ReadOnlyCollection<Guid> GetParentsStartingFromTop(Guid entityId);

        Guid? GetParentGroup(Guid entityId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        bool IsRosterSizeQuestion(Guid questionId);

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

        ReadOnlyCollection<Guid> GetAllQuestions();

        ReadOnlyCollection<Guid> GetAllVariables();

        ReadOnlyCollection<Guid> GetAllStaticTexts();

        ReadOnlyCollection<Guid> GetAllGroups();

        ReadOnlyCollection<Guid> GetAllRosters();

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestionsOutsideRosters(Guid? groupId);
        IEnumerable<Guid> GetAllUnderlyingVariablesOutsideRosters(Guid? groupId);

        IEnumerable<Guid> GetAllUnderlyingStaticTexts(Guid groupId);

        ReadOnlyCollection<Guid> GetAllUnderlyingInterviewerQuestions(Guid groupId);

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

        bool IsPrefilled(Guid questionId);
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
        IReadOnlyCollection<Guid> GetChildEntityIdsWithVariablesWithoutChache(Guid groupId);

        IEnumerable<QuestionnaireItemReference> GetChidrenReferences(Guid groupId);
        Guid? GetCommonParentRosterForLinkedQuestionAndItSource(Guid linkedQuestionId);
        string GetVariableLabel(Guid variableId);
        string GetVariableName(Guid variableId);
        bool HasVariable(Guid variableId);
        bool HasStaticText(Guid entityId);
        Guid GetFirstSectionId();
        IEnumerable<Guid> GetLinkedToSourceEntity(Guid linkedSourceEntityId);
        bool IsUsingExpressionStorage();

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
    }
}
