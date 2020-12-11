using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class SynchronizationLogItemMap : ClassMapping<SynchronizationLogItem>
    {
        public SynchronizationLogItemMap()
        {
            this.Table("SynchronizationLog");

            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));

            Property(x => x.InterviewerId);
            Property(x => x.InterviewId);
            Property(x => x.InterviewerName);
            Property(x => x.DeviceId);
            Property(x => x.LogDate);
            Property(x => x.Type);
            Property(x => x.Log);
            Property(x => x.ActionExceptionMessage);
            Property(x => x.ActionExceptionType);
        }
    }
}
