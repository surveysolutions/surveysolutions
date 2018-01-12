using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
        }

        public void ImportQuestionnaire(Guid interviewId, QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations,
            List<QuestionnaireAttachment> attachments)
        {
            //translationManagementService.Delete(questionnaireIdentity);
            //translationManagementService.Store(translations.Select(x => new TranslationInstance
            //{
            //    QuestionnaireId = questionnaireIdentity,
            //    Value = x.Value,
            //    QuestionnaireEntityId = x.QuestionnaireEntityId,
            //    Type = x.Type,
            //    TranslationIndex = x.TranslationIndex,
            //    TranslationId = x.TranslationId
            //}));
            appdomainsPerInterviewManager.SetupForInterview(interviewId, questionnaireDocument, supportingAssembly);
            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            //this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);

            //var attachmnetsToStore = attachments.Select(x => Tuple.Create(x, (object)x.Content.Id));
            //this.attachmentsStorage.Store(attachmnetsToStore);
        }
    }
}
