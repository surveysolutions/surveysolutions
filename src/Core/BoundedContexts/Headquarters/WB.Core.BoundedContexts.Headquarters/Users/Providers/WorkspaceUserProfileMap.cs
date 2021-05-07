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

            Id(x => x.Id, p => p.Column("id"));
            //Property(x => x.Id, p => p.Column("id"));
            Property(x => x.DeviceId, p => p.Column("device_id"));
            Property(x => x.DeviceRegistrationDate, p => p.Column("device_registration_date"));
            Property(x => x.SupervisorId, p => p.Column("supervisor_id"));
            Property(x => x.DeviceAppVersion, p => p.Column("device_app_version"));
            Property(x => x.StorageFreeInBytes, p => p.Column("storage_free_in_bytes"));
            Property(x => x.DeviceAppBuildVersion, p => p.Column("device_app_build_version"));
        }
    }
}