using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles
{
    public interface IInterviewerProfileFactory
    {
        Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid interviewerId);
    }

    public class InterviewerProfileFactory : IInterviewerProfileFactory
    {
        private readonly HqUserManager userManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public InterviewerProfileFactory(
            HqUserManager userManager,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository, IInterviewerVersionReader interviewerVersionReader)
        {
            this.userManager = userManager;
            this.interviewRepository = interviewRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        public async Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid userId)
        {
            var interviewer = await this.userManager.FindByIdAsync(userId);
            
            if (interviewer == null) return null;

            var supervisor = await this.userManager.FindByIdAsync(interviewer.Profile.SupervisorId.Value);

            var completedInterviewCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == userId && interview.Status == InterviewStatus.Completed));

            var approvedByHqCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == userId && interview.Status == InterviewStatus.ApprovedByHeadquarters));

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastSuccessByInterviewerId(userId);
            var hasUpdateForInterviewerApp = false;

            if (lastSuccessDeviceInfo != null)
            {
                int? interviewerApkVersion = interviewerVersionReader.Version;
                hasUpdateForInterviewerApp = interviewerApkVersion.HasValue && interviewerApkVersion.Value > lastSuccessDeviceInfo.AppBuildVersion;
            }

            return  new InterviewerProfileModel
            {
                Id = interviewer.Id,
                Email = interviewer.Email,
                LoginName = interviewer.UserName,
                IsArchived = interviewer.IsArchived,
                FullName = interviewer.FullName,
                Phone = interviewer.PhoneNumber,
                SupervisorName = supervisor.UserName,
                HasUpdateForInterviewerApp = hasUpdateForInterviewerApp,
                WaitingInterviewsForApprovalCount = completedInterviewCount,
                ApprovedInterviewsByHqCount = approvedByHqCount,
                TotalSuccessSynchronizationCount = this.deviceSyncInfoRepository.GetSuccessSynchronizationsCount(userId),
                TotalFailedSynchronizationCount = this.deviceSyncInfoRepository.GetFailedSynchronizationsCount(userId),
                LastSuccessDeviceInfo = lastSuccessDeviceInfo,
                LastSyncronizationDate = this.deviceSyncInfoRepository.GetLastSyncronizationDate(userId),
                LastFailedDeviceInfo = this.deviceSyncInfoRepository.GetLastFailedByInterviewerId(userId),
                AverageSyncSpeedBytesPerSecond = this.deviceSyncInfoRepository.GetAverageSynchronizationSpeedInBytesPerSeconds(userId),
                SynchronizationActivity = this.deviceSyncInfoRepository.GetSynchronizationActivity(userId),
                DeviceAssignmentDate = interviewer.Profile.DeviceRegistrationDate
            };
        }
    }
}
