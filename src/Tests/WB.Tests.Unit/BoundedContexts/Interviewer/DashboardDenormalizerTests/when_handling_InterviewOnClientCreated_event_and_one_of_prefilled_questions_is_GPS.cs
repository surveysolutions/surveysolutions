using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
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

            var questionnaire = 
                    Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.GpsCoordinateQuestion(questionId: prefilledGpsQuestionId, isPrefilled: true));

            var questionnaireId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$33";
            IQuestionnaireStorage questionnaireStorage =
                Mock.Of<IQuestionnaireStorage>(storage
                    => storage.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(questionnaireId)) == questionnaire);

            var storeAsyncTask = new Task(() => { });
            storeAsyncTask.Start();

            var interviewViewStorage = Mock.Of<IPlainStorage<InterviewView>>(writer =>
            writer.GetById(it.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.Store(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view);

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage,
                questionnaireStorage: questionnaireStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_store_prefilled_GPS_question_ID_to_result_dashboard_item = () =>
            dashboardItem.GpsLocation.PrefilledQuestionId.ShouldEqual(prefilledGpsQuestionId);

        private static InterviewerDashboardEventHandler denormalizer;
        private static IPublishedEvent<InterviewOnClientCreated> @event;
        private static InterviewView dashboardItem;
        private static Guid prefilledGpsQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("cccccccccccccccccccccccccccccccc");
    }
}