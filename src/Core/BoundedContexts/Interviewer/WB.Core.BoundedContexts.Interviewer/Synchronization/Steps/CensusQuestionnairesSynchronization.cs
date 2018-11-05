using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization.Steps
{
    public class CensusQuestionnairesSynchronization : SynchronizationStep
    {
        private readonly IInterviewerSynchronizationService interviewerSynchronizationService;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IQuestionnaireDownloader questionnaireDownloader;

        public CensusQuestionnairesSynchronization(
            IInterviewerSynchronizationService synchronizationService, 
            IInterviewerQuestionnaireAccessor questionnairesAccessor, 
            IQuestionnaireDownloader questionnaireDownloader, 
            ILogger logger,
            int sortOrder) : base (sortOrder, synchronizationService, logger)
        {
            this.interviewerSynchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
            this.questionnairesAccessor = questionnairesAccessor ?? throw new ArgumentNullException(nameof(questionnairesAccessor));
            this.questionnaireDownloader = questionnaireDownloader ?? throw new ArgumentNullException(nameof(questionnaireDownloader));
        }

        public override async Task ExecuteAsync()
        {
            var remoteCensusQuestionnaireIdentities =
                await this.interviewerSynchronizationService.GetCensusQuestionnairesAsync(this.Context.CancellationToken);
            var localCensusQuestionnaireIdentities = this.questionnairesAccessor.GetCensusQuestionnaireIdentities();

            var processedQuestionnaires = 0;
            var notExistingLocalCensusQuestionnaireIdentities = remoteCensusQuestionnaireIdentities
                .Except(localCensusQuestionnaireIdentities).ToList();

            var transferProgress = this.Context.Progress.AsTransferReport();

            foreach (var censusQuestionnaireIdentity in notExistingLocalCensusQuestionnaireIdentities)
            {
                this.Context.CancellationToken.ThrowIfCancellationRequested();
                this.Context.Progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        processedQuestionnaires,
                        notExistingLocalCensusQuestionnaireIdentities.Count,
                        InterviewerUIResources.Synchronization_Questionnaires),
                    Stage = SyncStage.UpdatingQuestionnaires,
                    StageExtraInfo = new Dictionary<string, string>()
                    {
                        { "processedCount", processedQuestionnaires.ToString() },
                        { "totalCount", notExistingLocalCensusQuestionnaireIdentities.Count.ToString()}
                    }
                });

                await this.questionnaireDownloader.DownloadQuestionnaireAsync(censusQuestionnaireIdentity,
                    this.Context.Statistics, transferProgress, this.Context.CancellationToken);

                processedQuestionnaires++;
            }
        }
    }
}
