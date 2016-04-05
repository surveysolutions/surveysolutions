using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
    public class InterviewPackageMap : ClassMapping<InterviewPackage>
    {
        public InterviewPackageMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            Property(x => x.InterviewId, cm => cm.Index("InterviewPackage_InterviewId"));
            Property(x => x.IncomingDate);
            Property(x => x.IsCensusInterview);
            Property(x => x.CompressedEvents);
            Property(x => x.InterviewStatus);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.ResponsibleId);
        }
    }

    [PlainStorage]
    public class BrokenInterviewPackageMap : ClassMapping<BrokenInterviewPackage>
    {
        public BrokenInterviewPackageMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            Property(x => x.InterviewId, cm => cm.Index("BrokenInterviewPackage_InterviewId"));
            Property(x => x.IncomingDate, cm => cm.Index("BrokenInterviewPackage_IncomingDate"));
            Property(x => x.ProcessingDate, cm => cm.Index("BrokenInterviewPackage_ProcessingDate"));
            Property(x => x.CompressedPackageSize, cm => cm.Index("BrokenInterviewPackage_CompressedPackageSize"));
            Property(x => x.PackageSize, cm => cm.Index("BrokenInterviewPackage_PackageSize"));
            Property(x => x.IsCensusInterview, cm => cm.Index("BrokenInterviewPackage_IsCensusInterview"));
            Property(x => x.CompressedEvents);
            Property(x => x.InterviewStatus);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.ResponsibleId);
            Property(x => x.ExceptionType, cm => cm.Index("BrokenInterviewPackage_ExceptionType"));
            Property(x => x.ExceptionMessage);
            Property(x => x.ExceptionStackTrace);
        }
    }
}