using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    [Subject(typeof(MapReportDenormalizer))]
    public abstract class MapReportDenormalizerTestContext
    {
        protected static MapReportDenormalizer CreateMapReportDenormalizer(IReadSideRepositoryWriter<MapReportPoint> mapPoints = null,
            IReadSideRepositoryWriter<InterviewSummary> interviews = null,
            QuestionnaireDocument questionnaire = null)
        {
            return new MapReportDenormalizer(interviews ?? new TestInMemoryWriter<InterviewSummary>(), 
                Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire), 
                mapPoints ?? new TestInMemoryWriter<MapReportPoint>());
        }
    }
}