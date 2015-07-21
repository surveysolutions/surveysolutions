using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class UserChangedFeedEntryMap : ClassMapping<UserChangedFeedEntry>
    {
        public UserChangedFeedEntryMap()
        {
            this.Id(x => x.EntryId, mapper => mapper.Generator(Generators.Assigned));

            this.Property(x => x.ChangedUserId);
            this.Property(x => x.Timestamp);
            this.Property(x => x.SupervisorId);
            this.Property(x => x.EntryType);
        }
    }
}