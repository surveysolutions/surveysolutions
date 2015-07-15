using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Mappings
{
    [PlainStorage]
    public class LocalUserChangedFeedEntryMap : ClassMapping<LocalUserChangedFeedEntry>
    {
        public LocalUserChangedFeedEntryMap()
        {
            Id(x => x.EntryId, id => id.Generator(Generators.Assigned));

            Property(x => x.IsProcessed);
            Property(x => x.UserDetailsUri);
            Property(x => x.ProcessedWithError);
            Property(x => x.ChangedUserId);
            Property(x => x.Timestamp);
            Property(x => x.SupervisorId);

            Property(x => x.EntryType);
        }
    }

    [PlainStorage]
    public class LocalInterviewFeedEntryMap : ClassMapping<LocalInterviewFeedEntry>
    {
        public LocalInterviewFeedEntryMap()
        {
            Id(x => x.EntryId, id => id.Generator(Generators.Assigned));

            Property(x => x.Processed);
            Property(x => x.InterviewUri);
            Property(x => x.ProcessedWithError);
            Property(x => x.InterviewerId);
            Property(x => x.Timestamp);
            Property(x => x.SupervisorId);
            Property(x => x.UserId);
            Property(x => x.EntryType);
            Property(x => x.InterviewId);
        }
    }

    [PlainStorage]
    public class LocalQuestionnaireFeedEntryMap : ClassMapping<LocalQuestionnaireFeedEntry>
    {
        public LocalQuestionnaireFeedEntryMap()
        {
            Id(x => x.EntryId, id => id.Generator(Generators.Assigned));

            Property(x => x.Processed);
            Property(x => x.ProcessedWithError);
            Property(x => x.EntryType);
            Property(x => x.Timestamp);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireUri);
            Property(x => x.QuestionnaireVersion);
        }
    }
}