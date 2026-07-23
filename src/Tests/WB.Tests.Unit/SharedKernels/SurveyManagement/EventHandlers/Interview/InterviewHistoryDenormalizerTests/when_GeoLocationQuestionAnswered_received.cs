using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_GeoLocationQuestionAnswered_received : InterviewHistoryDenormalizerTestContext
    {
        [Test]
        public void should_include_gps_provider_and_mode_when_provider_present()
        {
            Guid interviewId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            string variableName = "q1";

            var interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId, variable: variableName)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var answerEvents = new List<IEvent>
            {
                new GeoLocationQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1, 2, 3, 4,
                    new DateTimeOffset(new DateTime(1984, 4, 18)), gpsProvider: "gps", isFromMockProvider: true)
            };

            var denormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView, denormalizer);

            interviewHistoryView.Records[0].Parameters["answer"].Should().Be("1,2[3]4");
            interviewHistoryView.Records[0].Parameters["provider"].Should().Be("gps");
            interviewHistoryView.Records[0].Parameters["mode"].Should().Be("mock");
        }

        [Test]
        public void should_report_device_mode_when_not_from_mock_provider()
        {
            Guid interviewId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            string variableName = "q1";

            var interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId, variable: variableName)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var answerEvents = new List<IEvent>
            {
                new GeoLocationQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1, 2, 3, 4,
                    new DateTimeOffset(new DateTime(1984, 4, 18)), gpsProvider: "fused", isFromMockProvider: false)
            };

            var denormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView, denormalizer);

            interviewHistoryView.Records[0].Parameters["provider"].Should().Be("fused");
            interviewHistoryView.Records[0].Parameters["mode"].Should().Be("device");
        }

        [Test]
        public void should_not_include_gps_provider_and_mode_when_provider_absent()
        {
            Guid interviewId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            string variableName = "q1";

            var interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId, variable: variableName)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var answerEvents = new List<IEvent>
            {
                new GeoLocationQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1, 2, 3, 4,
                    new DateTimeOffset(new DateTime(1984, 4, 18)))
            };

            var denormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView, denormalizer);

            interviewHistoryView.Records[0].Parameters["answer"].Should().Be("1,2[3]4");
            interviewHistoryView.Records[0].Parameters.Should().NotContainKey("provider");
            interviewHistoryView.Records[0].Parameters.Should().NotContainKey("mode");
        }
    }
}
