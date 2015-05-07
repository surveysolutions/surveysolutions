using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class UserSyncPackageMetaMap : ClassMapping<UserSyncPackageMeta>
    {
        public UserSyncPackageMetaMap()
        {
            Table("UserSyncPackageMetas");
            Id(x => x.PackageId, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.UserId);
            Property(x => x.PackageId);
            Property(x => x.Timestamp);
            Property(x => x.SortIndex);
        }
    }
}