using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewPackageMap : ClassMapping<InterviewPackage>
    {
        public InterviewPackageMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            Property(x => x.InterviewId, cm => cm.Index("InterviewPackage_InterviewId"));
            Property(x => x.IncomingDate);
            Property(x => x.IsCensusInterview);
            Property(x => x.Events);
            Property(x => x.InterviewStatus);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.ResponsibleId);
            Property(x => x.ProcessAttemptsCount);
        }
    }
}
