using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class QuestionnaireCompilationVersionMap : ClassMapping<QuestionnaireCompilationVersion>
    {
        public QuestionnaireCompilationVersionMap()
        {
            this.Table("questionnairecompilationversions");
            this.Id(x => x.QuestionnaireId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.Version);
        }
    }
}