using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.Interviewer.ViewModel.Dashboard;
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

            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStorage =
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(storage
                    => storage.GetById("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$33") == versionedQuestionnaire);

            var questionnaireDtoDocumentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDTO>>();

            Mock.Get(questionnaireDtoDocumentStorage)
                .Setup(storage => storage.Store(it.IsAny<QuestionnaireDTO>(), interviewId.FormatGuid()))
                .Callback<QuestionnaireDTO, string>((view, id) => dashboardItem = view);

            denormalizer = Create.DashboardDenormalizer(
                questionnaireStorage: questionnaireStorage,
                questionnaireDtoDocumentStorage: questionnaireDtoDocumentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_store_prefilled_GPS_question_ID_to_result_dashboard_item = () =>
            dashboardItem.GpsLocationQuestionId.ShouldEqual(prefilledGpsQuestionId.FormatGuid());

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<InterviewOnClientCreated> @event;
        private static QuestionnaireDTO dashboardItem;
        private static Guid prefilledGpsQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("cccccccccccccccccccccccccccccccc");
    }
}