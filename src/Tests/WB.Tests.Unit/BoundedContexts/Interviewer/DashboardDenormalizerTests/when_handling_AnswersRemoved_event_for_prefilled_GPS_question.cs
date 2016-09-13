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
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardDenormalizerTests
{
    internal class when_handling_AnswersRemoved_event_for_prefilled_GPS_question
    {
        Establish context = () =>
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
            var gpsQuestionId = Guid.Parse("11111111111111111111111111111111");

            dashboardItem = Create.Entity.InterviewView();
            dashboardItem.QuestionnaireId = questionnaireIdentity.ToString();
            dashboardItem.GpsLocation = new InterviewGpsLocationView
            {
              PrefilledQuestionId  = gpsQuestionId,
              Coordinates = new InterviewGpsCoordinatesView
              {
                  Longitude = 10,
                  Latitude = 20
              }
            };

            @event = Create.Event.AnswersRemoved(Create.Entity.Identity("11111111111111111111111111111111", RosterVector.Empty)).ToPublishedEvent();

            var interviewViewStorage = Mock.Of<IPlainStorage<InterviewView>>(writer =>
            writer.GetById(it.IsAny<string>()) == dashboardItem);

            Mock.Get(interviewViewStorage)
                .Setup(storage => storage.Store(it.IsAny<InterviewView>()))
                .Callback<InterviewView>((view) => dashboardItem = view);

            var questionnaire = Mock.Of<IQuestionnaire>(q => q.IsPrefilled(gpsQuestionId) == true);
            var plainQuestionnaireRepository = Mock.Of<IQuestionnaireStorage>(r =>
                r.GetQuestionnaire(questionnaireIdentity, Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = Create.Service.DashboardDenormalizer(interviewViewRepository: interviewViewStorage, questionnaireStorage: plainQuestionnaireRepository);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_clear_GPS_location = () =>
            dashboardItem.GpsLocation.Coordinates.ShouldBeNull();

        private static InterviewerDashboardEventHandler denormalizer;
        private static IPublishedEvent<AnswersRemoved> @event;
        private static InterviewView dashboardItem;
    }
}