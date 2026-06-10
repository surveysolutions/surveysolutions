using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
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

        public async Task UploadLocalStatusChangesAsync(CancellationToken cancellationToken)
        {
            var localAssignments = this.assignmentsRepository.LoadAll();
            // Only upload assignments where a local status change is pending (tracked by StatusChangedAtUtc)
            var pendingChanges = localAssignments
                .Where(a => a.StatusChangedAtUtc.HasValue)
                .ToList();

            foreach (var local in pendingChanges)
            {
                var change = new AssignmentStatusChangeApiView
                {
                    Status = local.Status,
                    Comment = local.StatusComment
                };
                try
                {
                    await this.synchronizationService.ChangeAssignmentStatusAsync(local.Id, change, cancellationToken);
                    this.logger.Debug($"Uploaded status change for assignment {local.Id}: {local.Status}");
                    // Clear the pending-upload flag now that the upload succeeded
                    local.StatusChangedAtUtc = null;
                    this.assignmentsRepository.Store(local);
                }
                catch (SynchronizationException ex) when (
                    ex.Type == SynchronizationExceptionType.InvalidUrl ||  // HTTP 400/404/302/405 (server rejected transition) or transport URI error
                    ex.Type == SynchronizationExceptionType.Unauthorized)  // HTTP 401 (transient session) or HTTP 403 (permanent role denial)
                {
                    // Guard 1: transport-level URL misconfiguration — inner RestException.Type is
                    // RestExceptionType.InvalidUrl (not an HTTP response at all). The device cannot
                    // reach the server; do not discard the pending change. Re-throw so the sync
                    // fails visibly and the upload is retried on the next sync.
                    if (ex.Type == SynchronizationExceptionType.InvalidUrl
                        && ex.InnerException is RestException { Type: RestExceptionType.InvalidUrl })
                    {
                        throw;
                    }

                    // Guard 2: HTTP 401 Unauthorized — device already authenticated before this upload
                    // step ran, so a 401 here is a transient server-side auth failure (session expiry,
                    // load-balancer quirk). Preserve the pending flag and re-throw so the sync fails
                    // and the upload is retried on the next sync.
                    if (ex.Type == SynchronizationExceptionType.Unauthorized
                        && ex.InnerException is RestException { StatusCode: System.Net.HttpStatusCode.Unauthorized })
                    {
                        throw;
                    }

                    // Remaining cases are permanent business rejections:
                    //   - HTTP 400/404/302/405 → server refused the status transition (conflict,
                    //     invalid transition, assignment no longer exists for this interviewer).
                    //   - HTTP 403 → operation not permitted for this role/policy.
                    // Clear the pending flag so the assignment is not re-uploaded endlessly.
                    // The download phase will apply the server's authoritative state (or remove
                    // the assignment if the server no longer returns it in the list).
                    this.logger.Warn($"Status change upload skipped for assignment {local.Id} ({local.Status}): {ex.Message}. Server state will be applied on download.");
                    local.StatusChangedAtUtc = null;
                    this.assignmentsRepository.Store(local);
                }
                // Network-level errors (HostUnreachable, NoNetwork, RequestByTimeout, etc.) are NOT caught
                // here — they propagate and abort the sync so the upload is retried on the next sync.
            }
        }

        private async Task CreateAssignmentAsync(AssignmentApiView remoteItem, SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            var remoteAssignment = await this.synchronizationService.GetAssignmentAsync(remoteItem.Id, cancellationToken);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(remoteItem.QuestionnaireId, null);

            var newAssignment = this.assignmentDocumentFromDtoBuilder.GetAssignmentDocument(remoteAssignment, questionnaire);
            newAssignment.Status = remoteItem.Status;
            newAssignment.StatusComment = remoteItem.StatusComment;
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

            // Server status always overrides local status (server is authoritative).
            // Local status changes are best-effort: if the upload failed, the server's status
            // returned in the next sync will be applied here.
            // StatusComment and the pending-upload flag are only updated when the server
            // actually reports a different status — if status hasn't changed the local pending
            // change should be preserved so it is retried on the next sync.
            if (local.Status != remote.Status)
            {
                this.logger.Debug($"Updating Status for assignment {local.Id}: local {local.Status} → server {remote.Status}");
                local.Status = remote.Status;
                local.StatusComment = remote.StatusComment;
                local.StatusChangedAtUtc = null;
            }
            else if (local.StatusChangedAtUtc == null && local.StatusComment != remote.StatusComment)
            {
                // Status is unchanged, no pending local change to preserve; server comment is authoritative.
                local.StatusComment = remote.StatusComment;
            }

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
