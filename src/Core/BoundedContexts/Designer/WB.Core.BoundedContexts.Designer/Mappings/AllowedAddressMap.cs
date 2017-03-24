using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class AllowedAddressMap : ClassMapping<AllowedAddress>
    {
        public AllowedAddressMap()
        {
            Table("AllowedAddresses");
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));

            Property(x => x.Description);
            Property(x => x.Address, mapper => {
                mapper.Type<IpAddressAsString>();
                mapper.Column(clm => clm.SqlType("cidr"));
            });
        }
    }
}