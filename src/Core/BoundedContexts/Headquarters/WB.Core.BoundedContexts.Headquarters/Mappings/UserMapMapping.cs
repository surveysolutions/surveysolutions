using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class UserMapMapping : ClassMapping<UserMap>
    {
        public UserMapMapping()
        {
            Table("usermaps");

            Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.HighLow));
            Property(x => x.UserName, ptp => ptp.NotNullable(true));
            
            ManyToOne(x => x.Map, mtm => mtm.Column("map"));
        }
    }
}
