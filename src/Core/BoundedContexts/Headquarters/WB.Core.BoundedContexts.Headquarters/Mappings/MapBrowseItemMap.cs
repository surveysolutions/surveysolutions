using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class MapBrowseItemMap : ClassMapping<MapBrowseItem>
    {
        public MapBrowseItemMap()
        {
            this.Table("MapBrowseItems");

            Id(x => x.Id);

            Property(x => x.FileName);
            Property(x => x.Size);
            Property(x => x.ImportDate);
        }
    }
}
