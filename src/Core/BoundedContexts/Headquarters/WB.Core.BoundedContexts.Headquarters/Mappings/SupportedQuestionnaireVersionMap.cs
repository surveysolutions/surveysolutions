using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class SupportedQuestionnaireVersionMap : ClassMapping<SupportedQuestionnaireVersion>
    {
        public SupportedQuestionnaireVersionMap()
        {
            Table("supportedquestionnaireversion");
            Id(x => x.Id, idMap => idMap.Generator(Generators.Assigned));
            Property(x => x.MinQuestionnaireVersionSupportedByInterviewer);
        }
    }
}