using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    [Users]
    public class HqRoleMap : ClassMapping<HqRole>
    {
        public HqRoleMap()
        {
            this.Table("roles");
            this.Schema("users");
            this.Id(x => x.Id, m =>
            {
                m.Generator(Generators.Guid);
                m.Column("\"Id\"");
            });
            this.Property(x => x.Name, map =>
            {
                map.Length(255);
                map.NotNullable(true);
                map.Unique(true);
            });
            this.Bag(x => x.Users, map =>
            {
                map.Table("userroles");
                map.Schema("users");
                map.Cascade(Cascade.None);
                map.Key(k => k.Column("\"RoleId\""));
            }, rel => rel.ManyToMany(p => p.Column("\"UserId\"")));
        }
    }

    [Users]
    public class HqUserClaimMap : ClassMapping<HqUserClaim>
    {
        public HqUserClaimMap()
        {
            Table("userclaims");
            Schema("users");
            Id(x => x.Id, m =>
            {
                m.Generator(Generators.Identity);
                m.Column("\"Id\"");
            });
            Property(x => x.ClaimType);
            Property(x => x.ClaimValue);
            Property(x => x.UserId);
        }
    }

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
            Property(x => x.DeviceRegistrationDate);
            Property(x => x.SupervisorId);
            Property(x => x.DeviceAppVersion);
            Property(x => x.StorageFreeInBytes);
            Property(x => x.DeviceAppBuildVersion);

        }
    }
}
