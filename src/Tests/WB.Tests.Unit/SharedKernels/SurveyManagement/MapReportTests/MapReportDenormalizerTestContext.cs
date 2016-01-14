using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    [Subject(typeof(MapReportDenormalizer))]
    public abstract class MapReportDenormalizerTestContext
    {
        protected static MapReportDenormalizer CreateMapReportDenormalizer(IReadSideRepositoryWriter<MapReportPoint> mapPoints = null,
            IReadSideRepositoryWriter<InterviewSummary> interviews = null,
               IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionsInfos = null)
        {
            return new MapReportDenormalizer(interviews ?? new TestInMemoryWriter<InterviewSummary>(), 
                questionsInfos ?? new TestInMemoryWriter<QuestionnaireQuestionsInfo>(), 
                mapPoints ?? new TestInMemoryWriter<MapReportPoint>());
        }
    }
}