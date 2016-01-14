using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    class when_gps_question_answered : MapReportDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            interviewId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            gpsVariableName = "gps";

            var interviews = new TestInMemoryWriter<InterviewSummary>();
            interviews.Store(Create.InterviewSummary(interviewId, questionnaireId), interviewId);

            mapPoints = new TestInMemoryWriter<MapReportPoint>();
            var questionIdToVariable = new TestInMemoryWriter<QuestionnaireQuestionsInfo>();
            questionIdToVariable.Store(new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap = new Dictionary<Guid, string>
            {
                {questionId, gpsVariableName }
            }
            }, new QuestionnaireIdentity(questionnaireId, 1).ToString());

            denormalizer = CreateMapReportDenormalizer(mapPoints, interviews, questionIdToVariable);
            gpsQuestionAnswered = Create.Event.GeoLocationQuestionAnswered(Create.Identity(questionId, Empty.RosterVector), 1, 2).ToPublishedEvent(eventSourceId: interviewId);
        };

        Because of = () => denormalizer.Handle(gpsQuestionAnswered);

        It should_fill_data_in_map_point = () =>
        {
            MapReportPoint mapReportPoint = mapPoints.Dictionary.Values.Single();
            mapReportPoint.QuestionnaireId.ShouldEqual(questionnaireId);
            mapReportPoint.InterviewId.ShouldEqual(interviewId);
            mapReportPoint.Latitude.ShouldEqual(gpsQuestionAnswered.Payload.Latitude);
            mapReportPoint.Longitude.ShouldEqual(gpsQuestionAnswered.Payload.Longitude);
            mapReportPoint.Variable.ShouldEqual(gpsVariableName);
        };

        It should_generate_unique_id_for_point = () =>
        {
            MapReportPoint mapReportPoint = mapPoints.Dictionary.Values.Single();
            mapReportPoint.Id.ShouldEqual($"{interviewId}-{gpsVariableName}-{Empty.RosterVector}"); 
        };

        static MapReportDenormalizer denormalizer;
        static Guid questionId;
        static TestInMemoryWriter<MapReportPoint> mapPoints;
        static IPublishedEvent<GeoLocationQuestionAnswered> gpsQuestionAnswered;
        static Guid interviewId;
        static Guid questionnaireId;
        static string gpsVariableName;
    }
}