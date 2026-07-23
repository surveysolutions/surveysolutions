using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_geolocation_question_with_gps_provider_and_mode : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context()
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            questionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerGeoLocationQuestion(
                userId, questionId, new decimal[0], DateTime.Now,
                latitude: -1.234, longitude: 1.00025, accuracy: 10, altitude: 34, timestamp: new DateTimeOffset(DateTime.Now),
                gpsProvider: "gps", isFromMockProvider: true);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_GeoLocationQuestionAnswered_event_with_provider() =>
            eventContext.GetEvent<GeoLocationQuestionAnswered>().GpsProvider.Should().Be("gps");

        [NUnit.Framework.Test] public void should_raise_GeoLocationQuestionAnswered_event_with_mock_flag() =>
            eventContext.GetEvent<GeoLocationQuestionAnswered>().IsFromMockProvider.Should().BeTrue();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
    }
}
