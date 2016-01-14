using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NSubstitute;
using NSubstitute.Core;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_GeoLocationQuestionAnswered_event
    {
        Establish context = () =>
        {
            dashboardItem = Create.InterviewView();
            dashboardItem.GpsLocation = new InterviewGpsLocationView
            {
              PrefilledQuestionId  = Guid.Parse("11111111111111111111111111111111")
            };

            @event = Create.Event.GeoLocationQuestionAnswered(Create.Identity("11111111111111111111111111111111", RosterVector.Empty), answerLatitude, answerLongitude).ToPublishedEvent();

            var storeAsyncTask = new Task(()=> {});
            storeAsyncTask.Start();

            var interviewViewStorage = Mock.Of<IAsyncPlainStorage<InterviewView>>(writer => 
            writer.GetById(it.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.StoreAsync(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view)
                .Returns(storeAsyncTask);

            denormalizer = Create.DashboardDenormalizer(interviewViewRepository: interviewViewStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_GPS_location_latitude_to_answered_value = () =>
            dashboardItem.GpsLocation.Coordinates.Latitude.ShouldEqual(answerLatitude);

        It should_set_GPS_location_longitude_to_answered_value = () =>
            dashboardItem.GpsLocation.Coordinates.Longitude.ShouldEqual(answerLongitude);

        private static InterviewEventHandler denormalizer;
        private static IPublishedEvent<GeoLocationQuestionAnswered> @event;
        private static InterviewView dashboardItem;
        private static double answerLatitude = 10;
        private static double answerLongitude = 20;
        private static void aaa() { }

        public Establish Context
        {
            get
            {
                return context;
            }

            set
            {
                this.context = value;
            }
        }
    }
}