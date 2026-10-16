using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    [Users]
    public class HqUserProfileMap : ClassMapping<HqUserProfile>
    {
        public HqUserProfileMap()
        {
            Table("userprofiles");
            Schema("users");
            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Identity);
                idMap.Column("\"Id\"");
            });

            Property(x => x.DeviceId);
            Property(x => x.DeviceRegistrationDate, map => map.Type<PostgresDateType>());
            Property(x => x.DeviceAppVersion);
            Property(x => x.StorageFreeInBytes);
            Property(x => x.DeviceAppBuildVersion);
            Property(x => x.AllowRelinkDate);
        }
    }
}
