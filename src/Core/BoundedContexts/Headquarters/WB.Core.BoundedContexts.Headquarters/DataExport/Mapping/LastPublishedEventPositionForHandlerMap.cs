using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Mapping
{
    public class LastPublishedEventPositionForHandlerMap : ClassMapping<LastPublishedEventPositionForHandler>
    {
        public LastPublishedEventPositionForHandlerMap()
        {
            Id(x => x.EventHandlerName, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.EventSequenceOfLastSuccessfullyHandledEvent);
            Property(x => x.EventSourceIdOfLastSuccessfullyHandledEvent);
            Property(x => x.CommitPosition);
            Property(x => x.PreparePosition);
        }
    }
}