using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Migrator;
using WB.Core.BoundedContexts.Headquarters.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity.Migrations
{
    public class AddDeviceAppVersionToUserProfileMigration : DataMigration<HQIdentityDbContext>
    {
        public override string Id => "20170406013011";

        public override void Up(HQIdentityDbContext context)
        {
            var deviceContext = ServiceLocator.Current.GetInstance<HQPlainStorageDbContext>();

            var interviewerAppVersionList = (from info in deviceContext.DeviceSyncInfo
                group info by info.InterviewerId
                into g
                let last = g.OrderByDescending(i => i.Id).FirstOrDefault()
                select new
                {
                    g.Key,
                    last.AppVersion,
                    last.AppBuildVersion
                }).ToList();

            foreach (var interviewerAppVersion in interviewerAppVersionList)
            {
                var user = context.Users.Find(interviewerAppVersion.Key);
                if (user == null) continue;

                user.Profile.DeviceAppVersion = interviewerAppVersion.AppVersion;
                user.Profile.DeviceAppBuildVersion = interviewerAppVersion.AppBuildVersion;
            }

            context.SaveChanges();
        }
    }
}