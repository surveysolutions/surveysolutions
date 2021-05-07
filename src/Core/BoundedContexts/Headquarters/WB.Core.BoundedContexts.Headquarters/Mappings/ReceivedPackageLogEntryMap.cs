using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class ReceivedPackageLogEntryMap : ClassMapping<ReceivedPackageLogEntry>
    {
        public ReceivedPackageLogEntryMap()
        {
            Table("ReceivedPackageLogEntries");
            Id(x => x.Id, IdMap => IdMap.Generator(Generators.Identity));

            Property(x => x.FirstEventId);
            Property(x => x.LastEventId);
            Property(x => x.FirstEventTimestamp);
            Property(x => x.LastEventTimestamp);
        }
    }
}
