using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor,
            ITranslationManagementService translationManagementService,
            IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.translationManagementService = translationManagementService;
            this.attachmentsStorage = attachmentsStorage;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations,
            List<QuestionnaireAttachment> attachments)
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

            var attachmnetsToStore = attachments.Select(x => Tuple.Create(x, (object)x.Content.Id));
            this.attachmentsStorage.Store(attachmnetsToStore);
        }
    }
}
