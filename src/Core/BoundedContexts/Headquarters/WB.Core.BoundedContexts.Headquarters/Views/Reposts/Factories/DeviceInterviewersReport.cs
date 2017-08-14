using System.Linq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class DeviceInterviewersReport : IDeviceInterviewersReport
    {
        private readonly IUserRepository userRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public DeviceInterviewersReport(IUserRepository userRepository, IInterviewerVersionReader interviewerVersionReader)
        {
            this.userRepository = userRepository;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        public DeviceInterviewersReportView Load()
        {
            var targetVersion = interviewerVersionReader.Version;
            var data = (from u in userRepository.Users
                       group u by u.Profile.SupervisorId into teams
                       where teams.Key != null
                       select new DeviceInterviewersReportLine 
                       {
                           TeamId = teams.Key.Value,
                           NeverSynchedCount = teams.Count(x => x.Profile.SupervisorId != null &&
                                                                x.Profile.DeviceAppBuildVersion == null),
                           OutdatedCount = teams.Count(x => !(targetVersion.HasValue && targetVersion <= x.Profile.DeviceAppBuildVersion))
                       }).ToList();

            return new DeviceInterviewersReportView
            {
                Items = data,
                TotalCount = data.Count
            };
        }
    }
}