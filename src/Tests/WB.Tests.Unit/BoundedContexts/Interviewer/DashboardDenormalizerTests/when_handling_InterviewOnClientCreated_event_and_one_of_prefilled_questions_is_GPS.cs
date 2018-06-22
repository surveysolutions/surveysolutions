using System;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_InterviewOnClientCreated_event_and_one_of_prefilled_questions_is_GPS
    {
        [OneTimeSetUp]
        public void context()
        {
            @event = Create.Event
                .InterviewOnClientCreated(
                    questionnaireId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                    questionnaireVersion: 33)
                .ToPublishedEvent(
                    eventSourceId: interviewId);

            var questionnaire = 
                    Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.GpsCoordinateQuestion(questionId: prefilledGpsQuestionId, isPrefilled: true));

            var questionnaireId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$33";
            IQuestionnaireStorage questionnaireStorage =
                Mock.Of<IQuestionnaireStorage>(storage
                    => storage.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(questionnaireId)) == questionnaire);

            var interviewViewStorage = Mock.Of<IPlainStorage<InterviewView>>(writer =>
            writer.GetById(it.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.Store(it.IsAny<InterviewView>()))
                .Callback<InterviewView>(view => dashboardItem = view);

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage,
                questionnaireStorage: questionnaireStorage);
        }

        [SetUp]
        public void because_of () =>
            denormalizer.Handle(@event);

        [Test]
        public void should_store_prefilled_GPS_question_ID_to_result_dashboard_item() =>
            dashboardItem.LocationQuestionId.Should().Be(prefilledGpsQuestionId);

        static InterviewDashboardEventHandler denormalizer;
        static IPublishedEvent<InterviewOnClientCreated> @event;
        static InterviewView dashboardItem;
        static Guid prefilledGpsQuestionId = Guid.Parse("11111111111111111111111111111111");
        static Guid interviewId = Guid.Parse("cccccccccccccccccccccccccccccccc");
    }
}
