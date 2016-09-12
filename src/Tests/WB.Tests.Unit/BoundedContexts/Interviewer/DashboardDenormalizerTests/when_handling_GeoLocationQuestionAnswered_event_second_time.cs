using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_GeoLocationQuestionAnswered_event_second_time
    {
        Establish context = () =>
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            var gpsQuestionId = Guid.Parse("11111111111111111111111111111111");

            dashboardItem = Create.Entity.InterviewView();
            dashboardItem.QuestionnaireId = questionnaireIdentity.ToString();
            dashboardItem.GpsLocation = new InterviewGpsLocationView
            {
              PrefilledQuestionId  = gpsQuestionId
            };

            @event = Create.Event.GeoLocationQuestionAnswered(Create.Entity.Identity("11111111111111111111111111111111", RosterVector.Empty), answerLatitude, answerLongitude).ToPublishedEvent();

            var interviewViewStorage = Mock.Of<IAsyncPlainStorage<InterviewView>>(writer => 
            writer.GetById(it.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.Store(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view);

            var questionnaire = Mock.Of<IQuestionnaire>(q => q.IsPrefilled(gpsQuestionId) == true);
            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(r =>
                r.GetQuestionnaire(questionnaireIdentity, Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);

            denormalizer.Handle(@event);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_GPS_location_latitude_to_answered_value = () =>
            dashboardItem.GpsLocation.Coordinates.Latitude.ShouldEqual(answerLatitude);

        It should_set_GPS_location_longitude_to_answered_value = () =>
            dashboardItem.GpsLocation.Coordinates.Longitude.ShouldEqual(answerLongitude);

        private static InterviewerDashboardEventHandler denormalizer;
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