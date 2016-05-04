using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
    public class SynchronizationLogItemMap : ClassMapping<SynchronizationLogItem>
    {
        public SynchronizationLogItemMap()
        {
            Table("SynchronizationLog");

            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));

            Property(x => x.InterviewerId);
            Property(x => x.InterviewerName);
            Property(x => x.DeviceId);
            Property(x => x.LogDate);
            Property(x => x.Type);
            Property(x => x.Log);
        }
    }
}