using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class ReadonlyUserMap : ClassMapping<ReadonlyUser>
    {
        public ReadonlyUserMap()
        {
            Schema("users");
            Table("users");
            Id(x => x.Id, map =>
            {
                map.Generator(Generators.Assigned);
                map.Column("\"Id\"");
            });
            Property(x => x.Name, map => map.Column("\"UserName\""));
        }
    }
}