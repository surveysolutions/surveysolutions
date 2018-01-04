using System.Linq;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly ITranslationManagementService translationManagementService;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor,
            ITranslationManagementService translationManagementService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.translationManagementService = translationManagementService;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations)
        {
            translationManagementService.Delete(questionnaireIdentity);
            translationManagementService.Store(translations.Select(x => new TranslationInstance
            {
                QuestionnaireId = questionnaireIdentity,
                Value = x.Value,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                Type = x.Type,
                TranslationIndex = x.TranslationIndex,
                TranslationId = x.TranslationId
            }));

            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }

    public interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations);
    }
}
