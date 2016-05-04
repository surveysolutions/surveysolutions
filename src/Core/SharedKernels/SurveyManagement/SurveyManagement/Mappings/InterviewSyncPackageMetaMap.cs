using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [Obsolete]
    public class InterviewSyncPackageMetaMap : ClassMapping<InterviewSyncPackageMeta>
    {
        public InterviewSyncPackageMetaMap()
        {
            Table("InterviewSyncPackageMetas");
            Id(x => x.PackageId, id => id.Generator(Generators.Assigned));

            Property(x => x.InterviewId);
            Property(x => x.UserId);
            Property(x => x.ItemType);
            Property(x => x.SerializedPackageSize);
            Property(x => x.SortIndex);
        }
    }
}