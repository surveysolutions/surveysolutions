using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public class AssignmentsSynchronizer : IAssignmentsSynchronizer
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAssignmentDocumentFromDtoBuilder assignmentDocumentFromDtoBuilder;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IQuestionnaireDownloader questionnaireDownloader,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentDocumentFromDtoBuilder assignmentDocumentFromDtoBuilder,
            IPlainStorage<InterviewView> interviewViewRepository, 
            IPlainStorage<InterviewerDocument> interviewerViewRepository)
        {
            this.synchronizationService = synchronizationService;
            this.assignmentsRepository = assignmentsRepository;
            this.questionnaireDownloader = questionnaireDownloader;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentDocumentFromDtoBuilder = assignmentDocumentFromDtoBuilder;
            this.interviewViewRepository = interviewViewRepository;
            this.interviewerViewRepository = interviewerViewRepository;
        }

        public virtual async Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            List<AssignmentApiView> remoteAssignments =
                await this.synchronizationService.GetAssignmentsAsync(cancellationToken);

            IReadOnlyCollection<AssignmentDocument> localAssignments = this.assignmentsRepository.LoadAll();

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(0,
                    remoteAssignments.Count),
                Statistics = statistics,
                Status = SynchronizationStatus.Download,
                Stage = SyncStage.AssignmentsSynchronization,
                StageExtraInfo = new Dictionary<string, string>()
                {
                    { "processedCount", "0" },
                    { "totalCount", remoteAssignments.Count.ToString()}
                }
            });

            // removing local assignments if needed
            var remoteIds = remoteAssignments.ToLookup(ra => ra.Id);

            foreach (var assignment in localAssignments)
            {
                if (remoteIds.Contains(assignment.Id)) continue;

                statistics.RemovedAssignmentsCount += 1;
                this.assignmentsRepository.Remove(assignment.Id);
            }

            // adding new, updating quantity for existing
            var localAssignmentsLookup = this.assignmentsRepository.LoadAll().ToLookup(la => la.Id);
            var processedAssignmentsCount = 0;

            IProgress<TransferProgress> transferProgress = progress.AsTransferReport();

            foreach (var remoteItem in remoteAssignments)
            {
                await this.questionnaireDownloader.DownloadQuestionnaireAsync(remoteItem.QuestionnaireId, statistics,
                    transferProgress, cancellationToken);

                IQuestionnaire questionnaire =
                    this.questionnaireStorage.GetQuestionnaire(remoteItem.QuestionnaireId, null);

                var local = localAssignmentsLookup[remoteItem.Id].FirstOrDefault();
                if (local == null)
                {
                    AssignmentApiDocument remote =
                        await this.synchronizationService.GetAssignmentAsync(remoteItem.Id, cancellationToken);

                    local = this.assignmentDocumentFromDtoBuilder.GetAssignmentDocument(remote, questionnaire);

                    statistics.NewAssignmentsCount++;

                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(
                            processedAssignmentsCount, remoteAssignments.Count),
                        Statistics = statistics,
                        Status = SynchronizationStatus.Download,
                        Stage = SyncStage.AssignmentsSynchronization,
                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", processedAssignmentsCount.ToString() },
                            { "totalCount", remoteAssignments.Count.ToString()}
                        }
                    });

                    this.assignmentsRepository.Store(local);
                }
                else
                {
                    if (local.Quantity != remoteItem.Quantity)
                    {
                        local.Quantity = remoteItem.Quantity;
                    }

                    if (ReceivedByInterviewerTimeShouldBeReset(remoteItem.ResponsibleId, local.ResponsibleId,
                        local.OriginalResponsibleId))
                    {
                        local.ReceivedByInterviewerAt = null;
                    }

                    if (local.OriginalResponsibleId != remoteItem.ResponsibleId)
                    {
                        local.ResponsibleId = remoteItem.ResponsibleId;
                        local.OriginalResponsibleId = remoteItem.ResponsibleId;
                        local.ResponsibleName = remoteItem.ResponsibleName;
                    }

                    var interviewsCount = this.interviewViewRepository.Count(x => x.FromHqSyncDateTime == null && x.Assignment == local.Id);
                     
                    local.CreatedInterviewsCount = interviewsCount;
                    this.assignmentsRepository.Store(local);
                }

                await this.synchronizationService.LogAssignmentAsHandledAsync(local.Id, cancellationToken);

                processedAssignmentsCount++;
            }

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_AssignmentsFormat.FormatString(
                    processedAssignmentsCount, remoteAssignments.Count),
                Statistics = statistics,
                Status = SynchronizationStatus.Download,
                Stage = SyncStage.AssignmentsSynchronization,
                StageExtraInfo = new Dictionary<string, string>()
                {
                    { "processedCount", processedAssignmentsCount.ToString() },
                    { "totalCount", remoteAssignments.Count.ToString()}
                }
            });
        }

        public bool ReceivedByInterviewerTimeShouldBeReset(Guid remoteResponsibleId, Guid localResponsibleId,
            Guid localOriginalResponsibleId)
        {
            if (remoteResponsibleId == localResponsibleId)
                return false;

            var hqChangedResponsible = localOriginalResponsibleId != remoteResponsibleId;
            if (hqChangedResponsible)
            {
                // Supervisor's decision was overriden
                return true;
            }

            // hopefully users are synched
            var interviewerDocument = this.interviewerViewRepository.GetById(remoteResponsibleId.FormatGuid());
            var remoteUserIsStillInterviewer = interviewerDocument != null;
            return remoteUserIsStillInterviewer;
        }
    }
}
