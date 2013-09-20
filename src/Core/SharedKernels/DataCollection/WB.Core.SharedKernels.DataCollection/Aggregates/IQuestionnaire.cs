﻿using System;
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

        IQuestion GetQuestionByStataCaption(string stataCaption);

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetGroupTitle(Guid groupId);

        IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId);

        bool IsCustomValidationDefined(Guid questionId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomValidation(Guid questionId);

        string GetCustomValidationExpression(Guid questionId);

        IEnumerable<Guid> GetQuestionsWithInvalidCustomValidationExpressions();

        IEnumerable<Guid> GetAllQuestionsWithNotEmptyValidationExpressions();

        IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId);

        IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetGroupsWithInvalidCustomEnablementConditions();

        IEnumerable<Guid> GetQuestionsWithInvalidCustomEnablementConditions();

        bool ShouldQuestionPropagateGroups(Guid questionId);

        IEnumerable<Guid> GetGroupsPropagatedByQuestion(Guid questionId);

        int GetMaxAnswerValueForPropagatingQuestion(Guid questionId);

        IEnumerable<Guid> GetPropagatingQuestionsWhichReferToMissingOrNotPropagatableGroups();

        IEnumerable<Guid> GetParentPropagatableGroupsForQuestionStartingFromTop(Guid questionId);

        IEnumerable<Guid> GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(Guid groupId);

        int GetPropagationLevelForQuestion(Guid questionId);

        int GetPropagationLevelForGroup(Guid groupId);

        IEnumerable<Guid> GetAllMandatoryQuestions();

        IEnumerable<Guid> GetAllQuestionsWithNotEmptyCustomEnablementConditions();

        IEnumerable<Guid> GetAllGroupsWithNotEmptyCustomEnablementConditions();

        bool IsGroupPropagatable(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        IEnumerable<Guid> GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(Guid groupId);

        IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(Guid groupId);
    }
}