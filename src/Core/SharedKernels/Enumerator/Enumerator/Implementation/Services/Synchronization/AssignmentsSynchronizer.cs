using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;

        public AssignmentsSynchronizer(ISynchronizationService synchronizationService,
            IAssignmentDocumentsStorage assignmentsRepository,
            IQuestionnaireDownloader questionnaireDownloader,
            IQuestionnaireStorage questionnaireStorage,
            IAssignmentDocumentFromDtoBuilder assignmentDocumentFromDtoBuilder,
            IPlainStorage<InterviewView> interviewViewRepository, 
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            ILogger logger)
        {
            this.synchronizationService = synchronizationService;
            this.assignmentsRepository = assignmentsRepository;
            this.questionnaireDownloader = questionnaireDownloader;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentDocumentFromDtoBuilder = assignmentDocumentFromDtoBuilder;
            this.interviewViewRepository = interviewViewRepository;
            this.interviewerViewRepository = interviewerViewRepository;
            this.logger = logger;
        }

        public virtual async Task SynchronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            var remoteAssignments = await this.synchronizationService.GetAssignmentsAsync(cancellationToken);
            var localAssignments = this.assignmentsRepository.LoadAll();

            var remoteIds = remoteAssignments.Select(ra => ra.Id).ToHashSet();
            var removedIds = localAssignments.Select(x => x.Id).Where(x => !remoteIds.Contains(x)).ToArray();
            var localIds = localAssignments.Where(x => !removedIds.Contains(x.Id)).ToLookup(x => x.Id);
            
            var processedAssignmentsCount = 0;
            var transferProgress = progress.AsTransferReport();

            // removing local assignments if needed
            this.assignmentsRepository.Remove(removedIds);
            if (removedIds.Length > 0)
            {
                this.logger.Debug($"Removed {string.Join(",", removedIds)}");
            }
            statistics.RemovedAssignmentsCount += removedIds.Length;

            // download questionnaires
            foreach (var questionnaireIdentity in remoteAssignments.Select(x => x.QuestionnaireId).Distinct())
            {
                await this.questionnaireDownloader.DownloadQuestionnaireAsync(questionnaireIdentity, statistics,
                    transferProgress, cancellationToken);
            }

            // adding new, updating quantity for existing
            foreach (var remoteItem in remoteAssignments)
            {
                var local = localIds[remoteItem.Id].FirstOrDefault();
                if (local != null)
                    this.UpdateAssignment(local, remoteItem);
                else
                    await this.CreateAssignmentAsync(remoteItem, statistics, cancellationToken);

                processedAssignmentsCount++;
                progress.Report(new SyncProgressInfo
                {
                    Title = EnumeratorUIResources.Synchronization_Of_AssignmentsFormat.FormatString(
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

            var newAssignment = this.assignmentDocumentFromDtoBuilder.GetAssignmentDocument(remoteAssignment, questionnaire);
            this.assignmentsRepository.Store(newAssignment);

            await this.synchronizationService.LogAssignmentAsHandledAsync(remoteItem.Id, cancellationToken);

            statistics.NewAssignmentsCount++;
        }

        private void UpdateAssignment(AssignmentDocument local, AssignmentApiView remote)
        {
            if (local.Quantity != remote.Quantity)
            {
                this.logger.Debug($"Updating Quantity for assignment {local.Id} local: {local.Quantity} remote: {remote.Quantity}");
                local.Quantity = remote.Quantity;
            }
            
            if (local.TargetArea != remote.TargetArea)
            {
                this.logger.Debug($"Updating TargetArea for assignment {local.Id} local: {local.TargetArea} remote: {remote.TargetArea}");
                local.TargetArea = remote.TargetArea;
            }
           
            if (ReceivedByInterviewerTimeShouldBeReset(remote.ResponsibleId, local.ResponsibleId,
                local.OriginalResponsibleId))
            {
                this.logger.Debug($"Resetting ReceivedByInterviewerAt for assignment {local.Id}");
                local.ReceivedByInterviewerAt = null;
            }

            if (local.OriginalResponsibleId != remote.ResponsibleId)
            {
                this.logger.Debug($"Changing responsible assignment {local.Id}. local responsible: {local.ResponsibleId}, remote responsible: {remote.ResponsibleId}");

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
