using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
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

        protected QuestionnaireDownloader()
        {
        }

        public QuestionnaireDownloader(IAttachmentContentStorage attachmentContentStorage,
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            ISynchronizationService synchronizationService)
        {
            this.attachmentContentStorage = attachmentContentStorage;
            this.questionnairesAccessor = questionnairesAccessor;
            this.synchronizationService = synchronizationService;
        }

        public virtual async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            if (!this.questionnairesAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaireIdentity,
                    (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    cancellationToken);

                await this.questionnairesAccessor.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }

            if (!this.questionnairesAccessor.IsQuestionnaireExists(questionnaireIdentity))
            {
                var contentIds = await this.synchronizationService.GetAttachmentContentsAsync(questionnaireIdentity,
                    (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    cancellationToken);

                foreach (var contentId in contentIds)
                {
                    var isExistContent = this.attachmentContentStorage.Exists(contentId);
                    if (!isExistContent)
                    {
                        var attachmentContent = await this.synchronizationService.GetAttachmentContentAsync(contentId,
                            (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                            cancellationToken);

                        this.attachmentContentStorage.Store(attachmentContent);
                    }
                }

                var translationDtos = await this.synchronizationService.GetQuestionnaireTranslationAsync(questionnaireIdentity, cancellationToken);

                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaireIdentity,
                    (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    cancellationToken);

                this.questionnairesAccessor.StoreQuestionnaire(questionnaireIdentity,
                    questionnaireApiView.QuestionnaireDocument,
                    questionnaireApiView.AllowCensus, translationDtos);

                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
                statistics.SuccessfullyDownloadedQuestionnairesCount++;
            }
        }
    }
}
