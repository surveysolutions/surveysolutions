using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_InterviewOnClientCreated_event_and_one_of_prefilled_questions_is_GPS
    {
        Establish context = () =>
        {
            @event = Create.Event
                .InterviewOnClientCreated(
                    questionnaireId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                    questionnaireVersion: 33)
                .ToPublishedEvent(
                    eventSourceId: interviewId);

            QuestionnaireDocumentVersioned versionedQuestionnaire = Create.QuestionnaireDocumentVersioned(
                questionnaireDocument:
                    Create.QuestionnaireDocumentWithOneChapter(
                        Create.GpsCoordinateQuestion(questionId: prefilledGpsQuestionId, isPrefilled: true)),
                version: 1);

            var questionnaireId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$33";
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewStorage =
                Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>(storage
                    => storage.GetByIdAsync(questionnaireId) == Task.FromResult(new QuestionnaireDocumentView()
                    {
                        Id = questionnaireId,
                        Document = versionedQuestionnaire.Questionnaire
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

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<InterviewOnClientCreated> @event;
        private static InterviewView dashboardItem;
        private static Guid prefilledGpsQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("cccccccccccccccccccccccccccccccc");
    }
}