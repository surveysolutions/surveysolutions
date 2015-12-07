using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    class when_answer_removed_from_gps_qestion : MapReportDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            interviewId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            gpsVariableName = "gps";

            var interviews = new TestInMemoryWriter<InterviewSummary>();
            interviews.Store(Create.InterviewSummary(interviewId, questionnaireId), interviewId);

            var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                Create.Question(questionId,
                    variable: gpsVariableName,
                    questionType: QuestionType.GpsCoordinates));

            mapPoints = new TestInMemoryWriter<MapReportPoint>();
            var pointId = $"{interviewId}-{gpsVariableName}-{Empty.RosterVector}";
            mapPoints.Store(new MapReportPoint(pointId), pointId);

            denormalizer = CreateMapReportDenormalizer(mapPoints, interviews, questionnaire);
            answersRemoved = Create.Event.AnswersRemoved(Create.Identity(questionId, Empty.RosterVector)).ToPublishedEvent(eventSourceId: interviewId);
        };

        Because of = () => denormalizer.Handle(answersRemoved);

        It should_remove_point_from_map = () => mapPoints.Count().ShouldEqual(0);

        static MapReportDenormalizer denormalizer;
        static Guid questionId;
        static TestInMemoryWriter<MapReportPoint> mapPoints;
        static IPublishedEvent<AnswersRemoved> answersRemoved;
        static Guid interviewId;
        static Guid questionnaireId;
        static string gpsVariableName;
    }
}