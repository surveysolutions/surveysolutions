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
            Property(x => x.ItemType);
            Property(x => x.ContentSize);
            Property(x => x.MetaInfoSize);

            Property(x => x.SortIndex, pm =>
            {
                pm.Generated(PropertyGeneration.Insert);
                pm.Update(false);
                pm.Insert(false);
                pm.Column(cm => { cm.SqlType("SERIAL"); });
            });

            Property(x => x.Content, pm =>
            {
                pm.Lazy(true);
                pm.Update(false);
            });

            Property(x => x.Meta, pm => pm.Update(false));
        }
    }
}