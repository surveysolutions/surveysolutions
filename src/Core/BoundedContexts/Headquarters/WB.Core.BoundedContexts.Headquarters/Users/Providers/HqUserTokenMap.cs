using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
{
    [Users]
    public class HqUserTokenMap: ClassMapping<HqUserToken>
    {
        public HqUserTokenMap()
        {
            Table("usertokens");
            Schema("users");


            ComposedId(m =>
            {
                m.Property(x => x.UserId);
                m.Property(x => x.LoginProvider);
                m.Property(x => x.Name);
            });


            /*
            Property(x => x.UserId);
            Property(x => x.LoginProvider);
            Property(x => x.Name);*/
            Property(x => x.Value);
        }
    }
}
