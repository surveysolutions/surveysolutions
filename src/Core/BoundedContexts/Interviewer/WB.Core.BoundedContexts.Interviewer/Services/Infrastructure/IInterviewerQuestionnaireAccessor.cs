using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerQuestionnaireAccessor
    {
        void StoreQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument,
            bool census, List<TranslationDto> translationDtos);

        Task RemoveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity);

        Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly);

        List<QuestionnaireIdentity> GetCensusQuestionnaireIdentities();

        bool IsQuestionnaireExists(QuestionnaireIdentity questionnaireIdentity);

        bool IsQuestionnaireAssemblyExists(QuestionnaireIdentity questionnaireIdentity);

        List<QuestionnaireIdentity> GetAllQuestionnaireIdentities();

        bool IsAttachmentUsedAsync(string contentId);

        void StoreTranslations(QuestionnaireIdentity questionnaireIdentity, List<TranslationInstance> translationInstances);
    }
}