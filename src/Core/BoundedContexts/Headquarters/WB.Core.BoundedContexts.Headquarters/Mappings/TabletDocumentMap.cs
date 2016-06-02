using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class TabletDocumentMap : ClassMapping<TabletDocument>
    {
        public TabletDocumentMap()
        {
            Table("TabletDocuments");
            Id(x => x.Id, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.DeviceId);
            Property(x => x.AndroidId);
            Property(x => x.RegistrationDate);
        }
    }
}