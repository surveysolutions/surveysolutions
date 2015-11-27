using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.UI.Interviewer.ViewModel.Dashboard;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_AnswersRemoved_event_for_prefilled_GPS_question
    {
        Establish context = () =>
        {
            dashboardItem = Create.QuestionnaireDTO();
            dashboardItem.GpsLocationQuestionId = "11111111111111111111111111111111";
            dashboardItem.GpsLocationLatitude = 10;
            dashboardItem.GpsLocationLongitude = 20;

            @event = Create.Event.AnswersRemoved(Create.Identity("11111111111111111111111111111111", RosterVector.Empty)).ToPublishedEvent();

            var questionnaireDtoDocumentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDTO>>(writer
                => writer.GetById(it.IsAny<string>()) == dashboardItem);

            denormalizer = Create.DashboardDenormalizer(questionnaireDtoDocumentStorage: questionnaireDtoDocumentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_clear_GPS_location_latitude = () =>
            dashboardItem.GpsLocationLatitude.ShouldBeNull();

        It should_clear_GPS_location_longitude = () =>
            dashboardItem.GpsLocationLongitude.ShouldBeNull();

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<AnswersRemoved> @event;
        private static QuestionnaireDTO dashboardItem;
    }
}