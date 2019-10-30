using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IOptionsRepository
    {
        IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, 
            Guid questionId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null);

        CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionText, int? parentQuestionValue, Guid? translationId);

        CategoricalOption GetQuestionOptionByValue(QuestionnaireIdentity questionnaireId, Guid questionId, 
            decimal optionValue, Guid? translationId);

        CategoricalOption[] GetOptionsByValues(QuestionnaireIdentity questionnaireId, Guid questionId, int[] optionValues, Guid? translationId);

        void RemoveOptionsForQuestionnaire(QuestionnaireIdentity questionnaireId);

        void StoreOptionsForQuestion(QuestionnaireIdentity questionnaireId, Guid questionId, 
            List<Answer> answers, List<TranslationDto> optionsTranslations);

        void StoreOptions(List<OptionView> options);

        bool IsEmpty();
    }
}
