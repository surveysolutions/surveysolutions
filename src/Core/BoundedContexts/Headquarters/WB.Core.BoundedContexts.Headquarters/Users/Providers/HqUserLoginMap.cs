using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    /*[Users]
    public class HqUserLoginMap: ClassMapping<HqUserLogin>
    {
        public HqUserLoginMap()
        {
            Table("userlogins");
            Schema("users");

            ComposedId(m =>
            {
                m.Property(x => x.LoginProvider);
                m.Property(x => x.ProviderKey);
                m.Property(x => x.UserId);
            });
        }
    }*/
}
