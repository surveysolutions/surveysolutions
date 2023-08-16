using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDesignerWebTesterApi webTesterApi;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly ITranslationManagementService translationManagementService;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage;
        private readonly ICacheStorage<QuestionnaireAttachment, string> attachmentsStorage;
        private readonly IReusableCategoriesStorage categoriesManagementService;

        private static long version;

        public QuestionnaireImportService(
            IQuestionnaireStorage questionnaireStorage,
            IDesignerWebTesterApi webTesterApi,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            ITranslationManagementService translationManagementService,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage,
            ICacheStorage<QuestionnaireAttachment, string> attachmentsStorage,
            IReusableCategoriesStorage categoriesManagementService)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.webTesterApi = webTesterApi ?? throw new ArgumentNullException(nameof(webTesterApi));
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager ?? throw new ArgumentNullException(nameof(appdomainsPerInterviewManager));
            this.translationManagementService = translationManagementService ?? throw new ArgumentNullException(nameof(translationManagementService));
            this.questionnaireDocumentStorage = questionnaireDocumentStorage ?? throw new ArgumentNullException(nameof(questionnaireDocumentStorage));
            this.attachmentsStorage = attachmentsStorage ?? throw new ArgumentNullException(nameof(attachmentsStorage));
            this.categoriesManagementService = categoriesManagementService ?? throw new ArgumentNullException(nameof(categoriesManagementService));
        }

        public Dictionary<Guid, QuestionnaireIdentity> TokenToQuestionnaireMap { get; } = new Dictionary<Guid, QuestionnaireIdentity>();

        public void RemoveQuestionnaire(Guid designerToken)
        {
            lock (TokenToQuestionnaireMap)
            {
                if (!TokenToQuestionnaireMap.ContainsKey(designerToken)) return;

                var questionnaireId = TokenToQuestionnaireMap[designerToken];

                questionnaireStorage.DeleteQuestionnaireDocument(questionnaireId.QuestionnaireId, questionnaireId.Version);
                questionnaireDocumentStorage.Remove(questionnaireId.ToString());
                translationManagementService.Delete(questionnaireId);

                attachmentsStorage.RemoveArea(designerToken);
                TokenToQuestionnaireMap.Remove(designerToken);
            }
        }

        public async Task<QuestionnaireIdentity> ImportQuestionnaire(Guid designerToken)
        {
            var questionnaire = await webTesterApi.GetQuestionnaireAsync(designerToken.ToString());

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.Document.PublicKey, Interlocked.Increment(ref version));

            var translations = await webTesterApi.GetTranslationsAsync(designerToken.ToString());

            var attachments = new List<QuestionnaireAttachment>();

            foreach (Attachment documentAttachment in questionnaire.Document.Attachments)
            {
                var content = await webTesterApi.GetAttachmentContentAsync(designerToken.ToString(), documentAttachment.ContentId);
                
                attachments.Add(new QuestionnaireAttachment(documentAttachment.AttachmentId, content));
            }

            var categories = await webTesterApi.GetCategoriesAsync(designerToken.ToString());

            lock (TokenToQuestionnaireMap)
            {
                TokenToQuestionnaireMap[designerToken] = questionnaireIdentity;

                foreach (var attachment in attachments)
                {
                    this.attachmentsStorage.Store(attachment, attachment.Content.Id, designerToken);
                }
                
                this.categoriesManagementService.RemoveCategories(questionnaireIdentity);
                categories.GroupBy(x => x.CategoriesId).ForEach(x =>
                {
                    this.categoriesManagementService.Store(questionnaireIdentity, x.Key, x.Select(x => new CategoriesItem
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        Text = x.Text,
                        AttachmentName = x.AttachmentName,
                    }).ToList());
                });

                this.questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version,
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
            }
            
            var settings = await webTesterApi.GetQuestionnaireSettingsAsync(designerToken.ToString());

            this.appdomainsPerInterviewManager.SetupForInterview(designerToken,
                questionnaireIdentity,
                questionnaire.Assembly,
                settings);

            return questionnaireIdentity;
        }
    }
}
