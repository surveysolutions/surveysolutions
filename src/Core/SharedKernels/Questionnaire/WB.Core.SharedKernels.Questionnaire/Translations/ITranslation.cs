using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslation
    {
        string? GetTitle(Guid entityId);
        string? GetInstruction(Guid questionId);
        string? GetAnswerOption(Guid questionId, string? answerOptionValue, string? answerParentValue);
        string? GetSpecialValue(Guid questionId, string? specialValue);
        string? GetCriticalRuleMessage(Guid criticalityConditionId);
        
        string? GetValidationMessage(Guid entityId, int validationOneBasedIndex);
        string? GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue);
        bool IsEmpty();

        string? GetCategoriesText(Guid categoriesId, int id, int? parentId);
    }
}
