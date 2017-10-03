using System;
using System.Collections.Generic;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportDenormalizerTests
{
    class when_answer_removed_from_gps_qestion
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            interviewId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            gpsVariableName = "gps";

            var interviews = new TestInMemoryWriter<InterviewSummary>();
            interviews.Store(Create.Entity.InterviewSummary(interviewId, questionnaireId: questionnaireId, questionnaireVersion: 1), interviewId);

            mapPoints = new TestInMemoryWriter<MapReportPoint>();
            var pointId = $"{interviewId}-{gpsVariableName}-{Empty.RosterVector}";
            mapPoints.Store(new MapReportPoint(pointId), pointId);

            var questionnaireQuestionsInfo = new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap = new Dictionary<Guid, string>
                {
                    {questionId, gpsVariableName}
                }
            };

            denormalizer = Create.Service.MapReportDenormalizer(mapPoints, interviews, questionnaireQuestionsInfo: questionnaireQuestionsInfo);
            answersRemoved = Create.Event.AnswersRemoved(Create.Entity.Identity(questionId, Empty.RosterVector)).ToPublishedEvent(eventSourceId: interviewId);
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