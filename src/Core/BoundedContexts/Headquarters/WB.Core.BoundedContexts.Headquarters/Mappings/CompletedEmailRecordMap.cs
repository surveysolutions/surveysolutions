using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class CompletedEmailRecordMap : ClassMapping<CompletedEmailRecord>
    {
        public CompletedEmailRecordMap()
        {
            Table("completedemailrecords");
            Id(x => x.InterviewId);

            Property(x => x.RequestTime);
            Property(x => x.FailedCount);
        }
    }
}
