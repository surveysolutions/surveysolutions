using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Designer.UsageStats.QuestionnaireImport
{
    [PlainStorage]
    public class QuestionnaireImportedEntryMap : ClassMapping<QuestionnaireImportedEntry>
    {
        public QuestionnaireImportedEntryMap()
        {
            this.Table("QuestionnaireImportedEntries");
            this.Id(x => x.ImportDateUtc, mapper => mapper.Generator(Generators.Assigned));

            this.Property(x=>x.QuestionnaireId);
            this.Property(x => x.SupportedByHqVersion, pm =>
            {
                pm.Type<PostgresSqlArrayType<int>>();
                pm.Column(clm => clm.SqlType("numeric[]"));
            });
        }
    }
}