using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class TranslationsQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly IDesignerApi designerApi;
        private readonly ITranslationManagementService translationManagementService;
        private readonly ILogger logger;
        private List<TranslationDto> translationContent;

        public TranslationsQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, IDesignerApi designerApi, ITranslationManagementService translationManagementService, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.designerApi = designerApi;
            this.translationManagementService = translationManagementService;
            this.logger = logger;
        }
        public bool IsNeedProcessing()
        {
            return questionnaire.Translations?.Count > 0;
        }

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            this.logger.Debug($"loading translations {questionnaireIdentity.Id}");
            if (questionnaire.Translations?.Count != 0)
            {
                translationContent = await designerApi.GetTranslations(questionnaire.PublicKey);
            }

            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            this.logger.Debug($"save translations {questionnaireIdentity.Id}");

            translationManagementService.Delete(questionnaireIdentity);

            if (translationContent != null && translationContent.Count > 0)
            {
                translationManagementService.Store(translationContent.Select(x => new TranslationInstance
                {
                    QuestionnaireId = questionnaireIdentity,
                    Value = x.Value,
                    QuestionnaireEntityId = x.QuestionnaireEntityId,
                    Type = x.Type,
                    TranslationIndex = x.TranslationIndex,
                    TranslationId = x.TranslationId
                }));
            }
            progress.Report(100);
        }
    }
}
