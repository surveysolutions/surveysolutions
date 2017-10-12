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
            Property(x=> x.Wkid);
            Property(x=> x.XMaxVal);
            Property(x => x.XMinVal);
            Property(x => x.YMaxVal);
            Property(x => x.YMinVal);

            Property(x => x.MaxScale);
            Property(x => x.MinScale);
        }
    }
}