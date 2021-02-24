using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    public class WorkspaceUserProfileMap : ClassMapping<WorkspaceUserProfile>
    {
        public WorkspaceUserProfileMap()
        {
            this.DynamicUpdate(false);
            this.DynamicInsert(false);

            Table("user_profiles");
            //Table("user_profile");

            /*
            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Identity);
                idMap.Column("\"Id\"");
            });
            */

            Property(x => x.Id);
            Property(x => x.DeviceId);
            Property(x => x.DeviceRegistrationDate);
            Property(x => x.SupervisorId);
            Property(x => x.DeviceAppVersion);
            Property(x => x.StorageFreeInBytes);
            Property(x => x.DeviceAppBuildVersion);
        }
    }
}