using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewBinaryCleanupRecordMap : ClassMapping<InterviewBinaryCleanupRecord>
    {
        public InterviewBinaryCleanupRecordMap()
        {
            Table("interviewbinarycleanuprecords");
            Id(x => x.Id);

            Property(x => x.InterviewId);
            Property(x => x.FileName, map => map.Length(512));
            Property(x => x.QuestionType);
            Property(x => x.RequestedAt);
            Property(x => x.FailedCount);
        }
    }
}
