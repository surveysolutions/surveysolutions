using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public class QuestionnaireDownloader : IQuestionnaireDownloader
    {
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;

        protected QuestionnaireDownloader()
        {
        }

        public QuestionnaireDownloader(IAttachmentContentStorage attachmentContentStorage,
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            ISynchronizationService synchronizationService,
            ILogger logger)
        {
            this.attachmentContentStorage = attachmentContentStorage;
            this.questionnairesAccessor = questionnairesAccessor;
            this.synchronizationService = synchronizationService;
            this.logger = logger;
        }

        public virtual async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity,
            SynchronizationStatistics statistics, IProgress<TransferProgress> transferProgress,
            CancellationToken cancellationToken)
        {
            this.logger.Trace($"Loading of questionnaire requested {questionnaireIdentity}");
            if (!this.questionnairesAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaireIdentity,
                    transferProgress,
                    cancellationToken);

                await this.questionnairesAccessor.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }

            if (!this.questionnairesAccessor.IsQuestionnaireExists(questionnaireIdentity))
            {
                var contentIds = await this.synchronizationService.GetAttachmentContentsAsync(questionnaireIdentity,
                    transferProgress,
                    cancellationToken);

                foreach (var contentId in contentIds)
                {
                    var isExistContent = await this.attachmentContentStorage.ExistsAsync(contentId);
                    if (!isExistContent)
                    {
                        var attachmentContent = await this.synchronizationService.GetAttachmentContentAsync(contentId,
                            transferProgress,
                            cancellationToken);

                        await this.attachmentContentStorage.StoreAsync(attachmentContent);
                    }
                }

                var translationDtos = await this.synchronizationService.GetQuestionnaireTranslationAsync(questionnaireIdentity, cancellationToken);
                this.logger.Debug($"Get translations for questionnaire {questionnaireIdentity}. {translationDtos.Count} records.");

                var reusableCategories = await this.synchronizationService.GetQuestionnaireReusableCategoriesAsync(questionnaireIdentity, cancellationToken);
                this.logger.Debug($"Get categories for questionnaire {questionnaireIdentity}. {reusableCategories.Count} categories with {reusableCategories.Sum(c => c.Options.Count)} sum of items.");

                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(questionnaireIdentity, transferProgress, cancellationToken);

                this.questionnairesAccessor.StoreQuestionnaire(questionnaireIdentity,
                    questionnaireApiView.QuestionnaireDocument,
                    questionnaireApiView.AllowCensus, 
                    translationDtos,
                    reusableCategories);

                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
                statistics.SuccessfullyDownloadedQuestionnairesCount++;
            }
        }
    }
}
