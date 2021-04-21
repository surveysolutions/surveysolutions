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
}
