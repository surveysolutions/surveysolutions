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
    }
}