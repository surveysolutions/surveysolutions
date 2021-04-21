using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    [Users]
    public class HqUserMap : ClassMapping<HqUser>
    {
        public HqUserMap()
        {
            this.Table("users");
            this.Schema("users");
            this.Id(x => x.Id, m =>
            {
                m.Generator(Generators.Guid);
                m.Column("\"Id\"");
            });

            this.Property(x => x.AccessFailedCount);

            this.Property(x => x.Email);

            this.Property(x => x.CreationDate,pm => pm.Type<UtcDateTimeType>());

            this.Property(x => x.EmailConfirmed);

            this.Property(x => x.LockoutEnabled);

            this.Property(x => x.LockoutEndDateUtc,pm => pm.Type<UtcDateTimeType>());

            this.Property(x => x.PasswordHash);

            this.Property(x => x.PhoneNumber);

            this.Property(x => x.PhoneNumberConfirmed);
            this.Property(x => x.IsLockedBySupervisor);
            this.Property(x => x.IsLockedByHeadquaters);
            this.Property(x => x.PasswordHashSha1);
            this.Property(x => x.IsArchived);
            this.Property(x => x.FullName);
            this.Property(x => x.PasswordChangeRequired, c => c.Column("password_change_required"));

            this.Property(x => x.TwoFactorEnabled);

            this.Property(x => x.UserName, map =>
            {
                map.Length(255);
                map.NotNullable(true);
                map.Unique(true);
            });

            this.Property(x => x.SecurityStamp);
            this.Property(x => x.LastLoginDate);

            this.Bag(x => x.Claims, map =>
            {
                map.Key(k =>
                {
                    k.Column("\"UserId\"");
                    k.Update(false); // to prevent extra update afer insert
                });
                map.Cascade(Cascade.None);
            }, rel =>
            {
                rel.OneToMany();
            });

            Set(x => x.Logins, cam =>
                {
                    cam.Table("userlogins");
                    cam.Schema("users");
                    cam.Key(km => km.Column("\"UserId\""));
                    cam.Cascade(Cascade.All | Cascade.DeleteOrphans);
                },
                map =>
                {
                    map.Component(comp =>
                    {
                        comp.Property(p => p.LoginProvider);
                        comp.Property(p => p.ProviderKey);
                    });
                });

            Set(x => x.DeviceSyncInfos, s =>
            {
                s.Cascade(Cascade.None);
                s.Key(k =>
                {
                    k.Column("\"InterviewerId\"");
                });
                s.Inverse(true);
            }, sm => sm.OneToMany());

            Bag(x => x.Roles, map =>
            {
                map.Table("userroles");
                map.Schema("users");
                map.Key(k => k.Column("\"UserId\""));
            }, rel => rel.ManyToMany(p => p.Column("\"RoleId\"")));


            ManyToOne(x => x.WorkspaceProfile, oto =>
            {
                oto.Cascade(Cascade.None);
                oto.Insert(false);
                oto.Update(false);
                oto.Lazy(LazyRelation.Proxy);
                oto.Column("\"UserProfileId\"");
            });
            ManyToOne(x => x.Profile, oto =>
            {
                oto.Cascade(Cascade.All);
                oto.Column("\"UserProfileId\"");
            });
            
            Set(x => x.Workspaces, map =>
            {
                map.Key(k => k.Column("user_id"));
                map.Cascade(Cascade.All|Cascade.DeleteOrphans);
                map.Inverse(true);
            }, rel => rel.OneToMany());
        }
    }
}
