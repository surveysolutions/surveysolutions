using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class MapReportAnswersCollectionMap : ClassMapping<MapReportPoint>
    {
        public MapReportAnswersCollectionMap()
        {
            Id(x=> x.Id, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(ptp => ptp.Variable);
            Property(ptp => ptp.InterviewId);
            Property(ptp => ptp.Latitude);
            Property(ptp => ptp.Longitude);
        }
    }
}