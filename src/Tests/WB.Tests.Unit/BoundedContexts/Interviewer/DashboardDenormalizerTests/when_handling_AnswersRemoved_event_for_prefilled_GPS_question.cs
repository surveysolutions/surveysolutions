using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_AnswersRemoved_event_for_prefilled_GPS_question
    {
        Establish context = () =>
        {
            dashboardItem = Create.InterviewView();
            dashboardItem.GpsLocation = new InterviewGpsLocationView
            {
              PrefilledQuestionId  = Guid.Parse("11111111111111111111111111111111"),
              Coordinates = new InterviewGpsCoordinatesView
              {
                  Longitude = 10,
                  Latitude = 20
              }
            };

            @event = Create.Event.AnswersRemoved(Create.Identity("11111111111111111111111111111111", RosterVector.Empty)).ToPublishedEvent();

            var storeAsyncTask = new Task(() => { });
            storeAsyncTask.Start();

            var interviewViewStorage = Mock.Of<IAsyncPlainStorage<InterviewView>>(writer =>
            writer.GetByIdAsync(it.IsAny<string>()) == Task.FromResult(dashboardItem));

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.StoreAsync(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view)
                .Returns(storeAsyncTask);

            denormalizer = Create.DashboardDenormalizer(interviewViewRepository: interviewViewStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_clear_GPS_location = () =>
            dashboardItem.GpsLocation.Coordinates.ShouldBeNull();

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<AnswersRemoved> @event;
        private static InterviewView dashboardItem;
    }
}