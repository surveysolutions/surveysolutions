using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
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
}