using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }

        Guid? ResponsibleId { get; }

        void InitializeQuestionnaireDocument();

        [Obsolete("This method is for import service only and should be removed at all.")]
        IQuestion GetQuestionByStataCaption(string stataCaption);

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        bool IsQuestionLinked(Guid questionId);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetGroupTitle(Guid groupId);

        Guid? GetCascadingQuestionParentId(Guid questionId);

        IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId);

        string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue);

        decimal GetCascadingParentValue(Guid questionId, decimal answerOptionValue);

        int? GetMaxSelectedAnswerOptions(Guid questionId);

        bool IsCustomValidationDefined(Guid questionId);

        string GetCustomValidationExpression(Guid questionId);

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        bool ShouldQuestionSpecifyRosterSize(Guid questionId);

        IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId);

        int? GetListSizeForListQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId);

        IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null);

        int GetRosterLevelForQuestion(Guid questionId);

        int GetRosterLevelForGroup(Guid groupId);

        IEnumerable<Guid> GetAllMandatoryQuestions();

        bool IsRosterGroup(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildGroups(Guid groupId);

        Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId);
        
        bool IsQuestionMandatory(Guid questionId);

        bool IsQuestionInteger(Guid questionId);

        int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId);

        FixedRosterTitle[] GetFixedRosterTitles(Guid groupId);

        bool DoesQuestionSpecifyRosterTitle(Guid questionId);

        IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId);

        IEnumerable<Guid> GetNestedRostersOfGroupById(Guid rosterId);

        Guid? GetRosterSizeQuestion(Guid rosterId);

        IEnumerable<Guid> GetCascadingQuestionsThatDependUponQuestion(Guid questionId);

        IEnumerable<Guid> GetCascadingQuestionsThatDirectlyDependUponQuestion(Guid id);

        IEnumerable<Guid> GetAllChildCascadingQuestions();

        bool DoesCascadingQuestionHaveOptionsForParentValue(Guid questionId, decimal parentValue);
    }
}