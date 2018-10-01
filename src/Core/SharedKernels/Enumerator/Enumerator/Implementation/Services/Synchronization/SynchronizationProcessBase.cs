using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class SynchronizationProcessBase : AbstractSynchronizationProcess, ISynchronizationProcess
    {
        private readonly IHttpStatistician httpStatistician;
        private readonly IAssignmentDocumentsStorage assignmentsStorage;
        protected readonly IAuditLogSynchronizer auditLogSynchronizer;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        protected readonly IAssignmentsSynchronizer assignmentsSynchronizer;
        protected readonly ISynchronizationService synchronizationService;

        protected SynchronizationProcessBase(ISynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IAssignmentsSynchronizer assignmentsSynchronizer,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings) : base(synchronizationService, logger,
            httpStatistician, userInteractionService, principal,  
            interviewViewRepository, auditLogService, enumeratorSettings)
        {
            this.synchronizationService = synchronizationService;
            this.interviewViewRepository = interviewViewRepository;
            this.assignmentsSynchronizer = assignmentsSynchronizer;
            this.httpStatistician = httpStatistician;
            this.assignmentsStorage = assignmentsStorage;
            this.auditLogSynchronizer = auditLogSynchronizer;
        }

        
        protected override bool SendStatistics => true;
        protected override string SuccessDescription => InterviewerUIResources.Synchronization_Success_Description;

        public override SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics, Stopwatch stopwatch)
        {
            var httpStats = this.httpStatistician.GetStats();

            return new SyncStatisticsApiView
            {
                DownloadedInterviewsCount = statistics.NewInterviewsCount,
                UploadedInterviewsCount = statistics.SuccessfullyUploadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.SuccessfullyDownloadedQuestionnairesCount,
                RejectedInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.RejectedBySupervisor),
                NewInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.InterviewerAssigned && !inteview.CanBeDeleted),
                RemovedInterviewsCount = statistics.DeletedInterviewsCount,

                NewAssignmentsCount = statistics.NewAssignmentsCount,
                RemovedAssignmentsCount = statistics.RemovedAssignmentsCount,
                AssignmentsOnDeviceCount = this.assignmentsStorage.Count(),

                TotalDownloadedBytes = httpStats.DownloadedBytes,
                TotalUploadedBytes = httpStats.UploadedBytes,
                TotalConnectionSpeed = httpStats.Speed,
                TotalSyncDuration = stopwatch.Elapsed
            };
        }
    }
}
