using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IInterviewerQuestionnaireAccessor
    {
        void StoreQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument,
            bool census, List<TranslationDto> translationDtos, List<ReusableCategoriesDto> reusableCategoriesDtos);

        void RemoveQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly);

        QuestionnaireDocument GetQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        bool IsQuestionnaireExists(QuestionnaireIdentity questionnaireIdentity);

        bool IsQuestionnaireAssemblyExists(QuestionnaireIdentity questionnaireIdentity);

        List<QuestionnaireIdentity> GetAllQuestionnaireIdentities();

        IReadOnlyCollection<QuestionnaireDocumentView> LoadAll();

        void StoreTranslations(QuestionnaireIdentity questionnaireIdentity, List<TranslationInstance> translationInstances);
    }
}
