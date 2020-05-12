using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class UserMapMapping : ClassMapping<UserMap>
    {
        public UserMapMapping()
        {
            Table("usermaps");

            Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.HighLow));

            Property(x => x.Map, ptp => ptp.NotNullable(true));
            Property(x => x.UserName, ptp => ptp.NotNullable(true));
        }
    }
}
