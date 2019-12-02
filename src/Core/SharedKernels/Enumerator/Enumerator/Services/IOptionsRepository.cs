using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IOptionsRepository
    {
        IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, 
            Guid questionId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null);

        IEnumerable<CategoricalOption> GetFilteredCategoriesOptions(QuestionnaireIdentity questionnaireId, 
            Guid categoryId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null);

        CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionText, int? parentQuestionValue, Guid? translationId);

        CategoricalOption GetQuestionOptionByValue(QuestionnaireIdentity questionnaireId, Guid questionId, 
            decimal optionValue, Guid? translationId);

        CategoricalOption[] GetOptionsByValues(QuestionnaireIdentity questionnaireId, Guid questionId, int[] optionValues, Guid? translationId);

        void RemoveOptionsForQuestionnaire(QuestionnaireIdentity questionnaireId);

        void StoreOptionsForQuestion(QuestionnaireIdentity questionnaireId, Guid questionId, List<Answer> answers, List<TranslationDto> optionsTranslations);

        void StoreOptionsForCategory(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoriesItem> options, List<TranslationDto> translations);

        void StoreOptions(List<OptionView> options);

        bool IsEmpty();

        CategoricalOption[] GetReusableCategoriesById(QuestionnaireIdentity questionnaireIdentity, Guid categoryId);
        CategoricalOption GetCategoryOption(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, string optionText, int? parentQuestionValue, Guid? translationId);
        CategoricalOption GetCategoryOptionByValue(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, decimal optionValue, Guid? translationId);
        CategoricalOption[] GetCategoryOptionsByValues(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, int[] optionsValues, Guid? translationId);
    }
}
