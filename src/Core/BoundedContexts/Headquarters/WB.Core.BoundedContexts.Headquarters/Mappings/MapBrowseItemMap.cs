using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class MapBrowseItemMap : ClassMapping<MapBrowseItem>
    {
        public MapBrowseItemMap()
        {
            Table("mapbrowseitems");

            Id(x => x.Id);

            Property(x => x.FileName);
            Property(x => x.Size);
            Property(x => x.ImportDate, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.UploadedBy);
            Property(x=> x.Wkid);
            Property(x=> x.XMaxVal);
            Property(x => x.XMinVal);
            Property(x => x.YMaxVal);
            Property(x => x.YMinVal);

            Property(x => x.MaxScale);
            Property(x => x.MinScale);
            
            Property(x => x.ShapesCount);
            Property(x => x.ShapeType);
            Property(x => x.GeoJson, m => m.Lazy(true));
            Property(x => x.IsPreviewGeoJson);
            
            Set(x => x.Users,
                collection =>
                {
                    collection.Key(key => key.Column("map"));
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    
                    collection.Inverse(true);
                },
                rel => rel.OneToMany());

            Set(x => x.DuplicateLabels,
                collection =>
                {
                    collection.Key(key => key.Column("map"));
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    
                    collection.Inverse(true);
                },
                rel => rel.OneToMany());
        }
    }
}
