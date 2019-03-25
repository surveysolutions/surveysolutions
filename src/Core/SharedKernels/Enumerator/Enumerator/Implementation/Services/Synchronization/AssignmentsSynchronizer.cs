using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
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
            var remoteAssignments = await this.synchronizationService.GetAssignmentsAsync(cancellationToken);
            var localAssignments = this.assignmentsRepository.LoadAll();

            var remoteIds = remoteAssignments.ToLookup(ra => ra.Id);
            var removedAssignments = localAssignments.Where(x => !remoteIds.Contains(x.Id));
            var localIds = localAssignments.Except(removedAssignments).ToLookup(x => x.Id);
            
            var processedAssignmentsCount = 0;
            var transferProgress = progress.AsTransferReport();

            // removing local assignments if needed
            foreach (var assignment in removedAssignments)
            {
                statistics.RemovedAssignmentsCount += 1;
                this.assignmentsRepository.Remove(assignment.Id);
            }

            // adding new, updating quantity for existing
            foreach (var remoteItem in remoteAssignments)
            {
                await this.questionnaireDownloader.DownloadQuestionnaireAsync(remoteItem.QuestionnaireId, statistics,
                    transferProgress, cancellationToken);

                var local = localIds[remoteItem.Id].FirstOrDefault();
                if (local != null)
                    this.UpdateAssignment(local, remoteItem);
                else
                    await this.CreateAssignmentAsync(remoteItem, statistics, cancellationToken);

                processedAssignmentsCount++;
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
        }

        private async Task CreateAssignmentAsync(AssignmentApiView remoteItem, SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            var remoteAssignment = await this.synchronizationService.GetAssignmentAsync(remoteItem.Id, cancellationToken);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(remoteItem.QuestionnaireId, null);

            var localAssignment = this.assignmentDocumentFromDtoBuilder.GetAssignmentDocument(remoteAssignment, questionnaire);
            this.assignmentsRepository.Store(localAssignment);

            await this.synchronizationService.LogAssignmentAsHandledAsync(remoteItem.Id, cancellationToken);

            statistics.NewAssignmentsCount++;
        }

        private void UpdateAssignment(AssignmentDocument local, AssignmentApiView remote)
        {
            if (local.Quantity != remote.Quantity)
            {
                local.Quantity = remote.Quantity;
            }

            if (ReceivedByInterviewerTimeShouldBeReset(remote.ResponsibleId, local.ResponsibleId,
                local.OriginalResponsibleId))
            {
                local.ReceivedByInterviewerAt = null;
            }

            if (local.OriginalResponsibleId != remote.ResponsibleId)
            {
                local.ResponsibleId = remote.ResponsibleId;
                local.OriginalResponsibleId = remote.ResponsibleId;
                local.ResponsibleName = remote.ResponsibleName;
            }

            local.IsAudioRecordingEnabled = remote.IsAudioRecordingEnabled;

            var interviewsCount =
                this.interviewViewRepository.Count(x => x.FromHqSyncDateTime == null && x.Assignment == local.Id);

            local.CreatedInterviewsCount = interviewsCount;
            this.assignmentsRepository.Store(local);
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
