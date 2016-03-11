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
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.Assigned));
            Property(x => x.InterviewId, cm =>cm.Index("InterviewPackage_InterviewId"));
            Property(x => x.IncomingDate, cm => cm.Index("InterviewPackage_IncomingDate"));
            Property(x => x.PackageContent);
        }
    }

    [PlainStorage]
    public class BrokenInterviewPackageMap : ClassMapping<BrokenInterviewPackage>
    {
        public BrokenInterviewPackageMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.Assigned));
            Property(x => x.InterviewId, cm => cm.Index("BrokenInterviewPackage_InterviewId"));
            Property(x => x.IncomingDate, cm => cm.Index("BrokenInterviewPackage_IncomingDate"));
            Property(x => x.ProcessingDate, cm => cm.Index("BrokenInterviewPackage_ProcessingDate"));
            Property(x => x.PackageContent);
            Property(x => x.ExceptionType, cm => cm.Index("BrokenInterviewPackage_ExceptionType"));
            Property(x => x.ExceptionMessage);
            Property(x => x.ExceptionStackTrace);
        }
    }
}