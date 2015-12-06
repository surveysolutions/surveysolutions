using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_InterviewSynchronized_event_and_one_of_prefilled_questions_is_GPS_with_answer_of_string_type
    {
        Establish context = () =>
        {
            dashboardItem = Create.InterviewView(prefilledGpsQuestionId);

            @event = Create.Event
                .InterviewSynchronized(
                    Create.InterviewSynchronizationDto(
                        questionnaireId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                        questionnaireVersion: 33,
                        answers: new[] { Create.AnsweredQuestionSynchronizationDto(questionId: prefilledGpsQuestionId, answer: stringGpsAnswer) }))
                .ToPublishedEvent(
                    eventSourceId: interviewId);

            var questionnaireId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$33";
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(Create.GpsCoordinateQuestion(questionId: prefilledGpsQuestionId, isPrefilled: true));
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewStorage =
                Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>(storage
                    => storage.GetByIdAsync(questionnaireId) == Task.FromResult(new QuestionnaireDocumentView()
                    {
                        Id = questionnaireId,
                        Document = questionnaireDocument
                    }));

            var storeAsyncTask = new Task(() => { });
            storeAsyncTask.Start();

            var interviewViewStorage = Mock.Of<IAsyncPlainStorage<InterviewView>>(writer =>
            writer.GetByIdAsync(it.IsAny<string>()) == Task.FromResult(dashboardItem));

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.StoreAsync(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view)
                .Returns(storeAsyncTask);

            denormalizer = Create.DashboardDenormalizer(interviewViewRepository: interviewViewStorage,
                questionnaireDocumentViewRepository: questionnaireDocumentViewStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_store_prefilled_GPS_question_ID_to_result_dashboard_item = () =>
            dashboardItem.GpsLocation.PrefilledQuestionId.ShouldEqual(prefilledGpsQuestionId);

        It should_store_latitude_from_answer_to_prefilled_GPS_question_to_result_dashboard_item = () =>
            dashboardItem.GpsLocation.Coordinates.Latitude.ShouldEqual(prefilledGpsQuestionLatitude);

        It should_store_longitude_from_answer_to_prefilled_GPS_question_to_result_dashboard_item = () =>
            dashboardItem.GpsLocation.Coordinates.Longitude.ShouldEqual(prefilledGpsQuestionLongitude);

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<InterviewSynchronized> @event;
        private static InterviewView dashboardItem;
        private static Guid prefilledGpsQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("cccccccccccccccccccccccccccccccc");
        private static double prefilledGpsQuestionLatitude = 10.43;
        private static double prefilledGpsQuestionLongitude = -3.405;
        private static string stringGpsAnswer = "10.43,-3.405[0]0";
    }
}