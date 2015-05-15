using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class QuestionnaireSyncPackageMetaMap : ClassMapping<QuestionnaireSyncPackageMeta>
    {
        public QuestionnaireSyncPackageMetaMap()
        {
            Table("QuestionnaireSyncPackageMetas");
            Id(x => x.PackageId, mapper => mapper.Generator(Generators.Assigned));

            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.PackageId);
            Property(x => x.Timestamp);
            Property(x => x.SortIndex);
            Property(x => x.ItemType);
            Property(x => x.ContentSize);
            Property(x => x.MetaInfoSize);
        }
    }
}