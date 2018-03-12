using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslation
    {
        string GetTitle(Guid entityId);
        string GetInstruction(Guid questionId);
        string GetAnswerOption(Guid questionId, string answerOptionValue);
        string GetSpecialValue(Guid questionId, string specialValue);
        
        string GetValidationMessage(Guid entityId, int validationOneBasedIndex);
        string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue);
        bool IsEmpty();
    }
}