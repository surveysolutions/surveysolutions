using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDesignerWebTesterApi webTesterApi;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage;

        private static long version;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireStorage,
            IDesignerWebTesterApi webTesterApi,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            ITranslationManagementService translationManagementService,
            IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.webTesterApi = webTesterApi;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.translationManagementService = translationManagementService;
            this.attachmentsStorage = attachmentsStorage;
        }

        public Dictionary<Guid, QuestionnaireIdentity> TokenToQuestionnaireMap { get; } = new Dictionary<Guid, QuestionnaireIdentity>();

        public void RemoveQuestionnaire(Guid designerToken)
        {
            if (!TokenToQuestionnaireMap.ContainsKey(designerToken)) return;

            var questionnaireId = TokenToQuestionnaireMap[designerToken];
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(questionnaireId);

            questionnaireStorage.DeleteQuestionnaireDocument(questionnaireId.QuestionnaireId, questionnaireId.Version);
            translationManagementService.Delete(questionnaireId);
            
            foreach (var attachment in questionnaire.Attachments)
            {
                attachmentsStorage.Remove(attachment.ContentId);
            }

            TokenToQuestionnaireMap.Remove(designerToken);
        }

        public async Task<QuestionnaireIdentity> ImportQuestionnaire(Guid designerToken)
        {
            var questionnaire = await webTesterApi.GetQuestionnaireAsync(designerToken.ToString());

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.Document.PublicKey, Interlocked.Increment(ref version));

            TokenToQuestionnaireMap[designerToken] = questionnaireIdentity;

            var translations = await webTesterApi.GetTranslationsAsync(designerToken.ToString());

            var attachments = new List<QuestionnaireAttachment>();

            foreach (Attachment documentAttachment in questionnaire.Document.Attachments)
            {
                var content = await webTesterApi.GetAttachmentContentAsync(designerToken.ToString(), documentAttachment.ContentId);
                attachments.Add(new QuestionnaireAttachment
                {
                    Id = documentAttachment.AttachmentId,
                    Content = content
                });
            }

            var attachmnetsToStore = attachments.Select(x => Tuple.Create(x, (object)x.Content.Id));
            this.attachmentsStorage.Store(attachmnetsToStore);

            this.appdomainsPerInterviewManager.SetupForInterview(designerToken, questionnaire.Document, questionnaire.Assembly);
            this.questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version,
                questionnaire.Document);

            this.translationManagementService.Delete(questionnaireIdentity);
            this.translationManagementService.Store(translations.Select(x => new TranslationInstance
            {
                QuestionnaireId = questionnaireIdentity,
                Value = x.Value,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                Type = x.Type,
                TranslationIndex = x.TranslationIndex,
                TranslationId = x.TranslationId
            }));

            return questionnaireIdentity;
        }
    }
}