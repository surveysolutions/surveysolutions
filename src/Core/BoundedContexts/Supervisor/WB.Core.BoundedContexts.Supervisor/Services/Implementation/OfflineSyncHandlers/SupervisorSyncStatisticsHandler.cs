using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorSyncStatisticsHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<InterviewerSyncStatisticsView, int?> statisticsStorage;
        private readonly ISynchronizationCompleteSource synchronizationCompleteSource;
        private readonly IDeviceSynchronizationProgress deviceSynchronizationProgress;

        public SupervisorSyncStatisticsHandler(IPlainStorage<InterviewerSyncStatisticsView, int?> statisticsStorage,
            ISynchronizationCompleteSource synchronizationCompleteSource,
            IDeviceSynchronizationProgress deviceSynchronizationProgress)
        {
            this.statisticsStorage = statisticsStorage;
            this.synchronizationCompleteSource = synchronizationCompleteSource;
            this.deviceSynchronizationProgress = deviceSynchronizationProgress;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<SyncStatisticsRequest, OkResponse>(SyncStatisticsAsync);
            requestHandler.RegisterHandler<SyncCompletedRequest, OkResponse>(HandleSyncCompletedAsync);
            requestHandler.RegisterHandler<SendSyncProgressInfoRequest, OkResponse>(SendSyncProgressInfoAsync);
        }

        private Task<OkResponse> SendSyncProgressInfoAsync(SendSyncProgressInfoRequest request)
        {
            this.deviceSynchronizationProgress.Publish(new DeviceSyncStats
            {
                ProgressInfo = request.Info,
                InterviewerLogin = request.InterviewerLogin
            });

            return OkResponse.Task;
        }

        public Task<OkResponse> HandleSyncCompletedAsync(SyncCompletedRequest arg)
        {
            synchronizationCompleteSource.NotifyOnCompletedSynchronization(true);
            return OkResponse.Task;
        }

        private Task<OkResponse> SyncStatisticsAsync(SyncStatisticsRequest request)
        {
            this.statisticsStorage.Store(new InterviewerSyncStatisticsView
            {
                InterviewerId = request.UserId,
                UploadedInterviewsCount = request.Statistics.UploadedInterviewsCount,
                DownloadedInterviewsCount = request.Statistics.DownloadedInterviewsCount,
                DownloadedQuestionnairesCount = request.Statistics.DownloadedQuestionnairesCount,
                RejectedInterviewsOnDeviceCount = request.Statistics.RejectedInterviewsOnDeviceCount,
                NewInterviewsOnDeviceCount = request.Statistics.NewInterviewsOnDeviceCount,
                NewAssignmentsCount = request.Statistics.NewAssignmentsCount,
                RemovedAssignmentsCount = request.Statistics.RemovedAssignmentsCount,
                RemovedInterviewsCount = request.Statistics.RemovedInterviewsCount,
                TotalUploadedBytes = request.Statistics.TotalUploadedBytes,
                TotalDownloadedBytes = request.Statistics.TotalDownloadedBytes,
                TotalConnectionSpeed = request.Statistics.TotalConnectionSpeed,
                TotalSyncDuration = request.Statistics.TotalSyncDuration,
                AssignmentsOnDeviceCount = request.Statistics.AssignmentsOnDeviceCount,
            });
            return OkResponse.Task;
        }
    }
}
