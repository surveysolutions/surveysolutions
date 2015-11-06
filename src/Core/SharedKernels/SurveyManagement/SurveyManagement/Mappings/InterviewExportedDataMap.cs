using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewExportedDataMap : ClassMapping<InterviewExportedDataRecord>
    {
        public InterviewExportedDataMap()
        {
            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.Data);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
        }
    }
}