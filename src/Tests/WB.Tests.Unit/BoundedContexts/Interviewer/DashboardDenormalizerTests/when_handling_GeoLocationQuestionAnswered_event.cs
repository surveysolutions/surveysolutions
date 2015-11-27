using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.UI.Interviewer.ViewModel.Dashboard;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_GeoLocationQuestionAnswered_event
    {
        Establish context = () =>
        {
            dashboardItem = Create.QuestionnaireDTO();
            dashboardItem.GpsLocationQuestionId = "11111111111111111111111111111111";
            dashboardItem.GpsLocationLatitude = null;
            dashboardItem.GpsLocationLongitude = null;

            @event = Create.Event.GeoLocationQuestionAnswered(Create.Identity("11111111111111111111111111111111", RosterVector.Empty), answerLatitude, answerLongitude).ToPublishedEvent();

            var questionnaireDtoDocumentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDTO>>(writer
                => writer.GetById(it.IsAny<string>()) == dashboardItem);

            denormalizer = Create.DashboardDenormalizer(questionnaireDtoDocumentStorage: questionnaireDtoDocumentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_GPS_location_latitude_to_answered_value = () =>
            dashboardItem.GpsLocationLatitude.ShouldEqual(answerLatitude);

        It should_set_GPS_location_longitude_to_answered_value = () =>
            dashboardItem.GpsLocationLongitude.ShouldEqual(answerLongitude);

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<GeoLocationQuestionAnswered> @event;
        private static QuestionnaireDTO dashboardItem;
        private static double answerLatitude = 10;
        private static double answerLongitude = 20;
    }
}