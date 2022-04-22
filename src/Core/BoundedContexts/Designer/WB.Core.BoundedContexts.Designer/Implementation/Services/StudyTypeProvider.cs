using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class StudyTypeItem
    {
        public StudyType Code { get; set; }
        public string? Title { get; set; }
    }

    public static class StudyTypeProvider
    {
        public static List<StudyTypeItem> GetStudyTypeItems()
        {
            var codes = GetStudyTypeCodes();
            return codes.Select(code => new StudyTypeItem()
            {
                Code = code,
                Title = GetStudyTypeTitleByCode(code)
            }).ToList();
        }

        public static string? GetStudyTypeTitleByCode(StudyType code)
        {
            return Resources.StudyType.ResourceManager.GetString(code.ToString());
        }

        public static List<StudyType> GetStudyTypeCodes()
        {
            return new List<StudyType>()
            {
                StudyType.AdministrativeRecords,
                StudyType.AgriculturalCensus,
                StudyType.AgriculturalSurvey,
                StudyType.ChildLaborSurvey,
                StudyType.CoreWelfareIndicatorsQuestionnaire,
                StudyType.DemographicAndHealthSurvey,
                StudyType.EnterpriseSurvey,
                StudyType.EnterpriseCensus,
                StudyType.InformalSectorSurvey,
                StudyType.IntegratedSurvey,
                StudyType.MultipleIndicatorClusterSurvey,
                StudyType.LaborForceSurvey,
                StudyType.LivingStandardsMeasurementStudy,
                StudyType.HouseholdHealthSurvey,
                StudyType.HouseholdSurvey,
                StudyType.PriceSurvey,
                StudyType.PrioritySurvey,
                StudyType.PopulationAndHousingCensus,
                StudyType.SampleFrame,
                StudyType.ServiceProvisionAssessments,
                StudyType.SocioEconomicMonitoringSurvey,
                StudyType.StatisticalInfoAndMonitoringProg,
                StudyType.WorldFertilitySurvey,
                StudyType.WorldHealthSurvey
            };
        }
    }
}
