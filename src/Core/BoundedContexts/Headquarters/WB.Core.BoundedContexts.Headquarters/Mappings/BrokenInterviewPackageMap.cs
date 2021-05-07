using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class BrokenInterviewPackageMap : ClassMapping<BrokenInterviewPackage>
    {
        public BrokenInterviewPackageMap()
        {
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.InterviewId);
            this.Property(x => x.IncomingDate);
            this.Property(x => x.ProcessingDate);
            this.Property(x => x.PackageSize);
            this.Property(x => x.IsCensusInterview);
            this.Property(x => x.Events);
            this.Property(x => x.InterviewStatus);
            this.Property(x => x.QuestionnaireId);
            this.Property(x => x.QuestionnaireVersion);
            this.Property(x => x.ResponsibleId);
            this.Property(x => x.ExceptionType);
            this.Property(x => x.ExceptionMessage);
            this.Property(x => x.ExceptionStackTrace);
            this.Property(x => x.InterviewKey);
            this.Property(x => x.ReprocessAttemptsCount);
        }
    }
}
