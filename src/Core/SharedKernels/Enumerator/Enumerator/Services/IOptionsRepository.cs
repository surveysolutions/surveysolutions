using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IOptionsRepository
    {
        IReadOnlyList<CategoricalOption> GetQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId);

        IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId,
            int? parentValue, string filter, Guid? translationId);

        CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionText, Guid? translationId);

        CategoricalOption GetQuestionOptionByValue(QuestionnaireIdentity questionnaireId, Guid questionId, decimal optionValue, Guid? translationId);

        Task RemoveOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireId);
        //Task StoreQuestionOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument serializedQuestionnaireDocument);

        Task StoreOptionsForQuestionAsync(QuestionnaireIdentity questionnaireId, Guid questionId, List<Answer> answers, List<TranslationDto> optionsTranslations);

        bool IsEmpty();
    }
}