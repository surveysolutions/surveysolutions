using System;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewerStatisticsApiV1Controller : ApiController
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;


        public InterviewerStatisticsApiV1Controller(IDeviceSyncInfoRepository deviceSyncInfo)
        {
            this.deviceSyncInfoRepository = deviceSyncInfo ?? throw new ArgumentNullException(nameof(deviceSyncInfo));
        }

        public void Post(InterviewerSyncStatisticsDto statistics)
        {
            var deviceInfo = this.deviceSyncInfoRepository.GetLastByInterviewerId(statistics.InterviewerId);
            deviceInfo.Statistics = new SyncStatistics
            {
                DownloadedInterviewsCount = statistics.DownloadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.DownloadedQuestionnairesCount,
                UploadedInterviewsCount = statistics.UploadedInterviewsCount,
                NewInterviewsOnDeviceCount = statistics.NewInterviewsOnDeviceCount,
                RejectedInterviewsOnDeviceCount = statistics.RejectedInterviewsOnDeviceCount,
                RemovedAssignmentsCount = statistics.RemovedAssignmentsCount,
                RemovedInterviewsCount = statistics.RemovedInterviewsCount,
                SyncFinishDate = DateTime.UtcNow,
                TotalConnectionSpeed = statistics.TotalConnectionSpeed,
                TotalDownloadedBytes = statistics.TotalDownloadedBytes,
                TotalUploadedBytes = statistics.TotalUploadedBytes,
                TotalSyncDuration = statistics.TotalSyncDuration,

                AssignmentsOnDeviceCount = statistics.AssignmentsOnDeviceCount,
                NewAssignmentsCount = statistics.NewAssignmentsCount
            };

            this.deviceSyncInfoRepository.AddOrUpdate(deviceInfo);
        }
    }
}
