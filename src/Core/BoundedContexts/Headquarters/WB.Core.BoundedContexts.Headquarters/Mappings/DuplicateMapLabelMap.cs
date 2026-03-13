using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Mappings;

public class DuplicateMapLabelMap : ClassMapping<DuplicateMapLabel>
{
    public DuplicateMapLabelMap()
    {
        Table("duplicatemaplabels");

        Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.HighLow));
        Property(x => x.Label, ptp => ptp.NotNullable(true));
        Property(x => x.Count, ptp => ptp.NotNullable(true));
        Property(x => x.MapId, ptp =>
        {
            ptp.Column("map");
            ptp.Insert(false);
            ptp.Update(false);
        });
            
        ManyToOne(x => x.Map, mtm =>
        {
            mtm.Column("map");
            mtm.NotNullable(true);
        });
    }
}
