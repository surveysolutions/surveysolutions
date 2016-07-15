using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslation
    {
        string GetTitle(Guid entityId);
        string GetInstruction(Guid questionId);
        string GetAnswerOption(Guid questionId, string answerOptionValue);
        string GetValidationMessage(Guid entityId, int validationOneBasedIndex);
        string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue);
        bool IsEmpty();
    }
}