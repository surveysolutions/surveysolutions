using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }

        bool HasQuestion(Guid questionId);

        QuestionType GetQuestionType(Guid questionId);

        IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId);

        bool IsCustomValidationDefined(Guid questionId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomValidation(Guid questionId);

        string GetCustomValidationExpression(Guid questionId);

        IEnumerable<Guid> GetQuestionsWithInvalidCustomValidationExpressions();

        IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionForGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionForQuestion(Guid questionId);

        IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetGroupsWithInvalidCustomEnablementConditions();

        IEnumerable<Guid> GetQuestionsWithInvalidCustomEnablementConditions();

        bool ShouldQuestionPropagateGroups(Guid questionId);

        IEnumerable<Guid> GetGroupsPropagatedByQuestion(Guid questionId);

        int GetMaxAnswerValueForPropagatingQuestion(Guid questionId);

        IEnumerable<Guid> GetPropagatingQuestionsWhichReferToMissingOrNotPropagatableGroups();

        IEnumerable<Guid> GetParentPropagatableGroupsForQuestionStartingFromTop(Guid questionId);
    }
}