using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class CheckObsoleteQuestionnairesAsync : SynchronizationStep
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAttachmentsCleanupService attachmentsCleanupService;
        private readonly IInterviewsRemover interviewsRemover;

        public CheckObsoleteQuestionnairesAsync(ISynchronizationService synchronizationService, IInterviewerQuestionnaireAccessor questionnairesAccessor, IPlainStorage<InterviewView> interviewViewRepository,
            IAttachmentsCleanupService attachmentsCleanupService, IInterviewsRemover interviewsRemover,
            ILogger logger,
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.synchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
            this.questionnairesAccessor = questionnairesAccessor ?? throw new ArgumentNullException(nameof(questionnairesAccessor));
            this.interviewViewRepository = interviewViewRepository ?? throw new ArgumentNullException(nameof(interviewViewRepository));
            this.attachmentsCleanupService = attachmentsCleanupService ?? throw new ArgumentNullException(nameof(attachmentsCleanupService));
            this.interviewsRemover = interviewsRemover ?? throw new ArgumentNullException(nameof(interviewsRemover));
        }

        public override async Task ExecuteAsync()
        {
            this.Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                Statistics = this.Context.Statistics,
                Status = SynchronizationStatus.Download
            });

            var serverQuestionnaires = await this.synchronizationService.GetServerQuestionnairesAsync(this.Context.CancellationToken);
            var localQuestionnaires = this.questionnairesAccessor.GetAllQuestionnaireIdentities();

            var questionnairesToRemove = localQuestionnaires.Except(serverQuestionnaires).ToList();

            var removedQuestionnairesCounter = 0;
            foreach (var questionnaireIdentity in questionnairesToRemove)
            {
                this.Context.CancellationToken.ThrowIfCancellationRequested();
                removedQuestionnairesCounter++;

                this.Context.Progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                    Description = string.Format(
                        InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires_Description,
                        removedQuestionnairesCounter, questionnairesToRemove.Count),
                    Statistics = this.Context.Statistics,
                    Status = SynchronizationStatus.Download
                });

                var questionnaireId = questionnaireIdentity.ToString();

                var removedInterviews = this.interviewViewRepository
                    .Where(interview => interview.QuestionnaireId == questionnaireId)
                    .Select(interview => interview.InterviewId)
                    .ToList();
                this.interviewsRemover.RemoveInterviews(removedInterviews, this.Context.Statistics, this.Context.Progress);

                this.questionnairesAccessor.RemoveQuestionnaire(questionnaireIdentity);
            }

            if (questionnairesToRemove.Count > 0)
            {
                this.Context.Progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_AttachmentsCleanup
                });

                this.attachmentsCleanupService.RemovedOrphanedAttachments();
            }
        }
    }
}
