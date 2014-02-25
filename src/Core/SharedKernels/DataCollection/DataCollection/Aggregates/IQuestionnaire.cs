﻿using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }

        [Obsolete("This method is for import service only and should be removed at all.")]
        IQuestion GetQuestionByStataCaption(string stataCaption);

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        bool IsQuestionLinked(Guid questionId);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetGroupTitle(Guid groupId);

        IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId);

        string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue);

        int? GetMaxSelectedAnswerOptions(Guid questionId);

        bool IsCustomValidationDefined(Guid questionId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomValidation(Guid questionId);

        string GetCustomValidationExpression(Guid questionId);

        IEnumerable<Guid> GetAllQuestionsWithNotEmptyValidationExpressions();

        IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid groupId);

        IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId);

        IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId);

        bool ShouldQuestionSpecifyRosterSize(Guid questionId);

        IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId);

        int? GetMaxValueForNumericQuestion(Guid questionId);

        int? GetListSizeForListQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId);

        IEnumerable<Guid> GetFixedRosterGroups();

        int GetRosterLevelForQuestion(Guid questionId);

        int GetRosterLevelForGroup(Guid groupId);

        IEnumerable<Guid> GetAllMandatoryQuestions();

        IEnumerable<Guid> GetAllQuestionsWithNotEmptyCustomEnablementConditions();

        IEnumerable<Guid> GetAllGroupsWithNotEmptyCustomEnablementConditions();

        bool IsRosterGroup(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        IEnumerable<Guid> GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(Guid groupId);

        IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(Guid groupId);

        IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(Guid groupId);
        
        IEnumerable<Guid> GetUnderlyingMandatoryQuestions(Guid groupId);

        Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId);
        
        bool IsQuestionMandatory(Guid questionId);

        bool IsQuestionInteger(Guid questionId);

        int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId);

        IEnumerable<string> GetFixedRosterTitles(Guid groupId);

        bool DoesQuestionSpecifyRosterTitle(Guid questionId);

        IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId);

        void InitializeQuestionnaireDocument(QuestionnaireDocument source);
    }
}