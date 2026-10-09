using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class MapFileDeletionRequestMap : ClassMapping<MapFileDeletionRequest>
    {
        public MapFileDeletionRequestMap()
        {
            Table("mapfiledeletionrequests");

            Id(x => x.Id, m => m.Length(255));
            Property(x => x.FileName, m =>
            {
                m.Column("filename");
                m.Length(255);
            });
        }
    }
}
