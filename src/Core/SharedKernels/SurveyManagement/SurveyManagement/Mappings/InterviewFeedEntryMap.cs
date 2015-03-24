using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewFeedEntryMap : ClassMapping<InterviewFeedEntry>
    {
        public InterviewFeedEntryMap()
        {
            Id(x => x.EntryId, mapper => mapper.Generator(Generators.Assigned));

            Property(x => x.SupervisorId);
            Property(x => x.EntryType);
            Property(x => x.Timestamp);
            Property(x => x.InterviewId);
            Property(x => x.UserId);
            Property(x => x.InterviewerId);
        }
    }
}