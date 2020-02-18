using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class InterviewerStatisticsApiV1Controller : ControllerBase
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;


        public InterviewerStatisticsApiV1Controller(IDeviceSyncInfoRepository deviceSyncInfo)
        {
            this.deviceSyncInfoRepository = deviceSyncInfo ?? throw new ArgumentNullException(nameof(deviceSyncInfo));
        }

        [HttpPost]
        [Route("api/supervisor/v1/interviewerStatistics")]
        public IActionResult Post([FromBody]InterviewerSyncStatisticsApiView statistics)
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
            return NoContent();
        }
    }
}
