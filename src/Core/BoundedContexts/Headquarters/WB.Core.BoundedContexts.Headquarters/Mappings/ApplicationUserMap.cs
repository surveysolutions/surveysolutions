using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    //[PlainStorage]
    //public class NHIdentityUserMap: IdentityUserMap { }

    //[PlainStorage]
    //public class ApplicationUserMap : ClassMapping<ApplicationUser>
    //{
    //    public ApplicationUserMap()
    //    {
    //        this.Table("AspNetUsers");
    //        this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
    //        this.Property(x => x.Email);
    //        this.Property(x => x.PasswordHash);
    //        this.Property(x => x.PhoneNumber);
    //        this.Property(x => x.FullName);
    //        this.Property(x => x.IsArchived, y => y.Type(new NHibernate.Type.BooleanType()));
    //        this.Property(x => x.IsLockedBySupervisor, y => y.Type(new NHibernate.Type.BooleanType()));
    //        this.Property(x => x.IsLockedByHeadquaters, y => y.Type(new NHibernate.Type.BooleanType()));
    //        this.Property(x => x.UserName, map =>
    //        {
    //            map.Length(255);
    //            map.NotNullable(true);
    //            map.Unique(true);
    //        });

    //        this.Bag(x => x.Claims, map =>
    //        {
    //            map.Key(k =>
    //            {
    //                k.Column("UserId");
    //                k.Update(false); // to prevent extra update afer insert
    //            });
    //            map.Cascade(Cascade.All | Cascade.DeleteOrphans);
    //        }, rel =>
    //        {
    //            rel.OneToMany();
    //        });

    //        this.Set(x => x.Logins, cam =>
    //        {
    //            cam.Table("AspNetUserLogins");
    //            cam.Key(km => km.Column("UserId"));
    //            cam.Cascade(Cascade.All | Cascade.DeleteOrphans);
    //        },
    //                 map =>
    //                 {
    //                     map.Component(comp =>
    //                     {
    //                         comp.Property(p => p.LoginProvider);
    //                         comp.Property(p => p.ProviderKey);
    //                     });
    //                 });

    //        this.Bag(x => x.Roles, map =>
    //        {
    //            map.Table("AspNetUserRoles");
    //            map.Key(k => k.Column("UserId"));
    //        }, rel => rel.ManyToMany(p => p.Column("RoleId")));
    //    }
    //}

    //[PlainStorage]
    //public class IdentityRoleMap : ClassMapping<IdentityRole>
    //{
    //    public IdentityRoleMap()
    //    {
    //        this.Table("AspNetRoles");
    //        this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
    //        this.Property(x => x.Name, map =>
    //        {
    //            map.Length(255);
    //            map.NotNullable(true);
    //            map.Unique(true);
    //        });
    //        this.Bag(x => x.Users, map =>
    //        {
    //            map.Table("AspNetUserRoles");
    //            map.Cascade(Cascade.None);
    //            map.Key(k => k.Column("RoleId"));
    //        }, rel => rel.ManyToMany(p => p.Column("UserId")));
    //    }
    //}

    //[PlainStorage]
    //public class IdentityUserClaimMap : ClassMapping<IdentityUserClaim>
    //{
    //    public IdentityUserClaimMap()
    //    {
    //        Table("AspNetUserClaims");
    //        Id(x => x.Id, m => m.Generator(Generators.Identity));
    //        Property(x => x.ClaimType);
    //        Property(x => x.ClaimValue);

    //        ManyToOne(x => x.User, m => m.Column("UserId"));
    //    }
    //}
}